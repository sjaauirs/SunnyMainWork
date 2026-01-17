using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class ExportTaskRewardCollectionResponseDto : BaseResponseDto
    {
        public IList<ExportTaskRewardCollectionDto> TaskRewardCollections { get; set; } = new List<ExportTaskRewardCollectionDto>();
    }
}
