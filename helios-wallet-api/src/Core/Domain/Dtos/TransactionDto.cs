using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using System.Text.Json.Serialization;

namespace SunnyRewards.Helios.Wallet.Core.Domain.Dtos
{
    public class TransactionDto
    {
        public long TransactionId { get; set; }
        public long WalletId { get; set; }
        public string? TransactionCode { get; set; }
        public string? TransactionType { get; set; }
        public double PreviousBalance { get; set; }
        public double TransactionAmount { get; set; }
        public double Balance { get; set; }
        public string? PrevWalletTxnCode { get; set; }
        public long TransactionDetailId { get; set; }
        public DateTime CreateTs { get; set; }

    }
}
