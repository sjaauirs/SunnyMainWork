using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Wallet.Core.Domain.Dtos
{
    public class GetWalletTransactionResponseDto : BaseResponseDto
    {
        public List<WalletDto> Wallets { get; set; } = [];
        public List<TransactionEntryDto> Transactions { get; set; } = [];
    }
}
