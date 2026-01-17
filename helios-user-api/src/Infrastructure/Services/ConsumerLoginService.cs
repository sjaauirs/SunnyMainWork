using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SunnyRewards.Helios.Common.Core.Domain;
using SunnyRewards.Helios.Common.Core.Helpers;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using SunnyRewards.Helios.Common.Core.Services;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Constant;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Models;
using SunnyRewards.Helios.User.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.User.Infrastructure.ReadReplica;
using SunnyRewards.Helios.User.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.User.Infrastructure.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using JwtPayload = SunnyRewards.Helios.User.Infrastructure.Helpers.JwtPayload;

namespace SunnyRewards.Helios.User.Infrastructure.Services
{
    public class ConsumerLoginService : BaseService, IConsumerLoginService
    {
        private readonly ILogger<ConsumerLoginService> _consumerLoginLogger;
        private readonly IVault _vault;
        private readonly NHibernate.ISession _session;
        private readonly IReadOnlySession? _readOnlySession;
        private readonly IConsumerRepo _consumerRepo;
        private readonly IConsumerLoginRepo _consumerLoginRepo;
        private readonly IPersonRepo _personRepo;
        private readonly IRoleRepo _roleRepo;
        private readonly IPersonRoleRepo _personRoleRepo;
        private readonly ITenantClient _tenantClient;
        private readonly IServerLoginRepo _serverLoginRepo;
        private readonly IEncryptionHelper _encryptionHelper;

        private NHibernate.ISession ReadSession => _readOnlySession?.Session ?? _session;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="consumerLoginLogger"></param>
        /// <param name="vault"></param>
        /// <param name="session"></param>
        /// <param name="consumerRepo"></param>
        /// <param name="consumerLoginRepo"></param>
        /// <param name="personRepo"></param>
        /// <param name="roleRepo"></param>
        /// <param name="personRoleRepo"></param>
        public ConsumerLoginService(
            ILogger<ConsumerLoginService> consumerLoginLogger,
            IVault vault,
            NHibernate.ISession session,
            IConsumerRepo consumerRepo,
            IConsumerLoginRepo consumerLoginRepo,
            IPersonRepo personRepo,
            IRoleRepo roleRepo,
            IPersonRoleRepo personRoleRepo,
            IServerLoginRepo serverLoginRepo,
            ITenantClient tenantClient,
            IEncryptionHelper encryptionHelper,
            IReadOnlySession? readOnlySession = null)
        {
            _consumerLoginLogger = consumerLoginLogger;
            _vault = vault;
            _session = session;
            _readOnlySession = readOnlySession;
            _consumerRepo = consumerRepo;
            _consumerLoginRepo = consumerLoginRepo;
            _personRepo = personRepo;
            _roleRepo = roleRepo;
            _personRoleRepo = personRoleRepo;
            _serverLoginRepo = serverLoginRepo;
            _tenantClient = tenantClient;
            _encryptionHelper = encryptionHelper;
        }

