using SunnyRewards.Helios.Task.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Task.UnitTest.Fixtures.MockDtos
{
    public class GetTaskRewardByCodeResponseMockDto : GetTaskRewardByCodeResponseDto
    {
        public GetTaskRewardByCodeResponseMockDto()
        {
            TaskRewardDetail = new TaskRewardDetailDto()
            {

                Task = new TaskDto()
                {
                    TaskId = 1,
                    TaskTypeId = 1,
                    TaskCode = "tsk-210ddf7876234c11b64668d4246f0b44",
                    TaskName = "Annual wellness visit",
                    SelfReport = true,
                    ConfirmReport = true,
                },
                TaskReward = new TaskRewardDto()
                {
                    TaskId = 1,
                    RewardTypeId = 1,
                    TaskRewardId = 1,
                    TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4",
                    TaskRewardCode = "trw-8a154edc602c49efb210d67a7bfe22b4",
                    TaskActionUrl = "https://app.dev.sunnyrewards.com/sdk?consumer-code=cmr-4579a933e40f4ec63278f7",
                    MinTaskDuration = 2,
                    MaxTaskDuration = 2,
                    Expiry = DateTime.UtcNow,
                    Priority = 5,
                    Reward = "rewardAmount",
                },

                TaskDetail = new TaskDetailDto()
                {
                    TaskDetailId = 1,
                    TaskId = 1,
                    LanguageCode = "en-us",
                    TaskHeader = "Complete your annual wellness visit",
                    TaskDescription = "Feel better today.  Try virtual physical therapy.  Click the link below.",
                    TermsOfServiceId = 1,
                    UpdateTs = DateTime.UtcNow,
                    TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4"
                },
                TermsOfService = new TermsOfServiceDto()
                {
                    TermsOfServiceId = 1,
                    TermsOfServiceText = "We provide you access and use of our websites, including and other Internet sites, mobile applications",
                    LanguageCode = "en-us",
                    TermsOfServiceCode = "tos-8a154edc602c49efb210d67a7bfe22b4"
                },
            };
        }
    }
}