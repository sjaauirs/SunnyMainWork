using System.Collections.Generic;
using System.Threading.Tasks;
using SunnyRewards.Helios.Common.Core.Repositories.Interfaces;
using SunnyRewards.Helios.Tenant.Core.Domain.Models;


namespace SunnyRewards.Helios.Tenant.Infrastructure.Repositories.Interfaces
{
   public interface IWalletCategoryRepo : IBaseRepo<WalletCategoryModel>
{
    Task<IEnumerable<WalletCategoryModel>> GetByTenantCodeAsync(string tenantCode);
    Task<WalletCategoryModel?> GetByTenantAndWalletAsync(string tenantCode, int walletId);
}

}
