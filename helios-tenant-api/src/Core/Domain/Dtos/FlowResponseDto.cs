using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos.Json;

namespace SunnyRewards.Helios.Tenant.Core.Domain.Dtos
{
    public class FlowResponseDto : BaseResponseDto
    {
        public string? TenantCode { get; set; }
        public string? CohortCode { get; set; }
        public long FlowId { get; set; }
        public int VersionNumber { get; set; }
        public List<FlowStepDto> Steps { get; set; } = [];
    }

    public class FlowStepDto
    {
        public long StepId { get; set; }
        public int StepIdx { get; set; }
        public string? ComponentType { get; set; }
        public string? ComponentName { get; set; }
        public long? OnSuccessStepId { get; set; }
        public long? OnFailureStepId { get; set; }
        public string? StepConfigJson {  get; set; }
    }
}
