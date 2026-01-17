using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.User.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services
{
    public interface IConsumerCohortEventProcessor : IEventDtoProcessor<CohortEventDto>
    {
    }
}
