using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Task.Infrastructure.Services.Interface
{
    public interface ITermsOfServiceService
    {
        Task<BaseResponseDto> CreateTermsOfService(CreateTermsOfServiceRequestDto createTermsOfServiceRequestDto);
    }
}
