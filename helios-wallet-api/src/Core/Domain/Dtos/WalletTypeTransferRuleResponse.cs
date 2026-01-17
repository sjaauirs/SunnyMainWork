using SunnyRewards.Helios.Common.Core.Domain.Dtos;
namespace SunnyRewards.Helios.Wallet.Core.Domain.Dtos
{
    public class WalletTypeTransferRule 
    {  
        public WalletDto? TargetConsumerWallet { get; set; }
        public WalletTypeDto? TargetConsumerWalletType { get; set; }
        public WalletDto? OverflowedConsumerWallet { get; set; }
        public WalletTypeDto? OverflowedConsumerWalletType { get; set; }
        public double TransferRatio { get; set; }
    }

    public class MaxWalletTransferRuleResponseDto: BaseResponseDto
    {
        public bool WalletOverFlowed { get; set; }
        public List<WalletTypeTransferRule> walletTypeTransferRules { get; set; } = new();
    }
}

