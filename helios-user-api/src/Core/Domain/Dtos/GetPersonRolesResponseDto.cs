using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.User.Core.Domain.Dtos
{
    public class GetPersonRolesResponseDto : BaseResponseDto
    {
        public IList<PersonRoleDto> PersonRoles { get; set; } = new List<PersonRoleDto>();
    }
}
