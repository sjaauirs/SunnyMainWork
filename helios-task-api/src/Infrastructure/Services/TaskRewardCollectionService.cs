using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Task.Infrastructure.Services.Interface;

namespace SunnyRewards.Helios.Task.Infrastructure.Services
{
    public class TaskRewardCollectionService : ITaskRewardCollectionService
    {
        public const string className = nameof(TaskRewardCollectionService);

        private readonly ITaskRewardCollectionRepo _taskRewardCollectionRepo;
        private readonly ILogger<TaskRewardCollectionService> _logger;

        public TaskRewardCollectionService(ITaskRewardCollectionRepo taskRewardCollectionRepo, ILogger<TaskRewardCollectionService> logger)
        {
            _taskRewardCollectionRepo = taskRewardCollectionRepo;
            _logger = logger;
        }

        public async Task<ExportTaskRewardCollectionResponseDto> ExportTaskRewardCollection(ExportTaskRewardCollectionRequestDto requestDto)
        {
            const string methodName = nameof(ExportTaskRewardCollection);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName}: Started processing with TenantCode:{TenantCode}",
                    className, methodName, requestDto.TenantCode);

                var taskRewardCollections = await _taskRewardCollectionRepo.GetTaskRewardCollections(requestDto.TenantCode);
                if(taskRewardCollections.TaskRewardCollections.Count == 0)
                {
                    return new ExportTaskRewardCollectionResponseDto 
                    {   
                        ErrorCode = StatusCodes.Status404NotFound,
                        ErrorMessage = $"TaskReward Collection Not found with given TenantCode:{requestDto.TenantCode}"
                    };
                }

                return taskRewardCollections;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: Error occured with TenantCode:{TenantCode},ErrorCode:{ErrorCode},ERROR:{Error}",
                    className, methodName, requestDto.TenantCode, StatusCodes.Status500InternalServerError, ex.Message);
                throw;
            }
        }
    }
}
