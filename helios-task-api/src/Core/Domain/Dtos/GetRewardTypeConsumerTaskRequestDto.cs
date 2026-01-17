namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class GetRewardTypeConsumerTaskRequestDto
    {
        public string TenantCode { get; set; }
        public string ConsumerCode { get; set; }
        public string RewardTypeCode { get; set; }
        public string? LanguageCode { get; set; }
    }
}
