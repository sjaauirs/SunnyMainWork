using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Wallet.Core.Domain.Dtos
{
    public class CreateTransactionsResponseDto: BaseResponseDto
    {
        public long TransactionDetailId { get; set; }
    }

}
