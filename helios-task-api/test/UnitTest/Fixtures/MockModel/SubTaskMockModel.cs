using SunnyRewards.Helios.Task.Core.Domain.Models;

namespace SunnyRewards.Helios.Task.UnitTest.Fixtures.MockModel
{
    public class SubTaskMockModel : SubTaskModel
    {
        public SubTaskMockModel()
        {
            SubTaskId = 1;
            ParentTaskRewardId = 2;
            ChildTaskRewardId = 2;
            ConfigJson = "testttt";
        }
    }
}
