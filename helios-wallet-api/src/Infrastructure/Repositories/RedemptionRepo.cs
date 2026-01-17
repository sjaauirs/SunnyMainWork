using Microsoft.Extensions.Logging;
using NHibernate.Linq;
using SunnyRewards.Helios.Common.Core.Repositories;
using SunnyRewards.Helios.Wallet.Core.Domain.Models;
using SunnyRewards.Helios.Wallet.Infrastructure.Repositories.Interfaces;

namespace SunnyRewards.Helios.Wallet.Infrastructure.Repositories
{

    public class RedemptionRepo : BaseRepo<RedemptionModel>, IRedemptionRepo
    {
        private readonly NHibernate.ISession _session;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseLogger"></param>
        /// <param name="session"></param>
        public RedemptionRepo(ILogger<BaseRepo<RedemptionModel>> baseLogger, NHibernate.ISession session) : base(baseLogger, session)
        {
            _session = session;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="timeStamp"></param>
        /// <param name="redemptionId"></param>
        /// <param name="xmin"></param>
        /// <returns></returns>
        public int UpdateRedemption(DateTime timeStamp, long redemptionId, int xmin)
        {
            int rec = _session.Query<RedemptionModel>()
                .Where(x => x.RedemptionId == redemptionId && x.Xmin == xmin)
                .Update(x => new { update_ts = timeStamp });
            return rec;
        }
    }
}
