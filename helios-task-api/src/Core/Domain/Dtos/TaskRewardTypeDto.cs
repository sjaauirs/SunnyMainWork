using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class TaskRewardTypeDto
    {
        public long RewardTypeId { get; set; }
        public string? RewardTypeName { get; set; }
        public string? RewardTypeDescription { get; set; }
        public string? RewardTypeCode { get; set; }
    }
}
