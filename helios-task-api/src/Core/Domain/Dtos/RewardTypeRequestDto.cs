namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class RewardTypeRequestDto
    {
        public long TaskId { get; set; }
        public string? TenantCode { get; set; }
        public string? TaskCode { get; set; }
    }
}
