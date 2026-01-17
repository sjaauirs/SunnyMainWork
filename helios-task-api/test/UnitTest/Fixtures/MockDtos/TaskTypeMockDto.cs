using SunnyRewards.Helios.Task.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Task.UnitTest.Fixtures.MockDtos
{
    public class TaskTypeMockDto : TaskTypeDto
    {
        public TaskTypeMockDto()
        {
            TaskTypeId = 2;
            TaskTypeCode = "typ-cdfjdxjfvj5654656";
            TaskTypeName = "Test";
            TaskTypeDescription = "Test Done";

        }
    }
}
