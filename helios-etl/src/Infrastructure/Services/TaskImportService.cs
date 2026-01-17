extern alias SunnyRewards_Task;

using Amazon.DynamoDBv2.Model;
using CsvHelper;
using CsvHelper.Configuration;
using FirebaseAdmin.Messaging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NHibernate.Type;
using PuppeteerSharp.Helpers;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using SunnyRewards.Helios.Etl.Core.Domain.Dtos;
using SunnyRewards.Helios.Etl.Infrastructure.Helpers;
using SunnyRewards.Helios.ETL.Common.Constants;
using SunnyRewards.Helios.ETL.Common.CustomException;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Core.Domain.Enums;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using SunnyRewards.Helios.ETL.Infrastructure.AwsConfig;
using SunnyRewards.Helios.ETL.Infrastructure.Logs;
using SunnyRewards.Helios.ETL.Infrastructure.Logs.Interface;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Tenant.Core.Domain.Models;
using SunnyRewards_Task::SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards_Task::SunnyRewards.Helios.Task.Core.Domain.Models;
using System;
using System.Globalization;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ISession = NHibernate.ISession;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services
{
    public class TaskImportService : AwsConfiguration, ITaskImportService
    {
        private readonly ILogger<TaskImportService> _logger;
        private readonly IVault _vault;
        private readonly ISession _session;
        private readonly ITaskTypeRepo _taskTypeRepo;
        private readonly ITaskCategoryRepo _taskcategoryRepo;
        private readonly ITaskDetailRepo _taskdetailRepo;
        private readonly IRewardTypeRepo _rewardTypeRepo;
        private readonly ITaskRepo _taskRepo;
        private readonly ITaskRewardRepo _taskRewardRepo;
        private readonly ICohortRepo _cohortRepo;
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly IAwsS3Service _awsS3Service;
        private readonly IJobReportService _jobReportService;
        private readonly ITermOfServiceRepo _termOfServiceRepo;
        private readonly IConfiguration _configuration;

        private const string className = nameof(TaskImportService);

        private readonly IdGenerator _idGenerator = new IdGenerator(10, 4);
        private readonly IS3FileLogger _s3FileLogger;
        private static string BULLET = "{{BULLET}}";

        private static string RECURRING_DEFN_MONTH_JSON = "{\r\n  \"periodic\": {\r\n    \"period\": \"MONTH\",\r\n    \"periodRestartDate\": 1\r\n  },\r\n  \"recurrenceType\": \"PERIODIC\"\r\n}";
        private static string RECURRING_DEFN_QUARTER_JSON = "{\r\n  \"periodic\": {\r\n    \"period\": \"QUARTER\",\r\n    \"periodRestartDate\": 1\r\n  },\r\n  \"recurrenceType\": \"PERIODIC\"\r\n}";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="awsQueueService"></param>
        public TaskImportService(ILogger<TaskImportService> logger, IJobReportService jobReportService, IVault vault, ISession session, ITaskTypeRepo taskTypeRepo, IRewardTypeRepo rewardTypeRepo, ITaskRepo taskRepo,
            ITaskRewardRepo taskRewardRepo, ICohortRepo cohortRepo,
            IS3FileLogger s3FileLogger, IAwsS3Service awsS3Service, IConfiguration configuration,
        ITaskCategoryRepo taskcategoryRepo, ITaskDetailRepo taskDetailRepo, ITermOfServiceRepo termOfService
            ) : base(vault, configuration)
        {
            _logger = logger;
            _jobReportService = jobReportService;
            _vault = vault;
            _session = session;
            _taskTypeRepo = taskTypeRepo;
            _rewardTypeRepo = rewardTypeRepo;
            _taskRepo = taskRepo;
            _taskRewardRepo = taskRewardRepo;
            _cohortRepo = cohortRepo;
            _s3FileLogger = s3FileLogger;
            _awsS3Service = awsS3Service;
            _taskcategoryRepo = taskcategoryRepo;
            _taskdetailRepo = taskDetailRepo;
            _termOfServiceRepo = termOfService;
            _configuration = configuration;
        }

        public async Task<EtlTaskImportFileResponseDto> ImportTaskAsync(EtlExecutionContext etlExecutionContext)
        {
            const string methodName = nameof(ImportTaskAsync);
            var response = new EtlTaskImportFileResponseDto();
            var taskFilePath = etlExecutionContext.TaskImportFilePath;

            _logger.LogInformation("{ClassName}.{MethodName} - Started processing for TaskImportFilePath: {TaskImportFilePath}, TaskImportFileContents: {TaskImportFileContents}", className, methodName, etlExecutionContext.TaskImportFilePath, etlExecutionContext.TaskImportFileContents);

            try
            {
                if (string.IsNullOrEmpty(taskFilePath))
                {
                    _logger.LogError("{ClassName}.{MethodName} - taskFilePath is not valid", className, methodName);
                    throw new ETLException(ETLExceptionCodes.NullValue, "Task file path is not valid");
                }

                // retrieve environment from vault
                var environment = await _vault.GetSecret("env");
                if (string.IsNullOrEmpty(environment))
                {
                    _logger.LogError("{ClassName}.{MethodName} - environment is not valid", className, methodName);
                    throw new ETLException(ETLExceptionCodes.AWSSecretNotFound, "Secret is not configured AWS secret manager for 'env'");
                }

                _logger.LogInformation("{ClassName}.{MethodName} - processing task File: {FilePath}, Env: {Env}", className, methodName, taskFilePath, environment);
                var taskImportFileContents = etlExecutionContext.TaskImportFileContents;
                var taskText = taskImportFileContents?.Length > 0 ? Encoding.UTF8.GetString(taskImportFileContents!) : File.ReadAllText(taskFilePath);
                var tasks = JsonConvert.DeserializeObject<List<EtlTaskImportDto>>(taskText);
                if (tasks == null || tasks.Count == 0)
                {
                    _logger.LogError("{ClassName}.{MethodName} - No task records found in the file", className, methodName);
                    throw new ETLException(ETLExceptionCodes.NullValue, "No task records found in the file");
                }
                foreach (var task in tasks)
                {
                    response.TotalRecordsReceived++;
                    try
                    {
                        _logger.LogInformation("{ClassName}.{MethodName} - Started processing.Request {Record}", className, methodName, task.ToJson());
                        bool isTaskImported = await ImportTask(etlExecutionContext.TenantCode, task, environment);
                        if (isTaskImported)
                        {
                            // Increment the successful records count and add the record to the list of imported records.
                            response.ImportedTaskRecords?.Add(task);
                            response.TotalSuccessfulRecords++;
                        }
                        else
                        {
                            // Increment the failed records count as the task import was unsuccessful.
                            response.TotalFailedRecords++;
                        }
                    }
                    catch (Exception innerException)
                    {
                        _logger.LogWarning(innerException, "{ClassName}.{MethodName} - Failed to process record. TaskHeader: {TaskHeader}, TaskExternalCode: {TaskExternalCode}", className, methodName, task?.TaskName, task?.TaskExternalCode);
                        response.TotalFailedRecords++;
                    }
                    response.TotalRecordsProcessed++;
                }

                _logger.LogInformation("{ClassName}.{MethodName} - Ended processing successfully.", className, methodName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} Failed processing. ErrorCode:{Code}, ErrorMessage:{Message}", className, methodName, StatusCodes.Status500InternalServerError, ex.Message);
                await _s3FileLogger.AddErrorLogs(new S3LogContext()
                {
                    Message = $"ProcessTaskImportAsync: Error={ex.Message}",
                    TenantCode = etlExecutionContext?.TenantCode,
                    Ex = ex
                });
            }

            return response;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tenantCode"></param>
        /// <param name="taskDefinitionfilePath"></param>
        /// <returns></returns>
        public async Task Import(string tenantCode, string taskDefinitionfilePath = "")
        {
            const string methodName = nameof(Import);
            try
            {
                if (string.IsNullOrEmpty(taskDefinitionfilePath))
                {
                    _logger.LogError("{ClassName}.{MethodName} - taskDefinitionfilePath is null or empty", className, methodName);
                    throw new ETLException(ETLExceptionCodes.NullValue, "TaskDefinition filePath is null or empty");
                }

                // Retrieve environment from vault
                var environment = await _vault.GetSecret("env");
                if (string.IsNullOrEmpty(environment))
                {
                    _logger.LogError("{ClassName}.{MethodName} - environment is null or empty", className, methodName);
                    throw new ETLException(ETLExceptionCodes.AWSSecretNotFound, "AWS secret is not configured in secret manager for 'env'");
                }

                _logger.LogInformation(
                    "{ClassName}.{MethodName} - processing task-definition file: {FilePath}, Env: {Env}",
                    className, methodName, taskDefinitionfilePath, environment);

                await using var stream = File.OpenRead(taskDefinitionfilePath);

                // ✅ Deserialize JSON array (synchronously in memory)
                var records = await System.Text.Json.JsonSerializer.DeserializeAsync<List<EtlTaskImportDto>>(stream, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    ReadCommentHandling = System.Text.Json.JsonCommentHandling.Skip,
                    AllowTrailingCommas = true
                });

                if (records == null || records.Count == 0)
                {
                    _logger.LogWarning("{ClassName}.{MethodName} - No records found in JSON file", className, methodName);
                    return;
                }

                int idx = 0;
                foreach (var record in records)
                {
                    _logger.LogInformation(
                        "{ClassName}.{MethodName} - Id:{Id}, Task: {TaskHeader}, task_ext_code: {TaskExternalCode}",
                        className, methodName, idx, record.TaskName, record.TaskExternalCode);

                    await ImportTask(tenantCode, record, environment);
                    idx++;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "{ClassName}.{MethodName} - Error occurred while task import, ErrorCode:{Code}, ERROR:{Msg}",
                    className, methodName, StatusCodes.Status500InternalServerError, ex.Message);

                // this log using for s3FileLogger
                await _s3FileLogger.AddErrorLogs(new S3LogContext
                {
                    Message = ex.Message,
                    TenantCode = tenantCode,
                    Ex = ex
                });

                throw;
            }
        }


        private async Task<bool> ImportTask(string tenantCode, EtlTaskImportDto taskDef, string env)
        {
            const string methodName = nameof(ImportTask);
            if (string.IsNullOrEmpty(taskDef.TaskName) || string.IsNullOrEmpty(taskDef.Cohort)|| taskDef.PlanYear<=0)
            {
                _logger.LogError("{ClassName}.{MethodName} - Cannot import with null/empty task name or cohort label", className, methodName);
                return false;
            }
            var planYearSuffix = "_" + taskDef.PlanYear.ToString().Trim();
            taskDef.TaskName = taskDef.TaskName.Trim() + planYearSuffix;
            string enUSTaskHeader = string.Empty;

            if (taskDef.LocalizeInfo != null)
            {
                enUSTaskHeader = taskDef.LocalizeInfo.Where(x => x.Language == "en-US").FirstOrDefault()?.TaskHeader ?? string.Empty;
            }
            

            var now = DateTime.UtcNow;
            var createUser = "per-915325069cdb42c783dd4601e1d27704";
            var taskExternalCode = _idGenerator.GenerateTaskIdentifier(taskDef.TaskName);


            using var transaction = _session.BeginTransaction();
            try
            {
                var existingTaskRewards = await _taskRewardRepo.FindAsync(x => x.TenantCode == tenantCode && x.TaskExternalCode == taskExternalCode && x.DeleteNbr == 0);
                bool taskRewardExists = existingTaskRewards != null && existingTaskRewards.Count > 0;

                if (taskRewardExists)
                {
                    _logger.LogInformation("{ClassName}.{MethodName} - TaskExternalCode:{Code}, already exists in tenant: {Tenant}", className, methodName, taskExternalCode, tenantCode);
                }

                var cohort = await _cohortRepo.FindOneAsync(x => x.CohortName.ToUpper() == taskDef.Cohort.ToUpper() && x.DeleteNbr == 0);
                if (cohort == null)
                {
                    _logger.LogError("{ClassName}.{MethodName} - Cohort not found for CohortName: {Cohort}", className, methodName, taskDef.Cohort);
                    await transaction.RollbackAsync();
                    return false;
                }

                long? taskId = taskRewardExists ? existingTaskRewards?.FirstOrDefault()?.TaskId : null;


                var taskModel = await GetTask(taskDef.TaskName);
                ETLTaskTypeModel? taskTypeModel = null;
                if (!string.IsNullOrEmpty(taskDef.TaskTypeName))
                    taskTypeModel = await _taskTypeRepo.FindOneAsync(x => x.TaskTypeName.ToUpper() == taskDef.TaskTypeName.ToUpper() && x.DeleteNbr == 0);
                ETLTaskCategoryModel? taskCategoryModel = null;
                if (!string.IsNullOrEmpty(taskDef.TaskCategory))
                    taskCategoryModel = await _taskcategoryRepo.FindOneAsync(x => x.TaskCategoryName.ToUpper() == taskDef.TaskCategory.ToUpper() && x.DeleteNbr == 0);
                var taskTypeId = taskTypeModel?.TaskTypeId > 0 ? taskTypeModel.TaskTypeId : 1;
                var taskCategoryId = taskCategoryModel?.TaskCategoryId > 0 ? taskCategoryModel.TaskCategoryId : 1;

                if (taskModel == null && taskTypeId > 0 && taskId == null && !taskRewardExists)
                {
                    var task = new ETLTaskModel
                    {
                        TaskTypeId = taskTypeId,
                        TaskCategoryId = taskCategoryId,
                        TaskCode = CreateCode("tsk"),
                        TaskName = taskDef.TaskName,
                        SelfReport = taskDef.SelfReport ?? false,
                        ConfirmReport = false,
                        CreateTs = now,
                        CreateUser = createUser,
                        UpdateUser = null,
                        DeleteNbr = 0
                    };

                    await _session.SaveAsync(task);
                    taskId = task.TaskId;
                }
                else if (taskModel != null && taskTypeId > 0)
                {

                    taskModel.TaskTypeId = taskTypeId;
                    taskModel.TaskCategoryId = taskCategoryId;
                    taskModel.TaskName = taskDef.TaskName;
                    taskModel.SelfReport = taskDef.SelfReport ?? false;
                    taskModel.ConfirmReport = false;
                    taskModel.UpdateTs = now;
                    taskModel.UpdateUser = Constants.UpdateUser;


                    await _session.UpdateAsync(taskModel);
                    taskId = taskModel.TaskId;
                }


                if (taskId == null)
                {
                    _logger.LogError("{ClassName}.{MethodName} - TaskReward exists but TaskId is null for TaskExternalCode: {TaskExtCode}", className, methodName, taskExternalCode);
                    await transaction.RollbackAsync();
                    return false;
                }

                string? taskRewardCode;
                //if (!taskRewardExists)
                /* PSEUDOCODE / PLAN (detailed)
                - Replace duplicated "localized / default locale" task detail upsert logic with a single helper.
                - Helper responsibilities:
                  1. Accept: taskId, EtlTaskImportDto taskDef, tenantCode, now, createUser, and current NHibernate transaction.
                  2. Build a list of locales to process:
                     - If taskDef.LocalizeInfo is present, use that list.
                     - Otherwise create a single locale from taskDef.Language, TaskHeader, TaskDescription, CTA.
                  3. For each locale:
                     - Normalize language (default "en-US").
                     - Query _termOfServiceRepo for TermsOfService entries matching language and not deleted.
                     - If none found: log error, rollback the passed transaction, and return false.
                     - Query _taskdetailRepo for existing detail by taskId, tenantCode, language (not deleted).
                     - Compute header, description (via existing GetTaskDescription), cta.
                     - If there is no existing detail: create ETLTaskDetailModel, set fields, call _session.SaveAsync.
                     - If exists: update properties, set UpdateTs and UpdateUser, call _session.UpdateAsync.
                  4. Return true if all locales processed successfully.
                - Replace the original duplicated block by a single call to the helper. If the helper returns false, bubble up false to the caller.
                - Keep logging consistent and concise; keep behavior identical regarding rollback and return values.
                - Use fully-qualified NHibernate.ITransaction type so the helper can call RollbackAsync on the same transaction object.
                */

                #region Refactored: Upsert task details (replaces duplicated localize/non-localize block)

                // Replace the original block with a single call to this helper. This call should be placed where
                // the original localized/non-localized logic existed.
                if (!await UpsertTaskDetailsAsync((long)taskId!, taskDef, tenantCode, now, createUser, transaction))
                {
                    // UpsertTaskDetailsAsync already rolled back the transaction and logged error.
                    return false;
                }


                // Insert TaskReward
                var rewardObject = new
                {
                    rewardType = taskDef.RewardType,
                    rewardAmount = taskDef.RewardValue,
                    membershipType = taskDef.MembershipType
                };
                var reward = JsonConvert.SerializeObject(rewardObject);

                var isRecurring = taskDef.Recurring ?? false;
                taskDef.FrequencyForRecurringActions = taskDef.FrequencyForRecurringActions
    ?.Trim()
    .Replace("-", "_")
    .ToUpperInvariant();
                if (isRecurring && !Enum.GetNames(typeof(ScheduleFrequency)).Contains(taskDef.FrequencyForRecurringActions.ToUpper()))
                {
                    _logger.LogError("{ClassName}.{MethodName} - TaskExternalCode: {TaskExtCode} invalid recurring periodicity: {RecurringPeriodicity}", className, methodName, taskExternalCode, taskDef.FrequencyForRecurringActions);
                    await transaction.RollbackAsync();
                    return false;
                }

                var rewardType = await _rewardTypeRepo.FindOneAsync(x => x.RewardTypeName.ToUpper() == taskDef.RewardType.ToUpper() && x.DeleteNbr == 0);
                int rewardTypeId = rewardType?.RewardTypeId > 0 ? (int)rewardType.RewardTypeId : 1;
                string? recurrenceDefinitionJson = null;
                if (isRecurring)
                    recurrenceDefinitionJson = GenerateRecurrenceScheduleJson(taskDef.FrequencyForRecurringActions, taskDef.RecurringJson);
                string? taskcompletionCriteria = null;
                if (!string.IsNullOrEmpty(taskDef.CompletionCriteriaType)
                    && Enum.GetNames(typeof(CompletionCriteriaType)).Contains(taskDef.CompletionCriteriaType.ToUpper()))
                {
                    taskcompletionCriteria = await BuildCompletionCriteriaFromInput(taskDef.CompletionCriteriaType, taskDef.CompletionUIComponents, taskDef.TaskCompletionSteps, taskDef.FrequencyForRecurringActions, tenantCode, env);
                }
                if (!taskRewardExists)
                {
                    taskRewardCode = CreateCode("trw");

                    var taskReward = new ETLTaskRewardModel
                    {
                        TaskId = (long)taskId,
                        RewardTypeId = rewardTypeId,
                        TaskActionUrl = taskDef.DeepLink,
                        TenantCode = tenantCode,
                        TaskRewardCode = taskRewardCode,
                        Reward = reward,
                        Priority = taskDef.Priority ?? 0,
                        MinTaskDuration = 0,
                        MaxTaskDuration = 0,
                        TaskExternalCode = taskExternalCode,
                        CreateTs = now,
                        CreateUser = createUser,
                        UpdateUser = null,
                        DeleteNbr = 0,
                        // Replace this line:
                        // ValidStartTs = taskDef.StartDate,

                        // With this code:
                        ValidStartTs = DateTime.TryParse(taskDef.StartDate, out var validStartTs) ? validStartTs : (DateTime?)null,
                        Expiry = TryParseToDate(taskDef.EndDate) ?? DateTime.MaxValue,
                        IsRecurring = isRecurring,
                        SelfReport = taskDef.SelfReport ?? false,
                        RecurrenceDefinitionJson = recurrenceDefinitionJson,
                        TaskCompletionCriteriaJson = taskcompletionCriteria
                    };
                    await _session.SaveAsync(taskReward);
                }
                else
                {
                    var taskRewardModel = existingTaskRewards?.OrderByDescending(x => x.TaskRewardId).FirstOrDefault();
                    taskRewardCode = taskRewardModel?.TaskRewardCode;
                    taskRewardModel.TaskId = (long)taskId;
                    taskRewardModel.RewardTypeId = rewardTypeId;
                    taskRewardModel.TaskActionUrl = taskDef.DeepLink;
                    taskRewardModel.TenantCode = tenantCode;
                    taskRewardModel.TaskRewardCode = taskRewardCode;
                    taskRewardModel.Reward = reward;
                    taskRewardModel.Priority = taskDef.Priority ?? 0;
                    taskRewardModel.MinTaskDuration = 0;
                    taskRewardModel.MaxTaskDuration = 0;
                    taskRewardModel.TaskExternalCode = taskExternalCode;
                    taskRewardModel.UpdateUser = Constants.UpdateUser;
                    taskRewardModel.UpdateTs = DateTime.UtcNow;
                    taskRewardModel.ValidStartTs = TryParseToDate(taskDef.StartDate);
                    taskRewardModel.Expiry = TryParseToDate(taskDef.EndDate) ?? DateTime.MaxValue;
                    taskRewardModel.IsRecurring = isRecurring;
                    taskRewardModel.SelfReport = taskDef.SelfReport ?? false;
                    taskRewardModel.RecurrenceDefinitionJson = recurrenceDefinitionJson;
                    taskRewardModel.TaskCompletionCriteriaJson = taskcompletionCriteria;
                    await _session.UpdateAsync(taskRewardModel);

                }

                if (string.IsNullOrEmpty(taskRewardCode))
                {
                    _logger.LogError("{ClassName}.{MethodName} - Unable to create/lookup Task Reward for TaskExternalCode: {TaskExtCode} in Tenant: {Tenant}", className, methodName, taskExternalCode, tenantCode);
                    await transaction.RollbackAsync();
                    return false;
                }

                // Check if cohort connection already exists
                var existingCohortTenantTaskReward = (from coh in _session.Query<ETLCohortTenantTaskRewardModel>()
                                                      .Where(x => x.CohortId == cohort.CohortId && x.TenantCode == tenantCode && x.TaskRewardCode == taskRewardCode && x.DeleteNbr == 0)
                                                      select coh).ToList();
                if (existingCohortTenantTaskReward != null && existingCohortTenantTaskReward.Count > 0)
                {
                    _logger.LogError("{ClassName}.{MethodName} - Cohort Tenant Task Reward already exists TaskReward: {CohortTenantTaskReward}", className, methodName, existingCohortTenantTaskReward[0].CohortTenantTaskRewardId);
                    //await transaction.RollbackAsync();
                    //return false;
                }
                else
                {

                    // Insert CohortTenantTaskReward
                    var cohortTenantTaskReward = new ETLCohortTenantTaskRewardModel
                    {
                        CohortId = (long)cohort.CohortId,
                        TenantCode = tenantCode,
                        TaskRewardCode = taskRewardCode,
                        Recommended = true,
                        Priority = taskDef.Priority ?? 0,
                        CreateTs = now,
                        CreateUser = createUser,
                        UpdateUser = null,
                        DeleteNbr = 0
                    };
                    await _session.SaveAsync(cohortTenantTaskReward);
                }
                await transaction.CommitAsync();
                return true;
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Error importing task with TaskExternalCode: {TaskExtCode},ErrorCode:{Code},ERROR: {Msg}", className, methodName, taskExternalCode, StatusCodes.Status500InternalServerError, ex.Message);
                await transaction.RollbackAsync();
                _session.Clear();
                return false;
            }
        }
        private static string CleanTaskName(string taskName)
        {
            if (string.IsNullOrEmpty(taskName))
            {
                return string.Empty;
            }
            // Convert to lowercase, remove whitespace, and remove all non-alphanumeric symbols
            string cleanedTaskName = taskName.ToLower().Replace(" ", "")
            .Replace("!", "")
            .Replace("@", "")
            .Replace("#", "")
            .Replace("$", "")
            .Replace("%", "")
            .Replace("^", "")
            .Replace("&", "")
            .Replace("*", "")
            .Replace("(", "")
            .Replace(")", "")
            .Replace("[", "")
            .Replace("]", "")
            .Replace("{", "")
            .Replace("}", "")
            .Replace(":", "")
            .Replace(";", "")
            .Replace("'", "")
            .Replace("<", "")
            .Replace(">", "")
            .Replace(",", "")
            .Replace(".", "")
            .Replace("?", "")
            .Replace("/", "")
            .Replace(@"\", "")
            .Replace(@"|", "")
            .Replace(@"`", "")
            .Replace(@"~", "")
            .Replace(@"+", "")
            .Replace(@"-", "")
            .Replace(@"=", "")
            .Replace(@"_", "");  // Convert to lowercase
            cleanedTaskName = Regex.Replace(cleanedTaskName, @"\s+", "");  // Remove whitespace
            cleanedTaskName = Regex.Replace(cleanedTaskName, @"\W", "");  // Remove non-alphanumeric characters

            return cleanedTaskName;
        }
        public DateTime? TryParseToDate(string? dateString)
        {
            if (string.IsNullOrWhiteSpace(dateString))
                return null;

            // Define allowed formats (add more if needed)
            string[] formats = { "MM-dd-yyyy" };

            if (DateTime.TryParseExact(
                    dateString,
                    formats,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime parsedDate))
            {
                return parsedDate;
            }

            return null; // return null if parsing fails
        }
        private async Task<ETLTaskModel?> GetTask(string taskName)
        {
            var cleanedTaskName = CleanTaskName(taskName);

            var task = await _taskRepo.FindAsync(x => x.TaskName != null && x.TaskName.ToLower().Replace(" ", "")
            .Replace("!", "")
            .Replace("@", "")
            .Replace("#", "")
            .Replace("$", "")
            .Replace("%", "")
            .Replace("^", "")
            .Replace("&", "")
            .Replace("*", "")
            .Replace("(", "")
            .Replace(")", "")
            .Replace("[", "")
            .Replace("]", "")
            .Replace("{", "")
            .Replace("}", "")
            .Replace(":", "")
            .Replace(";", "")
            .Replace("'", "")
            .Replace("<", "")
            .Replace(">", "")
            .Replace(",", "")
            .Replace(".", "")
            .Replace("?", "")
            .Replace("/", "")
            .Replace(@"\", "")
            .Replace(@"|", "")
            .Replace(@"`", "")
            .Replace(@"~", "")
            .Replace(@"+", "")
            .Replace(@"-", "")
            .Replace(@"=", "")
            .Replace(@"_", "")

            == cleanedTaskName);


            if (task == null)
            {
                return null;
            }
            return task.OrderByDescending(x => x.TaskId).FirstOrDefault();
        }
        private string CreateCode(string prefix)
        {
            return prefix + "-" + Guid.NewGuid().ToString("N");
        }

        private string GetTaskDescription(object? taskDefDescription)
        {
            if (taskDefDescription == null)
                return string.Empty;

            // Handle if input is a JToken, JObject, or JArray
            if (taskDefDescription is JToken jToken)
            {
                return jToken.Type switch
                {
                    JTokenType.Array => ExtractTextFromBlocks((JArray)jToken),
                    JTokenType.String => ProcessText(jToken.ToString()),
                    _ => jToken.ToString()
                };
            }

            // Handle plain string input
            if (taskDefDescription is string str)
            {
                str = str.Trim();

                // If it looks like a JSON array, just return as-is
                if (str.StartsWith("[") && str.EndsWith("]"))
                    return str;

                return ProcessText(str);
            }

            // Fallback for unexpected types
            return taskDefDescription.ToString() ?? string.Empty;
        }

        private string ProcessText(string text)
        {
            text = text.Trim();

            if (text.Contains(BULLET))
            {
                string[] parts = text.Split(BULLET, StringSplitOptions.RemoveEmptyEntries);

                var jsonBuilder = new StringBuilder("[");
                string[] bulletItems;

                if (!text.StartsWith(BULLET))
                {
                    jsonBuilder.AppendLine($@"
            {{
                ""type"": ""paragraph"",
                ""data"": {{
                    ""text"": ""{parts[0].Trim().Replace("\r", "").Replace("\n", "")}""
                }}
            }},");
                    bulletItems = parts.Skip(1).ToArray();
                }
                else
                {
                    bulletItems = parts;
                }

                string[] formattedBullets = bulletItems
                    .Select(item => $"\"{item.Trim().Replace("\r", "").Replace("\n", "")}\"")
                    .ToArray();

                string jsonItems = string.Join(",", formattedBullets);
                jsonBuilder.AppendLine($@"
            {{
              ""type"": ""list"",
              ""data"": {{
                ""style"": ""unordered"",
                ""items"": [
                    {jsonItems}
                ]
              }}
            }}
        ]");

                return jsonBuilder.ToString();
            }

            // plain text
            return text;
        }
        private string ExtractTextFromBlocks(JArray blocks)
        {
            var paragraphs = new List<string>();

            foreach (var block in blocks)
            {
                var type = block["type"]?.ToString();
                if (type == "paragraph")
                {
                    var text = block["data"]?["text"]?.ToString();
                    if (!string.IsNullOrWhiteSpace(text))
                        paragraphs.Add(text);
                }
                else if (type == "list")
                {
                    var items = block["data"]?["items"] as JArray;
                    if (items != null)
                    {
                        foreach (var item in items)
                            paragraphs.Add("• " + item.ToString());
                    }
                }
            }

            return string.Join("\n", paragraphs);
        }


        /* PSEUDOCODE / PLAN (detailed)
        - Parse optional recurrenceJson into JObject (recurrenceData), ignore parse errors.
        - Determine today's date and try to extract a startDate from recurrenceData.schedules[0].startDate (format "dd-MM").
        - Normalize recurrenceType to upper-case and switch on its value.
        - For each supported recurrence type produce a sequence of schedule objects (startDate / expiryDate) appropriate to that frequency:
          - DAILY: one-per-day for remainder of year (up to 365 iterations but break when year changes)
          - WEEKLY: one-per-week for remainder of year (52 iterations)
          - MONTHLY: one-per-month for remainder of year (12 iterations)
          - QUARTERLY: one-every-3-months for remainder of year (4 iterations)
          - BI_ANNUALLY: one-every-6-months (2 iterations)
          - ANNUALLY: single schedule covering one year
          - SCHEDULE: if input recurrenceData contains "schedules", copy them into output
        - Compose final JObject { "schedules": [ ... ] } and return JSON string (indented).
        - Use nameof(ScheduleFrequency.X) for case labels to avoid Enum.GetName misuse; recurrenceType is upper-cased so nameof values must match enum identifiers (assumed uppercase).
        */

        public string GenerateRecurrenceScheduleJson(string recurrenceType, RecurrenceSettingsDto inputDto)
        {
            const string methodName = nameof(GenerateRecurrenceScheduleJson);
            var result = new RecurrenceSettingsDto();
            var schedules = new List<ScheduleSettings>();

            try
            {
                _logger.LogInformation(
                    "{ClassName}.{MethodName} - Start generating recurrence schedule. RecurrenceType:{RecurrenceType}, HasRecurrenceJson:{HasJson}",
                    className, methodName, recurrenceType, inputDto.ToJson());

                DateTime today = DateTime.Today;
                DateTime startDate = today;

                // Extract start date if provided from input
                if (inputDto?.Schedules != null && inputDto.Schedules.Length > 0)
                {
                    var first = inputDto.Schedules.FirstOrDefault();
                    if (!string.IsNullOrWhiteSpace(first?.StartDate) &&
                        DateTime.TryParseExact(first.StartDate, "MM-dd",
                            CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedStart))
                    {
                        startDate = new DateTime(today.Year, parsedStart.Month, parsedStart.Day);
                    }
                }

                recurrenceType = recurrenceType?.Trim().ToUpperInvariant();

                result.RecurrenceType = "SCHEDULE";

                switch (recurrenceType)
                {
                    case nameof(ScheduleFrequency.DAILY):
                        for (int day = 0; day < 365; day++)
                        {
                            DateTime start = startDate.AddDays(day);
                            if (start.Year != today.Year) break;

                            schedules.Add(new ScheduleSettings
                            {
                                StartDate = start.ToString("MM-dd")
                            });
                        }
                        break;

                    case nameof(ScheduleFrequency.WEEKLY):
                        for (int week = 0; week < 52; week++)
                        {
                            DateTime start = startDate.AddDays(week * 7);
                            if (start.Year != today.Year) break;

                            schedules.Add(new ScheduleSettings
                            {
                                StartDate = start.ToString("MM-dd")
                            });
                        }
                        break;

                    case nameof(ScheduleFrequency.BI_ANNUALLY):
                        for (int i = 0; i < 2; i++)
                        {
                            DateTime start = startDate.AddMonths(i * 6);
                            if (start.Year != today.Year) break;

                            schedules.Add(new ScheduleSettings
                            {
                                StartDate = start.ToString("MM-dd")
                            });
                        }
                        break;

                    case nameof(ScheduleFrequency.ANNUALLY):
                        schedules.Add(new ScheduleSettings
                        {
                            StartDate = startDate.ToString("MM-dd")
                        });
                        break;

                    // ALL OTHERS ARE PERIODIC
                    case nameof(ScheduleFrequency.MONTHLY):
                        result.Periodic = new PeriodicSettings { Period = "MONTH", PeriodRestartDate = 1 };
                        result.RecurrenceType = "PERIODIC";
                        break;

                    case nameof(ScheduleFrequency.QUARTER):
                        result.Periodic = new PeriodicSettings { Period = "QUARTER", PeriodRestartDate = 1 };
                        result.RecurrenceType = "PERIODIC";
                        break;

                    case nameof(ScheduleFrequency.SCHEDULE):
                        if (inputDto?.Schedules != null)
                            schedules.AddRange(inputDto.Schedules);
                        break;

                    default:
                        _logger.LogWarning("{ClassName}.{MethodName} - Unsupported recurrence type:{RecurrenceType}. Returning empty.",
                            className, methodName, recurrenceType);
                        break;
                }

                // SET EXPIRY DATES
                if (schedules.Count > 1)
                {
                    for (int i = 0; i < schedules.Count - 1; i++)
                    {
                        var start = DateTime.ParseExact(schedules[i].StartDate, "MM-dd", CultureInfo.InvariantCulture);
                        var nextStart = DateTime.ParseExact(schedules[i + 1].StartDate, "MM-dd", CultureInfo.InvariantCulture);

                        schedules[i].ExpiryDate = nextStart.AddDays(-1).ToString("MM-dd");
                    }
                }

                if (schedules.Count > 0)
                {
                    var lastStart = DateTime.ParseExact(schedules.Last().StartDate, "MM-dd", CultureInfo.InvariantCulture);

                    switch (recurrenceType)
                    {
                        case nameof(ScheduleFrequency.DAILY):
                            schedules.Last().ExpiryDate = lastStart.AddDays(1).ToString("MM-dd");
                            break;

                        case nameof(ScheduleFrequency.WEEKLY):
                            schedules.Last().ExpiryDate = lastStart.AddDays(7).ToString("MM-dd");
                            break;

                        case nameof(ScheduleFrequency.BI_ANNUALLY):
                            schedules.Last().ExpiryDate = lastStart.AddMonths(6).ToString("MM-dd");
                            break;

                        case nameof(ScheduleFrequency.ANNUALLY):
                            schedules.Last().ExpiryDate = lastStart.AddYears(1).ToString("MM-dd");
                            break;
                    }

                    result.Schedules = schedules.ToArray();
                }

                return JsonConvert.SerializeObject(result, Formatting.Indented,
                    new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Error generating recurrence schedule.");
                return JsonConvert.SerializeObject(new RecurrenceSettingsDto
                {
                    RecurrenceType = recurrenceType,
                    Schedules = Array.Empty<ScheduleSettings>()
                }, Formatting.Indented);
            }
        }



        public async Task<string> BuildCompletionCriteriaFromInput(
     string completionCriteriaType,
     string uiComponent,
     object inputJson,
     string completionPeriodType,
     string tenantCode,
     string env)
        {
            if (string.IsNullOrWhiteSpace(completionCriteriaType))
                throw new ArgumentException("CompletionCriteriaType cannot be null", nameof(completionCriteriaType));

            var type = completionCriteriaType.ToUpperInvariant();
            OutputCompletionCriteria output = new();
            OutputCompletionCriteriaCustom outputCustom = new();

            // 🔹 Normalize `inputJson` to string
            string jsonString = inputJson switch
            {
                string s => s,
                Object jObj => jObj.ToString(),
                _ => JsonConvert.SerializeObject(inputJson)
            };

            switch (type)
            {
                // 🖼️ IMAGE CRITERIA
                case "IMAGE":
                    {
                        var imgInput = JsonConvert.DeserializeObject<ImageCompletionCriteriaInput>(jsonString);
                        var img = imgInput?.ImageCriteria;
                        if (img == null) break;

                        output.CompletionCriteriaType = "IMAGE";
                        output.ImageCriteria = new ImageCriteriaOutput
                        {
                            Icon = String.IsNullOrEmpty(img.ImageCriteriaIconUrl) ? null : new Icon
                            {
                                Url = !string.IsNullOrEmpty(img.ImageCriteriaIconUrl)
                                    ? await DownloadAndSaveImageAsync(img.ImageCriteriaIconUrl, tenantCode, env)
                                    : null
                            },
                            ButtonLabel = img.ButtonLabel ?? null,
                            UnitLabel = img.OfRequiredImageCountLabel,
                            UnitType = img.OfRequiredImageCountLabel != null &&
                                        img.OfRequiredImageCountLabel.TryGetValue("en-US", out var enLabel)
                                ? enLabel
                                : null,
                            ImageCriteriaText = img.ImageDescription,
                            RequiredImageCount = img.RequiredImageCount,
                            ImageCriteriaTextAlignment = img.ImageCriteriaTextAlignment ?? null
                        };
                        break;
                    }

                // 🦶 STEPS
                case "STEPS":
                    {
                        var stepInput = JsonConvert.DeserializeObject<HealthCompletionCriteriaInput>(jsonString);
                        var hs = stepInput?.HealthCriteria;
                        if (hs == null) break;

                        output.CompletionCriteriaType = "HEALTH";
                        output.SelfReportType = uiComponent;
                        output.CompletionPeriodType = completionPeriodType;

                        output.HealthCriteria = new HealthCriteriaStepsOutput
                        {
                            ButtonLabel = hs.ButtonLabel ?? null,
                            UnitLabel = uiComponent == nameof(CompletionCriteriaComponentType.INPUT)
                                ? hs.OfRequiredStepCountLabel
                                : hs.OfRequiredAddSubtractInteraction,
                            UnitType = uiComponent == nameof(CompletionCriteriaComponentType.INPUT)
                                ? (hs.OfRequiredStepCountLabel != null &&
                                   hs.OfRequiredStepCountLabel.TryGetValue("en-US", out var enLabel)
                                    ? enLabel
                                    : null)
                                : (hs.OfRequiredAddSubtractInteraction != null &&
                                   hs.OfRequiredAddSubtractInteraction.TryGetValue("en-US", out var eniLabel)
                                    ? eniLabel
                                    : null),
                            InputLable = uiComponent == nameof(CompletionCriteriaComponentType.INPUT) ? hs.InputLabel : null,
                            InputPlaceholder = uiComponent == nameof(CompletionCriteriaComponentType.INPUT) ? hs.InputPlaceholder : null,
                            RequiredSteps = hs.RequiredSteps,
                            HealthTaskType = "STEPS"
                        };
                        break;
                    }

                // 😴 SLEEP
                case "SLEEP":
                    {
                        var sleepInput = JsonConvert.DeserializeObject<HealthCompletionCriteriaInput>(jsonString);
                        var sl = sleepInput?.HealthCriteria;
                        if (sl == null) break;

                        output.CompletionCriteriaType = "HEALTH";
                        output.SelfReportType = uiComponent;
                        output.CompletionPeriodType = completionPeriodType;

                        output.HealthCriteria = new HealthCriteriaSleepOutput
                        {
                            ButtonLabel = sl.ButtonLabel ?? null,
                            UnitLabel = uiComponent == nameof(CompletionCriteriaComponentType.INPUT)
                                ? sl.OfRequiredSleepCountLabel
                                : sl.OfRequiredAddSubtractInteraction,
                            UnitType = uiComponent == nameof(CompletionCriteriaComponentType.INPUT)
                                ? (sl.OfRequiredSleepCountLabel != null &&
                                   sl.OfRequiredSleepCountLabel.TryGetValue("en-US", out var enLabel)
                                    ? enLabel
                                    : null)
                                : (sl.OfRequiredAddSubtractInteraction != null &&
                                   sl.OfRequiredAddSubtractInteraction.TryGetValue("en-US", out var eniLabel)
                                    ? eniLabel
                                    : null),
                            InputLable = uiComponent == nameof(CompletionCriteriaComponentType.INPUT) ? sl.InputLabel : null,
                            InputPlaceholder = uiComponent == nameof(CompletionCriteriaComponentType.INPUT) ? sl.InputPlaceholder : null,
                            RequiredSleep = sl.RequiredSleep,
                            HealthTaskType = "SLEEP"
                        };
                        break;
                    }

                // ⚖️ OTHER
                case "OTHER":


                    {
                        if (uiComponent != "CUSTOM")
                        {
                            var otherInput = JsonConvert.DeserializeObject<HealthCompletionCriteriaInput>(jsonString);
                            var ho = otherInput?.HealthCriteria;
                            if (ho == null) break;

                            output.CompletionCriteriaType = "HEALTH";
                            output.SelfReportType = uiComponent;
                            output.CompletionPeriodType = completionPeriodType;

                            output.HealthCriteria = new HealthCriteriaOtherOutput
                            {
                                ButtonLabel = ho.ButtonLabel ?? null,
                                UnitLabel = uiComponent == nameof(CompletionCriteriaComponentType.INPUT)
                                    ? ho.OfRequiredCountLabel
                                    : ho.OfRequiredAddSubtractInteraction,
                                UnitType = uiComponent == nameof(CompletionCriteriaComponentType.INPUT)
                                    ? (ho.OfRequiredCountLabel != null &&
                                       ho.OfRequiredCountLabel.TryGetValue("en-US", out var enLabel)
                                        ? enLabel
                                        : null)
                                    : (ho.OfRequiredAddSubtractInteraction != null &&
                                       ho.OfRequiredAddSubtractInteraction.TryGetValue("en-US", out var eniLabel)
                                        ? eniLabel
                                        : null),
                                InputLable = uiComponent == nameof(CompletionCriteriaComponentType.INPUT) ? ho.InputLabel : null,
                                InputPlaceholder = uiComponent == nameof(CompletionCriteriaComponentType.INPUT) ? ho.InputPlaceholder : null,
                                RequiredUnits = ho.RequiredUnits,
                                HealthTaskType = "OTHER"
                            };
                            break;
                        }
                        else
                        {
                            var customInput = JsonConvert.DeserializeObject<CustomCompletionCriteriaInput>(jsonString);
                            outputCustom = await BuildCustomCompletionCriteriaOutput(customInput, completionPeriodType, tenantCode, env);
                            return JsonConvert.SerializeObject(
               outputCustom,
               Formatting.Indented,
               new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                        }
                    }

                // ⚙️ CUSTOM
                case "CUSTOM":
                    {
                        var customInput = JsonConvert.DeserializeObject<CustomCompletionCriteriaInput>(jsonString);
                        outputCustom = await BuildCustomCompletionCriteriaOutput(customInput, completionPeriodType, tenantCode, env);
                        return JsonConvert.SerializeObject(
                outputCustom,
                Formatting.Indented,
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                    }

                // 🎯 TRIVIA
                case "TRIVIA":
                    {
                        var triviaInput = JsonConvert.DeserializeObject<TriviaCompletionCriteriaInput>(jsonString);
                        output.DisableTriviaSplashScreen = triviaInput?.DisableTriviaSplashScreen ?? true;
                        break;
                    }

                default:
                    throw new InvalidOperationException($"Unsupported completion criteria type: {type}");

            }

            return JsonConvert.SerializeObject(
                output,
                Formatting.Indented,
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        }

        private async Task<OutputCompletionCriteriaCustom> BuildCustomCompletionCriteriaOutput(
            CustomCompletionCriteriaInput input,
            string completionPeriodType,
            string tenantCode,
            string env)
        {
            var h = input.HealthCriteria!;
            var healthCriteria = new CustomHealthCriteriaOutput
            {
                UnitType = h.OfRequiredAddSubtractInteraction != null &&
                   h.OfRequiredAddSubtractInteraction.TryGetValue("en-US", out var eniLabel)
            ? eniLabel
            : null,
                UnitLabel = h.OfRequiredAddSubtractInteraction,
                ButtonLabel = h.CompletionButtonLabel,
                RequiredUnits = h.RequiredTaskCompletionCount ?? 0,
                HealthTaskType = "OTHER",
                SkipDisclaimer = string.Equals(h.DisclaimerRequired, "Yes", StringComparison.OrdinalIgnoreCase),
                IsDialerRequired = string.Equals(h.IsDialerRequire, "Yes", StringComparison.OrdinalIgnoreCase),
                IsDisclaimerAutoChecked = string.Equals(h.IsDisclaimerAutochecked, "Yes", StringComparison.OrdinalIgnoreCase),
                UIComponents = new List<UIComponent>()
            };

            // 🧩 Build UI Components
            if (h.CompletionComponents != null && h.CompletionComponents.Count > 0)
            {
                for (int i = 0; i < h.CompletionComponents.Count; i++)
                {
                    string compType = h.CompletionComponents[i];

                    string labelEn = h.CompletionComponentLabels?["en-US"]?.ElementAtOrDefault(i) ?? "";
                    string placeholderEn = h.CompletionComponentPlaceholders?["en-US"]?.ElementAtOrDefault(i) ?? "";
                    DropdownConfigurationInput? dropdown = null;
                    if (compType == "DROPDOWN" && h.DropdownConfigurations != null)
                    {
                         dropdown = h.DropdownConfigurations
                            .FirstOrDefault(d => d.ConfigurationFor == labelEn || d.ConfigurationFor == compType);
                    }
                        var component = new UIComponent
                    {
                        ReportTypeLabel = ExtractLocalizedValue(h.CompletionComponentLabels, i),
                        Placeholder = ExtractLocalizedValue(h.CompletionComponentPlaceholders, i),
                        MultiSelect = dropdown==null ?null: dropdown.SelectionType?.ToUpperInvariant()=="SINGLE"?false:true,
                        SelfReportType= compType,
                        IsRequiredField = string.Equals(h.RequiredCompletionComponent, labelEn, StringComparison.OrdinalIgnoreCase),
                        Options = dropdown == null ? null:new List<Option>()
                    };

                    // ✅ Handle dropdowns
                    if (compType == "DROPDOWN" && h.DropdownConfigurations != null)
                    {                       

                        if (dropdown?.Options != null)
                        {
                            foreach (var langEntry in dropdown.Options)
                            {
                                var langCode = langEntry.Key;
                                foreach (var optName in langEntry.Value)
                                {
                                    var option = new Option
                                    {
                                        Value = optName,
                                        Label = new LocalizedText { { langCode, optName } }
                                    };

                                    // Optional modal display (localized)
                                    if (dropdown.OptionsSelectionCriteria != null &&
                                        dropdown.OptionsSelectionCriteria.TryGetValue(langCode, out var localizedDisplays) &&
                                        localizedDisplays != null)
                                    {
                                        var displayList = new List<RichTextElement>();

                                        string? modalUrl = null;
                                        if (localizedDisplays.ModalPopup != null)
                                        {
                                            try
                                            {
                                                option.Type = "modal";
                                                option.ModalImageUrl = localizedDisplays.ModalPopup.ModalImageUrl != null ? await DownloadAndSaveImageAsync(localizedDisplays.ModalPopup.ModalImageUrl, tenantCode, env) : null;
                                            }
                                            catch (Exception ex)
                                            {
                                                _logger?.LogWarning(ex, "Failed to download modal image {Url}", localizedDisplays.ModalPopup.ModalImageUrl);
                                                modalUrl = localizedDisplays.ModalPopup.ModalImageUrl; // fallback
                                            }


                                            option.OnSelectionDisplay = ConvertToOnSelectionDisplay(localizedDisplays.ModalPopup);
                                        }
                                        else if (localizedDisplays.OptionDescription != null)
                                        {
                                            option.Type = "option_description";
                                            option.OnSelectionDisplay = ConvertToOnSelectionDisplay(localizedDisplays.OptionDescription);

                                        }
                                    }

                                    component.Options!.Add(option);
                                }
                            }
                        }
                    }

                    healthCriteria.UIComponents!.Add(component);
                }
            }

            return new OutputCompletionCriteriaCustom
            {
                CompletionCriteriaType = "HEALTH",
                SelfReportType = "UI_COMPONENT",
                CompletionPeriodType = completionPeriodType,
                HealthCriteriaCustom = healthCriteria
            };
        }
        public static Dictionary<string, List<RichTextElement>>? ConvertToOnSelectionDisplay<T>(T input)
        {
            if (input == null) return null;

            var result = new Dictionary<string, List<RichTextElement>>(StringComparer.OrdinalIgnoreCase);

            // Get all public string properties
            foreach (var prop in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (prop.PropertyType != typeof(string)) continue;

                var value = prop.GetValue(input) as string;
                if (string.IsNullOrWhiteSpace(value)) continue;

                string name = prop.Name;

                // Try to detect locale (e.g., EnUs, Es)
                string locale = name.Contains("EnUs", StringComparison.OrdinalIgnoreCase) ? "en-US" :
                                name.Contains("Es", StringComparison.OrdinalIgnoreCase) ? "es" :
                                string.Empty;

                if (string.IsNullOrEmpty(locale)) continue;

                // Try to detect text type
                string type = name.Contains("Header", StringComparison.OrdinalIgnoreCase) ? "header" :
                              name.Contains("Body", StringComparison.OrdinalIgnoreCase) ||
                              name.Contains("Description", StringComparison.OrdinalIgnoreCase) ? "paragraph" :
                              string.Empty;

                if (string.IsNullOrEmpty(type)) continue;

                if (!result.ContainsKey(locale))
                    result[locale] = new List<RichTextElement>();

                result[locale].Add(new RichTextElement
                {
                    Type = type,
                    Value = value
                });
            }

            return result.Count > 0 ? result : null;
        }
        private static LocalizedText? ExtractLocalizedValue(
            Dictionary<string, List<string>>? localizedDict,
            int index)
        {
            if (localizedDict == null)
                return null;

            var result = new LocalizedText();
            foreach (var lang in localizedDict)
            {
                string value = lang.Value.ElementAtOrDefault(index) ?? "";
                result[lang.Key] = value;
            }
            return result;
        }

        public async Task<string> DownloadAndSaveImageAsync(
    string sourceImageUrl,
    string tenantCode,
    string envVariableName)
        {
            if (string.IsNullOrWhiteSpace(sourceImageUrl))
                throw new ArgumentException("Image URL cannot be empty.", nameof(sourceImageUrl));

            if (string.IsNullOrWhiteSpace(tenantCode))
                throw new ArgumentException("Tenant code cannot be empty.", nameof(tenantCode));

            string baseUrl = await GetImageBaseUrl();
            string bucketName = GetAwsSunnyPublicFileBucketName();
            if (string.IsNullOrEmpty(baseUrl))
                throw new InvalidOperationException($"Base URL not found for env variable: {envVariableName}");

            string imageName = Path.GetFileName(new Uri(sourceImageUrl).AbsolutePath);
            string cmsPath = $"cms/images/{tenantCode}/{imageName}";
            string destinationUrl = $"{baseUrl.TrimEnd('/')}/{cmsPath}";

            try
            {
                // 🧠 Preflight check: verify image URL is reachable before downloading
                using var headRequest = new HttpRequestMessage(HttpMethod.Head, sourceImageUrl);
                using var headResponse = await _httpClient.SendAsync(headRequest);

                if (!headResponse.IsSuccessStatusCode)
                {
                    _logger.LogError(
                        "Source image URL {Url} not reachable (HTTP {StatusCode}). Returning original URL.",
                        sourceImageUrl,
                        headResponse.StatusCode);

                    return sourceImageUrl;
                }

                // ⚡ Proceed to download only if preflight passes
                byte[] imageBytes = await _httpClient.GetByteArrayAsync(sourceImageUrl);

                await _awsS3Service.UploadImageToS3Async(imageBytes, bucketName, cmsPath);

                return destinationUrl;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "Failed to download image from {Url}. Returning original URL.", sourceImageUrl);
                return sourceImageUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in DownloadAndSaveImageAsync for {Url}", sourceImageUrl);
                return sourceImageUrl;
            }
        }



        // Helper method (inserted into the class)
        private async Task<bool> UpsertTaskDetailsAsync(long taskIdValue, EtlTaskImportDto taskDef, string tenantCode, DateTime now, string createUser, NHibernate.ITransaction transaction)
        {
            const string methodName = nameof(UpsertTaskDetailsAsync);

            // Build list of locales to process
            var localesToProcess = new List<dynamic>();
            if (taskDef.LocalizeInfo != null && taskDef.LocalizeInfo.Count > 0)
            {
                foreach (var loc in taskDef.LocalizeInfo)
                {
                    localesToProcess.Add(new
                    {
                        Language = loc.Language ?? "en-US",
                        TaskHeader = loc.TaskHeader,
                        TaskDescription = loc.TaskDescription,
                        Cta = loc.Cta
                    });
                }
            }

            foreach (var locale in localesToProcess)
            {
                string language = locale.Language ?? "en-US";

                // Retrieve latest TOS for the language
                IList<ETLTermsOfServiceModel>? tosModel;
                try
                {
                    tosModel = await _termOfServiceRepo.FindAsync(x => x.LanguageCode == language && x.DeleteNbr == 0);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "{ClassName}.{MethodName} - Error fetching TermsOfService for language {Lang}", className, methodName, language);
                    await transaction.RollbackAsync(); 
                    return false;
                }
                if (tosModel?.Count == 0)
                {
                    _logger.LogError("{ClassName}.{MethodName} - Terms of service not found for taskName: {TaskName}, Language: {Lang}", className, methodName, taskDef.TaskName, language);
                    tosModel = await _termOfServiceRepo.FindAsync(x => x.LanguageCode == "en-US" && x.DeleteNbr == 0);
                }
                var latestTOSrecord = tosModel != null && tosModel.Count > 0
                    ? tosModel.OrderByDescending(x => x.TermsOfServiceId).FirstOrDefault()
                    : null;

                if (latestTOSrecord == null)
                {
                    _logger.LogError("{ClassName}.{MethodName} - Terms of service not found for taskName: {TaskName}, Language: {Lang}", className, methodName, taskDef.TaskName, language);
                    continue;
                }

                // Find existing task detail for this locale
                ETLTaskDetailModel? taskdetailModel = null;
                try
                {
                    taskdetailModel = await _taskdetailRepo.FindOneAsync(x => x.TaskId == taskIdValue && x.TenantCode == tenantCode && x.LanguageCode == language && x.DeleteNbr == 0);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "{ClassName}.{MethodName} - Error fetching TaskDetail for TaskId:{TaskId}, Lang:{Lang}", className, methodName, taskIdValue, language);
                    await transaction.RollbackAsync();
                    return false;
                }

                string header = locale.TaskHeader;
                string description = GetTaskDescription(locale.TaskDescription);
                string cta = locale.Cta;
                string langCode = language;

                if (taskdetailModel == null)
                {
                    var taskDetail = new ETLTaskDetailModel
                    {
                        TaskId = taskIdValue,
                        TermsOfServiceId = latestTOSrecord.TermsOfServiceId,
                        TaskHeader = header,
                        TaskDescription = description,
                        LanguageCode = langCode,
                        TenantCode = tenantCode,
                        TaskCtaButtonText = cta,
                        CreateTs = now,
                        CreateUser = createUser,
                        UpdateUser = null,
                        DeleteNbr = 0
                    };

                    try
                    {
                        await _session.SaveAsync(taskDetail);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "{ClassName}.{MethodName} - Error saving TaskDetail for TaskId:{TaskId}, Lang:{Lang}", className, methodName, taskIdValue, langCode);
                        await transaction.RollbackAsync(); 
                        return false;
                    }
                }
                else
                {
                    taskdetailModel.TermsOfServiceId = latestTOSrecord.TermsOfServiceId;
                    taskdetailModel.TaskHeader = header;
                    taskdetailModel.TaskDescription = description;
                    taskdetailModel.LanguageCode = langCode;
                    taskdetailModel.TenantCode = tenantCode;
                    taskdetailModel.TaskCtaButtonText = cta;
                    taskdetailModel.UpdateTs = now;
                    taskdetailModel.UpdateUser = Constants.UpdateUser;

                    try
                    {
                        await _session.UpdateAsync(taskdetailModel);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "{ClassName}.{MethodName} - Error updating TaskDetailId:{DetailId}, TaskId:{TaskId}, Lang:{Lang}", className, methodName, taskdetailModel.TaskDetailId, taskIdValue, langCode);
                        await transaction.RollbackAsync();
                        return false;
                    }
                }
            }

            return true;
        }
        #endregion

    }
}

