namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos
{
    public class AwsQueueMessage
    {
        public string Message { get; set; } = "";

        public string QueueUrl { get; set; } = "";

    }
}
