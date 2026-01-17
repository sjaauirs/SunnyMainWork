namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos.Json
{
    public class JobManager
    {
        public string JobName { get; set; }              
        public string JobRunId { get; set; }           
        public string? LastProcessedKey { get; set; }

        public string? LastProcessedId { get; set; }
        public DateTime? LastProcessedTs { get; set; }
        public string? OutputArtifact { get; set; }
        public string ErrorMessage { get; set; }
        public FailCollector failCollector { get; set; } = new FailCollector();
    }


}
