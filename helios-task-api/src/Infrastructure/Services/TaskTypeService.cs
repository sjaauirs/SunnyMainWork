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
    public class TaskTypeService : BaseService, ITaskTypeService
    {

        private readonly ILogger<TaskTypeService> _tasktypeServiceLogger;
        private readonly IMapper _mapper;
        private readonly ITaskTypeRepo _taskTypeRepo;
        const string className = nameof(TaskTypeService);

        public TaskTypeService(ILogger<TaskTypeService> tasktypeServiceLogger, IMapper mapper, ITaskTypeRepo taskTypeRepo
            )
        {
            _tasktypeServiceLogger = tasktypeServiceLogger;
            _mapper = mapper;
            _taskTypeRepo = taskTypeRepo;


        }

        /// <summary>
        /// Retrieves a list of task types from the repository and returns them in a standardized response format.
        /// </summary>
        /// <returns>
        /// A <see cref="ListResponseDto{TaskTypeDto}"/> containing the task type data or error details.
        /// </returns>
        public async Task<TaskTypesResponseDto> GetTaskTypesAsync()
        {
            const string methodName = nameof(GetTaskTypesAsync);
            try
            {
                var result = await _taskTypeRepo.FindAsync(x => x.DeleteNbr == 0);

                if (result == null || result.Count == 0)
                {
                    _tasktypeServiceLogger.LogError("{ClassName}.{MethodName}: No task type was found. Error Code: {ErrorCode}", className, methodName, StatusCodes.Status404NotFound);
                    return new TaskTypesResponseDto
                    {
                        ErrorMessage = "No task type was found."
                    };
                }

                return new TaskTypesResponseDto
                {
                    TaskTypes = _mapper.Map<IList<TaskTypeDto>>(result)
                };
            }
            catch (Exception ex)
            {
                _tasktypeServiceLogger.LogError(ex, "{ClassName}.{MethodName}: Error Message: {Message}, Error Code: {ErrorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);

                return new TaskTypesResponseDto
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskTypeId"></param>
        /// <returns></returns>
        public async Task<TaskTypeResponseDto> GetTaskTypeById(long taskTypeId)
        {
            const string methodName = nameof(GetTaskTypeById);
            var response = new TaskTypeResponseDto();
            try
            {
                if (taskTypeId <= 0)
                {
                    _tasktypeServiceLogger.LogError("{className}.{methodName}: for given taskTypeId is Zero or less  then Zero: {taskTypeId},Error Code:{errorCode}", className, methodName, taskTypeId, StatusCodes.Status404NotFound);
                    return new TaskTypeResponseDto()
                    {
                        ErrorCode = StatusCodes.Status404NotFound
                    };
                }
                var responseTasktypeModel = await _taskTypeRepo.FindOneAsync(x => x.TaskTypeId == taskTypeId && x.DeleteNbr == 0);

                if (responseTasktypeModel != null)
                {
                    var responseDto = _mapper.Map<TaskTypeDto>(responseTasktypeModel);
                    _tasktypeServiceLogger.LogInformation("{className}.{methodName}: successfully retrieved data from  GetTaskTypeById API for taskTypeId: {taskTypeId}", className, methodName, taskTypeId);
                    response.TaskTypeId = responseDto.TaskTypeId;
                    response.TaskTypeCode = responseDto.TaskTypeCode;
                    response.TaskTypeName = responseDto.TaskTypeName;
                    response.TaskTypeDescription = responseDto.TaskTypeDescription;

                }
                return response;
            }

            catch (Exception ex)
            {
                _tasktypeServiceLogger.LogError(ex, "{className}.{methodName}: ERROR Msg:{msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return response;
            }
        }

        public async Task<TaskTypeResponseDto> GetTaskTypeByTypeCode(string taskTypeCode)
        {
            const string methodName = nameof(GetTaskTypeByTypeCode);
            var response = new TaskTypeResponseDto();
            try
            {
                if (string.IsNullOrEmpty(taskTypeCode))
                {
                    _tasktypeServiceLogger.LogError("{className}.{methodName}: for given taskTypeCode is null or empty: {taskTypeCode},Error Code:{errorCode}", className, methodName, taskTypeCode, StatusCodes.Status404NotFound);
                    return new TaskTypeResponseDto()
                    {
                        ErrorCode = StatusCodes.Status404NotFound
                    };
                }
                var responseTasktypeModel = await _taskTypeRepo.FindOneAsync(x => x.TaskTypeCode == taskTypeCode && x.DeleteNbr == 0);

                if (responseTasktypeModel != null)
                {
                    var responseDto = _mapper.Map<TaskTypeDto>(responseTasktypeModel);
                    _tasktypeServiceLogger.LogInformation("{className}.{methodName}: successfully retrieved data from  GetTaskTypeById API for taskTypeCode: {taskTypeCode}", className, methodName, taskTypeCode);
                    response.TaskTypeId = responseDto.TaskTypeId;
                    response.TaskTypeCode = responseDto.TaskTypeCode;
                    response.TaskTypeName = responseDto.TaskTypeName;
                    response.TaskTypeDescription = responseDto.TaskTypeDescription;

                }
                return response;
            }

            catch (Exception ex)
            {
                _tasktypeServiceLogger.LogError(ex, "{className}.{methodName}: ERROR Msg:{msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return response;
            }
        }

        /// <summary>
        /// Imports a list of task types from the request DTO. 
        /// Skips existing task types and logs errors for individual failures.
        /// </summary>
        /// <param name="taskTypeRequestDto">The import request containing task types.</param>
        /// <returns>Import result with error messages if any failures occur.</returns>
        public async Task<ImportTaskTypeResponseDto> ImportTaskTypesAsync(ImportTaskTypeRequestDto taskTypeRequestDto)
        {
            const string methodName = nameof(ImportTaskTypesAsync);
            var responseDto = new ImportTaskTypeResponseDto();
            var errorMessages = new StringBuilder();
            _tasktypeServiceLogger.LogInformation("{ClassName}.{MethodName} - Import started with {Count} task types.", 
                className,methodName, taskTypeRequestDto.TaskTypes?.Count ?? 0);
            foreach (var taskType in taskTypeRequestDto.TaskTypes!)
            {
                try
                {
                    var existingTaskType = await _taskTypeRepo.FindOneAsync(x =>
                        x.TaskTypeCode == taskType.TaskTypeCode && x.DeleteNbr == 0);

                    if (existingTaskType != null)
                    {
                        _tasktypeServiceLogger.LogInformation("{ClassName}.{MethodName} - TaskType '{TaskTypeCode}' already exists. Skipping.",
                            className,methodName, taskType.TaskTypeCode);
                        continue;
                    }

                    await CreateTaskType(taskType);
                    _tasktypeServiceLogger.LogInformation("{ClassName}.{MethodName} - TaskType '{TaskTypeCode}' imported successfully.",
                        className, methodName, taskType.TaskTypeCode);
                }
                catch (Exception ex)
                {
                    var errorMsg = $"Failed to import TaskType '{taskType.TaskTypeCode}': {ex.Message}";
                    _tasktypeServiceLogger.LogError(ex, "{ClassName}.{MethodName} - {ErrorMessage}",className, methodName, errorMsg);
                    errorMessages.AppendLine(errorMsg);
                }
            }
            if (errorMessages.Length > 0)
            {
                responseDto.ErrorCode = StatusCodes.Status206PartialContent;
                responseDto.ErrorMessage = errorMessages.ToString();
                _tasktypeServiceLogger.LogWarning("{ClassName}.{MethodName} - Partial import completed with errors.",className, methodName);
            }

            _tasktypeServiceLogger.LogInformation("{ClassName}.{MethodName} - Import completed successfully.",className, methodName);
            return responseDto;
        }

        /// <summary>
        /// Creates a new task type entry in the database.
        /// </summary>
        /// <param name="typeDto">The DTO containing task type details.</param>
        /// <returns>The created TaskTypeModel instance.</returns>
        private async Task<TaskTypeModel> CreateTaskType(TaskTypeDto typeDto)
        {
            var methodName = nameof(CreateTaskType);

            var taskTypeModel = _mapper.Map<TaskTypeModel>(typeDto);
            taskTypeModel.CreateTs = DateTime.UtcNow;
            taskTypeModel.CreateUser = Constant.ImportUser;

            await _taskTypeRepo.CreateAsync(taskTypeModel);

            _tasktypeServiceLogger.LogInformation("{ClassName}.{MethodName} - TaskType '{TaskTypeCode}' created.",
                className, methodName, taskTypeModel.TaskTypeCode);
            return taskTypeModel;
        }
    }
}
