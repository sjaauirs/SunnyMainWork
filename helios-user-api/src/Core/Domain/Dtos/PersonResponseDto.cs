using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.User.Core.Domain.Dtos
{
    public class PersonResponseDto : BaseResponseDto
    {
        public PersonDto? Person { get; set; }
    }
}
