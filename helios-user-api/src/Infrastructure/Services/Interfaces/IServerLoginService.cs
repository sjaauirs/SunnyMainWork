using SunnyRewards.Helios.User.Core.Domain.Dtos;

namespace SunnyRewards.Helios.User.Infrastructure.Services.Interfaces
{
    public interface IServerLoginService
    {
        /// <summary>
        /// Creates an API token for the specified server login request.
        /// </summary>
        /// <param name="serverLoginRequestDto"></param>
        /// <returns></returns>
        Task<ServerLoginResponseDto> CreateApiToken(ServerLoginRequestDto serverLoginRequestDto);
    }
}
