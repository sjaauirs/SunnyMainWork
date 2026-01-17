namespace SunnyRewards.Helios.Admin.Core.Domain.Dtos
{
    public class SubtaskUpdateRequestDto : BaseRequestDto
    {
        public long? CompleteConsumerTaskId { get; set; }
        public long TaskId { get; set; }
        public string? TaskStatus { get; set; }
        public string? TaskCode { get; set; }
        public string? PartnerCode { get; set; }
        public string? MemId { get; set; }
        public string? TaskName { get; set; }
    }
}
