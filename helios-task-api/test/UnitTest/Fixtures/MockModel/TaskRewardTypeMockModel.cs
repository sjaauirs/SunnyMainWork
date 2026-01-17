using SunnyRewards.Helios.Task.Core.Domain.Models;

namespace SunnyRewards.Helios.Task.UnitTest.Fixtures.MockModel
{
    public class TaskRewardTypeMockModel : TaskRewardTypeModel
    {
        public TaskRewardTypeMockModel()
        {
            RewardTypeId = 1;
            RewardTypeName = "MONETARY_DOLLARS";
            RewardTypeDescription = "Money";
            RewardTypeCode = "Code001";
        }
        public List<TaskRewardTypeModel> taskreward()
        {
            return new List<TaskRewardTypeModel>()
          {
              new TaskRewardTypeMockModel()
          };
        }
    }
}
