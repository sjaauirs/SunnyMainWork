using SunnyRewards.Helios.User.Core.Domain.enums;
using System.ComponentModel.DataAnnotations;

namespace SunnyRewards.Helios.User.Core.Domain.Dtos
{
    public class UpdateFlowStatusRequestDto
    {
        [Required]
        public string ConsumerCode { get; set; } = null!;
        [Required]
        public string TenantCode { get; set; } = null!;
        public string? CohortCode { get; set; } = null;
        [Required]
        public long FlowId { get; set; }
        [Required]
        public long FromFlowStepId { get; set; }
        [Required]
        public string Status { get; set; }
        [Required]
        public int VersionId { get; set; }
        public long? ToFlowStepId { get; set; } = null;
    }
}
