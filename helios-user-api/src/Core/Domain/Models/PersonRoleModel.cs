using SunnyRewards.Helios.Common.Core.Domain.Models;

namespace SunnyRewards.Helios.User.Core.Domain.Models
{
    public class PersonRoleModel : BaseModel
    {
        public virtual long PersonRoleId { get; set; }
        public virtual long PersonId { get; set; }
        public virtual long RoleId { get; set; }
        public virtual string? CustomerCode { get; set; }
        public virtual string? SponsorCode { get; set;}
        public virtual string? TenantCode { get; set; }

    }
}