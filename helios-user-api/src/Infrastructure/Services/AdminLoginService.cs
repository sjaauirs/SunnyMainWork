using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SunnyRewards.Helios.Common.Core.Domain;
using SunnyRewards.Helios.Common.Core.Helpers;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using SunnyRewards.Helios.Common.Core.Services;
using SunnyRewards.Helios.User.Core.Domain.Constant;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Models;
using SunnyRewards.Helios.User.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.User.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.User.Infrastructure.Services.Interfaces;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SunnyRewards.Helios.User.Infrastructure.Services
{
    public class AdminLoginService : BaseService, IAdminLoginService
    {
        private readonly ILogger<AdminLoginService> _logger;
        private readonly IVault _vault;
        private readonly NHibernate.ISession _session;
        private readonly IConsumerRepo _consumerRepo;
        private readonly IConsumerLoginRepo _consumerLoginRepo;
        private readonly IPersonRepo _personRepo;
        private readonly IRoleRepo _roleRepo;
        private readonly IPersonRoleRepo _personRoleRepo;
        private readonly ITenantClient _tenantClient;
        private readonly IEncryptionHelper _encryptionHelper;

        public AdminLoginService(
            ILogger<AdminLoginService> logger,
            IVault vault,
            NHibernate.ISession session,
            IConsumerRepo consumerRepo,
            IConsumerLoginRepo consumerLoginRepo,
            IPersonRepo personRepo,
            IRoleRepo roleRepo,
            IPersonRoleRepo personRoleRepo,
            IServerLoginRepo serverLoginRepo,
            ITenantClient tenantClient,
            IEncryptionHelper encryptionHelper)
        {
            _logger = logger;
            _vault = vault;
            _session = session;
            _consumerRepo = consumerRepo;
            _consumerLoginRepo = consumerLoginRepo;
            _personRepo = personRepo;
            _roleRepo = roleRepo;
            _personRoleRepo = personRoleRepo;
            _tenantClient = tenantClient;
            _encryptionHelper = encryptionHelper;
        }

        private const string ClassName = nameof(AdminLoginService);

        /// <summary>
        /// Creates a JWT token for the given consumer code.
        /// </summary>
        /// <param name="consumerCode">The consumer code.</param>
        /// <returns>An instance of <see cref="AdminLoginResponseDto"/> containing the token or an error message.</returns>
        public async Task<AdminLoginResponseDto> GenerateAdminTokenAsync(AdminLoginRequestDto adminLoginRequestDto)
        {
            const string MethodName = nameof(GenerateAdminTokenAsync);
            try
            {
                // Retrieve environment configuration
                var environment = await _vault.GetSecret("env");
                if (string.IsNullOrEmpty(environment) || environment == _vault.InvalidSecret)
                {
                    _logger.LogError("{ClassName}.{MethodName}: Environment secret is not configured.", ClassName, MethodName);
                    return new AdminLoginResponseDto { ErrorCode = StatusCodes.Status500InternalServerError, ErrorMessage = "Internal Error" };
                }

                // Validate consumer
                var consumer = await _consumerRepo.FindOneAsync(x => x.ConsumerCode == adminLoginRequestDto.ConsumerCode && x.DeleteNbr == 0);
                if (consumer == null || consumer.PersonId <= 0)
                {
                    _logger.LogWarning("{ClassName}.{MethodName}: Consumer not found for ConsumerCode: {ConsumerCode}.", ClassName, MethodName, adminLoginRequestDto.ConsumerCode);
                    return new AdminLoginResponseDto { ErrorCode = StatusCodes.Status404NotFound, ErrorMessage = "Consumer Not Found" };
                }

                var consumerLogin = (await _consumerLoginRepo.FindAsync(x => x.ConsumerId == consumer.ConsumerId  && x.TokenApp == Constant.AdminApp && x.DeleteNbr == 0))
                    .OrderByDescending(x => x.ConsumerLoginId)
                    .FirstOrDefault();

                using var transaction = _session.BeginTransaction();

                try
                {
                    var accessControlList = await GetAccessControlListAsync(consumer);

                    // Check if an existing token is valid
                    if (consumerLogin != null && (DateTime.UtcNow - consumerLogin.RefreshTokenTs)?.TotalSeconds < Constants.USER_JWT_EXPIRY_SECONDS)
                    {
                        consumerLogin.UpdateTs = DateTime.UtcNow;
                        await _session.UpdateAsync(consumerLogin);
                        await transaction.CommitAsync();

                        _logger.LogInformation("{ClassName}.{MethodName}: Valid token found for ConsumerCode: {ConsumerCode}.", ClassName, MethodName, adminLoginRequestDto.ConsumerCode);
                        return new AdminLoginResponseDto { ConsumerCode = consumer.ConsumerCode, Jwt = consumerLogin.AccessToken, Acl = accessControlList };
                    }

                    // Expired token handling
                    if (consumerLogin != null)
                    {
                        consumerLogin.DeleteNbr = consumerLogin.ConsumerLoginId;
                        await _session.UpdateAsync(consumerLogin);
                    }

                    // Generate a new token
                    var person = await _personRepo.FindOneAsync(x => x.PersonId == consumer.PersonId && x.DeleteNbr == 0);
                    if (person == null || person.PersonId <= 0)
                    {
                        _logger.LogWarning("{ClassName}.{MethodName}: Person not found for ConsumerCode: {ConsumerCode}.", ClassName, MethodName, adminLoginRequestDto.ConsumerCode);
                        return new AdminLoginResponseDto { ErrorCode = StatusCodes.Status404NotFound, ErrorMessage = "Consumer Not Found" };
                    }

                    var personRoles = await _personRoleRepo.FindAsync(x => x.PersonId == person.PersonId && x.DeleteNbr == 0);
                    if (personRoles == null || personRoles.Count == 0)
                    {
                        _logger.LogWarning("{ClassName}.{MethodName}: Person roles not found for ConsumerCode: {ConsumerCode}.", ClassName, MethodName, adminLoginRequestDto.ConsumerCode);
                        return new AdminLoginResponseDto { ErrorCode = StatusCodes.Status404NotFound, ErrorMessage = "Consumer Not Found" };
                    }

                    var newLogin = await CreateAdminTokenAsync(accessControlList, consumer, environment);

                    if (newLogin != null)
                    {
                        newLogin.TokenApp = Constant.AdminApp;
                        await _session.SaveAsync(newLogin);
                        await transaction.CommitAsync();

                        _logger.LogInformation("{ClassName}.{MethodName}: Token created for ConsumerCode: {ConsumerCode}.", ClassName, MethodName, adminLoginRequestDto.ConsumerCode);
                        return new AdminLoginResponseDto { ConsumerCode = consumer.ConsumerCode, Jwt = newLogin.AccessToken, Acl = accessControlList };
                    }

                    throw new InvalidOperationException("Failed to create a new token.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "{ClassName}.{MethodName}: Error during token creation for ConsumerCode: {ConsumerCode}.", ClassName, MethodName, adminLoginRequestDto.ConsumerCode);
                    await transaction.RollbackAsync();
                    return new AdminLoginResponseDto { ErrorMessage = ex.Message, ErrorDescription = ex.InnerException?.Message };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: Unexpected error. Message: {Message}", ClassName, MethodName, ex.Message);
                return new AdminLoginResponseDto { ErrorMessage = ex.Message, ErrorDescription = ex.InnerException?.Message };
            }
        }

        private async Task<List<RoleAccessDto>> GetAccessControlListAsync(ConsumerModel consumer)
        {
            var personRoles = await _personRoleRepo.FindAsync(x => x.PersonId == consumer.PersonId && x.DeleteNbr == 0);
            var roles = await _roleRepo.FindAsync(x => x.DeleteNbr == 0);

            return personRoles.Select(roleMapping =>
            {
                var role = roles.FirstOrDefault(r => r.RoleId == roleMapping.RoleId);
                return new RoleAccessDto
                {
                    CustomerCode = roleMapping.CustomerCode,
                    CustomerName = null,
                    SponsorCode = roleMapping.SponsorCode,
                    SponsorName = null,
                    TenantCode = roleMapping.TenantCode,
                    TenantName = null,
                    Role = role?.RoleName
                };
            }).ToList();
        }


        private async Task<ConsumerLoginModel> CreateAdminTokenAsync(IList<RoleAccessDto> acl, ConsumerModel consumer, string environment)
        {
            var now = DateTime.UtcNow;
            var jwtPayload = new AdminJwtPayload
            {
                ConsumerCode = consumer.ConsumerCode,
                Expiry = now.AddSeconds(Constants.USER_JWT_EXPIRY_SECONDS),
                Environment = environment,
                Acl = acl
            };

            var accessToken = await GenerateAdminJwtTokenAsync(jwtPayload);
            if (!string.IsNullOrEmpty(accessToken))
            {
                return new ConsumerLoginModel
                {
                    ConsumerId = consumer.ConsumerId,
                    LoginTs = now,
                    RefreshTokenTs = now,
                    AccessToken = accessToken,
                    CreateTs = now,
                    CreateUser = consumer.CreateUser,
                    DeleteNbr = 0
                };
            }

            throw new InvalidOperationException("Failed to generate access token.");
        }

        private async Task<string> GenerateAdminJwtTokenAsync(AdminJwtPayload jwtPayload)
        {
            const string MethodName = nameof(GenerateAdminJwtTokenAsync);

            try
            {
                var claims = new List<Claim>
                {
                    new(Constant.ConsumerCode, jwtPayload.ConsumerCode ?? string.Empty),
                    new(Constant.Exp, jwtPayload.Expiry.ToString()),
                    new(Constant.Env, jwtPayload.Environment ?? string.Empty)
                };

                foreach (var acl in jwtPayload.Acl)
                {
                    // Prepare the role name by sanitizing: replace spaces with underscores, convert to lowercase, and trim whitespace
                    var sanitizedRoleName = SanitizeString(acl.Role);

                    // Prepare the claim value by concatenating customer code, sponsor code, and tenant code
                    var claimValue = BuildClaimValue(acl.CustomerCode, acl.SponsorCode, acl.TenantCode);

                    // Add the claim, using the sanitized role name as the key and the concatenated value as the value
                    claims.Add(new(sanitizedRoleName, claimValue));
                }

                var secretKey = await _vault.GetSecret(SecretName.AdminJwtSecretKey);
                if (string.IsNullOrEmpty(secretKey) || secretKey == _vault.InvalidSecret)
                {
                    _logger.LogError("{ClassName}.{MethodName}: JWT secret key is not configured.", ClassName, MethodName);
                    throw new InvalidOperationException("JWT secret key is not configured.");
                }

                var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
                var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    issuer: JwtSettings.Issuer,
                    audience: JwtSettings.Audience,
                    claims: claims,
                    expires: jwtPayload.Expiry,
                    signingCredentials: credentials);

                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: Error generating JWT token. Message: {Message}", ClassName, MethodName, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Sanitizes a string by replacing spaces with underscores, converting to lowercase, and trimming whitespace.
        /// </summary>
        /// <param name="input">The string to sanitize.</param>
        /// <returns>The sanitized string.</returns>
        private static string SanitizeString(string? input) =>
            (input ?? string.Empty).Replace(" ", "_").Trim();

        /// <summary>
        /// Builds a claim value by concatenating customer code, sponsor code, and tenant code, ensuring proper formatting.
        /// </summary>
        /// <param name="customerCode">The customer code.</param>
        /// <param name="sponsorCode">The sponsor code.</param>
        /// <param name="tenantCode">The tenant code.</param>
        /// <returns>The formatted claim value.</returns>
        private static string BuildClaimValue(string? customerCode, string? sponsorCode, string? tenantCode)
        {
            // Concatenate the parts, convert to lowercase, trim whitespace, and remove trailing underscores
            return $"{customerCode ?? string.Empty}:{sponsorCode ?? string.Empty}:{tenantCode ?? string.Empty}"
                .Trim()
                .Trim(':');
        }
    }
}