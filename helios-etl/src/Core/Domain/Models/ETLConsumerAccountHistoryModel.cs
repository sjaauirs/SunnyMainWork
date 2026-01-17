using SunnyRewards.Helios.ETL.Common.Domain.Models;

namespace SunnyRewards.Helios.ETL.Core.Domain.Models
{
    public class ETLConsumerAccountHistoryModel : BaseModel
    {
        public virtual long ConsumerAccountHistoryId { get; set; }
        public virtual string? ConsumerAccountCode { get; set; }
        public virtual string? TenantCode { get; set; }
        public virtual string? ConsumerCode { get; set; }
        public virtual string? ProxyNumber { get; set; }
        public virtual DateTime ProxyUpdateTs { get; set; }
        public virtual string? CardLast4 { get; set; }
        public virtual bool SyncRequired { get; set; }
        public virtual string? SyncInfoJson { get; set; }
        public virtual string? ConsumerAccountConfigJson { get; set; }
        public virtual string? ClientUniqueId { get; set; }
        public virtual string? CardIssueStatus { get; set; }
        public virtual string? CardRequestStatus { get; set; }
    }
}
