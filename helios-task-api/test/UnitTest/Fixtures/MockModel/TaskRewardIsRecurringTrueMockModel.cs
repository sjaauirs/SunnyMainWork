using SunnyRewards.Helios.Task.Core.Domain.Models;

namespace SunnyRewards.Helios.Task.UnitTest.Fixtures.MockModel
{
    public class TaskRewardIsRecurringTrueMockModel : TaskRewardModel
    {
        DateTime validStartTs = new DateTime(2023, 11, 23, 9, 15, 0);
        DateTime expiry = new DateTime(2027, 11, 30, 9, 15, 0);
        public TaskRewardIsRecurringTrueMockModel()
        {
            TaskId = 1;
            RewardTypeId = 1;
            TaskRewardId = 1;
            TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4";
            TaskRewardCode = "trw-8a154edc602c49efb210d67a7bfe22b4";
            TaskActionUrl = "https://app.dev.sunnyrewards.com/sdk?consumer-code=cmr-4579a933e40f4ec63278f7";
            MinTaskDuration = 2;
            MaxTaskDuration = 2;
            Priority = 5;
            Reward = "rewardAmount";
            CreateTs = DateTime.UtcNow;
            UpdateTs = DateTime.UtcNow;
            CreateUser = "sunny";
            UpdateUser = "sunny rewards";
            DeleteNbr = 0;
            ValidStartTs = validStartTs;
            Expiry = expiry;
            TaskExternalCode = "NA";
            IsRecurring = true;
            RecurrenceDefinitionJson = "{\r\n  \"recurrenceType\": \"PERIODIC\",\r\n  \"periodic\": {\r\n    \"period\": \"MONTH\",\r\n    \"periodRestartDate\": 1\r\n  }\r\n}";
        }
        public List<TaskRewardModel> taskData()
        {
            return new List<TaskRewardModel>()
            {
                new TaskRewardIsRecurringTrueMockModel()
            };
        }
    }
    public class TaskRewardIsMockModel : TaskRewardModel
    {
        DateTime validStartTs = new DateTime(2022, 11, 13, 9, 15, 0);
        DateTime expiry = new DateTime(2025, 11, 15, 9, 15, 0);
        
        public TaskRewardIsMockModel()
        {
            TaskId = 1;
            RewardTypeId = 1;
            TaskRewardId = 1;
            TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4";
            TaskRewardCode = "trw-8a154edc602c49efb210d67a7bfe22b4";
            TaskActionUrl = "https://app.dev.sunnyrewards.com/sdk?consumer-code=cmr-4579a933e40f4ec63278f7";
            MinTaskDuration = 2;
            MaxTaskDuration = 2;
            Priority = 5;
            Reward = "rewardAmount";
            CreateTs = DateTime.UtcNow;
            UpdateTs = DateTime.UtcNow;
            CreateUser = "sunny";
            UpdateUser = "sunny rewards";
            DeleteNbr = 0;
            ValidStartTs = validStartTs; 
            Expiry = expiry;
            TaskExternalCode = "NA";
            IsRecurring = true;
            RecurrenceDefinitionJson = "{\r\n  \"recurrenceType\": \"PERIODIC\",\r\n  \"periodic\": {\r\n    \"period\": \"MONTH\",\r\n    \"periodRestartDate\": 1\r\n  }\r\n}";
        }
        public List<TaskRewardModel> taskRewardData()
        {
            return new List<TaskRewardModel>()
            {
                new TaskRewardIsMockModel(),
                new TaskRewardIsMockModel()
        {
            TaskId = 1,
            RewardTypeId = 1,
            TaskRewardId = 1,
            TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4",
            TaskRewardCode = "trw-8a154edc602c49efb210d67a7bfe22b4",
            TaskActionUrl = "https://app.dev.sunnyrewards.com/sdk?consumer-code=cmr-4579a933e40f4ec63278f7",
            MinTaskDuration = 2,
            MaxTaskDuration = 2,
            Priority = 5,
            Reward = "rewardAmount",
            CreateTs = DateTime.UtcNow,
            UpdateTs = DateTime.UtcNow,
            CreateUser = "sunny",
            UpdateUser = "sunny rewards",
            DeleteNbr = 0,
            ValidStartTs = validStartTs,
            Expiry = expiry,
            TaskExternalCode = "NA",
            IsRecurring = true,
            RecurrenceDefinitionJson = "{\"schedules\":[{\"startDate\":\"01-01\",\"expiryDate\":\"12-31\"}],\"recurrenceType\":\"SCHEDULE\"}"
        }
            };
        }
    }
}