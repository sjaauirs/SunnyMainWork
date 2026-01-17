namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class TaskRewardDetailDto
    {
        public TaskRewardDetailDto()
        {
            Task = new TaskDto();
        }
        public TaskDto Task { get; set; }
        public TaskRewardDto? TaskReward { get; set; }
        public TaskDetailDto? TaskDetail { get; set; }
        public TermsOfServiceDto? TermsOfService { get; set; }
        public TenantTaskCategoryDto? TenantTaskCategory { get; set; }
        public TaskTypeDto? TaskType { get; set; }
        public string? RewardTypeName { get; set; }
        public ConsumerTaskStatTSDto? ConsumerTask { get; set; }

        public DateTime? MinAllowedTaskCompleteTs { get; set; }

        public DateTime? ComputedTaskExpiryTs { get; set; }
        public ConsumerTaskDto? ConsumerTaskDto { get; set; }
    }
}
