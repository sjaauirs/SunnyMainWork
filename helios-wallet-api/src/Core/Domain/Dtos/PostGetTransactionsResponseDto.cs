using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Wallet.Core.Domain.Dtos
{
    public class PostGetTransactionsResponseDto : BaseResponseDto
    {
        public PostGetTransactionsResponseDto()
        {
            Transactions = new List<TransactionEntryDto>();
        }
        public List<TransactionEntryDto> Transactions { get; set; }
    }
}
