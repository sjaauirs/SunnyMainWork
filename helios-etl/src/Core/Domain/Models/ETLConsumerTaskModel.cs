using Newtonsoft.Json;
using SunnyRewards.Helios.ETL.Common.Domain.Models;

namespace SunnyRewards.Helios.ETL.Core.Domain.Models
{
    public class ETLConsumerTaskModel : BaseModel
    {
        public virtual string? ConsumerCode { get; set; }
        public virtual string? TenantCode { get; set; }
        public virtual long TaskId { get; set; }
        public virtual long ConsumerTaskId { get; set; }
        public virtual string TaskStatus { get; set; }
        public virtual int Progress { get; set; }
        public virtual string? Notes { get; set; }
        public virtual DateTime TaskStartTs { get; set; }
        public virtual DateTime TaskCompleteTs { get; set; }
        public virtual string? ProgressDetail { get; set; }
        public virtual int? ParentConsumerTaskId { get; set; }
    }
}
