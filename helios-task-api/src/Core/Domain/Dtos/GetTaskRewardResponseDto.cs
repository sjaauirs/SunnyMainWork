using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class GetTaskRewardResponseDto
    {
        /// <summary>
        /// 
        /// </summary>
        public GetTaskRewardResponseDto()
        {
            TaskRewardDetails = new List<TaskRewardDetailDto>();
        }
        public List<TaskRewardDetailDto>? TaskRewardDetails { get; set; }
    }
}
