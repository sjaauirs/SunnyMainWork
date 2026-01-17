namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class TriviaDataDto
    {
        public long TriviaId { get; set; }
        public string? TriviaCode { get; set; }
        public long TaskRewardId { get; set; }
        public string? CtaTaskExternalCode { get; set; }
        public string? ConfigJson { get; set; }
    }
}
