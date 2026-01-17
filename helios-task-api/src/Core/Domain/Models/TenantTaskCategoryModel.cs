using SunnyRewards.Helios.Common.Core.Domain.Models;

namespace SunnyRewards.Helios.Task.Core.Domain.Models
{
    public class TenantTaskCategoryModel : BaseModel
    {
        public virtual long TenantTaskCategoryId { get; set; }
        public virtual long TaskCategoryId { get; set; }
        public virtual string? TenantCode { get; set; }
        public virtual string? ResourceJson { get; set; }
    }
}
