namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class FindConsumerTaskResponseDto
    {
        public List<ConsumerTaskDto>? ConsumerTask { get; set; }

        public List<TaskRewardDetailDto>? TaskRewardDetail { get; set; }
    }
}
