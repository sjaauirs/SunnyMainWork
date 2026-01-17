using SunnyRewards.Helios.Task.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.UnitTest.Fixtures.MockDtos
{
    public class TaskDetailRequestMockDto : TaskDetailRequestDto
    {
        public TaskDetailRequestMockDto()
        {
            TaskId = 9;
            TaskHeader = "Select your PCP";
            TaskDescription = "Your Primary Care provider plays in important role in your healthcare.";
            LanguageCode = "en-US";
            TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4";
            TaskCtaButtonText = "Search Providers";
        }
    }
}
