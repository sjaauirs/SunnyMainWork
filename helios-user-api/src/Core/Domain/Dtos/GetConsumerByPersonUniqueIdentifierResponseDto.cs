using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.User.Core.Domain.Dtos
{
    public class GetConsumerByPersonUniqueIdentifierResponseDto : BaseResponseDto
    {
        public PersonDto? Person { get; set; }
        public ConsumerDto[] Consumer { get; set; } = Array.Empty<ConsumerDto>();
    }

}
