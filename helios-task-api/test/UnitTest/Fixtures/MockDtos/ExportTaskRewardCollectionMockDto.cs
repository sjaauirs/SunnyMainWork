using SunnyRewards.Helios.Task.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Task.UnitTest.Fixtures.MockDtos
{
    public class ExportTaskRewardCollectionMockDto : ExportTaskRewardCollectionDto
    {
        public ExportTaskRewardCollectionMockDto()
        {
            TaskRewardCollectionId = 101;
            ParentTaskRewardId = 7081;
            ParentTaskRewardCode = "trw-ab480b6a-bbea-4752-920e-5ae6dbb130aa";
            ChildTaskRewardId = 2628;
            ChildTaskRewardCode = "trw-0717c08ee15747b4b9d01c0c28905a61";
            UniqueChildCode = "9fdc70e9-c108-4f4c-a2e9-9bb52cdaac7d";
            ConfigJson = "{}";
        }
    }
}
