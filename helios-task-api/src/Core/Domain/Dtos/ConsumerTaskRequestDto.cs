
using System.ComponentModel;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class ConsumerTaskRequestDto
    {
        public string? TenantCode { get; set; }
        public string? ConsumerCode { get; set; }

        [DefaultValue(true)]
        public bool FilterTaskReward { get; set; } = true;
        public string? LanguageCode { get; set; }
    }
}
