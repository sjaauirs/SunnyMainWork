namespace Sunny.Benefits.Bff.Core.Domain.Dtos
{
    public class GetConsumerFlowRequestDto 
    {
        public string ConsumerCode { get; set; } = string.Empty;
        public string TenantCode { get; set; } = string.Empty;

    }

}