extern alias SunnyRewards_Task;
using SunnyRewards.Helios.ETL.Common.Domain.Dtos;
using SunnyRewards_Task::SunnyRewards.Helios.Task.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos
{
    public class AvailableRecurringTaskResponseDto : BaseResponseDto
    {
        public List<TaskRewardDetailDto>? AvailableTasks { get; set; }
    }
}
