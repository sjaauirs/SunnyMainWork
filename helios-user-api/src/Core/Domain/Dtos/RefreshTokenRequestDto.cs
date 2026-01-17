namespace SunnyRewards.Helios.User.Core.Domain.Dtos
{
    public class RefreshTokenRequestDto
    {
        public string? ConsumerCode { get; set; }
        public string? AccessToken { get; set; }
    }
}