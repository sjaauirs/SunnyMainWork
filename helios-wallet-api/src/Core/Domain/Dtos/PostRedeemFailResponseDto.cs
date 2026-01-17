using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Wallet.Core.Domain.Dtos
{
    public class PostRedeemFailResponseDto : BaseResponseDto
    {
        public TransactionDto? RevertSubEntry { get; set; }
        public TransactionDto? RevertAddEntry { get; set; }
        public TransactionDetailDto? TransactionDetail { get; set; }
        public RedemptionDto? Redemption { get; set; }
    }
}