using SunnyRewards.Helios.Task.Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Task.UnitTest.Fixtures.MockDtos
{
    public class TaskRewardCollectionMockModel:TaskRewardCollectionModel
    {
        public TaskRewardCollectionMockModel()
        {
            TaskRewardCollectionId = 1;
            ParentTaskRewardId = 1898;
            ChildTaskRewardId = 1;
            UniqueChildCode = "unc-00a4b30b3df844f1a04dd21b5f3b9e8e";
        }
    }
}
