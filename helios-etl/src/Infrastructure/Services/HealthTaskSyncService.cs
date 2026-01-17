extern alias SunnyRewards_Task;

using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NHibernate.Linq;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.ETL.Common.Constants;
using SunnyRewards.Helios.ETL.Common.Helpers.Interfaces;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos.Enums;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using SunnyRewards.Helios.ETL.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces;
using SunnyRewards_Task::SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards_Task::SunnyRewards.Helios.Task.Core.Domain.Models;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services
{
    public class HealthTaskSyncService : IHealthTaskSyncService
    {
        private readonly IConfiguration _configuration;
        private readonly ITaskRewardRepo _taskRewardRepo;
        private readonly IConsumerRepo _consumerRepo;
        private readonly IHealthMetricRepo _healthMetricRepo;
        private readonly IConsumerTaskRepo _consumerTaskRepo;
        private readonly ILogger<HealthTaskSyncService> _logger;
        private readonly IHealthClient _healthClient;
        private readonly ITenantRepo _tenantRepo;
        private readonly IJsonConvertWrapper _jsonWrapper;
        private readonly IAdminClient _adminClient;
        private readonly IMapper _mapper;
        private readonly IProcessRecurringTasksService _processRecurringTasksService;
        private const string className = nameof(HealthTaskSyncService);

        public HealthTaskSyncService(ILogger<HealthTaskSyncService> logger, IHealthClient healthClient, ITenantRepo tenantRepo,
            ITaskRewardRepo taskRewardRepo, IConsumerRepo consumerRepo, IHealthMetricRepo healthMetricRepo,
            IConsumerTaskRepo consumerTaskRepo, IConfiguration configuration, IJsonConvertWrapper jsonWrapper,
            IAdminClient adminClient, IMapper mapper, IProcessRecurringTasksService processRecurringTasksService)
        {
            _logger = logger;
            _healthClient = healthClient;
            _taskRewardRepo = taskRewardRepo;
            _consumerRepo = consumerRepo;
            _healthMetricRepo = healthMetricRepo;
            _consumerTaskRepo = consumerTaskRepo;
            _configuration = configuration;
            _tenantRepo = tenantRepo;
            _jsonWrapper = jsonWrapper;
            _adminClient = adminClient;
            _mapper = mapper;
            _processRecurringTasksService = processRecurringTasksService;
        }

        /// <summary>
        /// Processes health tasks.
        /// </summary>
        /// <param name="etlExecutionContext"></param>
        public async Task ProcessHealthTaskAsync(EtlExecutionContext etlExecutionContext)
        {
            const string methodName = nameof(ProcessHealthTaskAsync);

            try
            {
                _logger.LogInformation("{ClassName}.{MethodName} - Start processing at {Timestamp}", className, methodName, DateTime.UtcNow);

                var rollupPeriodStartTs = new DateTime(etlExecutionContext.Year, etlExecutionContext.Month, Constants.StartDate, 0, 0, 0, DateTimeKind.Utc);
                var rollupPeriodEndTs = rollupPeriodStartTs.AddMonths(1);

                IList<ETLTenantModel>? tenants;
                if (etlExecutionContext.TenantCode.Equals(Constants.ALL, StringComparison.OrdinalIgnoreCase))
                {
                    tenants = await _tenantRepo.FindAsync(x => x.DeleteNbr == 0);
                }
                else
                {
                    var tenantCodes = etlExecutionContext.TenantCode.Split(',').Select(code => code.Trim()).ToArray();
                    tenants = await _tenantRepo.FindAsync(x => tenantCodes.Contains(x.TenantCode) && x.DeleteNbr == 0);
                }

                if (tenants.Count == 0)
                {
                    _logger.LogError("{ClassName}.{MethodName} - No tenant found for TenantCode: {TenantCode}", className, methodName, etlExecutionContext.TenantCode);
                }
                foreach (var tenant in tenants)
                {
                    IDictionary<string, long> parameters = new Dictionary<string, long>();
                    var healthTaskRewards = await _adminClient.Get<TaskRewardsResponseDto>($"health-task-rewards/{tenant.TenantCode}", parameters);

                    _logger.LogInformation("{ClassName}.{MethodName} - Fetched {Count} health task rewards for TenantCode: {TenantCode}",
                        className, methodName, healthTaskRewards?.TaskRewards?.Count ?? 0, tenant.TenantCode);

                    if (healthTaskRewards?.TaskRewards?.Count == 0)
                    {
                        continue;
                    }

                    int start = 0;
                    int batchSize = 100;

                    while (true)
                    {
                        var consumers = _consumerRepo.GetConsumers(tenant.TenantCode, start, batchSize);
                        if (!consumers.Any())
                        {
                            break;
                        }

                        var taskRewards = _mapper.Map<IList<ETLTaskRewardModel>>(healthTaskRewards.TaskRewards);
                        foreach (var consumer in consumers)
                        {
                            await ProcessConsumers(consumer, taskRewards, etlExecutionContext, rollupPeriodStartTs, rollupPeriodEndTs);
                        }
                        start = start + batchSize;
                    }
                }
                _logger.LogInformation("{ClassName}.{MethodName} - Completed processing successfully for  at {Timestamp}", className, methodName, DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Error processing HealthTaskAsync. Error: {Message}",
                    className, methodName, ex.Message);
                throw;
            }
            finally
            {
                _logger.LogInformation("{ClassName}.{MethodName} - End processing at {Timestamp}", className, methodName, DateTime.UtcNow);
            }
        }

        /// <summary>
        /// Processes the consumers.
        /// </summary>
        /// <param name="consumers">The consumers.</param>
        /// <param name="taskReward">The task reward.</param>
        /// <param name="etlExecutionContext">The etl execution context.</param>
        /// <param name="rollupPeriodStartTs">The rollup period start ts.</param>
        /// <param name="rollupPeriodEndTs">The rollup period end ts.</param>
        private async Task ProcessConsumers(ETLConsumerModel consumer, IList<ETLTaskRewardModel> taskRewards, EtlExecutionContext etlExecutionContext, DateTime rollupPeriodStartTs, DateTime rollupPeriodEndTs)
        {
            const string methodName = nameof(ProcessConsumers);
            var erroredConsumers = new List<string>();
            var erroredTenantCode = new HashSet<string>();
            try
            {
                foreach (var taskReward in taskRewards)
                {
                    if (!IsValidMonthlyHealthTaskReward(taskReward))
                    {
                        continue;
                    }

                    consumer.TenantCode = taskReward.TenantCode;
                    var consumerHealthMetrics = await GetConsumerHealthMetricsAsync(consumer, rollupPeriodStartTs, rollupPeriodEndTs);

                    if (consumerHealthMetrics == null || consumerHealthMetrics.Count == 0)
                    {
                        erroredConsumers.Add(consumer.ConsumerCode);
                        continue;
                    }

                    var rollUpPeriodData = new RollUpPeriodData
                    {
                        MonthDetail = new MonthDetail
                        {
                            Year = etlExecutionContext.Year,
                            Month = etlExecutionContext.Month
                        }
                    };

                    var healthMetricRollupRequestDto = CreateHealthMetricRollupRequestDto(consumer, etlExecutionContext, taskReward, rollUpPeriodData);
                    var healthMetricRollupResponseDto = await ProcessConsumerHealthMetricAsync(etlExecutionContext, healthMetricRollupRequestDto);

                    if (healthMetricRollupResponseDto == null)
                    {
                        _logger.LogError("{ClassName}.{MethodName} - Error occurred while processing health metric roll up for ConsumerCode:{ConsumerCode} and TaskId:{TaskId}.", className, methodName, consumer.ConsumerCode, etlExecutionContext.TaskId);
                        erroredConsumers.Add(consumer.ConsumerCode);
                        continue;
                    }

                    var consumerTask = await _consumerTaskRepo.FindOneAsync(x =>
                        x.ConsumerCode == consumer.ConsumerCode &&
                        x.TenantCode == consumer.TenantCode &&
                        x.TaskId == taskReward.TaskId &&
                        x.TaskStatus == Constants.TaskStatusInProgress &&
                        x.DeleteNbr == 0);

                    if (consumerTask == null)
                    {
                        var requestDto = new CreateConsumerTaskDto()
                        {
                            TaskId = taskReward.TaskId,
                            ConsumerCode = consumer.ConsumerCode,
                            TaskStatus = Constants.TaskStatusInProgress,
                            TenantCode = consumer.TenantCode,
                            AutoEnrolled = true
                        };

                        var newConsumerTask = await _processRecurringTasksService.CreateConsumerTask(requestDto);

                        if (newConsumerTask?.ConsumerTask == null || newConsumerTask.ConsumerTask.ConsumerTaskId == 0)
                        {
                            _logger.LogError("{ClassName}.{MethodName} - No consumer task found for ConsumerCode:{ConsumerCode} and TaskId:{TaskId}.", className, methodName, consumer.ConsumerCode, etlExecutionContext.TaskId);
                            erroredConsumers.Add(consumer.ConsumerCode);
                            continue;
                        }

                        // Assign the newly created data to consumer task
                        consumerTask = _mapper.Map<ETLConsumerTaskModel>(newConsumerTask?.ConsumerTask);
                    }

                    UpdateConsumerTaskProgress(consumerTask, healthMetricRollupResponseDto, taskReward);
                    await UpdateHealthTaskProgress(consumerTask, healthMetricRollupResponseDto, taskReward);

                    var updateResponse = await _consumerTaskRepo.UpdateAsync(consumerTask);

                    if (ShouldCompleteTask(healthMetricRollupResponseDto, taskReward))
                    {
                        var taskUpdateRequestDto = new TaskUpdateRequestDto
                        {
                            ConsumerCode = consumer.ConsumerCode,
                            TaskId = taskReward.TaskId,
                            TaskCompletedTs = DateTime.UtcNow,
                            TaskStatus = Constants.TaskStatusCompleted
                        };
                        await UpdateTaskAsCompleted(taskUpdateRequestDto);
                    }


                    if (updateResponse == null)
                    {
                        _logger.LogError("{ClassName}.{MethodName} - Failed to update consumer task for ConsumerCode:{ConsumerCode}. Task: {Task}", className, methodName, consumer.ConsumerCode, consumerTask);
                        erroredConsumers.Add(consumer.ConsumerCode);
                    }
                    else
                    {
                        _logger.LogInformation("{ClassName}.{MethodName} - Successfully updated consumer task for ConsumerCode:{ConsumerCode}.", className, methodName, consumer.ConsumerCode);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Error processing consumer for ConsumerCode:{ConsumerCode}, TenantCode:{TenantCode}. Error: {Message}",
                    className, methodName, consumer.ConsumerCode, consumer.TenantCode, ex.Message);
            }

            //set job history status
            etlExecutionContext.JobHistoryStatus = erroredConsumers.Count == 0
                ? Constants.JOB_HISTORY_SUCCESS_STATUS
                : (erroredConsumers.Count == 1
                    ? Constants.JOB_HISTORY_FAILURE_STATUS
                    : Constants.JOB_HISTORY_PARTIAL_SUCCESS_STATUS);
            etlExecutionContext.JobHistoryErrorLog = erroredConsumers.Count != 0
                ? $"Errored records count: {erroredConsumers.Count}"
                : string.Empty;
        }

        /// <summary>
        /// Creates the health metric rollup request dto.
        /// </summary>
        /// <param name="consumer">The consumer.</param>
        /// <param name="etlExecutionContext">The etl execution context.</param>
        /// <param name="taskReward">The task reward.</param>
        /// <param name="rollUpPeriodData">The roll up period data.</param>
        /// <returns></returns>
        private HealthMetriRollupRequestDto CreateHealthMetricRollupRequestDto(ETLConsumerModel consumer, EtlExecutionContext etlExecutionContext, ETLTaskRewardModel taskReward, RollUpPeriodData rollUpPeriodData)
        {
            var healthMetricRollupRequestDto = new HealthMetriRollupRequestDto
            {
                TenantCode = consumer.TenantCode,
                ConsumerCode = consumer.ConsumerCode,
                RollUpPeriodTypeName = etlExecutionContext.RollupPeriodTypeName,
                RollUpPeriodData = _jsonWrapper.SerializeObject(rollUpPeriodData)
            };

            if (taskReward.TaskCompletionCriteria?.HealthCriteria?.HealthTaskType?.ToUpper() == HealthTaskType.SLEEP.ToString() &&
                taskReward.TaskCompletionCriteria.HealthCriteria.RequiredSleep?.MinSleepDuration > 0)
            {
                healthMetricRollupRequestDto.MinSleepDuration = taskReward.TaskCompletionCriteria.HealthCriteria.RequiredSleep.MinSleepDuration;
            }

            return healthMetricRollupRequestDto;
        }

        private void LogErrorAndReturn(string message, EtlExecutionContext etlExecutionContext)
        {
            _logger.LogError("{ClassName}.{MethodName} - {Message} , TaskId:{TaskId}", className, nameof(ProcessHealthTaskAsync), message, etlExecutionContext.TaskId);
        }

        /// <summary>
        /// Updates the consumer task progress.
        /// </summary>
        /// <param name="consumerTask">The consumer task.</param>
        /// <param name="healthMetricRollupResponseDto">The health metric rollup response dto.</param>
        /// <param name="taskReward">The task reward.</param>
        private void UpdateConsumerTaskProgress(ETLConsumerTaskModel consumerTask, HealthMetriRollupResponseDto healthMetricRollupResponseDto, ETLTaskRewardModel taskReward)
        {
            var healthProgressDetails = new HealthProgressDetails
            {
                DetailType = Constants.HealthCriteriaType,
                HealthProgress = healthMetricRollupResponseDto?.HealthMetricRollUpData?.RollupData
            };

            consumerTask.ProgressDetail = _jsonWrapper.SerializeObject(healthProgressDetails);
        }


        public async Task UpdateHealthTaskProgress(ETLConsumerTaskModel consumerDeviceTask, 
            HealthMetriRollupResponseDto healthMetricRollupResponseDto, ETLTaskRewardModel etlTaskReward)
        {
            var methodName = nameof(UpdateHealthTaskProgress);

            try
            {
                GetTasksAndTaskRewardsRequestDto requestDto = new GetTasksAndTaskRewardsRequestDto() { TenantCode = consumerDeviceTask.TenantCode };
                
                var healthTaskRewards = await _adminClient.Post<GetTasksAndTaskRewardsResponseDto>($"task-reward/get-task-rewards", requestDto);

                var validTasks = new[] { HealthTaskType.STEPS.ToString(), HealthTaskType.SLEEP.ToString(),
                HealthTaskType.HYDRATION.ToString(), HealthTaskType.OTHER.ToString() };

                var taskRewards = healthTaskRewards.taskAndTaskRewardDtos.Where(tr => tr.TaskReward != null &&
                !string.IsNullOrWhiteSpace(tr.TaskReward.TaskCompletionCriteriaJson)
                && tr.Task != null
                && tr.Task.SelfReport
                && tr.Task.TaskName != null
                ).ToList();

                var isTaskCompleted = false;

                foreach (var taskReward in taskRewards)
                {
                    var taskCompletionCriteria = JsonConvert.DeserializeObject<TaskCompletionCriteria>(taskReward.TaskReward.TaskCompletionCriteriaJson);

                    if ((taskCompletionCriteria?.HealthCriteria) == null
                        || taskCompletionCriteria.HealthCriteria.HealthTaskType == null
                        || !validTasks.Any(v => taskCompletionCriteria.HealthCriteria.HealthTaskType.ToLower().Contains(v.ToLower())))
                    {
                        continue;
                    }

                    var taskType = taskCompletionCriteria.HealthCriteria.HealthTaskType.ToUpper();

                    var consumerTasks = await _consumerTaskRepo.FindAsync(x =>
                        x.ConsumerCode == consumerDeviceTask.ConsumerCode &&
                        x.TenantCode == etlTaskReward.TenantCode &&
                        x.TaskStatus == Constants.TaskStatusInProgress &&
                        x.TaskId == taskReward.TaskReward.TaskId &&
                        x.DeleteNbr == 0);

                    foreach (var consumerTask in consumerTasks)
                    {
                        try
                        {
                            var progressDetail = string.IsNullOrWhiteSpace(consumerTask.ProgressDetail)
                                ? new HealthProgressDetails()
                                : JsonConvert.DeserializeObject<HealthProgressDetails>(consumerTask.ProgressDetail);
                            var healthTaskType = progressDetail.DetailType?.Trim().ToUpper();

                            if (string.IsNullOrWhiteSpace(healthTaskType))
                            {
                                if (taskType.Contains(Constants.HealthTaskTypeStep))
                                {
                                    healthTaskType = HealthTaskType.STEPS.ToString();
                                }
                                else if (taskType.Contains(HealthTaskType.SLEEP.ToString()))
                                {
                                    healthTaskType = HealthTaskType.SLEEP.ToString();
                                }
                                else
                                {
                                    healthTaskType = HealthTaskType.OTHER.ToString();
                                }
                            }

                            if (!Enum.IsDefined(typeof(HealthTaskType), healthTaskType))
                            {
                                var c = new ConsumerHealthTaskResponseUpdateDto
                                {
                                    ErrorCode = StatusCodes.Status400BadRequest,
                                    ErrorMessage = $"Invalid HealthTaskType: {healthTaskType}"
                                };
                                return;
                            }
                            var criteria = etlTaskReward?.TaskCompletionCriteria;
                            if (criteria?.CompletionCriteriaType.ToLower() != "health")
                            {
                                var c = new ConsumerHealthTaskResponseUpdateDto();
                                return;
                            }

                            consumerTask.ProgressDetail = UpdateProgressDetail(healthTaskType, healthMetricRollupResponseDto.HealthMetricRollUpData, consumerTask.ProgressDetail, criteria, consumerTask);
                            if (consumerTask.TaskStatus.ToLower() == "completed")
                            {
                                isTaskCompleted = true;
                            }

                            consumerTask.UpdateTs = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
                            consumerTask.UpdateUser = "SYSTEM";

                            await _consumerTaskRepo.UpdateAsync(consumerTask);

                            _logger.LogInformation("{ClassName}.{MethodName}: Successfully updated health task progress for ConsumerCode: {ConsumerCode}, TaskId: {TaskId}, TenantCode: {TenantCode}",
                                className, methodName, consumerTask.ConsumerCode, consumerTask.TaskId, consumerTask.TenantCode);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "{ClassName}.{MethodName} - An exception occurred while updating consumer task. ErrorCode: {ErrorCode}, {ConsumerCode}, TaskId: {TaskId}, TenantCode: {TenantCode}\",",
                            className, methodName, StatusCodes.Status500InternalServerError, consumerTask.ConsumerCode, consumerTask.TaskId, consumerTask.TenantCode);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - An exception occurred while updating consumer health task progress. ErrorCode: {ErrorCode}, {ConsumerCode}, TaskId: {TaskId}, TenantCode: {TenantCode}\",",
                className, methodName, StatusCodes.Status500InternalServerError, consumerDeviceTask.ConsumerCode, consumerDeviceTask.TaskId, consumerDeviceTask.TenantCode);
            }
        }

        private string UpdateProgressDetail(string healthTaskType,
            HealthMetricRollUpDto healthMetricRollupDto,
            string? existingDetail, TaskCompletionCriteria criteria, ETLConsumerTaskModel consumerTask)
        {
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Formatting = Formatting.Indented
            };
            TrackingDto newActivity = new()
            {
                TimeStamp = healthMetricRollupDto.CreateTs,
                Source = "Auto"
            };

            switch (healthTaskType)
            {
                // In the UpdateProgressDetail method, fix possible null dereference for SleepTracking
                case nameof(HealthTaskType.SLEEP):
                    return UpdateSleepTask(healthMetricRollupDto, existingDetail, criteria, consumerTask, settings, newActivity);
                case nameof(HealthTaskType.STEPS):
                    return UpdateStepsTask(healthMetricRollupDto, existingDetail, criteria, consumerTask, settings, newActivity);

                case nameof(HealthTaskType.HYDRATION):
                    var hydration = string.IsNullOrWhiteSpace(existingDetail)
                        ? new HealthProgressDetails<HydrationRollupDataDto>
                        {
                            DetailType = nameof(HealthTaskType.HYDRATION),
                            HealthProgress = new HydrationRollupDataDto
                            {
                                TotalDays = 0
                            }
                        }
                        : JsonConvert.DeserializeObject<HealthProgressDetails<HydrationRollupDataDto>>(existingDetail, settings);

                    return JsonConvert.SerializeObject(hydration, settings);
                case nameof(HealthTaskType.OTHER):
                    var otherTask = string.IsNullOrWhiteSpace(existingDetail)
                        ? new HealthProgressDetails<OtherHealthTaksRollupDataDto>
                        {
                            DetailType = nameof(HealthTaskType.OTHER),
                            HealthProgress = new OtherHealthTaksRollupDataDto
                            {
                                TotalUnits = 0,
                            }
                        }
                        : JsonConvert.DeserializeObject<HealthProgressDetails<OtherHealthTaksRollupDataDto>>(existingDetail, settings);

                    return JsonConvert.SerializeObject(otherTask, settings);

                default:
                    return existingDetail ?? string.Empty;
            }
        }

        private static string UpdateStepsTask(HealthMetricRollUpDto healthMetricRollupDto, string? existingDetail, TaskCompletionCriteria criteria, ETLConsumerTaskModel consumerTask, JsonSerializerSettings settings, TrackingDto newActivity)
        {
            var steps = string.IsNullOrWhiteSpace(existingDetail)
                                    ? new HealthProgressDetails<StepsRollupDataDto>
                                    {
                                        DetailType = nameof(HealthTaskType.STEPS),
                                        HealthProgress = new StepsRollupDataDto
                                        {
                                            TotalSteps = 0,
                                            ActivityLog = []
                                        }
                                    }
                                    : JsonConvert.DeserializeObject<HealthProgressDetails<StepsRollupDataDto>>(existingDetail, settings);


            if (steps.HealthProgress == null)
            {
                throw new Exception($"HealthProgress for Steps is recorded as null earlier for taskId {consumerTask.TaskId} consumer code {consumerTask.ConsumerCode}");
            }

            if(healthMetricRollupDto.RollupData.TotalSteps == 0)
            {
                return JsonConvert.SerializeObject(steps, settings);
            }

            var stepsActivityLog = steps.HealthProgress.ActivityLog;
            var autoStepsLogs = stepsActivityLog.Where(a => a.Source.ToLower() == "auto");

            if(autoStepsLogs.Any() && autoStepsLogs.Any(a => a.TimeStamp.Date.Equals(healthMetricRollupDto.CreateTs.Date)))
            {
                var unitsAdded = autoStepsLogs.Where(a => a.TimeStamp.Date.Equals(healthMetricRollupDto.CreateTs.Date)).Sum(a => a.UnitsAdded);
                steps.HealthProgress.TotalSteps -= unitsAdded;
                
                steps.HealthProgress.ActivityLog = autoStepsLogs.Where(a => !a.TimeStamp.Date.Equals(healthMetricRollupDto.CreateTs.Date)).ToArray();
            }

            newActivity.UnitsAdded = healthMetricRollupDto.RollupData.TotalSteps;
            steps.HealthProgress.ActivityLog = steps.HealthProgress.ActivityLog.Append(newActivity).ToArray();
            steps.HealthProgress.TotalSteps += healthMetricRollupDto.RollupData.TotalSteps;

            if (criteria?.HealthCriteria?.HealthTaskType == nameof(HealthTaskType.STEPS) &&
                criteria.HealthCriteria.RequiredSteps <= steps.HealthProgress.TotalSteps)
            {
                consumerTask.TaskStatus = Constants.COMPLETED;
                consumerTask.TaskCompleteTs = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            }

            return JsonConvert.SerializeObject(steps, settings);
        }

        private static string UpdateSleepTask(HealthMetricRollUpDto healthMetricRollupDto, string? existingDetail, TaskCompletionCriteria criteria, ETLConsumerTaskModel consumerTask, JsonSerializerSettings settings, TrackingDto newActivity)
        {
            var sleep = string.IsNullOrWhiteSpace(existingDetail)
                                    ? new HealthProgressDetails<SleepRollupDataDto>
                                    {
                                        DetailType = nameof(HealthTaskType.SLEEP),
                                        HealthProgress = new SleepRollupDataDto
                                        {
                                            SleepTracking = new SunnyRewards_Task.SunnyRewards.Helios.Task.Core.Domain.Dtos.SleepTrackingDto
                                            {
                                                NumDaysAtOrAboveMinDuration = 0
                                            }
                                        }
                                    }
                                    : JsonConvert.DeserializeObject<HealthProgressDetails<SleepRollupDataDto>>(existingDetail, settings);

            // Ensure SleepTracking is not null before dereferencing
            if (sleep != null && sleep.HealthProgress?.SleepTracking == null)
            {
                sleep.HealthProgress.SleepTracking = new SunnyRewards_Task.SunnyRewards.Helios.Task.Core.Domain.Dtos.SleepTrackingDto
                {
                    NumDaysAtOrAboveMinDuration = 0
                };
            }

            // Ensure healthMetricRollupDto.RollupData and SleepTracking are not null before dereferencing
            int numDaysToAdd = healthMetricRollupDto.RollupData?.SleepTracking?.NumDaysAtOrAboveMinDuration ?? 0;

            if (numDaysToAdd == 0)
            {
                return JsonConvert.SerializeObject(sleep, settings);
            }

            sleep.HealthProgress.SleepTracking.NumDaysAtOrAboveMinDuration += numDaysToAdd;

            var sleepActivityLog = sleep.HealthProgress.ActivityLog;

            var autoLogs = sleepActivityLog.Where(a => a.Source.ToLower() == "auto");
            if (autoLogs.Any() && autoLogs.Any(a => a.TimeStamp.Date.Equals(healthMetricRollupDto.CreateTs.Date)))
            {
                var previousUnitsAdded = autoLogs.Where(a => a.TimeStamp.Date.Equals(healthMetricRollupDto.CreateTs.Date)).Sum(a => a.UnitsAdded);
                sleep.HealthProgress.SleepTracking.NumDaysAtOrAboveMinDuration -= previousUnitsAdded;

                sleep.HealthProgress.ActivityLog = autoLogs.Where(a => !a.TimeStamp.Date.Equals(healthMetricRollupDto.CreateTs.Date)).ToArray();
            }

            newActivity.UnitsAdded = numDaysToAdd;
            sleep.HealthProgress.ActivityLog = sleep.HealthProgress.ActivityLog.Append(newActivity).ToArray();

            if (criteria?.HealthCriteria?.HealthTaskType == nameof(HealthTaskType.SLEEP) &&
                criteria.HealthCriteria.RequiredSleep?.NumDaysAtOrAboveMinDuration <= sleep.HealthProgress.SleepTracking.NumDaysAtOrAboveMinDuration)
            {
                consumerTask.TaskStatus = Constants.COMPLETED;
                consumerTask.TaskCompleteTs = DateTime.UtcNow;
            }

            return JsonConvert.SerializeObject(sleep, settings);
        }


        /// <summary>
        /// Shoulds the complete task.
        /// </summary>
        /// <param name="healthMetricRollupResponseDto">The health metric rollup response dto.</param>
        /// <param name="taskReward">The task reward.</param>
        /// <returns></returns>
        private bool ShouldCompleteTask(HealthMetriRollupResponseDto healthMetricRollupResponseDto, ETLTaskRewardModel taskReward)
        {
            var healthCriteria = taskReward.TaskCompletionCriteria?.HealthCriteria;
            return (healthCriteria?.HealthTaskType?.ToUpper() == HealthTaskType.STEPS.ToString() &&
                    healthMetricRollupResponseDto?.HealthMetricRollUpData?.RollupData?.TotalSteps >= healthCriteria?.RequiredSteps) ||
                   (healthCriteria?.HealthTaskType?.ToUpper() == HealthTaskType.SLEEP.ToString() &&
                    healthMetricRollupResponseDto?.HealthMetricRollUpData?.RollupData?.SleepTracking?.NumDaysAtOrAboveMinDuration >= healthCriteria?.RequiredSleep?.NumDaysAtOrAboveMinDuration);
        }

        /// <summary>
        /// Updates the task as completed.
        /// </summary>
        /// <param name="taskUpdateRequestDto">The task update request dto.</param>
        private async Task UpdateTaskAsCompleted(TaskUpdateRequestDto taskUpdateRequestDto)
        {
            const string methodName = nameof(UpdateTaskAsCompleted);
            try
            {
                var taskUpdateResponse = await _adminClient.PostFormData<ConsumerTaskUpdateResponseDto>("admin/consumer/task-update", taskUpdateRequestDto);

                if (taskUpdateResponse == null || (taskUpdateResponse.ErrorCode != null && taskUpdateResponse.ErrorCode != 200) ||
                    taskUpdateResponse.ConsumerTask == null)
                {
                    _logger.LogError("{ClassName}.{MethodName} - Error response received from admin client. ErrorCode: {ErrorCode}, ErrorMessage: {Response}, RequestData: {RequestData}",
                    className, methodName, taskUpdateResponse?.ErrorCode, taskUpdateResponse?.ErrorMessage, taskUpdateRequestDto.ToJson());
                }
                _logger.LogInformation("{ClassName}.{MethodName} - Task updated successfully. TaskId: {TaskId}, ConsumerCode: {ConsumerCode}",
                              className, methodName, taskUpdateRequestDto.TaskId, taskUpdateRequestDto.ConsumerCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - An exception occurred. ErrorCode: {ErrorCode}, Request: {Request}",
                className, methodName, StatusCodes.Status500InternalServerError, taskUpdateRequestDto);
            }
        }

        /// <summary>
        /// Processes the consumer health metric asynchronous.
        /// </summary>
        /// <param name="etlExecutionContext">The etl execution context.</param>
        /// <param name="healthMetriRollupRequestDto">The health metric rollup request dto.</param>
        /// <returns></returns>
        private async Task<HealthMetriRollupResponseDto?> ProcessConsumerHealthMetricAsync(EtlExecutionContext etlExecutionContext, HealthMetriRollupRequestDto healthMetriRollupRequestDto)
        {
            const string methodName = nameof(ProcessConsumerHealthMetricAsync);
            _logger.LogInformation("{ClassName}.{MethodName} - Start processing ConsumerHealthMetric ", className, methodName);
            try
            {
                var tenant = await _tenantRepo.FindOneAsync(x => x.TenantCode == healthMetriRollupRequestDto.TenantCode && x.DeleteNbr == 0);
                if (tenant == null)
                {
                    _logger.LogError("{ClassName}.{MethodName} - Tenant not found with TenantCode:{Code}", className, methodName, healthMetriRollupRequestDto.TenantCode);
                    return null;
                }

                var authHeaders = new Dictionary<string, string>
                {
                    { "X-API-KEY", tenant.ApiKey }
                };

                return await _healthClient.Post<HealthMetriRollupResponseDto>("process-consumer-health-metric", healthMetriRollupRequestDto, authHeaders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Error processing ConsumerhealthMetric, Request: {Request}", className, methodName, _jsonWrapper.SerializeObject(healthMetriRollupRequestDto));
                return null;
            }
            finally
            {
                _logger.LogInformation("{ClassName}.{MethodName} - End processing", className, methodName);
            }
        }


        private bool IsValidMonthlyHealthTaskReward(ETLTaskRewardModel taskReward)
        {
            return taskReward.TaskCompletionCriteria?.CompletionCriteriaType == Constants.HealthCriteriaType &&
                   taskReward.TaskCompletionCriteria?.CompletionPeriodType == Constants.MonthlyPeriodType;
        }

        private async Task<IList<HealthMetricModel>> GetConsumerHealthMetricsAsync(ETLConsumerModel consumer, DateTime rollupPeriodStartTs, DateTime rollupPeriodEndTs)
        {
            return await _healthMetricRepo.FindAsync(x =>
                x.ConsumerCode == consumer.ConsumerCode &&
                x.OsMetricTs >= rollupPeriodStartTs &&
                x.OsMetricTs <= rollupPeriodEndTs &&
                x.DeleteNbr == 0);
        }
    }
}


