using Sunny.Benefits.Bff.Core.Domain.Dtos;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;

namespace Sunny.Benefits.Bff.Infrastructure.Services.Interfaces
{
    public interface IWalletCategoryService
    {
        /// <summary>
        /// Get all wallet categories for a tenant.
        /// </summary>
        Task<IEnumerable<WalletCategoryResponseDto>> GetByTenant(string tenantCode);

        /// <summary>
        /// Get a wallet category by its ID.
        /// </summary>
        Task<WalletCategoryResponseDto?> GetById(long id);

        /// <summary>
        /// Get a wallet category by tenant code and wallet type ID.
        /// </summary>
        Task<IEnumerable<WalletCategoryResponseDto>> GetByTenantAndWallet(string tenantCode, long walletTypeId);
    }
}
