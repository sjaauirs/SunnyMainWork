using SunnyRewards.Helios.User.Core.Domain.Dtos;

namespace Sunny.Benefits.Bff.Infrastructure.Services.Interfaces
{
    public interface IConsumerActivityService
    {
        /// <summary>
        /// Handles the creation of a consumer activity.
        /// </summary>
        /// <param name="consumerActivityRequestDto">
        /// The DTO containing details of the consumer activity, such as TenantCode, ConsumerCode, ActivitySource, ActivityType, and ActivityJson.
        /// </param>
        /// <returns>
        /// A <see cref="ConsumerActivityResponseDto"/> object indicating the success of the operation.
        /// </returns>
        Task<ConsumerActivityResponseDto> CreateConsumerActivityAsync(ConsumerActivityRequestDto consumerActivityRequestDto);
    }
}
