using SunnyRewards.Helios.Task.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Task.UnitTest.Fixtures.MockDtos
{
    public class TaskMockDto:TaskDto
    {
        public TaskMockDto()
        {
            TaskId = 2;
            TaskTypeId = 1;
            TaskCode = "tsk-210ddf7876234c11b64668d4246f0b44";
            TaskName = "Annual wellness visit";
            SelfReport = true;
            ConfirmReport = true;
            IsSubtask = true;
            TaskCategoryId = 1;
        }
        
    }
}
