using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class ConsumerHealthTaskResponseUpdateDto : BaseResponseDto
    {
        public ConsumerTaskDto? ConsumerTask { get; set; }
        public bool IsTaskCompleted { get; set; } = false;
    }
}
