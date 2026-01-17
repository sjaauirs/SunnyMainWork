using SunnyRewards.Helios.Task.Core.Domain.Models;

namespace SunnyRewards.Helios.Task.UnitTest.Fixtures.MockModel
{
    public class TaskCategoryMockModel : TaskCategoryModel
    {
        public TaskCategoryMockModel()
        {
            TaskCategoryId = 1;
            TaskCategoryCode = "CAT-001";
            TaskCategoryDescription = "Description for category 1";
            TaskCategoryName = "Category One";
        }

        public List<TaskCategoryModel> GetTaskCategories()
        {
            return
             [
                 new TaskCategoryMockModel()
             ];
        }
    }
}
