using SunnyRewards.Helios.User.Core.Domain.Dtos;

namespace SunnyRewards.Helios.User.Infrastructure.Services.Interfaces
{
    public interface IZDService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="zdTokenRequestDto"></param>
        /// <returns></returns>

        Task<ZdTokenResponseDto> CreateZdToken(ZdTokenRequestDto zdTokenRequestDto);
    }
}
