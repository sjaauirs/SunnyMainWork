namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos.Json
{
    public class FailedRecords
    {

        public string RecordKey { get; set; }       // WalletId / ConsumerId
        public string RecordPayload { get; set; }   // JSON snapshot

        public string ErrorMessage { get; set; }
        public DateTime FailedTs { get; set; }
    }


}
