namespace SunnyRewards.Helios.ETL.Core.Domain.Models
{
    public class ETLConsumerAndWalletModel
    {
        public ETLConsumerModel? Consumer { get; set; }
        public ETLWalletModel? Wallet { get; set; }
    }
    public class ETLConsumerAndConsumerWalleModel
    {
        public ETLConsumerModel? Consumer { get; set; }
        public ETLConsumerWalletModel? Wallet { get; set; }
    }
}
