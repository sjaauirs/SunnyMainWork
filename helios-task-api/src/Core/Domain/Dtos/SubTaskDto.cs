using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class SubTaskDto : BaseDto
    {

        public long SubTaskId { get; set; }
        public long ParentTaskRewardId { get; set; }
        public long ChildTaskRewardId { get; set; }
        public string? ConfigJson  { get; set; } 
       
    }
}
