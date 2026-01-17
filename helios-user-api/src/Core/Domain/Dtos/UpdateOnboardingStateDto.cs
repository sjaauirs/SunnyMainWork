using SunnyRewards.Helios.User.Core.Domain.enums;
using System.ComponentModel.DataAnnotations;

namespace SunnyRewards.Helios.User.Core.Domain.Dtos
{
    public class UpdateOnboardingStateDto
    {
        [Required]
        public OnboardingState OnboardingState { get; set; }
        [Required]
        public string TenantCode { get; set; } = null!;
        [Required]
        public string? ConsumerCode { get; set; } = null;
        public Dictionary<string,string>? HtmlFileName { get; set; } = null;
        public string? LanguageCode { get; set; } = null;
    }
}
