using SunnyRewards.Helios.Common.Core.Domain.Models;

namespace SunnyRewards.Helios.User.Core.Domain.Models
{
    public class ServerLoginModel : BaseModel
    {
        public virtual long ServerLoginId { get; set; }
        public virtual string? TenantCode { get; set; }
        public virtual DateTime LoginTs { get; set; }
        public virtual DateTime RefreshTokenTs { get; set; }
        public virtual DateTime? LogoutTs { get; set; }
        public virtual string? ApiToken { get; set; }
    }
}
