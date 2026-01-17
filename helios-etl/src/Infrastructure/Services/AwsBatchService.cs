using Amazon;
using Amazon.Batch;
using Amazon.Batch.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using SunnyRewards.Helios.ETL.Common.Extensions;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services
{
    public class AwsBatchService : IAwsBatchService
    {
        private readonly IVault _vault;
        private readonly ILogger<AwsQueueService> _logger;
        private readonly IConfiguration _configuration;
        private const string className = nameof(AwsBatchService);

        public AwsBatchService(IVault vault, ILogger<AwsQueueService> logger, IConfiguration configuration)
        {
            _vault = vault;
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// Triggers the specified AWS Batch job with optional parameters.
        /// </summary>
        /// <param name="jobName">name with card60job will create</param>
        /// <param name="parameters">Parameters for card60 job</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<string> TriggerBatchJob(string jobName, Dictionary<string, string>? parameters = null)
        {
            const string methodName = nameof(TriggerBatchJob);
            try
            {
                return await TriggerBatchJobGeneric(jobName, "AWS:AWS_CARD_60_BATCH_JOB_DEFINITION_ARN", "AWS:AWS_BATCH_JOB_QUEUE_ARN", parameters);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Error occured while triggering batch Job, JobName:{jobName}, ErrorCode:{Code}, parameters:{Parameters} ERROR:{Msg}",
                    className, methodName, jobName, StatusCodes.Status500InternalServerError, parameters!.ToJson(), ex.Message);
                throw new Exception("Failed to trigger batch job");
            }

        }

        /// <summary>
        /// Get AWS Settings from configuration and vault
        /// </summary>
        /// <returns>awsAccessKey, awsSecretKey, batchjobDefinition, JobQueue</returns>
        private async Task<(string, string, string, string)> GetAWSSettings()
        {
            const string methodName = nameof(GetAWSSettings);
            _logger.LogInformation("{ClassName}.{MethodName} - Started processing...", className, methodName);

            try
            {
                (string awsAccessKey, string awsSecretKey) = await GetAWSAccessKeyAndSecretKey();
                string batchJobDefinition = _configuration.GetSection("AWS:AWS_CARD_60_BATCH_JOB_DEFINITION_ARN").Value?.ToString() ?? "";
                string batchJobQueue = _configuration.GetSection("AWS:AWS_BATCH_JOB_QUEUE_ARN").Value?.ToString() ?? "";

                _logger.LogInformation("{ClassName}.{MethodName} - Ended processing...", className, methodName);
                return (awsAccessKey, awsSecretKey, batchJobDefinition, batchJobQueue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Error occured processing GetAWSSettings,ErrorCode:{Code},ERROR:{Msg}", className, methodName, StatusCodes.Status500InternalServerError, ex.Message);
                throw;
            }
        }

        private async Task<(string awsAccessKey, string awsSecretKey)> GetAWSAccessKeyAndSecretKey()
        {
            string awsAccessKey = await _vault.GetSecret(_configuration.GetSection("AWS:AWS_ACCESS_KEY_NAME").Value?.ToString() ?? "");
            string awsSecretKey = await _vault.GetSecret(_configuration.GetSection("AWS:AWS_SECRET_KEY_NAME").Value?.ToString() ?? "");
            return (awsAccessKey, awsSecretKey);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jobName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<string> TriggerProcessDepositInstrcutionsBatchJob(string jobName, Dictionary<string, string>? parameters = null)
        {
            const string methodName = nameof(TriggerProcessDepositInstrcutionsBatchJob);
            try
            {
                return await TriggerBatchJobGeneric(jobName, "AWS:AWS_DEPOSIT_INSTRUCTION_JOB_DEFINITION_ARN", "AWS:AWS_BATCH_JOB_QUEUE_ARN", parameters);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Error occured while triggering batch Job, JobName:{jobName}, ErrorCode:{Code}, parameters:{Parameters} ERROR:{Msg}",
                    className, methodName, jobName, StatusCodes.Status500InternalServerError, parameters!.ToJson(), ex.Message);
                throw new Exception("Failed to trigger batch job");
            }
        }

        public async Task<string> TriggerBatchJobGeneric(string jobName, string jobDefinitionConfigKey, string jobQueueConfigKey, Dictionary<string, string>? parameters = null)
        {
            const string methodName = nameof(TriggerBatchJobGeneric);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName} - Triggering batch Job, JobName:{jobName}",
                    className, methodName, jobName);

                (string awsAccessKey, string awsSecretKey) = await GetAWSAccessKeyAndSecretKey();

                string batchJobDefinition = _configuration.GetSection(jobDefinitionConfigKey).Value ?? "";
                string batchJobQueue = _configuration.GetSection(jobQueueConfigKey).Value ?? "";

                var region = RegionEndpoint.USEast2;
                using var batchClient = new AmazonBatchClient(awsAccessKey, awsSecretKey, region);

                var request = new SubmitJobRequest
                {
                    JobName = jobName,
                    JobQueue = batchJobQueue,
                    JobDefinition = batchJobDefinition,
                    Parameters = parameters ?? new Dictionary<string, string>(),
                    ShareIdentifier = "1",
                    SchedulingPriorityOverride = 1
                };

                var response = await batchClient.SubmitJobAsync(request);

                if (response == null || string.IsNullOrEmpty(response.JobId))
                {
                    _logger.LogError("{ClassName}.{MethodName} - Failed triggering batch Job, JobName:{jobName}, ErrorCode:{Code}, parameters:{Parameters}",
                        className, methodName, jobName, StatusCodes.Status500InternalServerError, parameters!.ToJson());

                    throw new Exception("Failed to trigger batch job");
                }

                _logger.LogInformation(
                    "{ClassName}.{MethodName} - Successfully triggered batch Job, JobName:{jobName}, JobId:{JobId}",
                    className, methodName, jobName, response.JobId);

                return response.JobId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "{ClassName}.{MethodName} - Error triggering batch Job, JobName:{jobName}, Code:{Code}, parameters:{Parameters} ERROR:{Msg}",
                    className, methodName, jobName, StatusCodes.Status500InternalServerError, parameters!.ToJson(), ex.Message);

                throw new Exception("Failed to trigger batch job");
            }
        }

    }
}
