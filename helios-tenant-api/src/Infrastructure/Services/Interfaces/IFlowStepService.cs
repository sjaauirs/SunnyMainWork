using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Tenant.Infrastructure.Services.Interfaces
{
    public interface IFlowStepService
    {
        Task<FlowResponseDto> GetFlowSteps(FlowRequestDto flowRequestDto);
    }
}
