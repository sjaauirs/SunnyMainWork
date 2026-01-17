using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Sweepstakes.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces
{
    public interface ISweepstakesService
    {
        Task<BaseResponseDto> CreateSweepstakes(SweepstakesRequestDto sweepstakesRequestDto);

        /// <summary>
        /// This method is used to update sweepsatkes
        /// </summary>
        /// <param name="updateSweepstakesRequestDto"></param>
        /// <returns></returns>
        Task<UpdateSweepstakesResponseDto> UpdateSweepStakes(UpdateSweepstakesRequestDto updateSweepstakesRequestDto);
    }
}
