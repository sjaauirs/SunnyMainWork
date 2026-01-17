using Microsoft.Extensions.Logging;
using NHibernate.Linq;
using SunnyRewards.Helios.ETL.Common.Repositories;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces;

namespace SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo
{
    public class RedemptionRepo : BaseRepo<ETLRedemptionModel>, IRedemptionRepo
    {
        private readonly NHibernate.ISession _session;
        public RedemptionRepo(ILogger<BaseRepo<ETLRedemptionModel>> baseLogger, NHibernate.ISession session) :
           base(baseLogger, session)
        {
            _session = session;
        }

        public ETLRedemptionModel? GetRedemptionWithRedemptionRef(string redemptionRef)
        {
            var query = _session.Query<ETLRedemptionModel>()
                .Where(x => x.RedemptionRef == redemptionRef && x.DeleteNbr == 0);
            return query.FirstOrDefault();
        }

        public int UpdateRedemptionStatus(DateTime transactionTs, string redemptionStatus, string redemptionRef)
        {
            int rec = _session.Query<ETLRedemptionModel>()
                        .Where(x => x.RedemptionRef == redemptionRef && x.DeleteNbr == 0)
                        .Update(x => new { redemption_status = redemptionStatus, update_ts = transactionTs });
            return rec;
        }

    }
}
