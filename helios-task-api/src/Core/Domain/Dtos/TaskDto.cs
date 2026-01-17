using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class TaskDto
    {
        public long TaskId { get; set; }
        public long TaskTypeId { get; set; }
        public string? TaskCode { get; set; }
        public string? TaskName { get; set; }
        public bool SelfReport { get; set; }
        public bool ConfirmReport { get; set; }
        public long? TaskCategoryId { get; set; }
        public bool IsSubtask { get; set; }
    }
    public class ExportTaskDto
    {
        public string? TaskTypeCode { get; set; }
        public string? TaskCategoryCode { get; set; }
        public TaskDto? Task { get; set; }
    }
}
