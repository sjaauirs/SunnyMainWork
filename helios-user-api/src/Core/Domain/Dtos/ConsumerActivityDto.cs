namespace SunnyRewards.Helios.User.Core.Domain.Dtos
{
    public class ConsumerActivityDto
    {
        public long ConsumerActivityId { get; set; }
        public string? ConsumerActivityCode { get; set; }
        public string? TenantCode { get; set; }
        public string? ConsumerCode { get; set; }
        public string? ActivitySource { get; set; }
        public string? ActivityType { get; set; }
        public string? ActivityDetailJson { get; set; }
    }
}
