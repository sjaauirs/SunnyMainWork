namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class FindConsumerTaskRequestDto
    {
        public string? ConsumerCode { get; set; }
        public string? TaskStatus { get; set; } // can be either “PENDING” or “COMPLETED” 
        public string? LanguageCode { get; set; }
    }
}
