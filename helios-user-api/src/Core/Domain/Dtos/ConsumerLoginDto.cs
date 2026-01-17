using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.User.Core.Domain.Dtos
{
    public class ConsumerLoginDto : BaseDto
    {
        public long ConsumerLoginId { get; set; }
        public long ConsumerId { get; set; }
        public DateTime? LoginTs { get; set; }
        public DateTime? RefreshTokenTs { get; set; }
        public DateTime? LogoutTs { get; set; }
        public string? AccessToken { get; set; }
    }
}