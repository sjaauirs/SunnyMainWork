namespace SunnyRewards.Helios.User.Core.Domain.Dtos
{
    public class GetPersonRolesRequestDto
    {
        public string Email { get; set; } = string.Empty;
        public string PersonCode { get; set; } = string.Empty;
    }
}
