using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Task.Infrastructure.Services.Interface
{
    public interface IImportTaskRewardCollectionService
    {
        Task<BaseResponseDto> ImportTaskRewardCollection(ImportTaskRewardCollectionRequestDto taskRewardCollections);
    }
}
