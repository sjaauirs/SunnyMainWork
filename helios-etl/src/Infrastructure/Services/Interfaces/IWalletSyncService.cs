using SunnyBenefits.Fis.Core.Domain.Dtos;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces
{
    public interface IWalletSyncService
    {
        /// <summary>
        /// Synchronizes the wallets for tenant asynchronous.
        /// </summary>
        /// <param name="tenantDto">The tenant dto.</param>
        /// <param name="tenantAccountRequestDto">The tenant account request dto.</param>
        /// <param name="consumerCodes">The consumer codes.</param>
        /// <returns></returns>
        Task SyncWalletsForTenantAsync(TenantDto? tenantDto, TenantAccountRequestDto? tenantAccountRequestDto, string consumerCodes);
    }

}
