using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using System.Text.Json.Serialization;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class TaskDetailDto
    {
        public long TaskId { get; set; }
        public long TaskDetailId { get; set; }
        public long TermsOfServiceId { get; set; }
        public string? TaskHeader { get; set; }
        public string? TaskDescription { get; set; }
        public string? LanguageCode { get; set; }
        public string? TenantCode { get; set; }
        public string? TaskCtaButtonText { get; set; }

        [JsonIgnore]
        public DateTime UpdateTs { get; set; }
    }
}
