using SunnyRewards.Helios.ETL.Common.Domain.Models;

namespace SunnyRewards.Helios.ETL.Core.Domain.Models
{
    public class ETLCohortTenantTaskRewardModel : BaseModel
    {
        public virtual long CohortTenantTaskRewardId { get; set; }
        public virtual long CohortId { get; set; }
        public virtual string? TenantCode { get; set; }
        public virtual string? TaskRewardCode { get; set; }
        public virtual bool Recommended { get; set; }
        public virtual int Priority { get; set; }
    }
}
