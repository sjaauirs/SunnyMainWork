using SunnyRewards.Helios.Common.Core.Domain.Models;

namespace SunnyRewards.Helios.User.Core.Domain.Models
{
    public class RoleModel : BaseModel
    {
        public virtual long RoleId { get; set; }
        public virtual string? RoleCode { get; set; }
        public virtual string? RoleName { get; set; }
        public virtual string? RoleDescription { get; set; }
    }
}