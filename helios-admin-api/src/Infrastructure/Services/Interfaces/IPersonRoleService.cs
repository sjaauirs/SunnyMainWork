using SunnyRewards.Helios.User.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces
{
    public interface IPersonRoleService
    {
        /// <summary>
        /// Retrieves list of person roles
        /// </summary>
        /// <param name="getPersonRolesRequestDto">request contains email and personCode</param>
        /// <returns>ListPersonRoles and base responses with errorCodes</returns>
        Task<GetPersonRolesResponseDto> GetPersonRoles(GetPersonRolesRequestDto getPersonRolesRequestDto);

        /// <summary>
        /// Fetch the access control list for the specified auth0Token.
        /// </summary>
        /// <param name="auth0Token"></param>
        /// <returns></returns>
        Task<AccessControlListResponseDTO> GetAccessControlList(string auth0Token);
    }
}
