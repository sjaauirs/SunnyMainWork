using SunnyRewards.Helios.Task.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.UnitTest.Fixtures.MockDtos
{
    public class TaskRequestMockDto : TaskRequestDto
    {
        public TaskRequestMockDto() 
        {
            TaskTypeId = 101;
            TaskCode = "Task001";
            TaskName = "Sample Task";
            SelfReport = true;
            ConfirmReport = true;
            TaskCategoryId = 5;
            IsSubtask = false;
        }
    }
}
