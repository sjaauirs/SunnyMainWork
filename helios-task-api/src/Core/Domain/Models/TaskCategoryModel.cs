using SunnyRewards.Helios.Common.Core.Domain.Models;

namespace SunnyRewards.Helios.Task.Core.Domain.Models
{
    public class TaskCategoryModel : BaseModel
    {
        public virtual long TaskCategoryId { get; set; }
        public virtual string? TaskCategoryCode { get; set; }
        public virtual string? TaskCategoryDescription { get; set; } 
        public virtual string? TaskCategoryName { get; set; }    
    }
}
