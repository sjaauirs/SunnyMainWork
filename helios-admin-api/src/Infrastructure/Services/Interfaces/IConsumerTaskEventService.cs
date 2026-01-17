using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces
{
    public interface IConsumerTaskEventService
    {
        /// <summary>
        /// Check if a consumer has completed a task
        /// </summary>
        /// <param name="ConsumerTaskEventDto">dto for consumertaskEvent.</param>
        /// <returns></returns>
        BaseResponseDto ConsumerTaskEventProcess(ConsumerTaskEventRequestDto consumerTaskEventRequestDto);
    }
}
