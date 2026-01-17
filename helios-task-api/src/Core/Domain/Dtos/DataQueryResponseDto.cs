using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class DataQueryResponseDto :BaseResponseDto
    {
        public List<Dictionary<string, TaskRewardDetailItemDto>> TaskRewardDetail { get; set; } = new ();
    }

    public class TaskRewardDetailItemDto
    {
        public TaskDto Task { get; set; }
        public TaskRewardDto TaskReward { get; set; }
        public TaskDetailDto TaskDetail { get; set; }
        public ConsumerTaskDto ConsumerTask { get; set; }
    }
}

