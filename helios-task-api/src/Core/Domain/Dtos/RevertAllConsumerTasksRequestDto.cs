namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class RevertAllConsumerTasksRequestDto
    {
        public string? TenantCode { get; set; }
        public string? ConsumerCode { get; set; }
    }
}
