using SunnyRewards.Helios.ETL.Core.Domain.Models;

namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos
{
    public class WalletDetailsDto
    {
        public ETLWalletModel WalletModel { get; set; } = null!;
        public ETLConsumerWalletModel ConsumerWalletModel { get; set; } = null!;
    }
}
