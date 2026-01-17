using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Models;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class PageinatedCompletedConsumerTaskDto 
    {
        public int TotalRecords { get; set; }
        public List<ConsumerTaskModel>? CompletedTasks { get; set; }
    }

    public class PageinatedCompletedConsumerTaskResponseDto : BaseResponseDto
    {
        public int TotalRecords { get; set; }
        public List<ConsumerTaskDto>? CompletedTasks { get; set; }
    }
}
