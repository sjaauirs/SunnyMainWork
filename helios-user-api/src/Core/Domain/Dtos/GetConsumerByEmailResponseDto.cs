using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.User.Core.Domain.Dtos
{
    public class GetConsumerByEmailResponseDto : BaseResponseDto
    {

        public PersonDto? Person { get; set; } = new PersonDto();
        public ConsumerDto[] Consumer { get; set; } = new ConsumerDto[0];

    }
}
