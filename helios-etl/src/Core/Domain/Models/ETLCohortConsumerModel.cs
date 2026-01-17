using SunnyRewards.Helios.ETL.Common.Domain.Models;

namespace SunnyRewards.Helios.ETL.Core.Domain.Models
{
    public class ETLCohortConsumerModel : BaseModel
    {
        public virtual long CohortConsumerId { get; set; }
        public virtual long CohortId { get; set; }
        public virtual string? TenantCode { get; set; }
        public virtual string? ConsumerCode { get; set; }
        public virtual string CohortDetectDescription { get; set; }
    }
}
