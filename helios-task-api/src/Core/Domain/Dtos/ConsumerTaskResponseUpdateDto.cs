using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class ConsumerTaskResponseUpdateDto : BaseResponseDto
    {
        public ConsumerTaskDto? ConsumerTask { get; set; }
    }
}