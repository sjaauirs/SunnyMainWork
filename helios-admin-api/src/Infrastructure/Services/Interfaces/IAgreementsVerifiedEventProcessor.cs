using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces
{
    public interface IAgreementsVerifiedEventProcessor
    {
        /// <summary>
        /// Process AgreementsVerified event
        /// </summary>
        /// <param name="eventRequest"></param>
        /// <returns></returns>
        Task<bool> ProcessEvent(EventDto<AgreementsVerifiedEventDto> eventRequest);
    }
}
