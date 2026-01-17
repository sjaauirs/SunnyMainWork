using SunnyRewards.Helios.Common.Core.Domain.Models;

namespace SunnyRewards.Helios.User.Core.Domain.Models
{
    public class ConsumerLoginModel : BaseModel
    {
        public virtual long ConsumerLoginId { get; set; }
        public virtual long ConsumerId { get; set; }
        public virtual DateTime? LoginTs { get; set; }
        public virtual DateTime? RefreshTokenTs { get; set; }
        public virtual DateTime? LogoutTs { get; set; }
        public virtual string? AccessToken { get; set; }
        public virtual string? UserAgent { get; set; }
        public virtual string? TokenApp { get; set; }
    }
}