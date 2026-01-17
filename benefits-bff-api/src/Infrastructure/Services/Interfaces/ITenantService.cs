using Sunny.Benefits.Bff.Core.Domain.Dtos;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;

namespace Sunny.Benefits.Bff.Infrastructure.Services.Interfaces
{
    public interface ITenantService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tenantByConsumerCodeRequestDto"></param>
        /// <returns></returns>
        Task<GetTenantResponseDto> GetTenantByConsumerCode(string consumerCode);

        /// <summary>
        /// Get tenant by tenant code
        /// </summary>
        /// <param name="tenantCode"></param>
        /// <returns></returns>
        Task<TenantDto> GetTenantByTenantCode(string tenantCode);

        /// <summary>
        /// Check if costco membership support is enabled for the tenant
        /// </summary>
        /// <param name="tenant"></param>
        /// <returns></returns>
        bool CheckCostcoMemberhipSupport(TenantDto? tenant);
    }
}
