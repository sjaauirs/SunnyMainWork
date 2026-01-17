using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Sunny.Benefits.Bff.Core.Domain.Dtos;
using Sunny.Benefits.Bff.Infrastructure.HttpClients.Interfaces;
using Sunny.Benefits.Bff.Infrastructure.Services.Interfaces;
using SunnyBenefits.Fis.Core.Domain.Dtos;

namespace Sunny.Benefits.Bff.Infrastructure.Services
{
    public class TenantAccountService : ITenantAccountService
    {
        private readonly ILogger<TenantAccountService> _logger;
        private readonly IFisClient _fisClient;
        private const string className = nameof(TenantAccountService);

        public TenantAccountService(ILogger<TenantAccountService> logger, IFisClient fisClient)
        {
            _logger = logger;
            _fisClient = fisClient;
        }

        public async Task<ExportTenantAccountResponseDto> GetTenantAccount(ExportTenantAccountRequestDto requestDto)
        {
            const string methodName = nameof(GetTenantAccount);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName}: Fetching TenantAccount Details for TenantCode:{Tenant}", className, methodName, requestDto.TenantCode);
                var tenantAccount = await _fisClient.Post<ExportTenantAccountResponseDto>("tenant-account-export", requestDto);
                if (tenantAccount == null)
                {
                    _logger.LogError("{ClassName}.{MethodName} - TenantAccount Details Not Found for TenantCode : {TenantCode}", className, methodName, requestDto.TenantCode);
                    return new ExportTenantAccountResponseDto { ErrorCode = StatusCodes.Status404NotFound, ErrorMessage = $"Tenant Account Details Not Found, For TenantCode:{requestDto.TenantCode}" };
                }
                _logger.LogInformation("{ClassName}.{MethodName}: Successfully fetched TenantAccount Details for TenantCode:{Tenant}", className, methodName, requestDto.TenantCode);
                return tenantAccount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Error occured while fetching TenantAccount Details, TenantCode : {TenantCode} - ErrorCode:{ErrorCode}, ERROR:{ErrorMessage}", className, methodName, requestDto.TenantCode, StatusCodes.Status500InternalServerError, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Get tenantAccount by tenant code
        /// </summary>
        /// <param name="requestDto"></param>
        /// <returns></returns>
        public async Task<TenantAccountDto> GetTenantAccount(TenantAccountCreateRequestDto requestDto)
        {
            const string methodName = nameof(GetTenantAccount);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName}: Fetching TenantAccount Details for TenantCode:{Tenant}", className, methodName, requestDto.TenantCode);
                var tenantAccount = await _fisClient.Post<TenantAccountDto>("get-tenant-account", requestDto);
                if (tenantAccount == null)
                {
                    _logger.LogError("{ClassName}.{MethodName} - TenantAccount Details Not Found for TenantCode : {TenantCode}", className, methodName, requestDto.TenantCode);
                    return new TenantAccountDto { ErrorCode = StatusCodes.Status404NotFound, ErrorMessage = $"Tenant Account Details Not Found, For TenantCode:{requestDto.TenantCode}" };
                }
                _logger.LogInformation("{ClassName}.{MethodName}: Successfully fetched TenantAccount Details for TenantCode:{Tenant}", className, methodName, requestDto.TenantCode);
                return tenantAccount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Error occured while fetching TenantAccount Details, TenantCode : {TenantCode} - ErrorCode:{ErrorCode}, ERROR:{ErrorMessage}", className, methodName, requestDto.TenantCode, StatusCodes.Status500InternalServerError, ex.Message);
                throw;
            }
        }
    }
}
