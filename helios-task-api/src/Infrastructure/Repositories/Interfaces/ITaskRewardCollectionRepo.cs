using SunnyRewards.Helios.Common.Core.Repositories.Interfaces;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Models;

namespace SunnyRewards.Helios.Task.Infrastructure.Repositories.Interfaces
{
    public interface ITaskRewardCollectionRepo:IBaseRepo<TaskRewardCollectionModel>
    {
        Task<ExportTaskRewardCollectionResponseDto> GetTaskRewardCollections(string tenantCode);
    }
}
