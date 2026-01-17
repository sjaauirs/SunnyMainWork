using Amazon.DynamoDBv2.DataModel;

namespace SunnyRewards.Helios.ETL.Core.Domain.Models.DynamoDb
{
    public class JobDefinitionModel
    {
        [DynamoDBHashKey("jobDefinitionId")]
        public string? JobDefinitionId { get; set; }

        [DynamoDBHashKey("jobDefinition")]
        public string? JobDefinitionName { get; set; }

        [DynamoDBHashKey("jobDefinitionDescription")]
        public string? JobDefinitionDescription { get; set; }

        [DynamoDBHashKey("createdBy")]
        public string? CreatedBy { get; set; }

        [DynamoDBHashKey("createdTs")]
        public string? CreatedTs { get; set; }
    }
}
