using SunnyRewards.Helios.Task.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.UnitTest.Fixtures.MockDtos
{
    public class TaskRewardRequestMockDto : TaskRewardRequestDto
    { 
        public TaskRewardRequestMockDto()
        {
            TaskId = 12345;
            RewardTypeId = 6789;
            TenantCode = "tenant123";
            TaskRewardCode = "reward-001";
            TaskActionUrl = "https://example.com";
            Reward = "Bonus";
            Priority = 1;
            Expiry = DateTime.UtcNow.AddDays(30);
            MinTaskDuration = 10;
            MaxTaskDuration = 120;
            TaskExternalCode = "ext-001";
            ValidStartTs = DateTime.UtcNow;
            IsRecurring = true;
            RecurrenceDefinitionJson = "{\"frequency\":\"daily\",\"interval\":1}";
            SelfReport = true;
            TaskCompletionCriteriaJson = "{\"completion\":\"100% completion required\"}";
            ConfirmReport = true;
        }
    }
}
