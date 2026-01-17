using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.UnitTest.Fixtures.MockDtos
{
    public class TasksResponseMockDto : TasksResponseDto
    {
        public TasksResponseMockDto()
        {
            Tasks =
            [
                new TaskDto { TaskId = 1, TaskName = "Task1" },
                new TaskDto { TaskId = 2, TaskName = "Task2" }
            ];
        }
    }
}
