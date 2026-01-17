using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Task.UnitTest.Fixtures.MockDtos
{
    public class TaskRewardMockDto : TaskRewardDto
    {
        public TaskRewardMockDto() 
        {
            TaskId = 1;
            TaskRewardId = 1;
            RewardTypeId = 1;
            TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4";
            TaskRewardCode = "trw-8a154edc602c49efb210d67a7bfe22b4";
            TaskActionUrl = "https://app.dev.sunnyrewards.com/sdk?consumer-code=cmr-4579a933e40f4ec63278f7";
            Reward = "rewardAmount";
            Priority = 5;
            Expiry =DateTime.UtcNow;
            MinTaskDuration = 1;
            MaxTaskDuration = 1;
            TaskExternalCode = "NA";
            ValidStartTs=DateTime.UtcNow;
            IsRecurring=true;
            RecurrenceDefinitionJson = "{\r\n  \"periodic\": {\r\n    \"period\": \"MONTH\",\r\n    \"periodRestartDate\": 1\r\n  },\r\n  \"recurrenceType\": \"PERIODIC\"\r\n}";
            SelfReport=true;
            TaskCompletionCriteriaJson = "";
            TaskRewardConfigJson = "{\r\n  \"collectionConfig\": {\r\n    \"flattenTasks\": true,\r\n    \"includeInAllAvailableTasks\": false\r\n  }\r\n}";

        }
    }
}
