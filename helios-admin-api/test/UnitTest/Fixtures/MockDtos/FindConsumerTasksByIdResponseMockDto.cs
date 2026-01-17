using SunnyRewards.Helios.Task.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.UnitTest.Fixtures.MockDtos
{
    public class FindConsumerTasksByIdResponseMockDto : FindConsumerTasksByIdResponseDto
    {
        public FindConsumerTasksByIdResponseMockDto()
        {
            
                ConsumerTask = new ConsumerTaskDto
                {
                    ConsumerTaskId = 2,
                    TaskId = 2,
                    Progress = 1,
                    Notes = "note",
                    TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4",
                    ConsumerCode = "cmr-c457c5257c59451d8a93ea941a9f2e0a",
                    TaskStatus = "IN_PROGRESS",
                    TaskCompleteTs = DateTime.UtcNow,
                    TaskStartTs = DateTime.UtcNow,
                    CreateTs = DateTime.UtcNow,
                    CreateUser = "sunny",

                };

            TaskRewardDetail = new TaskRewardDetailDto
            {
                Task = new TaskDto()
                {
                    TaskId = 2,
                    TaskTypeId = 2,
                    TaskCode = "tasnvbgk55654045",
                    TaskName = "avilable",
                    SelfReport = true,
                    ConfirmReport = true
                },
                TaskReward = new TaskRewardDto()
                {
                    TaskId = 2,
                    TaskRewardId = 3,
                    RewardTypeId = 4,
                    TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4",
                    TaskRewardCode = "Tas567565kb54",
                    Reward = "{\"RewardAmount\": 100}",
                    Priority = 1,
                    Expiry = DateTime.UtcNow,
                    MinTaskDuration = 12,
                    MaxTaskDuration = 13,
                    TaskCompletionCriteriaJson = "{\r\n  \"imageCriteria\": {\r\n    \"requiredImageCount\": 2\r\n  }\r\n}"

                },
                TaskDetail = new TaskDetailDto()
                {

                    TaskId = 2,
                    TaskDetailId = 2,
                    TermsOfServiceId = 3,
                    TaskHeader = "sunny",
                    TaskDescription = "task done",
                    LanguageCode = "en",
                    UpdateTs = DateTime.UtcNow,
                },

                TermsOfService = new TermsOfServiceDto()
                {
                    TermsOfServiceId = 1,
                    TermsOfServiceText = "success",
                    LanguageCode = "en",
                },
            };
        }
    }
}


