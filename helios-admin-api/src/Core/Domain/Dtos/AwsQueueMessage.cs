namespace SunnyRewards.Helios.Admin.Core.Domain.Dtos
{
    public class AwsQueueMessage
    {
        public string Message { get; set; } = "";
        public string QueueUrl { get; set; } = "";
    }

}
