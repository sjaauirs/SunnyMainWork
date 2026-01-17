using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Admin.Core.Domain.Constants;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services
{
    public class TaskRewardTypeService : ITaskRewardTypeService
    {
        public readonly ILogger<ITaskRewardTypeService> _logger;
        public readonly ITaskClient _taskClient;
        public const string className = nameof(TaskService);

        public TaskRewardTypeService(ILogger<ITaskRewardTypeService> logger, ITaskClient taskClient)
        {
            _logger = logger;
            _taskClient = taskClient;
        }

        /// <summary>
        /// Retrieves a list of tasks from the tasks API and returns them in a standardized response format.
        /// </summary>
        /// <returns></returns>
        public async Task<TaskRewardTypesResponseDto> GetTaskRewardTypesAsync()
        {
            const string methodName = nameof(GetTaskRewardTypesAsync);
            try
            {
                return await _taskClient.Get<TaskRewardTypesResponseDto>(Constant.TaskRewardTypesApiUrl, new Dictionary<string, long>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: Error Message: {Message}, Error Code: {ErrorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);

                return new TaskRewardTypesResponseDto
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = ex.Message
                };
            }
        }
    }
}
