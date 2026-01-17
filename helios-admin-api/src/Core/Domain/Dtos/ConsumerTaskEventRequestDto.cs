namespace SunnyRewards.Helios.Admin.Core.Domain.Dtos
{
    public class ConsumerTaskEventRequestDto
    {
        public string TenantCode { get; set; } = null!;
        public string ConsumerCode { get; set; } = null!;
        public string? TaskExternalCode { get; set; }
    }
}