        const string className = nameof(ConsumerLoginService);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="consumerLoginRequestDto"></param>
        /// <returns></returns>
        public async Task<ConsumerLoginResponseDto> CreateToken(ConsumerLoginRequestDto consumerLoginRequestDto)
        {
            const string methodName = nameof(CreateToken);
            try
            {
                if (!string.IsNullOrEmpty(consumerLoginRequestDto.EncKeyId))
                {
                    var response = await ValidateEncryptedToken(consumerLoginRequestDto);
                    if (response.ErrorCode != null)
                        return response;
                }

                var env = await _vault.GetSecret("env");
                if (string.IsNullOrEmpty(env) || env == _vault.InvalidSecret)
                {
                    _consumerLoginLogger.LogError("{className}.{methodName}: ERROR - ENV is null or empty , Error Code:{errorCode}", className, methodName, StatusCodes.Status500InternalServerError);
                    return new ConsumerLoginResponseDto() { ErrorCode = 500, ErrorMessage = "Internal Error" };
                }
                var consumer = await GetConsumer(consumerLoginRequestDto);
                if (consumer == null || consumer.PersonId <= 0)
                {
                    _consumerLoginLogger.LogError("{className}.{methodName}: ERROR - Consumer not Found for Consumer code:{consumer} , Error Code:{errorCode}", className, methodName,consumerLoginRequestDto.ConsumerCode, StatusCodes.Status404NotFound);
                    return new ConsumerLoginResponseDto() { ErrorCode = 404, ErrorMessage = "Consumer Not Found" };
                }

                // Validate API token
                if (consumerLoginRequestDto?.ApiToken != null && consumer?.TenantCode != null)
                {
                    var isValidApiToken = await ValidateApiToken(consumer.TenantCode, consumerLoginRequestDto.ApiToken);
                    if (!isValidApiToken)
                    {
                        _consumerLoginLogger.LogError("{className}.{methodName}: ERROR - Please provide a valid api-token Consumer code:{consumer} , Error Code:{errorCode}", className, methodName, consumerLoginRequestDto.ConsumerCode, StatusCodes.Status401Unauthorized);
                        return new ConsumerLoginResponseDto() { ErrorCode = 401, ErrorMessage = "Please provide a valid ApiToken" };
                    }
                        
                }

                var person = await _personRepo.FindOneAsync(x => x.PersonId == consumer.PersonId && x.DeleteNbr == 0);
                // We have currently created the role only for the Subscriber role user.
                var subscriberRole = await _roleRepo.FindOneAsync(x => x.RoleCode == Constant.Subscriber && x.DeleteNbr == 0);
                var personRoles = await _personRoleRepo.FindAsync(x => x.PersonId == person.PersonId && x.RoleId == subscriberRole.RoleId && x.DeleteNbr == 0);
                var personRole = personRoles.FirstOrDefault();
                var role = await _roleRepo.FindOneAsync(x => x.RoleId == personRole.RoleId && x.DeleteNbr == 0);

                // in case some parallel ops cause multiple login records to be in undeleted state, allow multi record search
                var consumerLogin =  _consumerLoginRepo.FindAsync(x => x.ConsumerId == consumer.ConsumerId && x.TokenApp == Constant.RewardApp  && x.DeleteNbr == 0).Result
                    .OrderByDescending(x => x.ConsumerLoginId).FirstOrDefault();
                using (var transaction = _session.BeginTransaction())
                {
                    try
                    {
                        if (consumerLogin != null)
                        {
                            // Return existing AccessToken if not expired
                            var refreshThreshold = (DateTime.UtcNow - consumerLogin.RefreshTokenTs)?.TotalSeconds < Constants.USER_JWT_EXPIRY_SECONDS;
                            if (refreshThreshold)
                            {
                                consumerLogin.UpdateTs = DateTime.UtcNow;
                                consumerLogin.UserAgent = consumerLoginRequestDto.UserAgent;

                                await _session.UpdateAsync(consumerLogin);
                                await transaction.CommitAsync();
                                _consumerLoginLogger.LogInformation("{className}.{methodName}: Valid Token Found for Consumer: {consumer}", className, methodName, consumerLoginRequestDto.ConsumerCode);

                                return new ConsumerLoginResponseDto() { ConsumerCode = consumer.ConsumerCode, Jwt = consumerLogin.AccessToken };
                            }
                            else
                            {
                                // soft delete and re-create AccessToken if expired
                                consumerLogin.DeleteNbr = consumerLogin.ConsumerLoginId;
                                await _session.UpdateAsync(consumerLogin);
                            }
                        }

                        // Create and Save AccessToken
                        var newLogin = await CreateConsumerToken(role, consumer, env, person.Email, person.PersonUniqueIdentifier);
                        if (newLogin != null)
                        {
                            newLogin.TokenApp = Constant.RewardApp;
                            newLogin.UserAgent = consumerLoginRequestDto.UserAgent;
                        }
                        await _session.SaveAsync(newLogin);
                        await transaction.CommitAsync();

                        _consumerLoginLogger.LogInformation("{className}.{methodName}: Token Created for ConsumerCode: {ConsumerCode}", className, methodName, consumerLoginRequestDto?.ConsumerCode);

                        return new ConsumerLoginResponseDto() { ConsumerCode = consumer.ConsumerCode, Jwt = newLogin?.AccessToken };
                    }
                    catch (Exception ex)
                    {
                        _consumerLoginLogger.LogError(ex, "{className}.{methodName}: ERROR - Token not Created/Refreshed: msg: {msg}", className, methodName, ex.Message);
                        transaction.Rollback();
                        return new ConsumerLoginResponseDto() { ErrorMessage = ex.Message, ErrorDescription = ex.InnerException?.Message };
                    }
                }
            }
            catch (Exception ex)
            {
                _consumerLoginLogger.LogError(ex, "{className}.{methodName}: ERROR - msg: {msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return new ConsumerLoginResponseDto() { ErrorMessage = ex.Message, ErrorDescription = ex.InnerException?.Message };
            }
        }

        /// <summary>
        /// ValidateEncryptedToken
        /// </summary>
        /// <param name="consumerLoginRequestDto"></param>
        /// <returns></returns>
        private async Task<ConsumerLoginResponseDto> ValidateEncryptedToken(ConsumerLoginRequestDto consumerLoginRequestDto)
        {
            const string methodName = nameof(ValidateEncryptedToken);
            if (string.IsNullOrEmpty(consumerLoginRequestDto.TenantCode))
            {
                _consumerLoginLogger.LogError("{className}.{methodName}: ERROR - Please provide valid Tenant code:{tenant}, for Consumer code:{consumer} , Error Code:{errorCode}", className, methodName,consumerLoginRequestDto.TenantCode, consumerLoginRequestDto.ConsumerCode, StatusCodes.Status401Unauthorized);
                return new ConsumerLoginResponseDto() { ErrorCode = 401, ErrorMessage = "Invalid Tenant Code" };
            }
                
            if (string.IsNullOrEmpty(consumerLoginRequestDto.EncToken))
            {
                _consumerLoginLogger.LogError("{className}.{methodName}: ERROR - Invalid Encryption-token, for Consumer code:{consumer} , Error Code:{errorCode}", className, methodName, consumerLoginRequestDto.ConsumerCode, StatusCodes.Status401Unauthorized);
                return new ConsumerLoginResponseDto() { ErrorCode = 401, ErrorMessage = "Invalid Encryption Token" };
            }

            var symmetricEncryptionKey = await _vault.GetTenantSecret(consumerLoginRequestDto.TenantCode, SecretName.SymmetricEncryptionKey);

            if (string.IsNullOrEmpty(symmetricEncryptionKey) || symmetricEncryptionKey == _vault.InvalidSecret)
            {
                _consumerLoginLogger.LogError("{className}.{methodName}: ERROR - Invalid symmetric Encryption key, for Consumer code:{consumer} , Error Code:{errorCode}", className, methodName, consumerLoginRequestDto.ConsumerCode, StatusCodes.Status401Unauthorized);
                return new ConsumerLoginResponseDto() { ErrorCode = 401, ErrorMessage = "Invalid Symmetric Encryption Key" };
            }

            var signedJwtToken = _encryptionHelper.Decrypt(consumerLoginRequestDto.EncToken, Convert.FromBase64String(symmetricEncryptionKey));

            var jwtValidationKey = await _vault.GetTenantSecret(consumerLoginRequestDto.TenantCode, SecretName.CustomerJwtValidationKey);

            if (string.IsNullOrEmpty(jwtValidationKey) || jwtValidationKey == _vault.InvalidSecret)
            {
                _consumerLoginLogger.LogError("{className}.{methodName}: ERROR - Invalid Jwt validation key, for Consumer code:{consumer} , Error Code:{errorCode}", className, methodName, consumerLoginRequestDto.ConsumerCode, StatusCodes.Status401Unauthorized);
                return new ConsumerLoginResponseDto() { ErrorCode = 401, ErrorMessage = "Invalid JWT Validation Key" };
            }
            var tokenIssuer = await _vault.GetTenantSecret(consumerLoginRequestDto.TenantCode, SecretName.TokenIssuer);

            if (string.IsNullOrEmpty(tokenIssuer) || tokenIssuer == _vault.InvalidSecret)
            {
                _consumerLoginLogger.LogError("{className}.{methodName}: ERROR - Invalid JWT token Issuer, for Consumer code:{consumer} , Error Code:{errorCode}", className, methodName, consumerLoginRequestDto.ConsumerCode, StatusCodes.Status401Unauthorized);
                return new ConsumerLoginResponseDto() { ErrorCode = 401, ErrorMessage = "Invalid JWT Token Issuer" };
            }

            var claims = new Dictionary<string, string>();

            var isValidToken = ValidateAndExtractClaims(signedJwtToken, jwtValidationKey, tokenIssuer, out claims);
            if (!isValidToken)
            {
                _consumerLoginLogger.LogError("{className}.{methodName}: ERROR - Invalid JWT - Token, for Consumer code:{consumer} , Error Code:{errorCode}", className, methodName, consumerLoginRequestDto.ConsumerCode, StatusCodes.Status403Forbidden);
                return new ConsumerLoginResponseDto() { ErrorCode = 403, ErrorMessage = "Invalid Jwt Token" };
            }
                

            if (!claims.ContainsKey(Constant.PartnerCodeClaim) || !claims.ContainsKey(Constant.MemberIdClaim)
                || !claims.ContainsKey(Constant.KeyIdClaim))
            {
                _consumerLoginLogger.LogError("{className}.{methodName}: ERROR - Invalid JWT Token claims, for Consumer code:{consumer} , Error Code:{errorCode}", className, methodName, consumerLoginRequestDto.ConsumerCode, StatusCodes.Status403Forbidden);
                return new ConsumerLoginResponseDto() { ErrorCode = 403, ErrorMessage = "Invalid Jwt token claims" };
            }
                
            var partnerCode = claims[Constant.PartnerCodeClaim];
            var memberId = claims[Constant.MemberIdClaim];
            var encKeyId = claims[Constant.KeyIdClaim];

            if (consumerLoginRequestDto.EncKeyId != encKeyId)
            {
                _consumerLoginLogger.LogError("{className}.{methodName}: ERROR - Invalid Encryption key Id - Token, for Consumer code:{consumer} , Error Code:{errorCode}", className, methodName, consumerLoginRequestDto.ConsumerCode, StatusCodes.Status403Forbidden);
                return new ConsumerLoginResponseDto() { ErrorCode = 403, ErrorMessage = "Invalid Encryption Key Id" };
            }
                
            var getTenantByPartnerCodeRequestDto = new GetTenantByPartnerCodeRequestDto()
            {
                PartnerCode = partnerCode,
            };
            var tenantResponse = await _tenantClient.Post<GetTenantByPartnerCodeResponseDto>("tenant/get-by-partner-code", getTenantByPartnerCodeRequestDto);
            if (tenantResponse == null || tenantResponse?.Tenant?.TenantCode != consumerLoginRequestDto.TenantCode || tenantResponse.Tenant.EncKeyId != consumerLoginRequestDto.EncKeyId)
            {
                _consumerLoginLogger.LogError("{className}.{methodName}: ERROR - Invalid Partner Code, for Consumer code:{consumer} , Error Code:{errorCode}", className, methodName, consumerLoginRequestDto.ConsumerCode, StatusCodes.Status403Forbidden);
                return new ConsumerLoginResponseDto() { ErrorCode = 403, ErrorMessage = "Invalid Partner Code" };
            }

            consumerLoginRequestDto.MemberId = memberId;

            return new ConsumerLoginResponseDto();
        }

        /// <summary>
        /// ValidateAndExtractClaims
        /// </summary>
        /// <param name="jwtToken"></param>
        /// <param name="jwtValidationKey"></param>
        /// <param name="jwtIssuer"></param>
        /// <param name="tokenClaims"></param>
        /// <returns></returns>
        public bool ValidateAndExtractClaims(string jwtToken, string jwtValidationKey, string jwtIssuer, out Dictionary<string, string> tokenClaims)
        {
            tokenClaims = null;

            var key = Encoding.UTF8.GetBytes(jwtValidationKey);
            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                tokenHandler.ValidateToken(jwtToken, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = jwtIssuer,
                    ValidateAudience = false, // Adjust as needed
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtTokenClaims = (JwtSecurityToken)validatedToken;
                tokenClaims = new Dictionary<string, string>();

                foreach (var claim in jwtTokenClaims.Claims)
                {
                    tokenClaims.Add(claim.Type, claim.Value);
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="refreshTokenRequestDto"></param>
        /// <returns></returns>
        public async Task<RefreshTokenResponseDto> RefreshToken(RefreshTokenRequestDto refreshTokenRequestDto)
        {
            const string methodName = nameof(RefreshToken);
            try
            {
                var env = await _vault.GetSecret("env");
                if (string.IsNullOrEmpty(env) || env == _vault.InvalidSecret)
                {
                    _consumerLoginLogger.LogError("{className}.{methodName}: ERROR - Internal error ENV null or empty, for Consumer code:{consumer} , Error Code:{errorCode}", className, methodName, refreshTokenRequestDto.ConsumerCode, StatusCodes.Status500InternalServerError);
                    return new RefreshTokenResponseDto() { ErrorCode = 500, ErrorMessage = "Internal Error" };
                }
                    
                var consumer = await _consumerRepo.FindOneAsync(x => x.ConsumerCode == refreshTokenRequestDto.ConsumerCode && x.DeleteNbr == 0);
                if (consumer == null || consumer.PersonId <= 0)
                {
                    _consumerLoginLogger.LogError("{className}.{methodName}: ERROR - Consumer Not Found, for Consumer code:{consumer} , Error Code:{errorCode}", className, methodName, refreshTokenRequestDto.ConsumerCode, StatusCodes.Status404NotFound);
                    return new RefreshTokenResponseDto() { ErrorCode = 404, ErrorMessage = "ConsumerCode Not Found" };
                }

                var person = await _personRepo.FindOneAsync(x => x.PersonId == consumer.PersonId && x.DeleteNbr == 0);
                var personRole = await _personRoleRepo.FindOneAsync(x => x.PersonId == person.PersonId && x.DeleteNbr == 0);
                var role = await _roleRepo.FindOneAsync(x => x.RoleId == personRole.RoleId && x.DeleteNbr == 0);
                var consumerLogin = _consumerLoginRepo.FindAsync(x => x.AccessToken == refreshTokenRequestDto.AccessToken && x.DeleteNbr == 0)
                    .Result.OrderByDescending(x => x.ConsumerLoginId).FirstOrDefault();

                if (consumerLogin == null)
                {
                    _consumerLoginLogger.LogError("{className}.{methodName}: ERROR - consumer Token Not found, for Consumer code:{consumer} , Error Code:{errorCode}", className, methodName, refreshTokenRequestDto.ConsumerCode, StatusCodes.Status404NotFound);
                    return new RefreshTokenResponseDto() { ErrorCode = 404, ErrorMessage = "Token Not Found" };
                }
                    
                var refreshThreshold = (DateTime.UtcNow - consumerLogin.RefreshTokenTs)?.TotalSeconds < Constants.USER_JWT_EXPIRY_SECONDS;
                if (refreshThreshold)
                {
                    using (var transaction = _session.BeginTransaction())
                    {
                        var today = DateTime.UtcNow;
                        var jwtPayload = new JwtPayload()
                        {
                            ConsumerCode = consumer.ConsumerCode,
                            Email = person.Email,
                            TenantCode = consumer.TenantCode,
                            Role = role.RoleName,
                            Expiry = today.AddSeconds(Constants.USER_JWT_EXPIRY_SECONDS),
                            Environment = env,
                            PersonUniqueIdentifier = person.PersonUniqueIdentifier,
                            IsSSOUser = consumer.IsSSOUser
                        };
                        var accessToken = await GenerateJWToken(jwtPayload);

                        consumerLogin.AccessToken = accessToken;
                        consumerLogin.RefreshTokenTs = today;
                        consumerLogin.UpdateTs = today;

                        await _session.UpdateAsync(consumerLogin);
                        await transaction.CommitAsync();
                        _consumerLoginLogger.LogInformation("{className}.RefreshToken: Token Refreshed for ConsumerCode : {ConsumerCode}", className, refreshTokenRequestDto.ConsumerCode);

                        return new RefreshTokenResponseDto() { AccessToken = accessToken };
                    }
                }
                return new RefreshTokenResponseDto() { ErrorCode = 400, ErrorMessage = $"Token has Expired on : {consumerLogin.RefreshTokenTs}" };
            }
            catch (Exception ex)
            {
                _consumerLoginLogger.LogError(ex, "{className}.{methodName}: ERROR - msg: {msg}", className, methodName, ex.Message);
                return new RefreshTokenResponseDto() { ErrorMessage = ex.Message, ErrorDescription = ex.InnerException?.Message };
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="validateTokenRequestDto"></param>
        /// <returns></returns>
        public async Task<ValidateTokenResponseDto> ValidateToken(ValidateTokenRequestDto validateTokenRequestDto)
        {
            const string methodName = nameof(ValidateToken);
            try
            {
                var authToken = await _consumerLoginRepo.FindOneAsync(x => x.AccessToken == validateTokenRequestDto.AccessToken && x.DeleteNbr == 0);

                if (authToken == null || string.IsNullOrEmpty(authToken.AccessToken))
                    return new ValidateTokenResponseDto() { ErrorCode = 404, ErrorMessage = "token not found" };

                var expiration = GetTokenExpirationTime(authToken.AccessToken);
                var tokenDate = DateTimeOffset.FromUnixTimeSeconds(expiration).UtcDateTime;
                var now = DateTime.Now.ToUniversalTime();
                var valid = tokenDate >= now;

                if (valid)
                    _consumerLoginLogger.LogInformation("{className}.{methodName}: token is valid till : {tokenDate}", className, methodName, tokenDate);
                else
                    _consumerLoginLogger.LogError("{className}.{methodName}: token is expired on : {tokenDate}", className, methodName, tokenDate);

                return new ValidateTokenResponseDto() { IsValid = valid };
            }
            catch (Exception ex)
            {
                _consumerLoginLogger.LogError(ex, "{className}.{methodName}: ERROR - msg : {msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return new ValidateTokenResponseDto() { ErrorMessage = ex.Message, ErrorDescription = ex.InnerException?.Message };
            }
        }
       
        public async Task<ConsumerLoginDateResponseDto> GetConsumerLoginDetail(string consumerCode)
        {
            const string methodName = nameof(GetConsumerLoginDetail);
            try
            {
                var consumer= await GetConsumer( new ConsumerLoginRequestDto { ConsumerCode= consumerCode });
                var logints = await _consumerLoginRepo.GetFirstLoginDateAsync(consumer.ConsumerId);

                if (logints == null || logints == DateTime.MinValue)
                    return new ConsumerLoginDateResponseDto() { ErrorCode = 404, ErrorMessage = "Invalid Login Date" };

               
                return new ConsumerLoginDateResponseDto() {  ConsumerId= consumer.ConsumerId,LoginTs= logints,ConsumerCode= consumerCode };
            }
            catch (Exception ex)
            {
                _consumerLoginLogger.LogError(ex, "{className}.{methodName}: ERROR - msg : {msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                throw;
            }
        }
        public async Task<GetConsumerEngagementDetailResponseDto> GetConsumerEngagementDetail(GetConsumerEngagementDetailRequestDto consumerEngagementDetailRequestDto)
        {
            const string methodName = nameof(GetConsumerLoginDetail);
            try
            {
                var consumer = await GetConsumer(new ConsumerLoginRequestDto { ConsumerCode = consumerEngagementDetailRequestDto.ConsumerCode });
                var loginDetails = await _consumerLoginRepo.FindAsync(x => x.ConsumerId == consumer.ConsumerId && x.LoginTs>= consumerEngagementDetailRequestDto.EngagementFrom
                && x.LoginTs<= consumerEngagementDetailRequestDto.EngagementUntil,true);

                if (loginDetails == null || loginDetails.Count <=0)
                    return new GetConsumerEngagementDetailResponseDto() { ErrorCode = 404, ErrorMessage = "No engagement between date range found" };
                return new GetConsumerEngagementDetailResponseDto() { HasEngagement = true, ConsumerCode = consumerEngagementDetailRequestDto.ConsumerCode };

            }
            catch (Exception ex)
            {
                _consumerLoginLogger.LogError(ex, "{className}.{methodName}: ERROR - msg : {msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                throw;
            }
        }
        private async Task<ConsumerModel> GetConsumer(ConsumerLoginRequestDto consumerLoginRequestDto)
        {
            ConsumerModel consumer = new();
            if (!string.IsNullOrEmpty(consumerLoginRequestDto.ConsumerCode))
            {
                consumer = await _consumerRepo.FindOneAsync(x => x.ConsumerCode == consumerLoginRequestDto.ConsumerCode && x.DeleteNbr == 0);
            }
            else if (!string.IsNullOrEmpty(consumerLoginRequestDto.MemberId))
            {
                consumer = await _consumerRepo.FindOneAsync(x => x.TenantCode == consumerLoginRequestDto.TenantCode && x.MemberId == consumerLoginRequestDto.MemberId && x.DeleteNbr == 0);
            }
            else if (!string.IsNullOrEmpty(consumerLoginRequestDto.Email))
            {
                var person = await _personRepo.FindOneAsync(x => x.Email != null && x.Email == consumerLoginRequestDto.Email && x.DeleteNbr == 0);
                if (person.PersonId > 0)
                {
                    consumer = await _consumerRepo.FindOneAsync(x => x.TenantCode == consumerLoginRequestDto.TenantCode && x.PersonId == person.PersonId && x.DeleteNbr == 0);
                }
            }
            _consumerLoginLogger.LogInformation("{className}.GetConsumer: For Consumer Code: {consumerCode}", className, consumerLoginRequestDto.ConsumerCode);
            return consumer;
        }

        private async Task<ConsumerLoginModel> CreateConsumerToken(RoleModel role, ConsumerModel consumer, string env, string? email, string? personUniqueIdentifier)
        {
            if (role?.RoleName?.ToLower() == "subscriber")
            {
                var today = DateTime.UtcNow;
                var jwtPayload = new JwtPayload()
                {
                    ConsumerCode = consumer.ConsumerCode,
                    Email = email,
                    TenantCode = consumer.TenantCode,
                    Role = role.RoleName,
                    Expiry = today.AddSeconds(Constants.USER_JWT_EXPIRY_SECONDS),
                    Environment = env,
                    PersonUniqueIdentifier = personUniqueIdentifier,
                    IsSSOUser = consumer.IsSSOUser
                };
                var accessToken = await GenerateJWToken(jwtPayload);
                var consumerTokenData = new ConsumerLoginModel()
                {
                    ConsumerId = consumer.ConsumerId,
                    LoginTs = today,
                    RefreshTokenTs = today,
                    LogoutTs = null,
                    AccessToken = accessToken,
                    CreateTs = today,
                    CreateUser = consumer.CreateUser,
                    UpdateUser = consumer.UpdateUser,
                    DeleteNbr = 0
                };
                return consumerTokenData;
            }
            return new ConsumerLoginModel();
        }

        private async Task<string> GenerateJWToken(JwtPayload jwtPayload)
        {
            // Create claims
            var claims = new[]
            {
            new Claim("consumer_code", jwtPayload.ConsumerCode ?? string.Empty),
            new Claim("email", jwtPayload.Email ?? string.Empty),
            new Claim("person_unique_identifier", jwtPayload.PersonUniqueIdentifier ?? string.Empty),
            new Claim("is_sso_user", (jwtPayload.IsSSOUser ?? false).ToString().ToLowerInvariant()),
            new Claim("tenant_code", jwtPayload.TenantCode ?? string.Empty),
            new Claim("role", jwtPayload.Role ?? string.Empty),
            new Claim("exp", jwtPayload.Expiry.ToString() ?? string.Empty),
            new Claim("env", jwtPayload.Environment?.ToString() ?? string.Empty)
        };

            // Create token key
            string jwtSecretKey = await _vault.GetSecret(SecretName.JwtSecretKey);

            if (string.IsNullOrEmpty(jwtSecretKey) || jwtSecretKey == _vault.InvalidSecret)
            {
                _consumerLoginLogger.LogError("{className}.GenerateJWToken: jwtSecretKey is not configured.", className);
                throw new InvalidOperationException("jwtSecretKey is not configured.");
            }
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey));   // secretKey from aws secrets

            // Create signing credentials
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Create token
            var token = new JwtSecurityToken(
                issuer: JwtSettings.Issuer,
                audience: JwtSettings.Audience,
                claims: claims,
                expires: jwtPayload.Expiry,
                signingCredentials: creds);

            // Serialize token to string
            var tokenHandler = new JwtSecurityTokenHandler();
            var accessToken = tokenHandler.WriteToken(token);

            return await Task.FromResult(accessToken);
        }

        private static long GetTokenExpirationTime(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtSecurityToken = handler.ReadJwtToken(token);
            var tokenExp = jwtSecurityToken.Claims.First(claim => claim.Type.Equals("exp")).Value;
            var ticks = long.Parse(tokenExp);
            return ticks;
        }

        private async Task<bool> ValidateApiToken(string tenantCode, string apiToken)
        {
            try
            {
                const string methodName = nameof(ValidateApiToken);
                var getTenantCodeRequestDto = new GetTenantByTenantCodeRequestDto { TenantCode = tenantCode };

                // Retrieve tenant data
                var tenantResponse = await _tenantClient.Post<GetTenantByTenantCodeResponseDto>("tenant/get-by-tenant-code", getTenantCodeRequestDto);
                if (tenantResponse == null || tenantResponse.TenantCode == null)
                {
                    return false;
                }

                // Skip validation if server login is not enabled for the tenant
                if (!tenantResponse.EnableServerLogin)
                {
                    return true;
                }

                // Find server login model
                var serverLoginModel = await _serverLoginRepo.FindAsync(x => x.TenantCode == tenantResponse.TenantCode && x.ApiToken == apiToken && x.DeleteNbr == 0);
                var latestServerLogin = serverLoginModel?.OrderByDescending(x => x.ServerLoginId).FirstOrDefault();

                if (latestServerLogin == null)
                {
                    _consumerLoginLogger.LogWarning("{className}.{methodName}: Server login model not found for tenant code: {TenantCode} and API token: {ApiToken}", className, methodName, tenantCode, apiToken);
                    return false;
                }

                // Check if the API token is still valid
                var refreshThreshold = (DateTime.UtcNow - latestServerLogin.RefreshTokenTs).TotalSeconds < Constants.USER_JWT_EXPIRY_SECONDS;
                if (refreshThreshold)
                {
                    _consumerLoginLogger.LogInformation("{className}.{methodName}: API token validated successfully for tenant code: {TenantCode}", className, methodName, tenantCode);
                    return true;
                }
                else
                {
                    _consumerLoginLogger.LogInformation("{className}.{methodName}: API token expired for tenant code: {TenantCode}", className, methodName, tenantCode);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _consumerLoginLogger.LogError(ex, "Error occurred while validating API token for tenant code: {TenantCode}", tenantCode);
                throw;
            }
        }
    }
}