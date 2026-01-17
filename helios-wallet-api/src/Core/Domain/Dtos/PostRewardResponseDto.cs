using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Wallet.Core.Domain.Dtos
{
    public class PostRewardResponseDto : BaseResponseDto
    {
        public TransactionDto? SubEntry { get; set; }
        public TransactionDto? AddEntry { get; set; }
        public TransactionDetailDto? TransactionDetail { get; set; }
        public ConsumerTaskRewardInfoDto? ConsumerTaskRewardInfo { get; set; }
    }

    public class PostResponseMultiTransactionDto : BaseResponseDto
    {
        public List<PostRewardResponseDto>? PostRewardResponses { get; set; }

    }
}
