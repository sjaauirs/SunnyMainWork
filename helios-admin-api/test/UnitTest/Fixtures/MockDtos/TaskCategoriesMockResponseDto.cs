using SunnyRewards.Helios.Task.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.UnitTest.Fixtures.MockDtos
{
    public class TaskCategoriesMockResponseDto : TaskCategoriesResponseDto
    {
        public TaskCategoriesMockResponseDto()
        {
            TaskCategories =
            [
                new TaskCategoryDto { TaskCategoryId = 1, TaskCategoryName = "Category1" },
                new TaskCategoryDto { TaskCategoryId = 2, TaskCategoryName = "Category2" }
            ];
        }
    }
}
