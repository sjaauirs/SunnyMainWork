using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.User.Core.Domain.Dtos
{
    public class GetConsumerDeviceResponseDto : BaseResponseDto
    {
        public IList<ConsumerDeviceDto> ConsumerDevices { get; set; } = new List<ConsumerDeviceDto>();
    }
}
