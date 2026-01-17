using SunnyRewards.Helios.Common.Core.Domain.Models;

namespace SunnyRewards.Helios.Tenant.Core.Domain.Models
{
    public class CategoryModel : BaseModel
    {
        public override int Id { get; set; }
        public virtual string Name { get; set; } = string.Empty;
        public virtual string? GoogleType { get; set; }
        public virtual bool IsActive { get; set; }
    }
}
