using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Admin.UnitTest.Fixtures.MockDtos
{
    public class TaskResponseMockDto : TaskResponseDto
    {
        public TaskResponseMockDto()
        {
            Task = new TaskDto() { TaskId = 1, TaskCode = "Task001" };
        }
    }
}
