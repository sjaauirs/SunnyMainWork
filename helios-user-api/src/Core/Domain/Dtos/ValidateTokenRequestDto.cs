namespace SunnyRewards.Helios.User.Core.Domain.Dtos
{
    public class ValidateTokenRequestDto
    {
        public ValidateTokenRequestDto()
        {
            AccessToken = string.Empty;
        }
        public string AccessToken { get; set; }
    }
}