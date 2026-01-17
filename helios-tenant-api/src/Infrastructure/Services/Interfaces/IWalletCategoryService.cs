using SunnyRewards.Helios.Tenant.Core.Domain.Models;

namespace SunnyRewards.Helios.Tenant.Infrastructure.Services.Interfaces
{
    public interface IWalletCategoryService
{
    Task<IEnumerable<WalletCategoryModel>> GetByTenantCodeAsync(string tenantCode);
    Task<WalletCategoryModel?> GetByIdAsync(int id);
    Task<WalletCategoryModel?> GetByTenantAndWalletAsync(string tenantCode, int walletId);
}

}
