using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;

namespace Sunny.Benefits.Bff.Infrastructure.Services.Interfaces
{
    public interface IFlowStepService
    {
        Task<FlowResponseDto> GetFlowSteps(FlowRequestDto flowRequestDto);
    }
}
