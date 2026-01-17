namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class FindConsumerTasksByIdResponseDto
    {
        public ConsumerTaskDto? ConsumerTask { get; set; }

        public TaskRewardDetailDto? TaskRewardDetail { get; set; }
    }
}
