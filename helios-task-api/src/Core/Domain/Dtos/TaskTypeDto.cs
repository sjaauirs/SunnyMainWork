namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class TaskTypeDto
    {
        public long TaskTypeId { get; set; }
        public string? TaskTypeCode { get; set; }
        public string? TaskTypeName { get; set; }
        public string? TaskTypeDescription { get; set; }
        public bool IsSubtask { get; set; }
    }
}

