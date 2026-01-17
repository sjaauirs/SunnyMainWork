using Sunny.Benefits.Bff.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;

namespace Sunny.Benefits.Bff.Infrastructure.Services.Interfaces
{
    public interface IConsumerSummaryService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="consumerSummaryRequestDto"></param>
        /// <returns>ConsumerSummaryResponseDto</returns>
        Task<ConsumerSummaryResponseDto> GetConsumerSummary(ConsumerSummaryRequestDto consumerSummaryRequestDto);
        Task<GetConsumerByEmailResponseDto> GetConsumerDetails(ConsumerSummaryRequestDto consumerSummaryRequestDto);
    }
}
