using SunnyRewards.Helios.Task.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.UnitTest.Fixtures.MockDtos
{
    public class TaskRewardTypesResponseMockDto : TaskRewardTypesResponseDto
    {
        public TaskRewardTypesResponseMockDto()
        {
            TaskRewardTypes =
            [
                new TaskRewardTypeDto { RewardTypeId = 1, RewardTypeName = "Type1" },
                new TaskRewardTypeDto { RewardTypeId = 2, RewardTypeName = "Type2" }
            ];
        }
    }
}
