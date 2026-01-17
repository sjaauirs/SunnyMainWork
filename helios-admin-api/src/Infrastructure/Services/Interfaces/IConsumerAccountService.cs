using SunnyBenefits.Fis.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces
{
    public interface IConsumerAccountService
    {
        Task<ConsumerAccountDto> CreateConsumerAccount(CreateConsumerAccountRequestDto requestDto);
        Task<GetConsumerAccountResponseDto> GetConsumerAccount(GetConsumerAccountRequestDto requestDto);

        /// <summary>
        /// Updates the consumer account configuration.
        /// </summary>
        /// <param name="consumerAccountUpdateRequest">The consumer account update request.</param>
        /// <returns></returns>
        Task<ConsumerAccountUpdateResponseDto> UpdateConsumerAccountConfig(ConsumerAccountUpdateRequestDto consumerAccountUpdateRequest);
    }
}
