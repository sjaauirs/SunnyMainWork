using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class GetTaskByTenantCodeResponseDto : BaseResponseDto
    {
        /// <summary>
        /// 
        /// </summary>
        public GetTaskByTenantCodeResponseDto()
        {
            AvailableTasks = new List<TaskRewardDetailDto>();
            ConsumerTaskList = new List<GetConsumerTaskResponseDto>();
        }
        public List<TaskRewardDetailDto> AvailableTasks { get; set; }
        public List<GetConsumerTaskResponseDto> ConsumerTaskList { get; set; }
    }
}
