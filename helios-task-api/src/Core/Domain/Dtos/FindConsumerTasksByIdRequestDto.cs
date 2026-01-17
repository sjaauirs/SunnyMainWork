namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class FindConsumerTasksByIdRequestDto
    {
        public string? ConsumerCode { get; set; }
        public long TaskId { get; set; }
        public string? TaskStatus { get; set; }
        public string? TaskCode { get; set; }

        public string? TaskExternalCode { get; set; }

        public string? TenantCode { get; set; }
        public string? LanguageCode { get; set; }
    }
}