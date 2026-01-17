using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.User.Core.Domain.Dtos
{
    public class ConsumerFlowProgressDto : BaseDto
    {
        public long Pk { get; set; }
        public string ConsumerCode { get; set; } = string.Empty;
        public string TenantCode { get; set; } = string.Empty;
        public string? CohortCode { get; set; }
        public long FlowFk { get; set; }
        public int VersionNbr { get; set; }
        public long FlowStepPk { get; set; }
        public string? Status { get; set; }
        public string? ContextJson { get; set; }
    }
}