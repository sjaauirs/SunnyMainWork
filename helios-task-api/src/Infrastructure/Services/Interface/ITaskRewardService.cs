using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Task.Infrastructure.Services.Interface
{
    public interface ITaskRewardService
    {
        /// <summary>
        /// Retrieves a list of task rewards from the repository and returns them in a standardized response format.
        /// </summary>
        /// <returns></returns>
        Task<TaskRewardsResponseDto> GetTaskRewardsAsync();

        /// <summary>
        /// UpdateTaskRewardAsync
        /// </summary>
        /// <param name="taskRewardId"></param>
        /// <param name="taskRewardRequestDto"></param>
        /// <returns></returns>
        Task<TaskRewardResponseDto> UpdateTaskRewardAsync(long taskRewardId, TaskRewardRequestDto taskRewardRequestDto,bool isImport=false);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskRewardRequestDto"></param>
        /// <returns></returns>
        Task<FindTaskRewardResponseDto> GetTaskRewardDetails(FindTaskRewardRequestDto taskRewardRequestDto);

        /// <summary>
        /// Returns one Task Reward with all details of the Task matching the given TaskRewardCode
        /// </summary>
        /// <param name="taskRewardRequestDto"></param>
        /// <returns></returns>
        Task<GetTaskRewardByCodeResponseDto> GetTaskRewardByCode(GetTaskRewardByCodeRequestDto taskRewardRequestDto);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rewardTypeRequestDto"></param>
        /// <returns></returns>
        Task<RewardTypeResponseDto> RewardType(RewardTypeRequestDto rewardTypeRequestDto);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rewardTypeCodeRequestDto"></param>
        /// <returns></returns>
        Task<RewardTypeResponseDto> RewardTypeCode(RewardTypeCodeRequestDto rewardTypeCodeRequestDto);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="getTaskByTenantCodeRequestDto"></param>
        /// <returns></returns>
        Task<GetTaskByTenantCodeResponseDto> GetAllTaskByTenantCode(GetTaskByTenantCodeRequestDto getTaskByTenantCodeRequestDto);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskRewardId"></param>
        /// <returns></returns>
        Task<PeriodDescriptorResponseDto> CurrentPeriodDescriptor(long taskRewardId);

        Task<FindTaskRewardResponseDto> GetTaskRewards(FindTaskRewardRequestDto taskRewardRequestDto, bool includeSubtask);
        /// <summary>
        /// Creates new Task reward
        /// </summary>
        /// <param name="createTaskRewardRequestDto">Request contains data for Create Task Reward</param>
        /// <returns></returns>
        Task<BaseResponseDto> CreateTaskReward(CreateTaskRewardRequestDto createTaskRewardRequestDto);

        /// <summary>
        /// Retrives the tasks and taskrewards with tenant code
        /// </summary>
        /// <param name="getTaskRewardRequestDto"></param>
        /// <returns>Returns List of taskrewards matching with input tenantcode and List of tasks matching taskid in taskrewards</returns>
        Task<GetTasksAndTaskRewardsResponseDto> GetTasksAndTaskRewards(GetTasksAndTaskRewardsRequestDto getTasksAndTaskRewardsRequestDto);

        /// <summary>
        /// Retrieves the list of task reward details for a given tenant code.
        /// </summary>
        /// <param name="tenantCode"></param>
        /// <returns></returns>
        Task<TaskRewardDetailsResponseDto> GetTaskRewardDetails(string tenantCode, string? taskExternalCode, string languageCode);
        /// <summary>
        /// Retrieves the task reward collection details based on the provided request.
        /// </summary>
        /// <param name="taskRewardCollectionRequestDto">Request DTO containing TenantCode and TaskRewardCode.</param>
        /// <returns>Returns TaskRewardCollectionResponseDto containing the task reward details.</returns>
        Task<TaskRewardCollectionResponseDto> GetTaskRewardCollection(TaskRewardCollectionRequestDto taskRewardCollectionRequestDto);

        /// <summary>
        /// Retrieves the list of task health rewards for a given tenant code.
        /// </summary>
        /// <param name="tenantCode"></param>
        /// <returns></returns>
        Task<IList<TaskRewardDto>> GetHealthTaskRewards(string tenantCode);
        Task<AdventureTaskCollectionResponseDto> GetAdventuresAndTaskCollections(AdventureTaskCollectionRequestDto requestDto);
    }
}
