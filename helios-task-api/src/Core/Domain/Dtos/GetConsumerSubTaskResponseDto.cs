using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class GetConsumerSubTaskResponseDto : BaseResponseDto
    {
        public ConsumerTaskDto[] ConsumerTaskDto { get; set; } = new ConsumerTaskDto[0];
    }
}
