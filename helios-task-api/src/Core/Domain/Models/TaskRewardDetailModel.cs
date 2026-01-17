namespace SunnyRewards.Helios.Task.Core.Domain.Models
{
    public class TaskRewardDetailModel
    {
        public TaskModel? Task { get; set; }
        public TaskRewardModel? TaskReward { get; set; }
        public TaskDetailModel? TaskDetail { get; set; }
    }
}
