using Microsoft.AspNetCore.Http;
using Sunny.Benefits.Bff.Core.Domain.Dtos;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Dtos;

namespace Sunny.Benefits.Bff.Infrastructure.Helpers.Interface
{
    public interface IAuth0Helper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="patchUserRequestDto"></param>
        /// <returns></returns>
        Task<UpdateResponseDto> PatchUserOuter(PatchUserRequestDto patchUserRequestDto);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="patchUserRequestDto"></param>
        /// <returns></returns>
        Task<UpdateResponseDto> PatchUser(PatchUserRequestDto patchUserRequestDto);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        Task<(bool emailVerified, string email)> Validatetoken(string accessToken);

        /// <summary>
        /// Post Verification Email
        /// </summary>
        /// <param name="emailRequestDto"></param>
        /// <returns></returns>
        Task<UpdateResponseDto> PostVerificationEmail(VerificationEmailRequestDto emailRequestDto);

        /// <summary>
        /// Get User by UserId
        /// </summary>
        /// <param name="userRequestDto"></param>
        /// <returns></returns>
        Task<UserGetResponseDto> GetUserById(GetUserRequestDto userRequestDto);

        Task<GetConsumerByPersonUniqueIdentifierResponseDto?> GetConsumerByPersonUniqueIdentifier(string personUniqueIdentifier);

        Task<GetConsumerByEmailResponseDto> GetConsumerByIdentifierOrEmail(string? email);

        Task<GetConsumerByPersonUniqueIdentifierResponseDto?> GetConsumerDetails();

        Task<TenantDto> GetTenantByTenantCode(string tenantCode);

        Task<bool> SetAuthConfigToContext(HttpContext context);
    }
}
