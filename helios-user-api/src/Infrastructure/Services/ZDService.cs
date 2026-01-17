using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SunnyRewards.Helios.Common.Core.Domain;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using SunnyRewards.Helios.Common.Core.Services;
using SunnyRewards.Helios.User.Core.Domain.Constant;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Infrastructure.Helpers;
using SunnyRewards.Helios.User.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.User.Infrastructure.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SunnyRewards.Helios.User.Infrastructure.Services
{
    public class ZDService : BaseService, IZDService
    {
        private readonly ILogger<ZDService> _zdLogger;
        private readonly IVault _vault;
        private readonly IConsumerRepo _consumerRepo;
        private readonly IPersonRepo _personRepo;

        public ZDService(ILogger<ZDService> zdLogger,
         IVault vault, IConsumerRepo consumerRepo, IPersonRepo personRepo)
        {
            _zdLogger = zdLogger;
            _vault = vault;
            _consumerRepo = consumerRepo;
            _personRepo = personRepo;
        }

        const string className = nameof(ZDService);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="zdTokenRequestDto"></param>
        /// <returns></returns>
        public async Task<ZdTokenResponseDto> CreateZdToken(ZdTokenRequestDto zdTokenRequestDto)
        {
            const string methodName = nameof(CreateZdToken);
            try
            {
                var today = DateTime.UtcNow;
                var expiry = today.AddSeconds(Constants.USER_ZD_EXPIRY_SECONDS);

                var zenDeskKey = await _vault.GetSecret("ZENDESK_KEY");
                var zenDeskKid = await _vault.GetSecret("ZENDESK_KEYID");

                if (zdTokenRequestDto.ConsumerCode != null)
                {
                    var consumer = await _consumerRepo.FindOneAsync(x => x.ConsumerCode == zdTokenRequestDto.ConsumerCode);

                    var personData = await _personRepo.FindOneAsync(x => x.PersonId == consumer.PersonId);

                    if (string.IsNullOrEmpty(zenDeskKey) || zenDeskKey == _vault.InvalidSecret || zenDeskKid == _vault.InvalidSecret)
                    {
                        _zdLogger.LogError("{className}.{methodName}: ZenDeskKey not Found, Invalid Secret, for Consumer: {consumer}, Error Code:{errorCode}", className,methodName, zdTokenRequestDto.ConsumerCode, StatusCodes.Status500InternalServerError);
                        return new ZdTokenResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError, ErrorMessage = "Internal Error" };
                    }
                        
                    _zdLogger.LogInformation("{className}.CreateZdToken: Valid Token Found for Consumer: {consumer}", className, zdTokenRequestDto.ConsumerCode);
                    var payload = new ZDPayload()
                    {
                        name = $"{personData.FirstName} {personData.LastName}",
                        email = personData.Email,
                        external_Id = consumer.ConsumerCode,
                        exp = expiry
                    };

                    // Create claims
                    var claims = new[]
                    {
                       new Claim("external_id", payload.external_Id ?? string.Empty),
                       new Claim("email", payload.email ?? string.Empty),
                       new Claim("exp", payload.exp.ToString() ?? string.Empty),
                       new Claim("name", payload.name ?? string.Empty),
                       new Claim("scope", Constant.scope.ToString() ?? string.Empty),

                    };

                    // Create token key
                    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(zenDeskKey));
                    key.KeyId = zenDeskKid;
                    // Create signing credentials
                    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                    // Create token
                    var token = new JwtSecurityToken(
                        claims: claims,
                        signingCredentials: creds,
                        expires: payload.exp);

                    // Serialize token to string
                    var tokenHandler = new JwtSecurityTokenHandler();

                    var accessToken = tokenHandler.WriteToken(token);

                    _zdLogger.LogInformation("{className}.CreateZdToken: Valid Token Generate for ConsumerCode: {ConsumerCode}", className, zdTokenRequestDto.ConsumerCode);
                    return new ZdTokenResponseDto() { Jwt = accessToken };

                }
                _zdLogger.LogInformation("Token Expired for ConsumerCode: {ConsumerCode}", zdTokenRequestDto.ConsumerCode);
                return new ZdTokenResponseDto() { ErrorCode = 400, ErrorMessage = $"Token has Expired on : {expiry}" };
            }
            catch (Exception ex)
            {
                _zdLogger.LogError(ex, "{className}.{methodName}: ERROR - msg: {msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return new ZdTokenResponseDto() { ErrorMessage = ex.Message, ErrorDescription = ex.InnerException?.Message };
            }
        }
    }
}

