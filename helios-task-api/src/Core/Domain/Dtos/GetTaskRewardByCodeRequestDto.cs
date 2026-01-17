namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class GetTaskRewardByCodeRequestDto
    {
        public string? TaskRewardCode { get; set; }
        public string? LanguageCode { get; set; }
    }
}
