using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;

namespace Sunny.Benefits.Bff.Core.Domain.Dtos
{
    public class TransactionBySectionResponseDto : BaseResponseDto
    {
        public TransactionBySectionResponseDto()
        {
            Transaction = new Dictionary<string, List<TransactionEntryDto>>();
            TaskReward = new Dictionary<string, IEnumerable<TaskRewardDetailDto>>();
        }
        public Dictionary<string, List<TransactionEntryDto>>? Transaction { get; set; }

        public Dictionary<string, IEnumerable<TaskRewardDetailDto>>? TaskReward { get; set; }
    }
}
