using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Admin.Core.Domain.Constants;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services
{
    public class TaskExternalMappingService : ITaskExternalMappingService
    {
        public readonly ILogger<TaskExternalMappingService> _logger;
        public readonly ITaskClient _taskClient;
        public const string className = nameof(TaskExternalMappingService);

        public TaskExternalMappingService(ILogger<TaskExternalMappingService> logger, ITaskClient taskClient)
        {
            _logger = logger;
            _taskClient = taskClient;
        }
        public async Task<BaseResponseDto> CreateTaskExternalMapping(TaskExternalMappingRequestDto requestDto)
        {
            const string methodName = nameof(CreateTaskExternalMapping);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName}: Create Task process started for TaskExternalMappingRequest: {TaskCode}", className, methodName, requestDto.TaskExternalCode);

                var taskResponse = await _taskClient.Post<BaseResponseDto>(Constant.CreateTaskExternalMappingRequest, requestDto);
                if (taskResponse.ErrorCode != null)
                {
                    _logger.LogWarning("{ClassName}.{MethodName}: Error occurred while creating TaskExternalMappingRequest, request: {requestdata}, ErrorCode: {ErrorCode}", className, methodName, requestDto.ToJson(), taskResponse.ErrorCode);
                    return taskResponse;
                }
                _logger.LogInformation("{ClassName}.{MethodName}: Task created successfully, TaskExternalMappingRequest: {TaskCode}", className, methodName, requestDto.TaskExternalCode);
                return new BaseResponseDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: Exception occurred while creating Sub Task. ErrorMessage: {ErrorMessage}, StackTrace: {StackTrace},", className, methodName, ex.Message, ex.StackTrace);
                throw;
            }

        }

    }
}
