using SunnyRewards.Helios.Task.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.UnitTest.Fixtures.MockDtos
{
    public class TaskTypesResponseMockDto : TaskTypesResponseDto
    {
        public TaskTypesResponseMockDto()
        {
            TaskTypes =
            [
                new TaskTypeDto { TaskTypeId = 1, TaskTypeName = "Type1" },
                new TaskTypeDto { TaskTypeId = 2, TaskTypeName = "Type2" }
            ];
        }
    }
}
