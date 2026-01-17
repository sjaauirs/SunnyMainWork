using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using System.Runtime.Serialization;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces
{
    public interface IEventDtoProcessor
    {
        Task<bool> ProcessEvent(object eventDto);
    }

    public interface IEventDtoProcessor<T> : IEventDtoProcessor
    {
        Task<bool> ProcessEvent(EventDto<T> eventDto);
    }
}
