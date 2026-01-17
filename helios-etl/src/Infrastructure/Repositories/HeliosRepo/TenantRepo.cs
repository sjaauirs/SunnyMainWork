using SunnyRewards.Helios.ETL.Common.Repositories;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces;
using Microsoft.Extensions.Logging;
using System.Data;

namespace SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo
{
    public class TenantRepo : BaseRepo<ETLTenantModel>, ITenantRepo
    {
        private readonly NHibernate.ISession _session;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseLogger"></param>
        /// <param name="session"></param>
        public TenantRepo(ILogger<BaseRepo<ETLTenantModel>> baseLogger, NHibernate.ISession session) : base(baseLogger, session)
        {
            _session = session;
        }

        public async Task<(string customerCode, string sponsorCode)> GetCustomerAndSponsorCode(string tenantCode)
        {
            var tenant = (from t in _session.Query<ETLTenantModel>()
                          join sp in _session.Query<ETLSponsorModel>() on t.SponsorId equals sp.SponsorId
                          join cu in _session.Query<CustomerModel>() on sp.CustomerId equals cu.CustomerId
                          where t.TenantCode == tenantCode && t.DeleteNbr == 0
                          select new { cu.CustomerCode, sp.SponsorCode }).FirstOrDefault();

            return await Task.FromResult((tenant?.CustomerCode, tenant?.SponsorCode));
        }

        public async Task<long> GetSubscriberRoleId()
        {
            var subscriberRoleId = _session.Query<RoleModel>()
                .Where(r => r.RoleName == "subscriber" && r.DeleteNbr == 0)
                .Select(r => r.RoleId)
                .FirstOrDefault();

            return await Task.FromResult(subscriberRoleId);
        }

        public async Task<long> GetConsumerWalletTypeId()
        {
            var consumerWalletTypeId = _session.Query<ETLWalletTypeModel>()
                .Where(wt => wt.WalletTypeName == "Health Actions Reward" && wt.DeleteNbr == 0)
                .Select(wt => wt.WalletTypeId)
                .FirstOrDefault();

            return await Task.FromResult(consumerWalletTypeId);
        }
    }
}