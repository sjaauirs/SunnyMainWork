using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces
{
    public interface ISagaStep
    {
        Task<BaseResponseDto> ExecuteAsync();
        Task<BaseResponseDto> CompensateAsync();
    }
}
