using System.ComponentModel.DataAnnotations;

namespace SunnyRewards.Helios.Wallet.Core.Domain.Dtos
{
    public class CreateTransactionsRequestDto
    {
        [Required]
        public string ConsumerCode { get; set; } = null!;
        public TransactionDetailDto? TransactionDetail { get; set; }
        public long AddedWalletId { get; set; }
        public long RemovedWalletId { get; set; }
        public decimal TransactionAmount { get; set; }
    }

    public class RemoveTransactionsRequestDto
    {
        public long TransactionDetailId { get; set; }
    }

}
