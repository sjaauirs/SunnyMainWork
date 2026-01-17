using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Admin.Core.Domain.Constants;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services
{
    public class TaskTypeService : ITaskTypeService
    {
        public readonly ILogger<TaskTypeService> _logger;
        public readonly ITaskClient _taskClient;
        public const string className = nameof(TaskService);

        public TaskTypeService(ILogger<TaskTypeService> logger, ITaskClient taskClient)
        {
            _logger = logger;
            _taskClient = taskClient;
        }

        /// <summary>
        /// Retrieves a list of tasks from the TaskType API and returns them in a standardized response format.
        /// </summary>
        /// <returns></returns>
        public async Task<TaskTypesResponseDto> GetTaskTypesAsync()
        {
            const string methodName = nameof(GetTaskTypesAsync);
            try
            {
                return await _taskClient.Get<TaskTypesResponseDto>(Constant.TaskTypesApiUrl, new Dictionary<string, long>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: Error Message: {Message}, Error Code: {ErrorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);

                return new TaskTypesResponseDto
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = ex.Message
                };
            }
        }
    }
}

