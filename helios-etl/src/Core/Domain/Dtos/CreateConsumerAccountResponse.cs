using SunnyRewards.Helios.ETL.Core.Domain.Models;

namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos
{
    /// <summary>
    /// Used to collect error and success records when creating Consumer in card30
    /// </summary>
    public class CreateConsumerAccountResponse
    {
        public List<ETLConsumerAccountModel> ErrorRecords { get; set; } = new List<ETLConsumerAccountModel>();
        public List<ETLConsumerAccountModel> SuccessRecords { get; set; } = new List<ETLConsumerAccountModel>();
    }
}
