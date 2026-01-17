using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Admin.UnitTest.Fixtures.MockDtos
{
    public class GetTaskRewardByCodeResponseMockDto : GetTaskRewardByCodeResponseDto
    {
        public GetTaskRewardByCodeResponseMockDto()
        {
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
