using SunnyRewards.Helios.Common.Core.Domain.Models;

namespace SunnyRewards.Helios.User.Core.Domain.Models
{
    public class ConsumerOnboardingProgressHistoryModel : BaseModel
    {
        public virtual long Pk { get; set; }
        public virtual long ConsumerFlowProgressFk { get; set; }
        public virtual string ConsumerCode { get; set; } = string.Empty;
        public virtual string TenantCode { get; set; } = string.Empty;
        public virtual string? CohortCode { get; set; }
        public virtual long FlowFk { get; set; }
        public virtual int VersionNbr { get; set; }
        public virtual long? FromFlowStepPk { get; set; }
        public virtual long? ToFlowStepPk { get; set; }
        public virtual string Outcome { get; set; } = string.Empty;
    }
}