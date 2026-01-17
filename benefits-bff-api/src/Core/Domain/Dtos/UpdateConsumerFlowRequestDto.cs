namespace Sunny.Benefits.Bff.Core.Domain.Dtos
{
    public class UpdateConsumerFlowRequestDto 
    {
        public string ConsumerCode { get; set; } = string.Empty;
        public string TenantCode { get; set; } = string.Empty;
        public long CurrentStepId { get; set; }
        public long FlowId { get; set; }
        public string Status { get; set;} = String.Empty;

    }

}