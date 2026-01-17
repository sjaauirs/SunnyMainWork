using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace Sunny.Benefits.Bff.Core.Domain.Dtos
{
    public class GetCardStatusResponseDto : BaseResponseDto
    {
        public string? CardStatus { get; set; }
    }
}
