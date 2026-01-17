using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace SunnyRewards.Helios.Task.Core.Domain.Json
{
    public class TaskRewardConfigJson
    {
        [JsonProperty("collectionConfig")]
        public CollectionConfig? CollectionConfig { get; set; } = new();
        [JsonProperty("isOnBoardingSurvey")]
        public bool IsOnBoardingSurvey { get; set; } = false;
    }

    public class CollectionConfig
    {
        [JsonProperty("flattenTasks")]
        public bool FlattenTasks { get; set; }
        [JsonProperty("includeInAllAvailableTasks")]
        public bool IncludeInAllAvailableTasks { get; set; }
    }

}
