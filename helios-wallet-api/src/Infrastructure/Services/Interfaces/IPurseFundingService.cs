using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Wallet.Infrastructure.Services.Interfaces
{
    public interface IPurseFundingService
    {
        /// <summary>
        /// Purses the funding asynchronous.
        /// </summary>
        /// <param name="purseFundingRequestDto">The purse funding request dto.</param>
        /// <returns></returns>
        Task<PurseFundingResponseDto> PurseFundingAsync(PurseFundingRequestDto purseFundingRequestDto);
    }
}
