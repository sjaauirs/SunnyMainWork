using SunnyRewards.Helios.Admin.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces
{
    public interface IOnBoardingInitialFundingService
    {
        /// <summary>
        /// Processes the initial funding asynchronous.
        /// </summary>
        /// <param name="initialFundingRequest">The initial funding request.</param>
        /// <returns></returns>
        InitialFundingResponseDto ProcessInitialFundingAsync(InitialFundingRequestDto initialFundingRequest);
    }
}
