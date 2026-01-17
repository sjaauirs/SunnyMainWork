using SunnyRewards.Helios.Task.Core.Domain.Dtos;

namespace Sunny.Benefits.Bff.UnitTest.Fixtures.MockDtos
{
    public class GetTaskRewardByCodeResponseMockDto : GetTaskRewardByCodeResponseDto
    {
        public GetTaskRewardByCodeResponseMockDto()
        {
            TaskRewardDetail = new TaskRewardDetailDto()
            {
                
                    Task = new TaskDto
                    {
                        TaskId = 2,
                        TaskTypeId = 2,
                        TaskCode = "tas46543",
                        TaskName = "sunny reward",
                        SelfReport = true,
                        ConfirmReport = true,
                        TaskCategoryId = 2,
                        IsSubtask = true
                    },

                    TaskReward = new TaskRewardDto
                    {
                        TaskId = 8,
                        TaskRewardId = 2,
                        RewardTypeId = 4,
                        TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4",
                        TaskRewardCode = "tas-928a2bb959e836",
                        TaskActionUrl = "sunny",
                        Reward = "sunny",
                        Priority = 232,
                        Expiry = DateTime.Now,
                        MinTaskDuration = 12,
                        MaxTaskDuration = 10,
                        TaskExternalCode = "ext-928a2bb959e836",
                        ValidStartTs = DateTime.Now,
                        IsRecurring = true,
                        RecurrenceDefinitionJson = "recurrent reward"
                        
                    },

                    TaskDetail = new TaskDetailDto
                    {
                        TaskId = 2,
                        TaskDetailId = 2,
                        TermsOfServiceId = 5,
                        TaskHeader = "sunny header",
                        TaskDescription = "description ",
                        LanguageCode = "lan-928a2bb959e836",
                        TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4",
                        TaskCtaButtonText = "text"
                    },

                    TermsOfService = new TermsOfServiceDto
                    {
                        TermsOfServiceId = 4,
                        TermsOfServiceText = "service-text",
                        LanguageCode = "lan-928a2bb959e836"
                    },

                    TenantTaskCategory = new TenantTaskCategoryDto
                    {
                        TenantTaskCategoryId = 6,
                        TaskCategoryId = 5,
                        TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4",
                        ResourceJson = "resource-sunny"
                    },

                    TaskType = new TaskTypeDto
                    {
                        TaskTypeId = 8,
                        TaskTypeCode = "tas-928a2bb959e836",
                        TaskTypeName = "sunny",
                        TaskTypeDescription = "sunny type"
                    },

                    RewardTypeName = "sunny rewards",             
            };
        
        }
    }
}
