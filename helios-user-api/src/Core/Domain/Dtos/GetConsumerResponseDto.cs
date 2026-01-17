using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.User.Core.Domain.Dtos
{
    public class GetConsumerResponseDto : BaseResponseDto
    {
        public GetConsumerResponseDto()
        {
            Consumer = new ConsumerDto();
        }
        public ConsumerDto Consumer { get; set; }
    }
}
