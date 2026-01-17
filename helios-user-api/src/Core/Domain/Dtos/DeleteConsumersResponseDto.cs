using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.User.Core.Domain.Dtos
{
    public class DeleteConsumersResponseDto : BaseResponseDto
    {
        public List<ConsumerDataResponseDto>? ConsumersData { get; set; }

    }
}