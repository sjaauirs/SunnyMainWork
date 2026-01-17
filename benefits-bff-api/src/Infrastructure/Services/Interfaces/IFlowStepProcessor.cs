using Sunny.Benefits.Bff.Core.Domain.Dtos;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;

namespace Sunny.Benefits.Bff.Infrastructure.Services.Interfaces
{
    public interface IFlowStepProcessor
    {
        Task<List<FlowStepDto>> ProcessSteps(List<FlowStepDto>? flowSteps, string consumerCode);
    }
}
