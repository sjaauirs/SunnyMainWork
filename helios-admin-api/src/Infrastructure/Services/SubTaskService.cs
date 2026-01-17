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
    public class SubTaskService :ISubtaskService
    {
        public readonly ILogger<SubTaskService> _logger;
        public readonly ITaskClient _taskClient;
        public const string className = nameof(SubTaskService);

        public SubTaskService(ILogger<SubTaskService> logger, ITaskClient taskClient)
        {
            _logger = logger;
            _taskClient = taskClient;
        }
        public async Task<BaseResponseDto> CreateSubTask(SubtaskRequestDto requestDto)
        {
            const string methodName = nameof(CreateSubTask);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName}: Create SubTask process started for ParentTaskrewardCode: {TaskCode}", className, methodName, requestDto.ParentTaskRewardCode);

                var taskResponse = await _taskClient.Post<BaseResponseDto>(Constant.CreateSubTaskAPIUrl, requestDto);
                if (taskResponse.ErrorCode != null)
                {
                    _logger.LogWarning("{ClassName}.{MethodName}: Error occurred while creating SubTask, requestData: {requestData}, ErrorCode: {ErrorCode}", className, methodName, requestDto.ToJson(), taskResponse.ErrorCode);
                    return taskResponse;
                }
                _logger.LogInformation("{ClassName}.{MethodName}: SubTask created successfully, ParentTaskrewardCode: {TaskCode}", className, methodName, requestDto.ParentTaskRewardCode);
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
