using SunnyRewards.Helios.User.Core.Domain.Dtos;

namespace SunnyRewards.Helios.User.Infrastructure.Services.Interfaces
{
    public interface IPersonRoleService
    {
        Task<GetPersonRolesResponseDto> GetPersonRoles(GetPersonRolesRequestDto getPersonRolesRequestDto);

        /// <summary>
        /// Fetch the access control list for the specified consumer code.
        /// </summary>
        /// <param name="consumerCode"></param>
        /// <returns></returns>
        Task<AccessControlListResponseDTO> GetAccessControlList(string consumerCode);
    }
}
