namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class GetTaskRewardRequestDto
    {
        public GetTaskRewardRequestDto()
        {
           TaskRewardCodes = new List<string>();
        }
        public List<string> TaskRewardCodes { get; set; }
        public string? LanguageCode { get; set; }
    }
}
