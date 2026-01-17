using SunnyRewards.Helios.ETL.Common.Domain.Models;

namespace SunnyRewards.Helios.ETL.Core.Domain.Models
{
    public class HealthMetricTypeModel : BaseModel
    {
        public virtual long HealthMetricTypeId { get; set; }
        public virtual string? HealthMetricTypeCode { get; set; }
        public virtual string? HealthMetricTypeName { get; set; }
        public virtual string? SchemaJson { get; set; }
    }
}
