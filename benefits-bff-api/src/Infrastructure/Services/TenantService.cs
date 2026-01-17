using Microsoft.Extensions.Logging;
using Sunny.Benefits.Bff.Infrastructure.HttpClients.Interfaces;
using Sunny.Benefits.Bff.Infrastructure.Repositories.Interfaces;
using Sunny.Benefits.Bff.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using Sunny.Benefits.Bff.Core.Domain.Dtos;
using Newtonsoft.Json;

namespace Sunny.Benefits.Bff.Infrastructure.Services
{
    public class TenantService : ITenantService
    {
        private readonly ILogger<TenantService> _tenantServiceLogger;
        private readonly ITenantClient _tenantClient;
        private readonly IUserClient _userClient;
        private const string className = nameof(TenantService);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tenantServiceLogger"></param>
        /// <param name="tenantClient"></param>
        /// <param name="userClient"></param>
        public TenantService(ILogger<TenantService> tenantServiceLogger, ITenantClient tenantClient, IUserClient userClient)
        {
            _tenantServiceLogger = tenantServiceLogger;
            _tenantClient = tenantClient;
            _userClient = userClient;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tenantByConsumerCodeRequestDto"></param>
        /// <returns></returns>
        public async Task<GetTenantResponseDto> GetTenantByConsumerCode(string consumerCode)
        {
            const string methodName = nameof(GetTenantByConsumerCode);
            var Consumer = new BaseRequestDto()
            {
                consumerCode = consumerCode,
            };
            var consumerDto = await GetConsumer(Consumer);
            var tenantData = await GetTenantByTenantCode(consumerDto?.Consumer?.TenantCode ?? string.Empty);
            _tenantServiceLogger.LogInformation("{ClassName}.{MethodName} - Retrieved Tenant Data Successfully TenantCode:{TenantCode}, ConsumerCode :{ConsumerCode}", 
                className, methodName, consumerCode, tenantData.TenantCode);

            return new GetTenantResponseDto()
            {
                Tenant = tenantData
            };
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="consumerSummaryRequestDto"></param>
        /// <returns></returns>
        private async Task<GetConsumerResponseDto> GetConsumer(BaseRequestDto consumerSummaryRequestDto)
        {
            const string methodName = nameof(GetConsumer);
            var consumer = await _userClient.Post<GetConsumerResponseDto>("consumer/get-consumer", consumerSummaryRequestDto);
            if (consumer.Consumer == null)
            {
                _tenantServiceLogger.LogError("{ClassName}.{MethodName} - Consumer Details Not Found For ConsumerCode : {ConsumerCode}",className, methodName,consumerSummaryRequestDto.consumerCode);
                return new GetConsumerResponseDto();
            }
            _tenantServiceLogger.LogInformation("{ClassName}.{MethodName} - Retrieved Consumer Details Successfully For ConsumerCode : {ConsumerCode}", className, methodName, consumerSummaryRequestDto.consumerCode);

            return consumer;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tenantCode"></param>
        /// <returns></returns>
        public async Task<TenantDto> GetTenantByTenantCode(string tenantCode)
        {
            const string methodName = nameof(GetTenantByTenantCode);
            var getTenantCodeRequestDto = new GetTenantCodeRequestDto()
            {
                TenantCode = tenantCode,
            };
            var tenantResponse = await _tenantClient.Post<TenantDto>("tenant/get-by-tenant-code", getTenantCodeRequestDto);
            if (tenantResponse.TenantCode == null)
            {
                _tenantServiceLogger.LogError("{ClassName}.{MethodName} - TenantDetails Not Found for TenantCode : {TenantCode}", className, methodName, getTenantCodeRequestDto.TenantCode);
                return new TenantDto();
            }
            _tenantServiceLogger.LogInformation("Retrieved Tenant Successfully for TenantCode : {TenantCode}", getTenantCodeRequestDto.TenantCode);

            return tenantResponse;
        }

        /// <summary>
        /// Check if costco membership support is enabled for the tenant
        /// </summary>
        /// <param name="tenant"></param>
        /// <returns></returns>
        public bool CheckCostcoMemberhipSupport(TenantDto? tenant)
        {
            if (tenant == null || string.IsNullOrEmpty(tenant.TenantAttribute)) return false;

            var tenantAttributes = JsonConvert.DeserializeObject<TenantAttribute>(tenant.TenantAttribute);
            if (tenantAttributes == null) return false;
            return tenantAttributes.CostcoMemberShipSupport;
        }

    }
}
