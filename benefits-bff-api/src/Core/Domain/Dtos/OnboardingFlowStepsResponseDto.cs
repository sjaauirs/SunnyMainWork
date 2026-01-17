using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace Sunny.Benefits.Bff.Core.Domain.Dtos
{
    public class OnboardingFlowStepsResponseDto : BaseResponseDto
    {
        public string ConsumerCode { get; set; } = string.Empty;
        public string TenantCode { get; set; } = string.Empty;
        public long FlowId { get; set; }
        public string OnboardingFlowStatus { get; set; } = string.Empty;
        public bool canSkipStep { get; set; } = false;
        public string CurrentStepName { get; set; } = String.Empty;
        public long CurrentStepId { get; set; } 
        public string SuccessStepName { get; set; } = String.Empty;
        public string FailedStepName { get; set; } = String.Empty;

    }

}