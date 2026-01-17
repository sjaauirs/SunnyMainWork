using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.UnitTest.Fixtures.MockModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Task.UnitTest.Fixtures.MockDtos
{
    public class GetTaskByTenantCodeResponseMockDto : GetTaskByTenantCodeResponseDto
    {
        public GetTaskByTenantCodeResponseMockDto()
        {
            AvailableTasks = new List<TaskRewardDetailDto>()
            {
                new TaskRewardDetailDto()
                {
                    TaskReward = new TaskRewardMockDto(),
                    Task = new TaskMockDto(),
                    TaskDetail = new TaskDetailDto()
                    {
                        TaskDetailId = 1,
                        TaskId = 1,
                        LanguageCode = "en-us",
                        TaskHeader = "Complete your annual wellness visit",
                        TaskDescription = "Feel better today.  Try virtual physical therapy.  Click the link below.",
                        TermsOfServiceId = 1,
                        TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4",
                    },
                }
            };
        }
    }
}
