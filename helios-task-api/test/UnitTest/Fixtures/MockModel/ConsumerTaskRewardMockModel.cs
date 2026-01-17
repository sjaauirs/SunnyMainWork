using Microsoft.AspNetCore.Http.HttpResults;
using SunnyRewards.Helios.Task.Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static Azure.Core.HttpHeader;

namespace SunnyRewards.Helios.Task.UnitTest.Fixtures.MockModel
{
    public class ConsumerTaskRewardMockModel: ConsumerTaskRewardModel
    {
        public ConsumerTaskRewardMockModel()
        {
            ConsumerTask = new ConsumerTaskModel()
            {           
                TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4",
                ConsumerTaskId = 203,
                Progress = 0,
                Notes = "done",
                TaskStartTs = DateTime.UtcNow,
                TaskCompleteTs = DateTime.UtcNow.AddMonths(-3),
                ConsumerCode = "cmr-c69905fc68ce4f36851f877bae38f22e",
                TaskStatus = "COMPLETED",
                TaskId = 1,
                CreateTs = DateTime.UtcNow,
                UpdateTs = DateTime.UtcNow,
                CreateUser = "sunny",
                UpdateUser = "sunny rewards",
                DeleteNbr = 0,
            };

            TaskReward = new TaskRewardModel()
            {
                TaskId = 0,
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
                ValidStartTs = DateTime.UtcNow,
                Expiry =  DateTime.UtcNow,
                TaskExternalCode = "NA",
                IsRecurring = false,
                RecurrenceDefinitionJson = "{\r\n  \"recurrenceType\": \"PERIODIC\",\r\n  \"periodic\": {\r\n    \"period\": \"MONTH\",\r\n    \"periodRestartDate\": 1\r\n  }\r\n}",
        };
        }
    }
}
