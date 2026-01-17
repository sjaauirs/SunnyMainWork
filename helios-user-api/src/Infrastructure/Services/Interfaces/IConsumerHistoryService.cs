using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Dtos;

namespace SunnyRewards.Helios.User.Infrastructure.Services.Interfaces
{
    public interface IConsumerHistoryService
    {
        Task<BaseResponseDto> InsertConsumerHistory(IList<ConsumerDto> consumers);
    }
}