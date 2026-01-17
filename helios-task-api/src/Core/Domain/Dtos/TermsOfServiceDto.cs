using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class TermsOfServiceDto
    {
        public long TermsOfServiceId { get; set; }
        public string? TermsOfServiceText { get; set; }
        public string TermsOfServiceCode { get; set; } = null!;
        public string? LanguageCode { get; set; }
    }
}
