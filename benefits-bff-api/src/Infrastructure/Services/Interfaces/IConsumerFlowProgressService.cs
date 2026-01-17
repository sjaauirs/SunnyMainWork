using Sunny.Benefits.Bff.Core.Domain.Dtos;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Dtos;

namespace Sunny.Benefits.Bff.Infrastructure.Services.Interfaces
{
    public interface IConsumerFlowProgressService
    {
        Task<OnboardingFlowStepsResponseDto> UpdateConsumerFlowStatusAsync(UpdateConsumerFlowRequestDto consumerFlowStatusRequestDto);
        Task<OnboardingFlowStepsResponseDto> GetConsumerFlowProgressAsync(GetConsumerFlowRequestDto ConsumerFlowRequestDto, FlowResponseDto? flowSteps = null);
    }
}
