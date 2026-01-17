using SunnyRewards.Helios.Task.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Task.Infrastructure.Services.Interface
{
    public interface ITaskRewardCollectionService
    {
        Task<ExportTaskRewardCollectionResponseDto> ExportTaskRewardCollection(ExportTaskRewardCollectionRequestDto requestDto);

    }
}
