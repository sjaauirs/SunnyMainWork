using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos
{
    public class ExecuteCardOperationRequestDto
    {
        public string? TenantCode { get; set; }
        public string? ConsumerCode { get; set; }
        public string? CardOperation { get; set; }
    }

    public class ExecuteCardOperationResponseDto : BaseResponseDto
    {
        public string? Success { get; set; }
    }
}
