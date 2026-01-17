using SunnyRewards.Helios.Admin.Core.Domain.Models;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces
{
    public interface IEventProcessor
    {
        /// <summary>
        /// Interface for factory Method
        /// </summary>
        /// <param name="eventRequest"></param>
        Task<bool> ProcessEvent(PostEventRequestModel eventRequest);
    }
}
