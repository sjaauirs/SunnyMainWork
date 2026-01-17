namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class TaskRewardDetailsDto
    {
        public TaskDto? Task { get; set; }
        public TaskRewardDto? TaskReward { get; set; }
        public TaskDetailDto? TaskDetail { get; set; }
    }
}
