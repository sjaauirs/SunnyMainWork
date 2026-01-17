using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class AdventureTaskCollectionResponseDto:BaseResponseDto
    {
        public List<AdventureTaskCollectionDto> AdventureTaskRewards { get; set; } = [];
    }

    public class AdventureTaskCollectionDto
    {
        public AdventureDto? Adventure { get; set; }
        public IList<TaskRewardDetailDto> TaskRewards { get; set; } = [];
    }
}
