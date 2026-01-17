using SunnyRewards.Helios.User.Core.Domain.Dtos;

namespace SunnyRewards.Helios.User.Infrastructure.Services.Interfaces
{
    public interface IConsumerLoginService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="consumerLoginRequestDto"></param>
        /// <returns></returns>
        Task<ConsumerLoginResponseDto> CreateToken(ConsumerLoginRequestDto consumerLoginRequestDto);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="refreshTokenRequestDto"></param>
        /// <returns></returns>
        Task<RefreshTokenResponseDto> RefreshToken(RefreshTokenRequestDto refreshTokenRequestDto);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="validateTokenRequestDto"></param>
        /// <returns></returns>
        Task<ValidateTokenResponseDto> ValidateToken(ValidateTokenRequestDto validateTokenRequestDto);
        Task<ConsumerLoginDateResponseDto> GetConsumerLoginDetail(string consumerCode);
        Task<GetConsumerEngagementDetailResponseDto> GetConsumerEngagementDetail(GetConsumerEngagementDetailRequestDto consumerEngagementDetailRequestDto);

        /// <summary>
        /// ValidateAndExtractClaims
        /// </summary>
        /// <param name="jwtToken"></param>
        /// <param name="jwtValidationKey"></param>
        /// <param name="jwtIssuer"></param>
        /// <param name="tokenClaims"></param>
        /// <returns></returns>
        bool ValidateAndExtractClaims(string jwtToken, string jwtValidationKey, string jwtIssuer, out Dictionary<string, string> tokenClaims);
    }
}