using AutoMapper;
using FluentNHibernate.Conventions;
using log4net.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using SunnyRewards.Helios.Common.Core.Domain;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Common.Core.Services;
using SunnyRewards.Helios.Task.Core.Domain.Constants;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Models;
using SunnyRewards.Helios.Task.Infrastructure.Repositories;
using SunnyRewards.Helios.Task.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Task.Infrastructure.Services.Interface;
using static SunnyRewards.Helios.Task.Core.Domain.Dtos.SpinWheelProgressDto;

namespace SunnyRewards.Helios.Task.Infrastructure.Services
{
    public class SubTaskService : BaseService, ISubtaskService
    {
        private readonly ILogger<SubTaskService> _subtaskServiceLogger;
        private readonly ITaskRepo _taskRepo;
        private readonly ITaskRewardRepo _taskRewardRepo;
        private readonly ITaskTypeRepo _taskTypeRepo;
        private readonly ISubTaskRepo _subTaskRepo;
        private readonly IConsumerTaskRepo _consumerTaskRepo;
        private readonly IMapper _mapper;
        const string className = nameof(SubTaskService);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="subtaskServiceLogger"></param>
        /// <param name="taskRepo"></param>
        /// <param name="taskRewardRepo"></param>
        /// <param name="taskTypeRepo"></param>
        /// <param name="subTaskRepo"></param>
        /// <param name="consumerTaskRepo"></param>
        /// <param name="mapper"></param>
        public SubTaskService(ILogger<SubTaskService> subtaskServiceLogger,
            ITaskRepo taskRepo,
            ITaskRewardRepo taskRewardRepo,
            ITaskTypeRepo taskTypeRepo,
            ISubTaskRepo subTaskRepo,
            IConsumerTaskRepo consumerTaskRepo,
            IMapper mapper)
        {
            _subtaskServiceLogger = subtaskServiceLogger;
            _taskRepo = taskRepo;
            _taskRewardRepo = taskRewardRepo;
            _taskTypeRepo = taskTypeRepo;
            _subTaskRepo = subTaskRepo;
            _consumerTaskRepo = consumerTaskRepo;
            _mapper = mapper;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="consumerTaskDto"></param>
        /// <returns></returns>
        public async System.Threading.Tasks.Task CreateConsumerSubtask(UpdateConsumerTaskDto consumerTaskDto)
        {
            const string methodName = nameof(CreateConsumerSubtask);
            try
            {
                TaskRewardModel taskRewardModel = null;

                if (consumerTaskDto.TaskId > 0)
                {
                    taskRewardModel = await _taskRewardRepo.FindOneAsync(x => x.TenantCode == consumerTaskDto.TenantCode && x.TaskId == consumerTaskDto.TaskId && x.DeleteNbr == 0);
                }
                else if (!string.IsNullOrEmpty(consumerTaskDto.TaskCode))
                {
                    var taskModel = await _taskRepo.FindOneAsync(x => x.TaskCode == consumerTaskDto.TaskCode && x.DeleteNbr == 0);
                    taskRewardModel = await _taskRewardRepo.FindOneAsync(x => x.TenantCode == consumerTaskDto.TenantCode && x.TaskId == taskModel.TaskId && x.DeleteNbr == 0);
                }

                var subtask = await _subTaskRepo.FindOneAsync(x => x.ParentTaskRewardId == taskRewardModel.TaskRewardId && x.DeleteNbr == 0);

                if (subtask == null)
                {
                    _subtaskServiceLogger.LogError("{className}.{methodName}: Subtask  Not found for Task Reward Id:{taskRewardId} Error Code:{errorCode}", className, methodName, taskRewardModel.TaskRewardId, StatusCodes.Status404NotFound);
                    return;
                }
                var subtaskRewardModel = await _taskRewardRepo.FindOneAsync(x => x.TaskRewardId == subtask.ChildTaskRewardId && x.DeleteNbr == 0);

                var subtaskModel = await _taskRepo.FindOneAsync(x => x.TaskId == subtaskRewardModel.TaskId && x.DeleteNbr == 0);

                var subtaskTypeModel = await _taskTypeRepo.FindOneAsync(x => x.TaskTypeId == subtaskModel.TaskTypeId && x.DeleteNbr == 0);

                await CreateSpecificConsumerSubtask(consumerTaskDto, subtask, subtaskRewardModel, subtaskTypeModel);
            }
            catch (Exception ex)
            {
                _subtaskServiceLogger.LogError(ex, "{className}.{methodName}: - ERROR Msg:{msg}", className, methodName, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="getConsumerSubtasksRequestDto"></param>
        /// <returns></returns>
        public async Task<GetConsumerSubTaskResponseDto> GetConsumerSubtask(GetConsumerSubtasksRequestDto getConsumerSubtasksRequestDto)
        {
            GetConsumerSubTaskResponseDto consumerSubTaskResponse = new();
            const string methodName = nameof(GetConsumerSubtask);
            try
            {
                var consumerPendingSubtask = await _consumerTaskRepo.FindAsync(x => x.ConsumerCode == getConsumerSubtasksRequestDto.ConsumerCode && x.TaskStatus == Constants.InProgress && x.DeleteNbr == 0);
                if (consumerPendingSubtask.Count == 0)
                {
                    _subtaskServiceLogger.LogError("{className}.{methodName}: ConsumerCode NotFound: {consumerCode}, Error Code:{errorCode}", className, methodName, getConsumerSubtasksRequestDto.ConsumerCode, StatusCodes.Status404NotFound);
                    return new GetConsumerSubTaskResponseDto()
                    {
                        ErrorCode = StatusCodes.Status404NotFound
                    };
                }
                var consumerTaskDtoList = _mapper.Map<List<ConsumerTaskDto>>(consumerPendingSubtask);

                consumerSubTaskResponse.ConsumerTaskDto = consumerTaskDtoList.ToArray();

                _subtaskServiceLogger.LogInformation("{className}.{methodName}: Successfully retrieved data from  GetConsumerSubtask API for ConsumerCode: {ConsumerCode}", className, methodName, getConsumerSubtasksRequestDto.ConsumerCode);

                return consumerSubTaskResponse;
            }
            catch (Exception ex)
            {
                _subtaskServiceLogger.LogError(ex, "{className}.{methodName}: ERROR - msg : {msg}", className, methodName, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="updateSubtaskRequestDto"></param>
        /// <returns></returns>
        public async Task<UpdateSubtaskResponseDto> UpdateConsumerSubtask(UpdateSubtaskRequestDto updateSubtaskRequestDto)
        {
            const string methodName = nameof(UpdateConsumerSubtask);
            try
            {
                UpdateSubtaskResponseDto updateSubtaskResponseDto = new();
                int spinValue = 0;
                var consumersubtaskModel = await _consumerTaskRepo.FindOneAsync(x => x.ConsumerTaskId == updateSubtaskRequestDto.ConsumerTaskId);
                if (consumersubtaskModel.TaskStatus != Constants.InProgress)
                {
                    _subtaskServiceLogger.LogError("{className}.{methodName}: Task is In-progress State, Unable to process. Consumer Task Id:{consumerTaskId}, Error Code:{errorCode}", className, methodName, updateSubtaskRequestDto.ConsumerTaskId, StatusCodes.Status422UnprocessableEntity);
                    StatusCodes.Status422UnprocessableEntity.ToString();
                }
                var subtaskModel = await _taskRepo.FindOneAsync(x => x.TaskId == consumersubtaskModel.TaskId);
                if (!subtaskModel.IsSubtask)
                {
                    _subtaskServiceLogger.LogError("{className}.{methodName}: Task Model not IsSubtask Unable to process. Consumer Task Id:{consumerTaskId}, Error Code:{errorCode}", className, methodName, updateSubtaskRequestDto.ConsumerTaskId, StatusCodes.Status422UnprocessableEntity);
                    StatusCodes.Status422UnprocessableEntity.ToString();
                }
                var subtaskTypeModel = await _taskTypeRepo.FindOneAsync(x => x.TaskTypeId == subtaskModel.TaskTypeId && x.DeleteNbr == 0);
                if (subtaskTypeModel.TaskTypeName == Constant.TaskTypeName_SpinWheel)
                {
                    var progressdetailJsonData = Newtonsoft.Json.JsonConvert.DeserializeObject<SpinWheelProgressDtos>(consumersubtaskModel.ProgressDetail);
                    var spinwheelJsonData = progressdetailJsonData?.spinwheelProgress.spinwheelConfig.itemDefinition;
                    spinValue = Convert.ToInt32(spinwheelJsonData?[progressdetailJsonData.spinwheelProgress.finalSlotIndex].itemText);

                    var now = DateTime.UtcNow;
                    var taskStatus = updateSubtaskRequestDto.TaskStatus ?? Constants.Completed;

                    if (spinValue > 1)
                    {
                        //var completedConsumerTask = await _consumerTaskRepo.FindOneAsync(x => x.ConsumerTaskId == updateSubtaskRequestDto.CompletedTaskId &&
                        //    x.TaskStatus == Constants.Completed && x.DeleteNbr == 0);
                        var completedParentConsumerTask = await _consumerTaskRepo.FindOneAsync(x => x.ConsumerTaskId == consumersubtaskModel.ParentConsumerTaskId &&
                            x.TaskStatus == Constants.Completed && x.DeleteNbr == 0);

                        var parentTaskReward = await _taskRewardRepo.FindOneAsync(x => x.TaskId == completedParentConsumerTask.TaskId &&
                            x.TenantCode == completedParentConsumerTask.TenantCode && x.DeleteNbr == 0);
                        double additionalAmount = 0;
                        if (parentTaskReward != null && parentTaskReward?.Reward != null)
                        {
                            RewardDto taskRewardAmount = JsonConvert.DeserializeObject<RewardDto>(parentTaskReward?.Reward);

                            // Access the value of the "rewardAmount" field
                            double parentTaskrewardAmount = Convert.ToDouble(taskRewardAmount?.RewardAmount);

                            additionalAmount = (spinValue - 1) * parentTaskrewardAmount;
                        }
                        if (consumersubtaskModel != null && consumersubtaskModel.ConsumerTaskId > 0)
                        {
                            consumersubtaskModel.Notes = updateSubtaskRequestDto.Notes ?? string.Empty;
                            consumersubtaskModel.UpdateTs = now;
                            //consumersubtaskModel.UpdateUser = updateSubtaskRequestDto.UpdateUser;
                            consumersubtaskModel.TaskStatus = taskStatus;
                            if (taskStatus.ToLower() == Constants.Completed.ToLower())
                            {
                                consumersubtaskModel.TaskCompleteTs = now;
                            }
                            else
                            {
                                consumersubtaskModel.TaskCompleteTs = default;
                            }
                            await _consumerTaskRepo.UpdateAsync(consumersubtaskModel);
                            _subtaskServiceLogger.LogInformation("{className}.{methodName}: Successfully retrieved data from  CompleteSubtask API for ConsumerTaskId: {ConsumerTaskId}", className, methodName, updateSubtaskRequestDto.ConsumerTaskId);

                            updateSubtaskResponseDto.AdditionalAmount = additionalAmount;
                        }

                    }
                    else
                    {
                        consumersubtaskModel.Notes = updateSubtaskRequestDto.Notes ?? string.Empty;
                        consumersubtaskModel.UpdateTs = now;
                        //consumersubtaskModel.UpdateUser = updateSubtaskRequestDto.UpdateUser;
                        consumersubtaskModel.TaskStatus = taskStatus;
                        if (taskStatus.ToLower() == Constants.Completed.ToLower())
                        {
                            consumersubtaskModel.TaskCompleteTs = now;
                        }
                        else
                        {
                            consumersubtaskModel.TaskCompleteTs = default;
                        }
                        await _consumerTaskRepo.UpdateAsync(consumersubtaskModel);
                        _subtaskServiceLogger.LogInformation("{className}.{methodName}: Successfully retrieved data from  CompleteSubtask API for ConsumerTaskId: {ConsumerTaskId}", className, methodName, updateSubtaskRequestDto.ConsumerTaskId);
                    }
                }
                return updateSubtaskResponseDto;
            }
            catch (Exception ex)
            {
                _subtaskServiceLogger.LogError(ex, "{className}.{methodName}: ERROR - msg : {msg}", className, methodName, ex.Message);
                throw;
            }

        }

        #region PrivateMethod
        /// <summary>
        /// 
        /// </summary>
        /// <param name="consumerTaskDto"></param>
        /// <param name="subtask"></param>
        /// <param name="subtaskRewardModel"></param>
        /// <param name="subtaskTypeModel"></param>
        /// <returns></returns>
        private async System.Threading.Tasks.Task CreateSpecificConsumerSubtask(UpdateConsumerTaskDto consumerTaskDto, SubTaskModel? subtask,
            TaskRewardModel subtaskRewardModel, TaskTypeModel subtaskTypeModel)
        {
            const string methodName = nameof(CreateSpecificConsumerSubtask);
            if (subtask != null && subtaskTypeModel.TaskTypeName == Constant.TaskTypeName_SpinWheel)
            {
                ConsumerTaskModel requestConsumerTaskModel = CreateConsumerTaskPayload(consumerTaskDto, subtaskRewardModel);

                var progressDetail = await SpinnerJsonObject(subtask.ConfigJson);
                _subtaskServiceLogger.LogInformation("{className}.{methodName}: Successfully retrieved some random index for ConsumerCode: {consumerCode}", className, methodName, consumerTaskDto.ConsumerCode);

                requestConsumerTaskModel.ProgressDetail = progressDetail;

                await _consumerTaskRepo.CreateAsync(requestConsumerTaskModel);
                _subtaskServiceLogger.LogInformation("{className}.{methodName}: Successfully added record in ConsumerTask table for consumerCode: {consumerCode}", className, methodName, consumerTaskDto.ConsumerCode);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="consumerTaskDto"></param>
        /// <param name="taskRewardsModel"></param>
        /// <returns></returns>
        private static ConsumerTaskModel CreateConsumerTaskPayload(UpdateConsumerTaskDto consumerTaskDto, TaskRewardModel taskRewardsModel)
        {
            try
            {
                var requestConsumerTaskModel = new ConsumerTaskModel();
                requestConsumerTaskModel.TaskId = taskRewardsModel.TaskId;
                requestConsumerTaskModel.TenantCode = consumerTaskDto.TenantCode;
                requestConsumerTaskModel.TaskStatus = Constants.InProgress;
                requestConsumerTaskModel.ConsumerCode = consumerTaskDto.ConsumerCode;
                requestConsumerTaskModel.Progress = consumerTaskDto.Progress;
                requestConsumerTaskModel.Notes = consumerTaskDto.Notes ?? string.Empty;
                requestConsumerTaskModel.TaskStartTs = DateTime.UtcNow;
                requestConsumerTaskModel.TaskCompleteTs = DateTime.UtcNow;
                requestConsumerTaskModel.UpdateTs = default;
                requestConsumerTaskModel.CreateTs = DateTime.UtcNow;
                requestConsumerTaskModel.UpdateUser = null;
                requestConsumerTaskModel.CreateUser = Constants.CreateUser;
                requestConsumerTaskModel.DeleteNbr = 0;
                requestConsumerTaskModel.AutoEnrolled = false;
                requestConsumerTaskModel.ParentConsumerTaskId = consumerTaskDto.ConsumerTaskId; // linkage to parent consumer_task record

                return requestConsumerTaskModel;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="configJson"></param>
        /// <returns></returns>
        private async System.Threading.Tasks.Task<string> SpinnerJsonObject(string configJson)
        {
            var finalSlot = System.Text.Json.JsonSerializer.Deserialize<SpinWheelDtos>(configJson);

            var finalSlotIndex = await FinalSlotSpinner(finalSlot);

            var jsonObject = JObject.Parse(configJson);
            var spinwheelConfig = jsonObject["spinwheelConfig"];
            var spinwheelProgress = new JObject();
            spinwheelProgress.Add("finalSlotIndex", finalSlotIndex);
            spinwheelProgress.Add("spinwheelConfig", spinwheelConfig);
            jsonObject.Remove("spinwheelConfig");
            jsonObject.Add("spinwheelProgress", spinwheelProgress);
            return jsonObject.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="finalSlotIndex"></param>
        /// <returns></returns>
        private async Task<int> FinalSlotSpinner(SpinWheelDtos finalSlotIndex)
        {

            var itemDefinition = finalSlotIndex.spinwheelConfig.itemDefinition.ToArray();
            var random = new Random();
            var val = random.NextDouble();
            int i = 0;

            for (i = 0; i < itemDefinition.Length; i++)
            {
                if (val >= Convert.ToDouble(itemDefinition[i].lowProbability) && val < Convert.ToDouble(itemDefinition[i].highProbability))
                    return i;
            }
            return i;
        }


        #endregion

        public async Task<BaseResponseDto> CreateSubTask(SubtaskRequestDto requestDto)
        {
            const string methodName = nameof(CreateSubTask);

            try
            {
                if (requestDto == null)
                {
                    _subtaskServiceLogger.LogError("{className}.{methodName}: Failed to Saved data for  subtasks API for request: {requestDto}", className, methodName, requestDto?.ToJson());
                    return new BaseResponseDto { ErrorCode = StatusCodes.Status404NotFound, ErrorMessage = "Subtask Not Found" };
                }
                var parentTaskRewards = await _taskRewardRepo.FindOneAsync(x => x.TaskRewardCode == requestDto.ParentTaskRewardCode && x.DeleteNbr == 0);
                var childTaskRewards = await _taskRewardRepo.FindOneAsync(x => x.TaskRewardCode == requestDto.ChildTaskRewardCode && x.DeleteNbr == 0);
                if (parentTaskRewards == null || childTaskRewards == null)
                {
                    _subtaskServiceLogger.LogInformation("{className}.{methodName}: Parent and Child  Task record not found for request: {requestDto}", className, methodName, requestDto?.ToJson());
                    return new BaseResponseDto { ErrorCode = StatusCodes.Status404NotFound, ErrorMessage = "  Parent and Child Task record not found" };

                }
                var subtasks = await _subTaskRepo.FindOneAsync(x => (x.ParentTaskRewardId == parentTaskRewards.TaskRewardId && x.ChildTaskRewardId == childTaskRewards.TaskRewardId) && x.DeleteNbr == 0);

                if (subtasks == null)
                {
                    SubTaskModel subTaskModel = new SubTaskModel();
                    subTaskModel = _mapper.Map<SubTaskModel>(requestDto.Subtask);
                    subTaskModel.ChildTaskRewardId = childTaskRewards?.TaskRewardId ?? 0;
                    subTaskModel.ParentTaskRewardId = parentTaskRewards?.TaskRewardId ?? 0;
                    subTaskModel.DeleteNbr = 0;
                    subTaskModel.ConfigJson = JsonConvert.SerializeObject(requestDto?.Subtask?.ConfigJson, new JsonSerializerSettings
                    {
                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                    });
                    subTaskModel.CreateTs = DateTime.Now;
                    subTaskModel.CreateUser = requestDto?.Subtask?.CreateUser ?? Constant.SystemUser;
                    subTaskModel = await _subTaskRepo.CreateAsync(subTaskModel);
                    if (subTaskModel.SubTaskId > 0)
                    {
                        _subtaskServiceLogger.LogInformation("{className}.{methodName}: Successfully Saved data for  subtasks API for request: {requestDto}", className, methodName, requestDto?.ToJson());
                        return new BaseResponseDto();
                    }
                    else
                    {
                        _subtaskServiceLogger.LogError("{className}.{methodName}: Failed to Saved data for  subtasks API for request: {requestDto}", className, methodName, requestDto?.ToJson());
                        return new BaseResponseDto { ErrorCode = StatusCodes.Status404NotFound, ErrorMessage = "Subtask Not Created" };
                    }

                }
                _subtaskServiceLogger.LogInformation("{className}.{methodName}: record exists for request: {requestDto}", className, methodName, requestDto.ToJson());
                return new BaseResponseDto { ErrorCode = StatusCodes.Status409Conflict, ErrorMessage = "Subtask already exists" };

            }
            catch (Exception ex)
            {
                _subtaskServiceLogger.LogError(ex, "{className}.{methodName}: ERROR - msg : {msg}, for requestDto: {RequestDto}", className, methodName, ex.Message, requestDto.ToJson());
                return new BaseResponseDto { ErrorCode = StatusCodes.Status500InternalServerError, ErrorMessage = "Subtask Not Created" };

            }
        }

        /// <summary>
        /// Updates an existing Subtask based on the provided request data.
        /// </summary>
        /// <param name="requestDto">The request data containing the details to update.</param>
        /// <returns>A response DTO indicating success or failure.</returns>
        public async Task<SubtaskResponseDto> UpdateSubtask(SubTaskUpdateRequestDto requestDto)
        {
            const string methodName = nameof(UpdateSubtask);
            try
            {
                var existingSubtask = await _subTaskRepo.FindOneAsync(x => x.SubTaskId == requestDto.SubTaskId && x.DeleteNbr == 0);
                if (existingSubtask == null)
                {
                    _subtaskServiceLogger.LogError("{ClassName}.{MethodName}: No Subtask found for SubtaskId: {SubtaskId}", className, methodName, requestDto.SubTaskId);
                    return new SubtaskResponseDto
                    {
                        ErrorCode = StatusCodes.Status404NotFound,
                        ErrorMessage = $"No Subtask found for SubtaskId: {requestDto.SubTaskId}"
                    };
                }
                _mapper.Map(requestDto, existingSubtask);

                existingSubtask.UpdateUser = Constant.ImportUser;
                existingSubtask.UpdateTs = DateTime.UtcNow;

                await _subTaskRepo.UpdateAsync(existingSubtask);

                return new SubtaskResponseDto { Subtask = _mapper.Map<SubTaskDto>(existingSubtask) };
            }
            catch (Exception ex)
            {
                _subtaskServiceLogger.LogError(ex, "{ClassName}.{MethodName}: Error processing. Request: {RequestData}, ErrorMessage: {ErrorMessage}, ErrorCode: {ErrorCode}", className, methodName, requestDto.ToJson(), ex.Message, StatusCodes.Status500InternalServerError);
                return new SubtaskResponseDto { ErrorCode = StatusCodes.Status500InternalServerError, ErrorMessage = ex.Message };
            }
        }
    }
}


