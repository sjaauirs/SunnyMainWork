using SunnyBenefits.Fis.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;

namespace Sunny.Benefits.Bff.Core.Domain.Dtos
{
    public class ConsumerSummaryResponseDto : BaseResponseDto
    {
        public GetConsumerByEmailResponseDto? consumerInfo { get; set; }
        public GetTenantResponseDto? TenantInfo { get; set; }
        public WalletResponseDto? WalletInfo { get; set; }
        public bool HasCompletedMembershipAction { get; set; }
        public bool HasPendingMembershipAction { get; set; }
        public List<TaskRewardDetailDto> MembershipTaskRewards { get; set; } = new List<TaskRewardDetailDto>();
        public TenantAccountDto? TenantAccountInfo { get; set; }
        public IDictionary<string, DateTime?> HealthMetricsQueryStartTsMap { get; set; } = new Dictionary<string, DateTime?>();
        public string? CardIssueStatus { get; set; }
    }
}
