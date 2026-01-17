using Microsoft.Extensions.Logging;
using NHibernate;
using NHibernate.Linq;
using SunnyRewards.Helios.Common.Core.Repositories;
using SunnyRewards.Helios.Tenant.Core.Domain.Models;
using SunnyRewards.Helios.Tenant.Infrastructure.Repositories.Interfaces;

namespace SunnyRewards.Helios.Tenant.Infrastructure.Repositories
{public class WalletCategoryRepo : BaseRepo<WalletCategoryModel>, IWalletCategoryRepo
{
    private readonly ISession _session;

    public WalletCategoryRepo(ILogger<BaseRepo<WalletCategoryModel>> logger, ISession session)
        : base(logger, session)
    {
        _session = session;
    }

    public async Task<IEnumerable<WalletCategoryModel>> GetByTenantCodeAsync(string tenantCode)
    {
        var query = _session.Query<WalletCategoryModel>()
            .Where(x => x.TenantCode == tenantCode && x.DeleteNbr == 0);

        // NHibernate's async LINQ requires an INhQueryProvider. In unit tests we often return
        // an in-memory IQueryable (LINQ to Objects) which doesn't support ToListAsync/FirstOrDefaultAsync.
        // Detect provider and fall back to synchronous execution when necessary.
        if (query.Provider is NHibernate.Linq.INhQueryProvider)
        {
            return await query.ToListAsync();
        }

        return query.ToList();
    }

    public async Task<WalletCategoryModel?> GetByTenantAndWalletAsync(string tenantCode, int walletId)
    {
        var query = _session.Query<WalletCategoryModel>();

        // Use provider-aware async when possible, otherwise fallback to synchronous execution for tests
        if (query.Provider is NHibernate.Linq.INhQueryProvider)
        {
            return await query.FirstOrDefaultAsync(x => x.TenantCode == tenantCode && x.WalletTypeId == walletId && x.DeleteNbr == 0);
        }

        return query.FirstOrDefault(x => x.TenantCode == tenantCode && x.WalletTypeId == walletId && x.DeleteNbr == 0);
    }
}}