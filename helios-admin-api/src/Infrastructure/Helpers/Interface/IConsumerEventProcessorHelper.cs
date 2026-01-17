using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Infrastructure.Helpers.Interface
{
    public interface IConsumerEventProcessorHelper
    {
        Task<bool> ProcessEventAsync<T>(EventDto<T> eventRequest, Dictionary<string, object> argInstances, string eventHandlerName);
    }
}
