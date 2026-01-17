using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces
{
    public interface IAgreementsVerifiedEventService
    {
        /// <summary>
        /// Process AgreementsVerified event - checks consumer and cohort agreement status
        /// and sets card issue status to ELIGIBLE_TO_ORDER if both are AGREEMENTS_VERIFIED
        /// </summary>
        /// <param name="agreementsVerifiedEventRequestDto">dto for agreements verified event.</param>
        /// <returns></returns>
        BaseResponseDto AgreementsVerifiedEventProcess(AgreementsVerifiedEventRequestDto agreementsVerifiedEventRequestDto);
    }
}
