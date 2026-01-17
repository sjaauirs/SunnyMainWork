using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using SunnyRewards.Helios.ETL.Common.Constants;
using SunnyRewards.Helios.ETL.Common.CustomException;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos.Json;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using SunnyRewards.Helios.ETL.Infrastructure.Helpers.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Logs;
using SunnyRewards.Helios.ETL.Infrastructure.Logs.Interface;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos.Json;
using System.Globalization;
using System.Text.RegularExpressions;
using ISecretHelper = SunnyRewards.Helios.ETL.Infrastructure.Helpers.Interfaces.ISecretHelper;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services
{
    public class TaskUpdateService : ITaskUpdateService
    {
        private readonly ILogger<TaskUpdateService> _logger;
        private readonly IVault _vault;
        private readonly IS3FileLogger _s3FileLogger;
        private readonly IDataFeedClient _dataFeedClient;
        private readonly ITenantRepo _tenantRepo;
        private readonly IConsumerTaskRepo _consumerTaskRepo;
        private readonly IPersonRepo _personRepo;
        private readonly ITaskExternalMappingRepo _taskExternalMappingRepo;
        private readonly ITaskRewardRepo _taskRewardRepo;
        private readonly ITaskRepo _taskRepo;
        private readonly ISecretHelper _secretHelper;
        private readonly ITokenService _tokenService;
        private readonly IProcessRecurringTasksService _processRecurringTasksService;
        private readonly IAdminClient _adminClient;
        private const string className=nameof(TaskUpdateService);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="awsQueueService"></param>
        public TaskUpdateService(ILogger<TaskUpdateService> logger,
            IVault vault, IS3FileLogger s3FileLogger, IDataFeedClient dataFeedClient, ITenantRepo tenantRepo,
            IConsumerTaskRepo consumerTaskRepo, IPersonRepo personRepo, ITaskExternalMappingRepo taskExternalMappingRepo,
            ITaskRewardRepo taskRewardRepo, ITaskRepo taskRepo, ISecretHelper secretHelper, ITokenService tokenService,
            IProcessRecurringTasksService processRecurringTasksService, IAdminClient adminClient)
        {
            _logger = logger;
            _vault = vault;
            _s3FileLogger = s3FileLogger;
            _dataFeedClient = dataFeedClient;
            _tenantRepo = tenantRepo;
            _consumerTaskRepo = consumerTaskRepo;
            _personRepo = personRepo;
            _taskExternalMappingRepo = taskExternalMappingRepo;
            _taskRewardRepo = taskRewardRepo;
            _taskRepo = taskRepo;
            _secretHelper = secretHelper;
            _tokenService = tokenService;
            _processRecurringTasksService = processRecurringTasksService;
            _adminClient = adminClient;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="tenant"></param>
        /// <returns></returns>
        private async Task ProcessBatchAsync(EtlExecutionContext etlExecutionContext, List<ETLMemberTaskUpdateDetailDto> batch , ETLTenantModel tenant)
        {
            const string methodName=nameof(ProcessBatchAsync);

            _logger.LogInformation("{ClassName}.{MethodName} - Started processing task update with TenantCode:{Code},Sponsporid:{Id}", className, methodName, tenant.TenantCode, tenant.SponsorId);
            var taskUpdatesBatch = new ETLMemberTaskUpdatesRequestDto
            {
                MemberTaskUpdates = batch
            };
            var xApiKeySecret = await _secretHelper.GetTenantSecret(tenant.TenantCode, Constants.XApiKeySecret);
            var customerRequestDto = new CustomerRequestDto()
            {
                CustomerCode = etlExecutionContext.CustomerCode,
                CustomerLabel = etlExecutionContext.CustomerLabel
            };
            var tokenResponse = await _tokenService.GetXAPISessionToken(tenant.TenantCode, customerRequestDto);
            if (tokenResponse.ErrorCode != null)
            {
                _logger.LogError("{ClassName}.{MethodName} - Error occured while getting XAPISessionToken,ErrorCode:{Code},ERROR:{Msg}", className, methodName, tokenResponse.ErrorCode, tokenResponse.ErrorMessage);
            }
            var authHeaders = new Dictionary<string, string>
                {
                    { Constants.XApiKey, xApiKeySecret },
                    { Constants.XApiSessionKey, tokenResponse.JWT },
                };

            _logger.LogInformation("{ClassName}.{MethodName} - Invoking datafeed/member-task-update api with PartnerCode:{Code},TaskNames:{Names},TaskId:{Id}",
                className, methodName, tenant.PartnerCode, taskUpdatesBatch.MemberTaskUpdates.Select(e => e.TaskName).ToList(), taskUpdatesBatch.MemberTaskUpdates.Select(e => e.TaskId).ToList());

            await _dataFeedClient.Post<ETLMemberTaskUpdatesResponseDto>(Constants.DataFeedTaskUpdateAPIUrl, taskUpdatesBatch, authHeaders);

            _logger.LogInformation("{ClassName}.{MethodName} - Processed datafeed/member-task-update api with PartnerCode:{Code},TaskNames:{Names},TaskId:{Id}",
                className, methodName, tenant.PartnerCode, taskUpdatesBatch.MemberTaskUpdates.Select(e => e.TaskName).ToList(), taskUpdatesBatch.MemberTaskUpdates.Select(e => e.TaskId).ToList());

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskUpdatefilePath"></param>ta
        /// <returns></returns>
        public async Task ProcessTaskUpdates(string taskUpdatefilePath = "", byte[]? taskUpdateFileContent = null,
            EtlExecutionContext? etlExecutionContext = null)
        {
            const string methodName=nameof(ProcessTaskUpdates);
            try
            {
                if (string.IsNullOrEmpty(taskUpdatefilePath) && taskUpdateFileContent?.Length <= 0)
                {
                    _logger.LogError("{ClassName}.{MethodName} - taskUpdatefilePath is not valid", className, methodName);
                    throw new ETLException(ETLExceptionCodes.NullValue, $"Task update filePath is not valid");
                }

                // retrieve environment from vault
                var environment = await _vault.GetSecret("env");
                if (string.IsNullOrEmpty(environment))
                {
                    _logger.LogError("{ClassName}.{MethodName} - environment is not valid", className, methodName);
                    throw new ETLException(ETLExceptionCodes.AWSSecretNotFound, "AWS secret is not configured in secret manager for 'env'");
                }

                if (!string.IsNullOrEmpty(etlExecutionContext?.CustomFormat) && etlExecutionContext.CustomFormat == Constants.TASK_UPDATE_CUSTOM_FORMAT)
                {
                    await ProcessTaskUpdateWithCustomFormat(etlExecutionContext, taskUpdatefilePath, taskUpdateFileContent, environment);
                }
                else
                {
                    await ProcessTaskUpdateWithCSV(etlExecutionContext, taskUpdatefilePath, taskUpdateFileContent, environment);
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Error occured while processing TaskUpdated,ErrorCode:{Code}, ERROR:{Msg}", className, methodName, StatusCodes.Status500InternalServerError,ex.Message);
                // this log using for s3FileLogger
                await _s3FileLogger.AddErrorLogs(new S3LogContext()
                {
                    Message = $"ProcessTaskUpdates: Error={ex.Message}",
                    TenantCode = etlExecutionContext?.TenantCode,
                    Ex = ex
                });
                throw;
            }
        }

        private async Task ProcessTaskUpdateWithCustomFormat(EtlExecutionContext etlExecutionContext, string taskUpdatefilePath, byte[]? taskUpdateFileContent, string environment)
        {
            const string methodName=nameof(ProcessTaskUpdateWithCustomFormat);
            _logger.LogInformation("{ClassName}.{MethodName} - processing task-update file: {FilePath}, Env: {Env}", className, methodName, taskUpdatefilePath, environment);
           
            using (var reader = (taskUpdateFileContent != null && taskUpdateFileContent?.Length > 0) ?
             new StreamReader(new MemoryStream(taskUpdateFileContent)) : new StreamReader(taskUpdatefilePath))

            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = "\t" , PrepareHeaderForMatch = args => args.Header.ToLowerInvariant() }))
            {
                ETLTenantModel? tenant = null;
                var buffer = new List<ETLMemberTaskUpdateDetailDto>();
                var batchSize = 100;
                while (await csv.ReadAsync())
                {
                    try
                    {
                        var record = csv.GetRecord<EtlTaskUpdateCustomFormatRecordDto>();
                        if (record == null || !IsValidRecord(record))
                            continue;

                        tenant = await _tenantRepo.FindOneAsync(x => x.PartnerCode == record.PartnerCode && x.DeleteNbr == 0);
                        if (tenant == null)
                            continue;

                        var isSupportLiveTransferToRewardsPurse = GetSupportLiveTransferToRewardsPurseFlag(tenant);

                        var tenantCode = tenant.TenantCode!;

                        ETLConsumerModel? consumer = await _personRepo.GetConsumerByPersonUniqueIdentifierAndTenantCode(record.PersonUniqueIdentifier, tenantCode);
                        if (consumer == null)
                            continue;

                        var taskExternalMapping = await GetTaskExternalMapping(record, tenantCode);
                        if (taskExternalMapping == null)
                            continue;

                        var taskReward = await GetTaskReward(taskExternalMapping, tenantCode, record);
                        _logger.LogInformation(taskReward.ToJson());
                        if (taskReward == null)
                            continue;

                        var completionCheck = await CheckCompletionValidity(record, taskReward);
                        if (!completionCheck.IsTaskCompletionValid)
                            continue;

                        if (!string.IsNullOrWhiteSpace(tenant.UtcTimeOffset)
                                && completionCheck.CompletionDate.HasValue)
                        {
                            // "UTC-08:00" → "-08:00"
                            var offsetPart = tenant.UtcTimeOffset.Replace("UTC", "");

                            if (TimeSpan.TryParse(offsetPart, out var offset))
                            {
                                // PST → UTC (invert the offset)
                                completionCheck.CompletionDate =
                                    completionCheck.CompletionDate.Value.Subtract(offset);
                            }
                        }



                        var task = await GetTask(taskReward);
                        if (task == null) continue;

                        buffer.Add(new ETLMemberTaskUpdateDetailDto
                        {
                            Completion = true,
                            MemberId = consumer.MemberId,
                            Progress = 100,
                            TaskId = task.TaskId,
                            TaskName = task.TaskName,
                            SupportLiveTransferToRewardsPurse = isSupportLiveTransferToRewardsPurse,
                            IsAutoEnrollEnabled = true,
                            TaskCompletedTs = completionCheck.CompletionDate,
                            PartnerCode = record.PartnerCode,
                            SkipValidation = completionCheck.SkipValidation
                        });

                        if (buffer.Count >= batchSize)
                        {
                            await ProcessBatchAsync(etlExecutionContext, buffer, tenant);
                            buffer.Clear();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "{ClassName}.{MethodName} - Error occured while processing task update with custome format,ErrorCode:{Code}, ERROR:{Msg}", className, methodName,StatusCodes.Status500InternalServerError, ex.Message);
                        await _s3FileLogger.AddErrorLogs(new S3LogContext()
                        {
                            Message = $"ProcessTaskUpdateWithCustomFormat: Error={ex.Message}",
                            TenantCode = etlExecutionContext?.TenantCode,
                            Ex = ex
                        });
                    }
                    
                }

                if (buffer.Count > 0 && tenant != null)
                {
                    await ProcessBatchAsync(etlExecutionContext, buffer , tenant);
                }
            }

            await _s3FileLogger.AddErrorLogs(new S3LogContext { Message = _failedRecords.ToJson() });
        }

        private static bool GetSupportLiveTransferToRewardsPurseFlag(ETLTenantModel tenant)
        {
            if (tenant != null)
            {
                var tenantOption = !string.IsNullOrEmpty(tenant.TenantOption)
                    ? JsonConvert.DeserializeObject<TenantOption>(tenant.TenantOption)
                    : new TenantOption();

                if (tenantOption?.Apps?.Any(x => string.Equals(x, Constants.Benefits, StringComparison.OrdinalIgnoreCase)) == true)
                {
                    var tenantAttributes = !string.IsNullOrEmpty(tenant.TenantAttribute)
                        ? JsonConvert.DeserializeObject<TenantAttribute>(tenant.TenantAttribute)
                        : new TenantAttribute();
                    return tenantAttributes?.SupportLiveTransferToRewardsPurse ?? false;
                }
            }

            return false;
        }

        private async Task<ETLConsumerModel?> GetConsumer(EtlTaskUpdateCustomFormatRecordDto record, string tenantCode)
        {
            const string methodName=nameof(GetConsumer);
            var consumer = await _personRepo.GetConsumerByPersonUniqueIdentifierAndTenantCode(record.PersonUniqueIdentifier, tenantCode);
            if (consumer == null)
            {
                var errorMessage = $"Error processing task update custom format file: consumer not found for PersonUniqueIdentifier={record.PersonUniqueIdentifier}, TaskThirdPartyCode={record.TaskThirdPartyCode}";
                _logger.LogError("{ClassName}.{MethodName} - {ErrorMessage}", className, methodName,errorMessage);
                await _s3FileLogger.AddErrorLogs(new S3LogContext { Message = errorMessage, TenantCode = tenantCode });
            }

            if (consumer?.EnrollmentStatus != Constants.ENROLLMENT_STATUS_ACTIVE)
            {
                var errorMessage = $"Error processing task update custom format file: consumer is not enrolled for PersonUniqueIdentifier={record.PersonUniqueIdentifier}, TaskThirdPartyCode={record.TaskThirdPartyCode}";
                _logger.LogError("{ClassName}.{MethodName} - {ErrorMessage}", className, methodName, errorMessage);
                await _s3FileLogger.AddErrorLogs(new S3LogContext { Message = errorMessage, TenantCode = tenantCode });
                return null;
            }

            return consumer;
        }

        private async Task<IList<ETLTaskExternalMappingModel>> GetTaskExternalMapping(EtlTaskUpdateCustomFormatRecordDto record, string tenantCode)
        {
            const string methodName = nameof(GetTaskExternalMapping);

            var normalizedModuleName = NormalizeModuleName(record.TaskThirdPartyCode);
            var taskExternalMapping = await _taskExternalMappingRepo.FindAsync(x => x.TenantCode == tenantCode &&
                                                                                      x.TaskThirdPartyCode == normalizedModuleName &&
                                                                                      x.DeleteNbr == 0);
            if (taskExternalMapping == null)
            {
                var errorMessage = $"Error processing task update custom format file: External task mapping not found for PersonUniqueIdentifier={record.PersonUniqueIdentifier}, TaskThirdPartyCode={record.TaskThirdPartyCode}";
                _logger.LogError("{ClassName}.{MethodName} - {errorMessage}", className, methodName,errorMessage);
                await _s3FileLogger.AddErrorLogs(new S3LogContext { Message = errorMessage, TenantCode = tenantCode });
            }

            return taskExternalMapping;
        }

        private async Task<ETLTaskRewardModel?> GetTaskReward(IList<ETLTaskExternalMappingModel> taskExternalMappings, string tenantCode, EtlTaskUpdateCustomFormatRecordDto record)
        {
            const string methodName = nameof(GetTaskReward);
            var completedTs = record.Completed;

            var taskExternalCodes = taskExternalMappings
                .Select(x => x.TaskExternalCode)
                .ToList();

            var taskReward = await _taskRewardRepo.FindOneAsync(x =>
                taskExternalCodes.Contains(x.TaskExternalCode) &&
                x.TenantCode == tenantCode &&
                x.DeleteNbr == 0 &&
                x.ValidStartTs <= completedTs &&
                x.Expiry >= completedTs
            );
            if (taskReward == null)
            {
                var errorMessage = $"Error processing task update custom format file: TaskReward not found for TaskExternalCode={taskExternalCodes}";
                _logger.LogError("{ClassName}.{MethodName} - {ErrorMessage}", className, methodName,errorMessage);
                await _s3FileLogger.AddErrorLogs(new S3LogContext { Message = errorMessage, TenantCode = tenantCode });
            }
            return taskReward;
        }

        private async Task<ETLTaskModel?> GetTask(ETLTaskRewardModel taskReward)
        {
            const string methodName=nameof(GetTask);
            var task = await _taskRepo.FindOneAsync(x => x.TaskId == taskReward.TaskId && x.DeleteNbr == 0);
            if (task == null)
            {
                var errorMessage = $"Error processing task update custom format file: Task not found for TaskId={taskReward.TaskId}";
                _logger.LogError("{ClassName}.{MethodName} - {ErrorMessage}", className, methodName,errorMessage);
                await _s3FileLogger.AddErrorLogs(new S3LogContext { Message = errorMessage, TenantCode = taskReward.TenantCode });
            }
            return task;
        }
        private readonly List<FailedRecordInfo> _failedRecords = new();

        bool IsFutureDate(DateTime? date) => date.HasValue && date.Value.Date > DateTime.UtcNow.Date;
        private CompletionEligibilityresult ValidResult(DateTime completionDate) =>
            new() { IsTaskCompletionValid = true, CompletionDate = completionDate };

        bool IsFutureTask(ETLTaskRewardModel task) => task.ValidStartTs.HasValue && task.ValidStartTs.Value.Date > DateTime.UtcNow.Date;
        bool IsTaskExpired(ETLTaskRewardModel task) => task.Expiry.Date < DateTime.UtcNow.Date;

        private CompletionEligibilityresult Fail(EtlTaskUpdateCustomFormatRecordDto record, string reason)
        {
            _failedRecords.Add(new FailedRecordInfo
            {
                Record = record,
                Reason = reason
            });

            return new CompletionEligibilityresult { IsTaskCompletionValid = false };
        }

        private bool IsInRange(DateTime date, DateTime start, DateTime end) =>
            date >= start && date <= end;



        private async Task<CompletionEligibilityresult> CheckCompletionValidity(EtlTaskUpdateCustomFormatRecordDto record, ETLTaskRewardModel taskReward)
        {
            const string methodName = nameof(CheckCompletionValidity);
            if (record.Completed == null || taskReward.ValidStartTs == null)
            {
                var errorReason = record.Completed == null ? "CompletedAt is null" : "ValidStartTs is null";
                var errorMessage = $"Task completion validation failed: {errorReason}. TaskId={taskReward.TaskId}, TenantCode={taskReward.TenantCode}";
                return Fail(record, errorReason);
            }

            var completedAt = record.Completed.Value;
            var validStart = taskReward.ValidStartTs.Value;
            var validEnd = taskReward.Expiry;
            var today = DateTime.UtcNow;

            var isFutureTask = validStart.Date > today.Date;
            var isExpiredTask = validEnd.Date < today.Date;

            // Load window config
            var eligibilityConfig = JsonConvert.DeserializeObject<CompletionEligibilityJson>(taskReward.CompletionEligibilityJson ?? string.Empty);
            var earlyWindowStart = validStart.AddDays(-(eligibilityConfig?.EarlyCompletionDays ?? 0));
            var lateWindowEnd = validEnd.AddDays(eligibilityConfig?.LateCompletionDays ?? 0);

            if (!isFutureTask && !isExpiredTask)
            {
                // Case 1: CompletedAt within [validStart, validEnd]
                if (IsInRange(completedAt, validStart, validEnd))
                {
                    if (!IsFutureDate(completedAt))
                        return ValidResult(completedAt);

                    // if completedAt is future , complete today
                    return ValidResult(today);

                }


                // Case 2: CompletedAt within [earlyWindowStart, validStart]
                if (IsInRange(completedAt, earlyWindowStart, validStart))
                {
                    return ValidResult(validStart);
                }

                // Case 3: CompletedAt within [validEnd, lateWindowEnd]
                if (IsInRange(completedAt, validEnd, lateWindowEnd))
                {
                    return ValidResult(today);
                }
            }

            // handle expired task and future task

            if (IsFutureTask(taskReward))
            {
                if (IsInRange(completedAt, earlyWindowStart, lateWindowEnd))
                {
                    return new CompletionEligibilityresult
                    {
                        IsTaskCompletionValid = true,
                        CompletionDate = completedAt,
                        SkipValidation = true
                    };
                }
                else
                {
                    //Extra future task
                    return Fail(record, $"Future task cannot be connot be completed as completedAt date : {completedAt} is not in date range for early and late window - {earlyWindowStart} -{lateWindowEnd}" );
                }
            }

            if (IsTaskExpired(taskReward))
            {
                // Check if completedAt is within valid start and end range
                if (IsInRange(completedAt, earlyWindowStart, lateWindowEnd))
                {
                    return new CompletionEligibilityresult
                    {
                        IsTaskCompletionValid = true,
                        CompletionDate = completedAt,
                        SkipValidation = true
                    };
                }
                else
                {
                    return Fail(record, $"Expired task cannot be connot be completed as completedAt date : {completedAt} is not in date range for early and late window - {earlyWindowStart} -{lateWindowEnd}");
                }
            }


            return Fail(record, "task does not fall in any validation window");
        }

        private async Task ProcessTaskUpdateWithCSV(EtlExecutionContext etlExecutionContext, string taskUpdatefilePath, byte[]? taskUpdateFileContent, string environment)
        {
            const string methodName=nameof(ProcessTaskUpdateWithCSV);
            _logger.LogInformation("{ClassName}.{MethodName} - processing task-update File: {FilePath}, Env: {Env}", className, methodName, taskUpdatefilePath, environment);

            ETLMemberTaskUpdatesRequestDto taskUpates = new ETLMemberTaskUpdatesRequestDto();

            taskUpates.MemberTaskUpdates = new List<ETLMemberTaskUpdateDetailDto>();
            using var reader = taskUpdateFileContent != null && taskUpdateFileContent.Length > 0
                ? new StreamReader(new MemoryStream(taskUpdateFileContent))
                : new StreamReader(taskUpdatefilePath);

            ETLTenantModel? tenant = null;
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
            {
                var batchSize = 100;
                var buffer = new List<ETLMemberTaskUpdateDetailDto>();

                while (await csv.ReadAsync())
                {
                    try
                    {
                        var record = csv.GetRecord<EtlTaskUpdateCsvRecordDto>();

                        if (record == null)
                            continue;


                        tenant = await _tenantRepo.FindOneAsync(x => x.PartnerCode == record.PartnerCode && x.DeleteNbr == 0);
                        var isSupportLiveTransferToRewardsPurse = GetSupportLiveTransferToRewardsPurseFlag(tenant);

                        var taskUpdateDto = record.ToTaskUpdateDto(environment);

                        var task = await _taskRepo.FindOneAsync(x => x.TaskCode == taskUpdateDto.TaskCode && x.DeleteNbr == 0);
                        if (task == null)
                        {
                            _logger.LogError("{ClassName}.{MethodName} - Task not found with TaskCode:{Code}", className, methodName, taskUpdateDto.TaskCode);
                            continue;
                        }

                        if (taskUpdateDto.ValidTask())
                        {
                            buffer.Add(new ETLMemberTaskUpdateDetailDto()
                            {
                                Completion = true,
                                MemberId = taskUpdateDto.MemberId,
                                Progress = 100,
                                TaskId = task.TaskId,
                                TaskName = taskUpdateDto.TaskName,
                                SupportLiveTransferToRewardsPurse = isSupportLiveTransferToRewardsPurse,
                                IsAutoEnrollEnabled = true
                            });

                            if (buffer.Count == batchSize)
                            {
                                await ProcessBatchAsync(etlExecutionContext, buffer, tenant);
                                buffer.Clear();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "{ClassName}.{MethodName} - Error occured while processing ProcessTaskUpdateWithCSV,ErrorCode:{Code}, ERROR:{Msg}", className, methodName,StatusCodes.Status500InternalServerError, ex.Message);
                        await _s3FileLogger.AddErrorLogs(new S3LogContext()
                        {
                            Message = $"ProcessTaskUpdateWithCSV: Error={ex.Message}",
                            TenantCode = etlExecutionContext?.TenantCode,
                            Ex = ex
                        });
                    }
                   
                }

                // Process the remaining records in the last batch
                if (buffer.Count > 0)
                {
                    await ProcessBatchAsync(etlExecutionContext, buffer, tenant);
                }
            }
        }

        private static bool IsValidRecord(EtlTaskUpdateCustomFormatRecordDto record)
        {
            return !string.IsNullOrEmpty(record.PersonUniqueIdentifier) && !string.IsNullOrEmpty(record.TaskThirdPartyCode) &&
           !string.IsNullOrEmpty(record.CompletionStatus) && record.CompletionStatus.ToUpper() == Constants.ASSIGNMENT_STATUS_COMPLETED 
           && !string.IsNullOrEmpty(record.PartnerCode);
        }

        private static string NormalizeModuleName(string moduleName)
        {
            string pattern = @"[\W_]+";
            string replaced = Regex.Replace(moduleName, pattern, " ").Trim().Replace(" ", "_")
                .ToLowerInvariant();
            return replaced;
        }

        /// <summary>
        /// Soft delete expired consumer tasks which are in pending state, cleanup recurring tasks progress as recurrence definition
        /// </summary>
        /// <param name="etlExecutionContext"></param>
        /// <returns></returns>
        public async Task ProcessRecurringTasks(EtlExecutionContext etlExecutionContext)
        {
            const string methodName = nameof(ProcessRecurringTasks);
            _logger.LogInformation("{ClassName}.{MethodName} - Started processing...", className, methodName);

            var tenants = await _tenantRepo.FindAsync(x => x.DeleteNbr == 0 );

            foreach (var tenant in tenants)
            {
                if (string.IsNullOrEmpty(tenant.TenantCode))
                {
                    _logger.LogError("{ClassName}.{MethodName} - Invaid tenant code {TenantCode}", className, methodName, tenant.TenantCode);
                    continue;
                }

                var batchSize = 1000;
                var now = DateTime.UtcNow;
                var pendingState = Constants.InProgress;
                var deletedRecordsCount = 0;

                var totalCount = await _consumerTaskRepo.GetConsumerTasksCount(tenant.TenantCode, pendingState);
                
                while (deletedRecordsCount < totalCount)
                {
                    var consumerTasksAndRewards = await _consumerTaskRepo.GetConsumerTasksWithRewards(tenant.TenantCode, pendingState, batchSize);

                    foreach (var taskAndReward in consumerTasksAndRewards)
                    {
                        try
                        {
                            var consumerTask = taskAndReward.ConsumerTask;
                            var taskReward = taskAndReward.TaskReward;
                            
                            //if (taskReward != null && now > taskReward.Expiry && taskReward.Expiry >= DateTime.MinValue && !taskReward.IsRecurring)
                            //{
                            //    consumerTask.DeleteNbr = consumerTask.ConsumerTaskId;
                            //    consumerTask.UpdateTs = DateTime.UtcNow;
                            //    await _consumerTaskRepo.UpdateAsync(consumerTask);
                            //}

                            if (taskReward != null && taskReward.IsRecurring && !string.IsNullOrEmpty(taskReward.RecurrenceDefinitionJson) && IsInValidRecurrence(consumerTask, taskReward.RecurrenceDefinitionJson))
                            {
                                // Mark the task for deletion if the recurrence is no longer valid
                                await DeleteTaskAsync(consumerTask);

                                // Mark the child tasks for deletion if the parent task recurrence is no longer valid
                                await DeleteChildTasksAsync(consumerTask);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "{ClassName}.{MethodName} - Exception during soft delete consumer task, TaskId: {TaskId},ErrorCode:{Code},ERROR: {Error}", className, methodName, taskAndReward.ConsumerTask.TaskId, StatusCodes.Status500InternalServerError,ex.Message);
                        }
                    }
                    deletedRecordsCount += batchSize;
                }

                var childConsumerTasks = await _consumerTaskRepo.GetAllChildConsumerTasks(tenant.TenantCode, pendingState);

                foreach (var childConsumerTask in childConsumerTasks)
                {
                    if (!string.IsNullOrEmpty(childConsumerTask.TenantCode) && childConsumerTask.ParentConsumerTaskId != null)
                    {
                        var parentTaskAndReward = await _consumerTaskRepo.GetConsumerTaskWithReward(childConsumerTask.TenantCode, (int)childConsumerTask.ParentConsumerTaskId, Constants.COMPLETED);

                        var consumerTask = parentTaskAndReward?.ConsumerTask;
                        var taskReward = parentTaskAndReward?.TaskReward;

                        if (consumerTask != null && taskReward != null && taskReward.IsRecurring && !string.IsNullOrEmpty(taskReward.RecurrenceDefinitionJson) && IsInValidRecurrence(consumerTask, taskReward.RecurrenceDefinitionJson))
                        {
                            // Mark the child tasks for deletion if the parent task recurrence is no longer valid
                            await DeleteChildTasksAsync(consumerTask);
                        }
                    }
                }
                // as part of RES-1253:
                // To ensure recurring tasks appear in the Available section for the new period,
                // the existing recurrence is deleted and re-created. Hence, the below logic for
                // creating new recurring tasks(posting consumer task) is temporarily commented.
                //_logger.LogInformation("{ClassName}.{MethodName} :Starting process for creating new RecurringTasks", className, methodName);
                //await _processRecurringTasksService.RecurringTaskCreationProcess(tenant.TenantCode);
            }

            _logger.LogInformation("{ClassName}.{MethodName} - successfully completed delete expired consumer tasks for TenantCode: {TenantCode}", className, methodName, etlExecutionContext.TenantCode);
        }

        private bool IsInValidRecurrence(ETLConsumerTaskModel consumerTask, string recurrenceDefinitionJson)
        {
            const string methodName = nameof(IsInValidRecurrence);
            // Deserialize the JSON input into a RecurrenceSettingsDto object
            var recurrenceDefinition = JsonConvert.DeserializeObject<RecurrenceSettingsDto>(recurrenceDefinitionJson);

            if (recurrenceDefinition?.Periodic?.PeriodRestartDate != null && recurrenceDefinition?.Periodic?.PeriodRestartDate > 28)
            {
                _logger.LogError("{ClassName}.{MethodName} - Invalid PeriodRestartDate: {PeriodRestartDate} for ConsumerTaskId: {ConsumerTaskId}. \n Valid PeriodRestartDate:[1-28]", className, methodName, recurrenceDefinition?.Periodic?.PeriodRestartDate, consumerTask.ConsumerTaskId);
                return false;
            }

            // Verify Monthly Recurrence
            // Check if the recurrence type is periodic and the period is monthly
            // Validate if the monthly recurrence is still valid using the PeriodRestartDate
            if (string.Equals(recurrenceDefinition?.RecurrenceType, Constants.PeriodicRecurrenceType, StringComparison.OrdinalIgnoreCase)
                && string.Equals(recurrenceDefinition?.Periodic?.Period, Constants.MonthlyPeriodType, StringComparison.OrdinalIgnoreCase)
                && recurrenceDefinition?.Periodic?.PeriodRestartDate != null
                && !IsValidMonthlyRecurrence(consumerTask.TaskStartTs,recurrenceDefinition.Periodic.PeriodRestartDate))
            {
                // Mark the task for deletion if the monthly recurrence is no longer valid
                return true;
            }

            // Verify Quarterly Recurrence
            // Check if the recurrence type is periodic and the period is quarterly
            // Validate if the quarterly recurrence is still valid using the PeriodRestartDate
            else if (string.Equals(recurrenceDefinition?.RecurrenceType, Constants.PeriodicRecurrenceType, StringComparison.OrdinalIgnoreCase)
                && string.Equals(recurrenceDefinition?.Periodic?.Period, Constants.QuarterlyPeriodType, StringComparison.OrdinalIgnoreCase)
                && recurrenceDefinition?.Periodic?.PeriodRestartDate != null
                && !IsValidQuarterlyRecurrence(consumerTask.TaskStartTs,recurrenceDefinition.Periodic.PeriodRestartDate))
            {
                // Mark the task for deletion if the quarterly recurrence is no longer valid
                return true;
            }

            // Verify Scheduled Recurrence
            // Check if the recurrence type is scheduled
            // Validate if the recurrence schedules are valid
            else if (string.Equals(recurrenceDefinition?.RecurrenceType, Constants.ScheduleRecurrenceType, StringComparison.OrdinalIgnoreCase)
                && !IsValidScheduleRecurring(consumerTask.TaskStartTs,recurrenceDefinition?.Schedules))
          {
                // Mark the task for deletion if no valid recurrence schedule is found
                return true;
            }

            return false;
        }

        /// <summary>
        /// DeleteChildTasksAsync
        /// </summary>
        /// <param name="consumerTask"></param>
        /// <returns></returns>
        private async Task DeleteChildTasksAsync(ETLConsumerTaskModel parentConsumerTask)
        {
            if (!string.IsNullOrEmpty(parentConsumerTask.TenantCode) && parentConsumerTask.ConsumerTaskId > 0)
            {
                var childConsumerTasks = await _consumerTaskRepo.GetChildConsumerTasks(parentConsumerTask.TenantCode, (int)parentConsumerTask.ConsumerTaskId, Constants.InProgress);

                foreach (var childConsumerTask in childConsumerTasks)
                {
                    // Set the delete number to the ID of the task, marking it for deletion
                    childConsumerTask.DeleteNbr = childConsumerTask.ConsumerTaskId;

                    // Update UpdateUser field with a constant value for ETL processing
                    childConsumerTask.UpdateUser = Constants.UpdateUser;

                    // Update the timestamp to the current date and time without specifying the time zone, indicating when the task was marked for deletion
                    childConsumerTask.UpdateTs = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

                    // Asynchronously update the task in the repository to persist the changes
                    await _consumerTaskRepo.UpdateAsync(childConsumerTask);
                }
            }
        }

        /// <summary>
        /// DeleteTaskAsync
        /// </summary>
        /// <param name="consumerTask"></param>
        /// <returns></returns>
        private async Task DeleteTaskAsync(ETLConsumerTaskModel consumerTask)
        {
            // Set the delete number to the ID of the task, marking it for deletion
            consumerTask.DeleteNbr = consumerTask.ConsumerTaskId;

            // Update UpdateUser field with a constant value for ETL processing
            consumerTask.UpdateUser = Constants.UpdateUser;

            // Update the timestamp to the current date and time without specifying the time zone, indicating when the task was marked for deletion
            consumerTask.UpdateTs = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

            // Asynchronously update the task in the repository to persist the changes
            await _consumerTaskRepo.UpdateAsync(consumerTask);
        }

        /// <summary>
        /// Return true if the calculated monthly recurrence end date is in the future
        /// </summary>
        /// <param name="periodRestartDate"></param>
        /// <returns></returns>
        private static bool IsValidMonthlyRecurrence(DateTime existingStartDate,int periodRestartDate)
        {
            // Get the current date in UTC (with only the date part, no time)
            var today = DateTime.UtcNow.Date;

            // Start with the current month
            var nextMonth = today;

            // If the period restart date has already passed in the current month, move to the next month
            if (periodRestartDate < today.Day)
            {
                nextMonth = today.AddMonths(1);
            }

            // Get the number of days in the target month (current or next month as determined above)
            int daysInNextMonth = DateTime.DaysInMonth(nextMonth.Year, nextMonth.Month);

            // Determine the target day, which is either the period restart date or the last day of the month if it exceeds the month's days
            int targetDay = Math.Min(periodRestartDate, daysInNextMonth);

            // Set the monthly recurrence end date to the calculated target date in the identified month
            var monthlyRecurrenceEndDate = new DateTime(nextMonth.Year, nextMonth.Month, targetDay);

            // Return true if the calculated monthly recurrence end date is in the future
            return monthlyRecurrenceEndDate > today|| (targetDay== existingStartDate.Day && existingStartDate.Month==today.Month && existingStartDate.Year==today.Year);
        }

        /// <summary>
        /// Return true if the quarterly recurrence end date is in the future
        /// </summary>
        /// <param name="periodRestartDate"></param>
        /// <returns></returns>
        private static bool IsValidQuarterlyRecurrence(DateTime existingaTaskStartDate,int periodRestartDate)
        {
            // Get the current date in UTC (with only the date part, no time)
            var today = DateTime.UtcNow.Date;

            // Calculate the quarterly recurrence end date based on today's date and the period restart day
            var (quarterlyRecurrenceStartDate, quarterlyRecurrenceEndDate) = GetQuarterlyRecurrenceEndDate(today, periodRestartDate);

            // Return true if the quarterly recurrence end date is in the future
            return (quarterlyRecurrenceEndDate > today && quarterlyRecurrenceStartDate<today) && !(existingaTaskStartDate< quarterlyRecurrenceStartDate);
        }

        /// <summary>
        /// GetQuarterlyRecurrenceEndDate
        /// </summary>
        /// <param name="givenDate"></param>
        /// <param name="periodRestartDate"></param>
        /// <returns></returns>
        public static (DateTime,DateTime) GetQuarterlyRecurrenceEndDate(DateTime givenDate, int periodRestartDate)
        {
            // Define the start dates for each quarter based on the given date's year and periodRestartDate
            var quarter1StartDate = new DateTime(givenDate.Year, 1, periodRestartDate, 0, 0, 0, DateTimeKind.Utc);
            var quarter2StartDate = new DateTime(givenDate.Year, quarter1StartDate.AddMonths(3).Month, periodRestartDate, 0, 0, 0, DateTimeKind.Utc);
            var quarter3StartDate = new DateTime(givenDate.Year, quarter2StartDate.AddMonths(3).Month, periodRestartDate, 0, 0, 0, DateTimeKind.Utc);
            var quarter4StartDate = new DateTime(givenDate.Year, quarter3StartDate.AddMonths(3).Month, periodRestartDate, 0, 0, 0, DateTimeKind.Utc);
            var lastquarterStartDate = new DateTime(quarter1StartDate.Year - 1, quarter1StartDate.AddMonths(-3).Month, periodRestartDate, 0, 0, 0, DateTimeKind.Utc);


            // Determine the quarter based on the given date
            if (givenDate < quarter1StartDate)
            {
               var quarterEndDate = new DateTime(quarter1StartDate.Year, quarter1StartDate.Month, periodRestartDate-1, 0, 0, 0, DateTimeKind.Utc);

                // Before the first quarter of the year
                return (lastquarterStartDate, quarterEndDate);
            }
            else if (givenDate >= quarter1StartDate && givenDate < quarter2StartDate)
            {

                // In the first quarter
                return (quarter1StartDate,quarter2StartDate.AddDays(-1));
            }
            else if (givenDate >= quarter2StartDate && givenDate < quarter3StartDate)
            {
                // In the second quarter
                return (quarter2StartDate,quarter3StartDate.AddDays(-1));
            }
            else if (givenDate >= quarter3StartDate && givenDate < quarter4StartDate)
            {
                // In the third quarter
                return (quarter3StartDate,quarter4StartDate.AddDays(-1));
            }
            else
            {

                // In the fourth quarter
                return (quarter4StartDate, quarter1StartDate.AddDays(-1));
            }
        }

        /// <summary>
        /// IsValidScheduleRecurring
        /// </summary>
        /// <param name="schedules"></param>
        /// <returns></returns>
        public static bool IsValidScheduleRecurring(DateTime existingTaskDate, ScheduleSettings[]? schedules)
        {
            bool result = false;
            if (schedules?.Length > 0)
            {
                // Filter the schedules based on valid start and expiry dates
                var validSchedules = schedules.Where(schedule => !string.IsNullOrEmpty(schedule.StartDate) && !string.IsNullOrEmpty(schedule.ExpiryDate))
                    .Select(schedule => new
                    {
                        StartDate = DateTime.ParseExact($"{DateTime.Now.Year}-{schedule.StartDate}", "yyyy-MM-dd", CultureInfo.InvariantCulture),
                        ExpiryDate = DateTime.ParseExact($"{DateTime.Now.Year}-{schedule.ExpiryDate}", "yyyy-MM-dd", CultureInfo.InvariantCulture)
                    });

                // Current date
                var today = DateTime.UtcNow;

                // Check if task complete date falls within any valid schedule range
                result = validSchedules.Any(schedule => today >= schedule.StartDate && today <= schedule.ExpiryDate && !(existingTaskDate< schedule.StartDate));
            }
            return result;
        }

        /// <summary>
        /// UpdateTaskAsCompleted
        /// </summary>
        /// <param name="taskUpdateRequestDto"></param>
        /// <returns></returns>
        public async Task<ConsumerTaskUpdateResponseDto> UpdateTaskAsCompleted(TaskUpdateRequestDto taskUpdateRequestDto)
        {
            const string methodName = nameof(UpdateTaskAsCompleted);
            try
            {
                var taskUpdateResponse = await _adminClient.PostFormData<ConsumerTaskUpdateResponseDto>("admin/consumer/task-update", taskUpdateRequestDto);

                if (taskUpdateResponse == null || (taskUpdateResponse.ErrorCode != null && taskUpdateResponse.ErrorCode != 200) ||
                    taskUpdateResponse.ConsumerTask == null)
                {
                    _logger.LogError("{ClassName}.{MethodName} - Failed to update the task. ErrorCode: {ErrorCode}, ErrorMessage: {Response}, RequestData: {RequestData}",
                    className, methodName, taskUpdateResponse?.ErrorCode, taskUpdateResponse?.ErrorMessage, taskUpdateRequestDto.ToJson());
                    throw new ETLException(ETLExceptionCodes.ErrorFromAPI, taskUpdateResponse?.ErrorMessage ?? "Failed to update the task");
                }
                _logger.LogInformation("{ClassName}.{MethodName} - Task updated successfully. TaskId: {TaskId}, ConsumerCode: {ConsumerCode}",
                              className, methodName, taskUpdateRequestDto.TaskId, taskUpdateRequestDto.ConsumerCode);
                return taskUpdateResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - An exception occurred. ErrorCode: {ErrorCode}, Request: {Request}",
                className, methodName, StatusCodes.Status500InternalServerError, taskUpdateRequestDto);
                throw;
            }
        }
    }
}
