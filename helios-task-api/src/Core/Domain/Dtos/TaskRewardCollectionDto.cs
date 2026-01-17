namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class TaskRewardCollectionDto
    {
        public long TaskRewardCollectionId { get; set; }
        public long ParentTaskRewardId { get; set; }
        public long ChildTaskRewardId { get; set; }
        public string UniqueChildCode { get; set; } = string.Empty;
        public string ConfigJson { get; set; } = string.Empty;
    }
}
