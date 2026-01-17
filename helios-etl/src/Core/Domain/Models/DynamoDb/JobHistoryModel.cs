

using Amazon.DynamoDBv2.DataModel;

namespace SunnyRewards.Helios.ETL.Core.Domain.Models.DynamoDb
{
    public class JobHistoryModel
    {
        [DynamoDBHashKey("jobHistoryId")]
        public string? JobHistoryId { get; set; }
        [DynamoDBProperty("customerCode")]
        public string? CustomerCode { get; set; }
        [DynamoDBProperty("sponsorCode")]
        public string? SponsorCode { get; set; }
        [DynamoDBProperty("tenantCode")]
        public string? TenantCode { get; set; }
        [DynamoDBProperty("endTime")]
        public string? EndTime { get; set; }
        [DynamoDBProperty("errorLog")]
        public string? ErrorLog { get; set; }
        [DynamoDBProperty("fileType")]
        public string? FileType { get; set; }
        [DynamoDBProperty("jobDefinition")]
        public string? JobDefinition { get; set; }
        [DynamoDBProperty("jobId")]
        public string? JobId { get; set; }
        [DynamoDBProperty("metadata")]
        public string? Metadata { get; set; }
        [DynamoDBProperty("outputData")]
        public string? OutputData { get; set; }
        [DynamoDBProperty("processType")]
        public string? ProcessType { get; set; }
        [DynamoDBProperty("retries")]
        public int? Retries { get; set; }
        [DynamoDBProperty("runStatus")]
        public string? RunStatus { get; set; }
        [DynamoDBProperty("runDuration")]
        public string? RunDuration { get; set; }
        [DynamoDBProperty("scheduleTime")]
        public string? ScheduleTime { get; set; }
        [DynamoDBProperty("startTime")]
        public string? StartTime { get; set; }
        [DynamoDBProperty("createdBy")]
        public string? CreatedBy { get; set; }
        [DynamoDBProperty("createdTs")]
        public string? CreatedTs { get; set; }
        [DynamoDBProperty("updatedBy")]
        public string? UpdatedBy { get; set; }
        [DynamoDBProperty("updatedTs")]
        public string? UpdatedTs { get; set; }
    }
}
