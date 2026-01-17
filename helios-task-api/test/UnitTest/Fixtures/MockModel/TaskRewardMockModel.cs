using SunnyRewards.Helios.Task.Core.Domain.Models;

namespace SunnyRewards.Helios.Task.UnitTest.Fixtures.MockModel
{
    public class TaskRewardMockModel : TaskRewardModel
    {
        DateTime DT = new DateTime(2026, 11, 30, 9, 15, 0);
        DateTime DT1 = new DateTime(2045, 11, 23, 9, 15, 0);
        public TaskRewardMockModel()
        {
            TaskId = 0;
            RewardTypeId = 1;
            TaskRewardId = 1;
            TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4";
            TaskRewardCode = "trw-8a154edc602c49efb210d67a7bfe22b4";
            TaskActionUrl = "https://app.dev.sunnyrewards.com/sdk?consumer-code=cmr-4579a933e40f4ec63278f7";
            MinTaskDuration = 2;
            MaxTaskDuration = 2;
            Priority = 5;
            Reward = "{\r\n  \"rewardAmount\": 50\r\n}";
            CreateTs = DateTime.UtcNow;
            UpdateTs = DateTime.UtcNow;
            CreateUser = "sunny";
            UpdateUser = "sunny rewards";
            DeleteNbr = 0;
            ValidStartTs = DateTime.UtcNow;
            Expiry = DT;
            TaskExternalCode = "NA";
            IsRecurring = false;
            RecurrenceDefinitionJson = "{\r\n  \"recurrenceType\": \"PERIODIC\",\r\n  \"periodic\": {\r\n    \"period\": \"MONTH\",\r\n    \"periodRestartDate\": 1\r\n  }\r\n}";
            TaskRewardConfigJson = "{\r\n  \"collectionConfig\": {\r\n    \"flattenTasks\": true,\r\n    \"includeInAllAvailableTasks\": false\r\n  }\r\n}";

        }
        public List<TaskRewardModel> taskData()
        {
            return new List<TaskRewardModel>()
            {
                new TaskRewardMockModel()
            };
        }
    }

    public class TaskRewardExternalCodeMockModel : TaskRewardModel
    {
        DateTime DT =  DateTime.UtcNow;
        DateTime DT1 = DateTime.UtcNow;
        public TaskRewardExternalCodeMockModel()
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
            ValidStartTs = DateTime.UtcNow;
            Expiry = DT;
            TaskExternalCode = "NA";
            IsRecurring = false;
            RecurrenceDefinitionJson = "{\r\n  \"recurrenceType\": \"PERIODIC\",\r\n  \"periodic\": {\r\n    \"period\": \"MONTH\",\r\n    \"periodRestartDate\": 1\r\n  }\r\n}";
        }
        public List<TaskRewardModel> taskExternalCodeData()
        {
            return new List<TaskRewardModel>()
            {
                new TaskRewardExternalCodeMockModel()
            };
        }
    }

    public class TaskRewardDetailMockModel : TaskRewardModel
    {
        DateTime DT = new DateTime(2024, 11, 30, 9, 15, 0);
        DateTime DT1 = new DateTime(2023, 11, 23, 9, 15, 0);
        public TaskRewardDetailMockModel()
        {
            TaskId = 0;
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
            ValidStartTs = DT1;
            Expiry = DT;
            TaskExternalCode = "NA";
            IsRecurring = false;
            RecurrenceDefinitionJson = "{\r\n  \"recurrenceType\": \"PERIODIC\",\r\n  \"periodic\": {\r\n    \"period\": \"MONTH\",\r\n    \"periodRestartDate\": 1\r\n  }\r\n}";
        }
        public List<TaskRewardModel> taskData()
        {
            return new List<TaskRewardModel>()
            {
                new TaskRewardDetailMockModel()
            };
        }
    }
}