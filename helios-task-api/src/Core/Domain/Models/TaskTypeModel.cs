using SunnyRewards.Helios.Common.Core.Domain.Models;

namespace SunnyRewards.Helios.Task.Core.Domain.Models
{
    public class TaskTypeModel : BaseModel
    {
        public virtual long  TaskTypeId { get; set; }
        public virtual string?  TaskTypeCode { get; set; }
        public virtual string? TaskTypeName { get; set; }
        public virtual string? TaskTypeDescription { get; set; }  
        public virtual bool IsSubtask { get; set; }
    }
}
