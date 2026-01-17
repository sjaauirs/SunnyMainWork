using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using System.Text.Json.Serialization;

namespace SunnyRewards.Helios.Wallet.Core.Domain.Dtos
{
    public class ConsumerWalletDto : XminBaseDto
    {
        public long ConsumerWalletId { get; set; }
        public long WalletId { get; set; }
        public string? TenantCode { get; set; }
        public string? ConsumerCode { get; set; }
        public string? ConsumerRole { get; set; }
        public decimal EarnMaximum { get; set; }
        
        public DateTime CreateTs { get; set; }
        
        [JsonIgnore]
        public string? CreateUser { get; set; }=string.Empty;   
    }
}
