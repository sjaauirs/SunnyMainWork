using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class ConsumerTaskResponseDto : BaseResponseDto
    {
        public List<TaskRewardDetailDto>? AvailableTasks { get; set; }
        public List<TaskRewardDetailDto>? PendingTasks { get; set; }
        public List<TaskRewardDetailDto>? CompletedTasks { get; set; }
    }
}
