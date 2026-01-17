using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using SunnyRewards.Helios.ETL.Common.Constants;
using SunnyRewards.Helios.ETL.Common.CustomException;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Core.Domain.Models.DynamoDb;
using SunnyRewards.Helios.ETL.Infrastructure.AwsConfig;
using SunnyRewards.Helios.ETL.Infrastructure.Helpers.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces.DynamoDb;
using SunnyRewards.Helios.Tenant.Core.Domain.Models;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services.DynamoDb
{
    public class JobHistoryService : AwsConfiguration, IJobHistoryService
    {
        private readonly IDynamoDbHelper _dynamoDbHelper;
        private readonly ILogger<JobHistoryService> _logger;
        private readonly ITenantRepo _tenantRepo;
        const string className = nameof(JobHistoryService);

        public JobHistoryService(IVault vault, ILogger<JobHistoryService> logger, IConfiguration configuration,
            IDynamoDbHelper dynamoDbHelper, ITenantRepo tenantRepo) : base(vault, configuration)
        {
            _logger = logger;
            _dynamoDbHelper = dynamoDbHelper;
            _tenantRepo = tenantRepo;
        }

        /// <summary>
        /// GetJobHistoryById
        /// </summary>
        /// <param name="jobHistoryId"></param>
        /// <returns></returns>
        public async Task<JobHistoryModel> GetJobHistoryById(string jobHistoryId)
        {
            const string methodName = nameof(GetJobHistoryById);
            try
            {
                _logger.LogInformation("{className}.{methodName}: Started processing GetJobHistoryById from dynamo db jobHistoryId: {jobHistoryId}",
                    className, methodName, jobHistoryId);
                JobHistoryModel jobHistory = new JobHistoryModel();
                var tableName = GetDynamoDbJobHistoryTableName().Result;
                if (tableName == null) {
                    _logger.LogError("{className}.{methodName}:Job History table name was not configured in AWS secrets.",
                        className, methodName);
                    throw new ETLException(ETLExceptionCodes.AWSSecretNotFound, "Job History table name was not configured in AWS secrets.");
                }
                var request = new GetItemRequest
                {
                    TableName = tableName,
                    Key = new Dictionary<string, AttributeValue>
                    {
                        { "jobHistoryId", new AttributeValue { S = jobHistoryId } }
                    }
                };
                jobHistory = await _dynamoDbHelper.GetItemAsync<JobHistoryModel>(request);
                if (string.IsNullOrEmpty(jobHistory.JobHistoryId))
                    throw new ETLException(ETLExceptionCodes.NotFoundInDynamoDb, $"No record found in dynamo DB with jobHistory Id:{jobHistoryId}");

                _logger.LogInformation("{className}.{methodName}: GetJobHistoryById from DynamoDb is successful",
                    className, methodName);
                return jobHistory;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{className}.{methodName}:Error while GetJobHistoryById from DynamoDB - ERROR Msg:{msg}, jobHistoryId:{jobHistoryId}",
                    className, methodName, ex.Message, jobHistoryId);
                throw;
            }
        }

        /// <summary>
        /// Update JobHistory
        /// </summary>
        /// <param name="jobHistory"></param>
        /// <returns></returns>
        public async Task<JobHistoryModel> UpdateJobHistory(JobHistoryModel jobHistory)
        {
            const string methodName = nameof(UpdateJobHistory);
            try
            {
                _logger.LogInformation("{className}.{methodName}: Started processing Updating JobHistory in dynamo db with request: {jobHistory}",
                    className, methodName, jobHistory.ToJson());
                var tableName = GetDynamoDbJobHistoryTableName().Result;
                if (tableName == null)
                {
                    _logger.LogError("{className}.{methodName}:Job History table name was not configured in AWS secrets.",
                        className, methodName);
                    throw new ETLException(ETLExceptionCodes.AWSSecretNotFound, "Job History table name was not configured in AWS secrets.");
                }
                if (jobHistory.RunStatus == Constants.JOB_HISTORY_FAILURE_STATUS || jobHistory.RunStatus == Constants.JOB_HISTORY_SUCCESS_STATUS
                    || jobHistory.RunStatus == Constants.JOB_HISTORY_PARTIAL_SUCCESS_STATUS)
                {
                    var diffOfDates = DateTime.UtcNow.Subtract(DateTime.Parse(jobHistory.StartTime));
                    jobHistory.RunDuration = $"{diffOfDates.Days} Days, {diffOfDates.Hours} Hours, {diffOfDates.Minutes} Minutes, " +
                        $"{diffOfDates.Seconds} Seconds, {diffOfDates.Milliseconds} Milliseconds";
                }

                // Prepare the update request
                var request = new UpdateItemRequest
                {
                    TableName = tableName,
                    Key = new Dictionary<string, AttributeValue>
                    {
                        { "jobHistoryId", new AttributeValue { S = jobHistory.JobHistoryId } }
                    },
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                    {
                        { ":runStatus", new AttributeValue { S =  string.IsNullOrEmpty(jobHistory.RunStatus) ? " " : jobHistory.RunStatus } },
                        { ":runDuration", new AttributeValue { S =  string.IsNullOrEmpty(jobHistory.RunDuration) ? " " : jobHistory.RunDuration } },
                        { ":updatedTs", new AttributeValue { S = string.IsNullOrEmpty(DateTime.UtcNow.ToString()) ? " " : DateTime.UtcNow.ToString() } },
                        { ":updatedBy", new AttributeValue { S = string.IsNullOrEmpty(Constants.UpdateUser)?" ":Constants.UpdateUser } },
                        { ":endTime", new AttributeValue { S = string.IsNullOrEmpty(DateTime.UtcNow.ToString())?" ":DateTime.UtcNow.ToString() } },
                        { ":errorLog", new AttributeValue { S = string.IsNullOrEmpty(jobHistory.ErrorLog) ? " " : jobHistory.ErrorLog } }
   
                    },
                    UpdateExpression = "SET endTime = :endTime, runStatus = :runStatus, updatedTs = :updatedTs, " +
                    "updatedBy = :updatedBy, errorLog = :errorLog, runDuration = :runDuration"
                };

                //get sponserCodes for multiple TenantCodes
                if (!string.IsNullOrEmpty(jobHistory.TenantCode) && !jobHistory.TenantCode.Contains(Constants.DUMMY_TENANT_PREFIX))
                {
                    var tenantCodes = jobHistory.TenantCode.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                               .Select(t => t.Trim())
                                               .ToList();

                    var sponsorCodes = new List<string>();
                    var customerCodes = new List<string>();

                    foreach (var tenant in tenantCodes)
                    {
                        var (customerCode, sponsorCode) = await _tenantRepo.GetCustomerAndSponsorCode(tenant);
                        if (!string.IsNullOrEmpty(sponsorCode))
                        {
                            sponsorCodes.Add(sponsorCode);
                        }

                        if (!string.IsNullOrEmpty(customerCode))
                        {
                            customerCodes.Add(customerCode);
                        }
                    }
                    jobHistory.SponsorCode = sponsorCodes.Any() ? string.Join(",", sponsorCodes) : " ";
                    jobHistory.CustomerCode = customerCodes.Any() ? string.Join(",", customerCodes) : " ";

                    //update request
                    request.ExpressionAttributeValues.Add(":customerCode", new AttributeValue { S = string.IsNullOrEmpty(jobHistory.CustomerCode) ? " " : jobHistory.CustomerCode });
                    request.ExpressionAttributeValues.Add(":tenantCode", new AttributeValue { S = string.IsNullOrEmpty(jobHistory.TenantCode) ? " " : jobHistory.TenantCode });
                    request.ExpressionAttributeValues.Add(":sponsorCode", new AttributeValue { S = string.IsNullOrEmpty(jobHistory.SponsorCode) ? " " : jobHistory.SponsorCode });

                    request.UpdateExpression = request.UpdateExpression + ", tenantCode = :tenantCode , sponsorCode =:sponsorCode, customerCode=:customerCode";

                }

                var response = await _dynamoDbHelper.UpdateItemAsync(request);
                _logger.LogInformation("{className}.{methodName}: Updating JobHistory in DynamoDb is successful",
                    className, methodName);
                return jobHistory;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{className}.{methodName}:Error while Updating JobHistory in DynamoDB - ERROR Msg:{msg}, jobHistory request: {jobHistory}",
                    className, methodName, ex.Message, jobHistory.ToJson());
                throw;
            }
        }


        /// <summary>
        /// Insert JobHistory
        /// </summary>
        /// <param name="jobHistory"></param>
        /// <returns></returns>
        public async Task<JobHistoryModel> InsertJobHistory(JobHistoryModel jobHistory)
        {
            const string methodName = nameof(InsertJobHistory);
            try
            {
                _logger.LogInformation("{className}.{methodName}: Started processing inserting JobHistory into dynamo db with request: {jobHistory}",
                    className, methodName, jobHistory.ToJson());
                var tableName = GetDynamoDbJobHistoryTableName().Result;
                if (tableName == null)
                {
                    _logger.LogError("{className}.{methodName}:Job History table name was not configured in AWS secrets.",
                        className, methodName);
                    throw new ETLException(ETLExceptionCodes.AWSSecretNotFound, "Job History table name was not configured in AWS secrets.");
                }

                // Prepare the insert request
                var request = new PutItemRequest
                {
                    TableName = tableName,
                    Item = new Dictionary<string, AttributeValue>()
                    {
                        { "jobHistoryId", new AttributeValue { S = jobHistory.JobHistoryId }},
                        { "customerCode", new AttributeValue { S = jobHistory.CustomerCode }},
                        { "sponsorCode", new AttributeValue { S = jobHistory.SponsorCode }},
                        { "tenantCode", new AttributeValue { S = jobHistory.TenantCode }},
                        { "endTime", new AttributeValue { S = "" }},
                        { "errorLog", new AttributeValue { S = "" }},
                        { "fileType", new AttributeValue { S = "" }},
                        { "jobDefinition", new AttributeValue { S = jobHistory.JobDefinition }},
                        { "jobId", new AttributeValue { S = jobHistory.JobId }},
                        { "metadata", new AttributeValue { S = jobHistory.Metadata }},
                        { "outputData", new AttributeValue { S = "" }},
                        { "processType", new AttributeValue { S = "" }},
                        { "retries", new AttributeValue { S = jobHistory.Retries.ToString() }},
                        { "runStatus", new AttributeValue { S = jobHistory.RunStatus }},
                        { "runDuration", new AttributeValue { S = "0" }},
                        { "scheduleTime", new AttributeValue { S = jobHistory.ScheduleTime }},
                        { "startTime", new AttributeValue { S = jobHistory.StartTime }},
                        { "createdBy", new AttributeValue { S = jobHistory.CreatedBy }},
                        { "createdTs", new AttributeValue { S = jobHistory.CreatedTs }},
                        { "updatedBy", new AttributeValue { S = jobHistory.UpdatedBy }},
                        { "updatedTs", new AttributeValue { S = jobHistory.UpdatedTs }}
                    }
                };
                var response = await _dynamoDbHelper.PutItemAsync(request);
                if(response.HttpStatusCode != System.Net.HttpStatusCode.OK)
                    throw new ETLException(ETLExceptionCodes.DynamoDbInsertFailed, $"Inserting jobHistory record into Dynamo DB failed with request:{request.ToJson()}");

                _logger.LogInformation("{className}.{methodName}: inserting JobHistory in DynamoDb is successful",
                    className, methodName);
                return jobHistory;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{className}.{methodName}:Error while inserting JobHistory into DynamoDB - ERROR Msg:{msg}, jobHistory request: {jobHistory}",
                    className, methodName, ex.Message, jobHistory.ToJson());
                throw;
            }
        }

        /// <summary>
        /// Update job definition column in Job History table
        /// </summary>
        /// <param name="jobHistory"></param>
        /// <param name="jobDefinitionId"></param>
        /// <returns></returns>
        public async Task<JobHistoryModel> UpdateJobDefinitionInJobHistory(JobHistoryModel jobHistory, string jobDefinitionId)
        {
            const string methodName = nameof(UpdateJobDefinitionInJobHistory);
            try
            {
                _logger.LogInformation("{className}.{methodName}: Started processing Updating jobDefinition column" +
                    " in JobHistory table in dynamo db with request: {jobHistory}, jobDefinitionId: {jobDefinitionId}",
                    className, methodName, jobHistory.ToJson(), jobDefinitionId);

                if (jobHistory.CreatedBy == Constants.UpdateUser && string.IsNullOrEmpty(jobHistory.JobDefinition))
                {
                    var jobDefinition = await GetJobDefinition(jobDefinitionId);
                    var tableName = GetDynamoDbJobHistoryTableName().Result;
                    if (tableName == null)
                    {
                        _logger.LogError("{className}.{methodName}:Job History table name was not configured in AWS secrets.",
                            className, methodName);
                        throw new ETLException(ETLExceptionCodes.AWSSecretNotFound, "Job History table name was not configured in AWS secrets.");
                    }
                    // Prepare the update request
                    var request = new UpdateItemRequest
                    {
                        TableName = tableName,
                        Key = new Dictionary<string, AttributeValue>
                    {
                        { "jobHistoryId", new AttributeValue { S = jobHistory.JobHistoryId } }
                    },
                        ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                    {
                        { ":jobDefinition", new AttributeValue { S = jobDefinition.JobDefinitionName } },
                        { ":updatedTs", new AttributeValue { S = DateTime.UtcNow.ToString() } },
                        { ":updatedBy", new AttributeValue { S = Constants.UpdateUser } }
                    },
                        UpdateExpression = "SET jobDefinition = :jobDefinition, updatedBy = :updatedBy, updatedTs = :updatedTs"
                    };

                    var response = await _dynamoDbHelper.UpdateItemAsync(request);
                    if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
                        throw new ETLException(ETLExceptionCodes.DynamoDbUpdateFailed, $"Updating jobHistory record in Dynamo DB failed with request:{request.ToJson()}");

                    _logger.LogInformation("{className}.{methodName}: Updating JobHistory in DynamoDb is successful",
                        className, methodName);
                }
                return jobHistory;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{className}.{methodName}:Error while Updating JobHistory in DynamoDB - ERROR Msg:{msg}, jobHistory request: {jobHistory}",
                    className, methodName, ex.Message, jobHistory.ToJson());
                throw;
            }

        }

        /// <summary>
        /// Get job history create request
        /// </summary>
        /// <param name="etlExecutionContext"></param>
        /// <returns></returns>
        public async Task<JobHistoryModel> GetJobHistoryCreateRequest(EtlExecutionContext etlExecutionContext)
        {
            const string methodName = nameof(GetJobHistoryCreateRequest);
            JobHistoryModel? jobHistory;
            try
            {
                _logger.LogInformation("{className}.{methodName}: Started processing preparing job history create request, ETLContext: {ETLContext}",
                    className, methodName, etlExecutionContext.ToJson());

                var currentDateTime = DateTime.UtcNow;
                var newGuid = Guid.NewGuid().ToString("N");

                jobHistory = new JobHistoryModel
                {
                    JobHistoryId = $"{Constants.JOB_HISTORY_ID_PREFIX}-{newGuid}",
                    JobId = $"{Constants.DUMMY_JOB_ID_PREFIX}_{newGuid}",
                    TenantCode = etlExecutionContext.TenantCode,
                    CustomerCode = etlExecutionContext.CustomerCode,
                    JobDefinition = string.Empty,
                    Metadata = etlExecutionContext.ToJson(),
                    Retries = 0,
                    RunStatus = Constants.JOB_HISTORY_STARTED_STATUS,
                    ScheduleTime = Constants.DEFAULT_SCHEDULE_TIME,
                    StartTime = currentDateTime.ToString(),
                    CreatedTs = currentDateTime.ToString(),
                    CreatedBy = Constants.UpdateUser,
                    UpdatedTs = currentDateTime.ToString(),
                    UpdatedBy = Constants.UpdateUser
                };

                if (!string.IsNullOrEmpty(etlExecutionContext.TenantCode))
                {
                    var (customerCode, sponsorCode) = await _tenantRepo.GetCustomerAndSponsorCode(etlExecutionContext.TenantCode);
                    jobHistory.CustomerCode = customerCode;
                    jobHistory.SponsorCode = sponsorCode;
                }

                jobHistory.CustomerCode = string.IsNullOrEmpty(jobHistory.CustomerCode)
                                ? $"{Constants.DUMMY_CUSTOMER_PREFIX}_{Guid.NewGuid().ToString("N")}" : jobHistory.CustomerCode;
                jobHistory.SponsorCode = string.IsNullOrEmpty(jobHistory.SponsorCode)
                                ? $"{Constants.DUMMY_SPONSOR_PREFIX}_{Guid.NewGuid().ToString("N")}" : jobHistory.SponsorCode;
                jobHistory.TenantCode = string.IsNullOrEmpty(jobHistory.TenantCode)
                                ? $"{Constants.DUMMY_TENANT_PREFIX}_{Guid.NewGuid().ToString("N")}" : jobHistory.TenantCode;

                _logger.LogInformation("{className}.{methodName}: preparing jobHistory create request is successful, jobHistory request: {jobHistory}",
                    className, methodName, jobHistory.ToJson());
                return jobHistory;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{className}.{methodName}:Error while preparing jobHistory insert request - ERROR Msg:{msg}, ETL context:{context}",
                    className, methodName, ex.Message, etlExecutionContext.ToJson());
                throw;
            }
        }

        /// <summary>
        /// Get job definition using jobDefinitionId from Dynamo DB
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        private async Task<JobDefinitionModel> GetJobDefinition(string jobDefinitionId)
        {
            const string methodName = nameof(GetJobDefinition);
            try
            {
                _logger.LogInformation("{className}.{methodName}: Started processing fetching job definition from Dynamo DB, jobDefinitionId: {jobDefinitionId}",
                    className, methodName, jobDefinitionId);

                JobDefinitionModel jobDefinition = new JobDefinitionModel();
                var tableName = GetDynamoDbJobDefinitionTableName().Result;
                if (tableName == null)
                {
                    _logger.LogError("{className}.{methodName}:Job Definition table name was not configured in AWS secrets.",
                        className, methodName);
                    throw new ETLException(ETLExceptionCodes.AWSSecretNotFound, "Job Definition table name was not configured in AWS secrets.");
                }
                var request = new GetItemRequest
                {
                    TableName = tableName,
                    Key = new Dictionary<string, AttributeValue>
                    {
                        { "jobDefinitionId", new AttributeValue { S = jobDefinitionId } }
                    }
                };
                jobDefinition = await _dynamoDbHelper.GetItemAsync<JobDefinitionModel>(request);

                _logger.LogInformation("{className}.{methodName}: fetching job definition is successful, jobDefinition: {jobDefinition}",
                    className, methodName, jobDefinition.ToJson());
                return jobDefinition;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{className}.{methodName}:Error while fetching job definition from Dynamo DB - ERROR Msg:{msg}, ETL jobDefinitionId:{jobDefinitionId}",
                    className, methodName, ex.Message, jobDefinitionId);
                throw;
            }
        }
    }
}
