using Sunny.Benefits.Bff.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Dtos;

namespace Sunny.Benefits.Bff.Infrastructure.Services.Interfaces
{
    public interface IConsumerService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="consumerRequestDto"></param>
        /// <returns></returns>
        Task<ConsumerResponseDto> UpdateConsumerAsync(long consumerId, ConsumerRequestDto consumerRequestDto);

        Task<ConsumerResponseDto> DeactivateConsumer(DeactivateConsumerRequestDto consumerRequestDto);

        Task<ConsumerResponseDto> ReactivateConsumer(ReactivateConsumerRequestDto consumerRequestDto);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="consumerAttributesRequestDto"></param>
        /// <returns></returns>
        Task<ConsumerAttributesResponseDto> ConsumerAttributes(ConsumerAttributesRequestDto consumerAttributesRequestDto);

        Task<GetConsumerResponseDto> GetConsumer(GetConsumerRequestDto consumerSummaryRequestDto);

        Task<BaseResponseDto> UpdateConsumerSubscriptionStatus(ConsumerSubscriptionStatusRequestDto requestDto);
    }

}
