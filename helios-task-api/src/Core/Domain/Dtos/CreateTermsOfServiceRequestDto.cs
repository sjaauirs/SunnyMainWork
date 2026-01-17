namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class CreateTermsOfServiceRequestDto
    {
        public long TermsOfServiceId { get; set; }
        public string? TermsOfServiceText { get; set; }
        public string TermsOfServiceCode { get; set; } = null!;
        public string? LanguageCode { get; set; }
        public string? CreateUser { get; set; }
    }
}
