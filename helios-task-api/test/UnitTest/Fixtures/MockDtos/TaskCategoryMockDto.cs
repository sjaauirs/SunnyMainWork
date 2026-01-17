using SunnyRewards.Helios.Task.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Task.UnitTest.Fixtures.MockDtos
{
    public class TaskCategoryMockDto : TaskCategoryDto
    {
        public TaskCategoryMockDto()
        {
            TaskCategoryId = 1;
            TaskCategoryCode = "CAT-001";
            TaskCategoryDescription = "Description for category 1";
            TaskCategoryName = "Category One";
        }
    }
}
