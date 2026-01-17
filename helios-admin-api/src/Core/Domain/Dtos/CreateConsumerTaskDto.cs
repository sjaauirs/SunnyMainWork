using System.ComponentModel;

namespace SunnyRewards.Helios.Admin.Core.Domain.Dtos
{
    public class CreateConsumerTaskDto : BaseRequestDto
    {
        public string? TenantCode { get; set; }
        public long TaskId { get; set; }
        public string? TaskStatus { get; set; }
        public bool AutoEnrolled { get; set; }
        [DefaultValue(false)]
        public bool SkipValidation { get; set; } = false;
    }
}
