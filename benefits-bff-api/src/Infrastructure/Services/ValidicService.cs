using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sunny.Benefits.Bff.Core.Constants;
using Sunny.Benefits.Bff.Core.Domain.Dtos;
using Sunny.Benefits.Bff.Infrastructure.Helpers.Interface;
using Sunny.Benefits.Bff.Infrastructure.HttpClients.Interfaces;
using Sunny.Benefits.Bff.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using SunnyRewards.Helios.Tenant.Core.Domain.Models;

namespace Sunny.Benefits.Bff.Infrastructure.Services
{
    public class ValidicService : IValidicService
    {
        private readonly ILogger<ValidicService> _validicServiceLogger;
        private readonly IValidicClient _validicClient;
        private readonly IConfiguration _configuration;
        private readonly IVault _vault;
        private const string className = nameof(ValidicService);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="validicServiceLogger"></param>
        /// <param name="validicClient"></param>
        /// <param name="configuration"></param>
        public ValidicService(ILogger<ValidicService> validicServiceLogger, IValidicClient validicClient, IConfiguration configuration, IVault vault)
        {
            _validicServiceLogger = validicServiceLogger;
            _validicClient = validicClient;
            _configuration = configuration;
            _vault = vault;
        }

        public async Task<CreateValidicUserResponseDto> CreateValidicUser(CreateValidicUserRequestDto request)
        {
            const string methodName = nameof(CreateValidicUser);
            try
            {
                var token = await GetTenantSecret(request.TenantCode, CommonConstants.ValidicToken);
                if (string.IsNullOrEmpty(token))
                {
                    _validicServiceLogger.LogError("{ClassName}.{MethodName} - Token not found for tenant {TenantCode}", className, methodName, request.TenantCode);
                    return new CreateValidicUserResponseDto()
                    {
                        ErrorCode = StatusCodes.Status400BadRequest,
                        ErrorMessage = "Validic token not found for the tenant."
                    };
                }
                var organizationId = await GetTenantSecret(request.TenantCode, CommonConstants.ValidicOrgId);
                if (string.IsNullOrEmpty(organizationId))
                {
                    _validicServiceLogger.LogError("{ClassName}.{MethodName} - Org ID not found for tenant {TenantCode}", className, methodName, request.TenantCode);
                    return new CreateValidicUserResponseDto()
                    {
                        ErrorCode = StatusCodes.Status400BadRequest,
                        ErrorMessage = "Validic org id not found for the tenant."
                    };
                }

                string getUrl = $"organizations/{organizationId}/users/{request.ConsumerCode}/?token={token}";
                var parameters = new Dictionary<string, string>();
                var getResponse = await _validicClient.GetId<CreateValidicUserResponseDto>(getUrl, parameters);
                getResponse.OrgID = organizationId;

                if (getResponse != null && getResponse.Id != null)
                {
                    _validicServiceLogger.LogInformation("{ClassName}.{MethodName} - Validic user already exists", className, methodName);
                    return getResponse;
                }

                var postUrl = $"organizations/{organizationId}/users?token={token}";

                var validicRequestPayload = new ValidicCreateUserRequestPayloadDto
                {
                    uid = request.ConsumerCode
                };

                var response = await _validicClient.Post<CreateValidicUserResponseDto>(postUrl, validicRequestPayload);
                response.OrgID = organizationId;
                if (response.Id == null)
                {
                    _validicServiceLogger.LogError("{ClassName}.{MethodName} - Creating Validic user failed", className, methodName);
                    return new CreateValidicUserResponseDto()
                    {
                        ErrorCode = StatusCodes.Status422UnprocessableEntity,
                        ErrorMessage = response.errors != null && response.errors.Count > 0 ? string.Join(", ", response.errors) : "Failed to create Validic user.",
                    };
                }

                _validicServiceLogger.LogInformation("{ClassName}.{MethodName} - Created Validic User Successfully", className, methodName);

                return response;
            }
            catch (Exception ex)
            {
                _validicServiceLogger.LogError(ex, "{ClassName}.{MethodName} - Exception: {ExceptionMessage}", className, methodName, ex.Message);
                throw;
            }
        }

        private async Task<string> GetTenantSecret(string tenantCode, string secretKey)
        {
            const string methodName = nameof(GetTenantSecret);

            var secret = await _vault.GetTenantSecret(tenantCode ?? string.Empty, secretKey);

            if (string.IsNullOrEmpty(secret) || secret == _vault.InvalidSecret)
            {
                _validicServiceLogger.LogError($"{className}.{methodName}: Failed processing., Error Code:{StatusCodes.Status500InternalServerError}");
                return secret;
            }

            _validicServiceLogger.LogInformation($"{className}.{methodName}: Completed processing.");
            return secret;
        }
    }
}
