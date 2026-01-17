using SunnyRewards.Helios.ETL.Common.Domain.Models;

namespace SunnyRewards.Helios.ETL.Common.Repositories.Interfaces
{
    public interface IAuditTrailRepo : IBaseRepo<AuditTrailModel>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<long?> PostAuditTrail(AuditTrailModel model);
    }
}
