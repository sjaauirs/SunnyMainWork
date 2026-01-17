using SunnyRewards.Helios.Task.Core.Domain.Models;

namespace SunnyRewards.Helios.Task.UnitTest.Fixtures.MockModel
{
    public class TaskTypeMockModel : TaskTypeModel
    {
        public TaskTypeMockModel()
        {
            TaskTypeId = 2;
            TaskTypeCode = "typ-cdfjdxjfvj5654656";
            TaskTypeName = "Test";
            TaskTypeDescription = "Test Done";
        }

        public List<TaskTypeModel> tasktype()
        {
            return new List<TaskTypeModel>()
             {
                 new TaskTypeMockModel()
             };
        }
    }
}
