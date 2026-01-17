using SunnyBenefits.Fis.Core.Domain.Dtos;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces
{
    public interface IConsumerAccountSyncService
    {
        /// <summary>
        /// Sync the consumer account config with tenant_account config
        /// </summary>
        /// <param name="tenantDto">The tenant dto.</param>
        /// <param name="tenantAccountRequestDto">The tenant account request dto.</param>
        /// <param name="consumerCode">The consumer code.</param>
        /// <returns></returns>
        Task SyncConsumerAccountAsync(TenantDto? tenantDto, TenantAccountRequestDto? tenantAccountRequestDto, string consumerCode);
    }
}
