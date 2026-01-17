using SunnyRewards.Helios.Task.Core.Domain.Models;

namespace SunnyRewards.Helios.Task.UnitTest.Fixtures.MockModel
{
    public class TaskMockModel : TaskModel
    {
        public TaskMockModel()
        {
            TaskId = 1;
            TaskTypeId = 1;
            TaskCode = "tsk-210ddf7876234c11b64668d4246f0b44";
            TaskName = "Annual wellness visit";
            SelfReport = true;
            ConfirmReport = true;
            IsSubtask = true;
            TaskCategoryId = 1;
            CreateTs = DateTime.UtcNow;
            UpdateTs = DateTime.UtcNow;
            CreateUser = "sunny";
            UpdateUser = "sunny rewards";
            DeleteNbr = 0;

            
        }
        public List<TaskModel> taskModel()
        {
            return new List<TaskModel>()
            {
                new TaskMockModel()
            };
        }
    }
}
