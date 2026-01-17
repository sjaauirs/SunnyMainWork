using SunnyRewards.Helios.ETL.Common.Domain.Models;

namespace SunnyRewards.Helios.ETL.Core.Domain.Models
{
    public class HealthMetricModel : BaseModel
    {
        public virtual long HealthMetricId { get; set; }
        public virtual long HealthMetricTypeId { get; set; }
        public virtual string? TenantCode { get; set; }
        public virtual string? ConsumerCode { get; set; }
        public virtual string? DataJson { get; set; }
        public virtual DateTime CaptureTs { get; set; }
        public virtual DateTime OsMetricTs { get; set; }
    }
}
