using SunnyRewards.Helios.Admin.Core.Domain.Models;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces
{
    public interface IEventProcessorHelper
    {
        /// <summary>
        /// Execute event
        /// </summary>
        /// <param name="eventRequest"></param>
        /// <param name="argInstances"></param>
        /// <returns></returns>
        Task<bool> ProcessEventAsync(PostEventRequestModel eventRequest, Dictionary<string, object> argInstances, string eventHandlerName);
    }
}
