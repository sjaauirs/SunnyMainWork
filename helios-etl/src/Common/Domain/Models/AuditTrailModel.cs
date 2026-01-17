using SunnyRewards.Helios.ETL.Common.Domain.Models;

namespace SunnyRewards.Helios.ETL.Common.Domain.Models
{
    public class AuditTrailModel : BaseModel
    {
        public virtual long AuditTrailId { get; set; }
        public virtual string? SourceModule { get; set; }
        public virtual string? SourceContext { get; set; }
        public virtual string? AuditName { get; set; }
        public virtual string? AuditMessage { get; set; }
        public virtual string? AuditData { get; set; }
        public virtual string? AuditJsonData { get; set; }
    }
}
