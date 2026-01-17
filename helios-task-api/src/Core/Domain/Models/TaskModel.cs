using SunnyRewards.Helios.Common.Core.Domain.Models;

namespace SunnyRewards.Helios.Task.Core.Domain.Models
{
    public class TaskModel : BaseModel
    {
        public virtual long TaskId { get; set; }
        public virtual long TaskTypeId { get; set; }
        public virtual string? TaskCode { get; set; }
        public virtual string? TaskName { get; set; }
        public virtual bool SelfReport { get; set; }
        public virtual bool ConfirmReport { get; set; }
        public virtual long? TaskCategoryId { get; set; }
        public virtual bool IsSubtask { get; set; }
    }
}