using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces
{
    public interface IConsumerTaskEventProcesser
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventRequest"></param>
        /// <returns></returns>
        Task<bool> ProcessEvent(EventDto<ConsumerTaskEventDto> eventRequest);
    }
}
