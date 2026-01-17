namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class UpdateSubtaskRequestDto : ConsumerTaskDto
    {
        public long CompletedTaskId { get; set; }
    }
}
