using SunnyBenefits.Fis.Core.Domain.Dtos;

namespace Sunny.Benefits.Bff.Infrastructure.Services.Interfaces
{
    public interface IConsumerAccountService
    {
        Task<ConsumerAccountUpdateResponseDto> UpdateConsumerAccountConfig(ConsumerAccountUpdateRequestDto consumerAccountUpdateRequest);
        Task<ConsumerAccountResponseDto> UpdateConsumerAccountCardIssue(UpdateCardIssueRequestDto updateCardIssueRequestDto);
        Task<GetConsumerAccountResponseDto> GetConsumerAccount(GetConsumerAccountRequestDto getConsumerAccountRequestDto);
    }
}
