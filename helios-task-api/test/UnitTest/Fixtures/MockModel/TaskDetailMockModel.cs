using SunnyRewards.Helios.Task.Core.Domain.Models;

namespace SunnyRewards.Helios.Task.UnitTest.Fixtures.MockModel
{
    public class TaskDetailMockModel : TaskDetailModel
    {
        public TaskDetailMockModel()
        {
            TaskDetailId = 1;
            TaskId = 1;
            LanguageCode = "en-us";
            TaskHeader = "Complete your annual wellness visit";
            TaskDescription = "Feel better today.  Try virtual physical therapy.  Click the link below.";
            TermsOfServiceId = 1;
            CreateTs = DateTime.UtcNow;
            UpdateTs = DateTime.UtcNow;
            CreateUser = "sunny";
            UpdateUser = "sunny rewards";
            DeleteNbr = 0;
            TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4";
        }
        public List<TaskDetailModel> taskDetail()
        {
            return new List<TaskDetailModel>()
             {
                 new TaskDetailMockModel()
             };
        }
    }
}