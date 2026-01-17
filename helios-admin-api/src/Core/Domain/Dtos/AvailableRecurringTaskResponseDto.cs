using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Core.Domain.Dtos
{
    public class AvailableRecurringTaskResponseDto : BaseResponseDto
    {
        public List<TaskRewardDetailDto>? AvailableTasks { get; set; }
    }
}
