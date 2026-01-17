using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Common.Core.Services;
using SunnyRewards.Helios.Task.Core.Domain.Constants;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Models;
using SunnyRewards.Helios.Task.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Task.Infrastructure.Services.Interface;
using System.Text;

namespace SunnyRewards.Helios.Task.Infrastructure.Services
{
    public class TaskRewardTypeService : BaseService, ITaskRewardTypeService
    {
        private readonly ILogger<ITaskRewardTypeService> _logger;
        private readonly IMapper _mapper;
        private readonly ITaskRewardTypeRepo _taskRewardTypeRepo;

        const string className = nameof(TaskRewardTypeService);

        public TaskRewardTypeService(ILogger<ITaskRewardTypeService> logger, IMapper mapper, ITaskRewardTypeRepo taskRewardTypeRepo)
        {
            _logger = logger;
            _mapper = mapper;
            _taskRewardTypeRepo = taskRewardTypeRepo;
        }

        /// <summary>
        /// Retrieves a list of task rewards from the repository and returns them in a standardized response format.
        /// </summary>
        /// <returns></returns>
        public async Task<TaskRewardTypesResponseDto> GetTaskRewardTypesAsync()
        {
            const string methodName = nameof(GetTaskRewardTypesAsync);
            try
            {
                var result = await _taskRewardTypeRepo.FindAsync(x => x.DeleteNbr == 0);

                if (result == null || result.Count == 0)
                {
                    _logger.LogError("{ClassName}.{MethodName}: No task reward type was found. Error Code: {ErrorCode}", className, methodName, StatusCodes.Status404NotFound);
                    return new TaskRewardTypesResponseDto
                    {
                        ErrorMessage = "No task reward type was found."
                    };
                }

                return new TaskRewardTypesResponseDto
                {
                    TaskRewardTypes = _mapper.Map<IList<TaskRewardTypeDto>>(result)
                };
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

        /// <summary>
        /// UpdateTaskRewardTypeAsync
        /// </summary>
        /// <param name="rewardTypeId"></param>
        /// <param name="taskRewardTypeRequestDto"></param>
        /// <returns></returns>
        public async Task<TaskRewardTypeResponseDto> UpdateTaskRewardTypeAsync(long rewardTypeId, TaskRewardTypeRequestDto taskRewardTypeRequestDto)
        {
            const string methodName = nameof(UpdateTaskRewardTypeAsync);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName}: Started processing for RewardTypeId: {RewardTypeId}", className, methodName, rewardTypeId);

                var taskRewardTypeModel = await _taskRewardTypeRepo.FindOneAsync(x => x.RewardTypeId == rewardTypeId && x.DeleteNbr == 0);

                if (taskRewardTypeModel == null)
                {
                    return new TaskRewardTypeResponseDto() { TaskRewardType = _mapper.Map<TaskRewardTypeDto>(taskRewardTypeRequestDto), ErrorCode = StatusCodes.Status404NotFound, ErrorMessage = $"No task reward type found for given task reward type id: {rewardTypeId}" };
                }

                taskRewardTypeModel.RewardTypeName = taskRewardTypeRequestDto.RewardTypeName;
                taskRewardTypeModel.RewardTypeDescription = taskRewardTypeRequestDto.RewardTypeDescription;
                taskRewardTypeModel.RewardTypeCode = taskRewardTypeRequestDto.RewardTypeCode;
                taskRewardTypeModel.UpdateTs = DateTime.UtcNow;

                await _taskRewardTypeRepo.UpdateAsync(taskRewardTypeModel);

                _logger.LogInformation("{ClassName}.{MethodName}: Ended Successfully.", className, methodName);

                return new TaskRewardTypeResponseDto() { TaskRewardType = _mapper.Map<TaskRewardTypeDto>(taskRewardTypeModel) };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: Error Message: {Message}, Error Code: {ErrorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);

                return new TaskRewardTypeResponseDto() { TaskRewardType = _mapper.Map<TaskRewardTypeDto>(taskRewardTypeRequestDto), ErrorCode = StatusCodes.Status409Conflict, ErrorMessage = ex.Message };
            }
        }

        /// <summary>
        /// Imports a list of reward types, skipping any that already exist.
        /// </summary>
        /// <param name="requestDto">The request DTO containing reward types to import.</param>
        /// <returns>Response DTO with success or partial failure status.</returns>
        public async Task<ImportRewardTypeResponseDto> ImportRewardTypesAsync(ImportRewardTypeRequestDto rewardTypeRequestDto)
        {
            const string methodName = nameof(ImportRewardTypesAsync);
            var responseDto = new ImportRewardTypeResponseDto();
            var errorMessages = new StringBuilder();

            _logger.LogInformation("{ClassName}.{MethodName} - Starting import of {Count} reward types.", className,methodName, rewardTypeRequestDto.RewardTypes.Count);

            foreach (var rewardTypeDto in rewardTypeRequestDto.RewardTypes)
            {
                try
                {
                    var existingRewardType = await _taskRewardTypeRepo.FindOneAsync(x =>
                        x.RewardTypeCode == rewardTypeDto.RewardTypeCode && x.DeleteNbr == 0);

                    if (existingRewardType != null)
                    {
                        _logger.LogInformation("{ClassName}.{MethodName} - RewardType '{Code}' already exists. Skipping.", className,methodName, rewardTypeDto.RewardTypeCode);
                        continue;
                    }

                    await CreateRewardTypeAsync(rewardTypeDto);
                    _logger.LogInformation("{ClassName}.{MethodName} - RewardType '{Code}' created successfully.", className, methodName, rewardTypeDto.RewardTypeCode);
                }
                catch (Exception ex)
                {
                    var msg = $"Failed to import RewardType '{rewardTypeDto.RewardTypeCode}': {ex.Message}";
                    _logger.LogError(ex, "{ClassName}.{MethodName} - {Error}", className,methodName, msg);
                    errorMessages.AppendLine(msg);
                }
            }

            if (errorMessages.Length > 0)
            {
                responseDto.ErrorCode = StatusCodes.Status206PartialContent;
                responseDto.ErrorMessage = errorMessages.ToString();
                _logger.LogWarning("{ClassName}.{MethodName} - Import completed with some errors.",className, methodName);
            }

            _logger.LogInformation("{ClassName}.{MethodName} - All reward types imported successfully.", className, methodName);

            return responseDto;
        }

        /// <summary>
        /// Creates a new reward type in the database.
        /// </summary>
        /// <param name="dto">DTO representing the reward type to be created.</param>
        private async Task<TaskRewardTypeModel> CreateRewardTypeAsync(TaskRewardTypeDto dto)
        {
            var model = _mapper.Map<TaskRewardTypeModel>(dto);
            model.CreateTs = DateTime.UtcNow;
            model.CreateUser = Constant.ImportUser;

            await _taskRewardTypeRepo.CreateAsync(model);
            return model;
        }
    }
}


