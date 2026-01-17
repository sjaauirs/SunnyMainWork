using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class ImportTaskResponseDto:BaseResponseDto
    {
        public List<ImportTaskRewardDto> TaskRewardList { get; set; } = [];
    }
}
