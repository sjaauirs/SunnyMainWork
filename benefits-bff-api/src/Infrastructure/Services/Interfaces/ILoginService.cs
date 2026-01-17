using Sunny.Benefits.Bff.Core.Domain.Dtos;
using SunnyBenefits.Fis.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Dtos;

namespace Sunny.Benefits.Bff.Infrastructure.Services.Interfaces
{
    public interface ILoginService
    {
        /// <summary>
        /// //
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        Task<GetConsumerByEmailResponseDto> GetConsumerByEmail(string? email);
        Task<VerifyMemberResponseDto> VerifyMember(VerifyMemberDto verifyMemberDto);

        Task<LoginResponseDto> InternalLogin(LoginRequestDto loginRequestDto);

        Task<GetConsumerByEmailResponseDto> GetPersonAndConsumerDetails(string consumerCode);

        Task<GetConsumerByEmailResponseDto> GetConsumerByPersonUniqueIdentifier(string? email);
    }
}
