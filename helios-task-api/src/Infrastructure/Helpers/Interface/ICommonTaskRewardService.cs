using SunnyRewards.Helios.Task.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Task.Infrastructure.Helpers.Interface
{
    public interface ICommonTaskRewardService
    {
        System.Threading.Tasks.Task RecurrenceTaskProcess(TaskRewardDetailDto taskRewardDetailDto);
        Task<List<TaskRewardDetailDto>> GetAvailableTasksAsync(List<TaskRewardDetailDto> taskRewardDetailDtos, TaskRewardCollectionRequestDto requestDto);
    }
}
