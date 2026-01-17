namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class ExportTaskRewardCollectionDto : TaskRewardCollectionDto
    {
        public string ParentTaskRewardCode { get; set; } = string.Empty;
        public string ChildTaskRewardCode { get; set; } = string.Empty;
    }
}
