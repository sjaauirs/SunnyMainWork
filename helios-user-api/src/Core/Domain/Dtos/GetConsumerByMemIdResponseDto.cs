using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.User.Core.Domain.Dtos
{
    public class GetConsumerByMemIdResponseDto : BaseResponseDto
    {
        public ConsumerDto? Consumer { get; set; }
    }
}
