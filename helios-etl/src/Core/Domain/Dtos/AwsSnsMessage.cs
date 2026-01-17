namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos
{
    public class AwsSnsMessage
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public AwsSnsMessage(string message)
        {
            Message = message;
        }

        /// <summary>
        /// 
        /// </summary>
        public string Message { get; set; } = "";
    }
}
