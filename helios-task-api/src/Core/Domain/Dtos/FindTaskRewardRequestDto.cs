namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class FindTaskRewardRequestDto
    {
        public string? TenantCode { get; set; }
        public string? ConsumerCode { get; set; }
        public string? LanguageCode { get; set; }
    }
}
