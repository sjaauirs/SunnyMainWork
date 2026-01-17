using SunnyRewards.Helios.User.Core.Domain.Dtos;

namespace SunnyRewards.Helios.User.Infrastructure.Services.Interfaces
{
    public interface IConsumerFlowProgressService
    {
        Task<ConsumerFlowProgressResponseDto> UpdateOnboardingStatusFlow(UpdateFlowStatusRequestDto updateOnboardingStatusDto);

        Task<ConsumerFlowProgressResponseDto> GetConsumerFlowProgressAsync(ConsumerFlowProgressRequestDto consumerFlowProgressRequestDto);
    }
}