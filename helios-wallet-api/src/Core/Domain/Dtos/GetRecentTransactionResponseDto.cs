using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Wallet.Core.Domain.Dtos
{
    public class GetRecentTransactionResponseDto : BaseResponseDto
    {
        public GetRecentTransactionResponseDto()
        {
            Transactions = new List<TransactionEntryDto>();
        }
        public List<TransactionEntryDto> Transactions { get; set; }
    }
}
