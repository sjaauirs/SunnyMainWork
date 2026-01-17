using Amazon;
using Amazon.Runtime;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Infrastructure.AwsConfig;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services
{
    public class AwsNotificationService : AwsConfiguration, IAwsNotificationService
    {
        private readonly ILogger<AwsNotificationService> _logger;
        private const string className=nameof(AwsNotificationService);
        public AwsNotificationService(IVault vault, ILogger<AwsNotificationService> logger, IConfiguration configuration)
           : base(vault, configuration)
        {
            _logger = logger;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="snsMessage"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<(bool, string)> PushNotificationToAwsTopic(AwsSnsMessage snsMessage, string topicName, bool isFifo = false, string messageGroupId = "", string deDuplicationId = "")
        {
            const string methodName=nameof(PushNotificationToAwsTopic);
            try
            {
                string awsAccessKey = await GetAwsAccessKey();
                string awsSecretKey = await GetAwsSecretKey();

                // Configure AWS credentials

                AWSCredentials credentials = new BasicAWSCredentials(awsAccessKey, awsSecretKey);
                AmazonSimpleNotificationServiceClient snsClient = new(credentials, RegionEndpoint.USEast2);

                // Replace with your SNS topic ARN
                string topicArn = GetAwsSnsTopic(topicName);

                var publishRequest = new PublishRequest
                {
                    TopicArn = topicArn,
                    Message = snsMessage.Message,
                };

                if (isFifo)
                {
                    publishRequest.MessageGroupId = messageGroupId;
                    publishRequest.MessageDeduplicationId = deDuplicationId;
                }

                PublishResponse publishResponse = await snsClient.PublishAsync(publishRequest);

                return (true, publishResponse.MessageId);
            }
            catch (AmazonSimpleNotificationServiceException snsException)
            {
                _logger.LogError(snsException, "{ClassName}.{MethodName} - Error occured in PushNotificationToAwsTopic,ErrorCode:{Code}, ERROR:{Message}",className,methodName,StatusCodes.Status500InternalServerError,snsException.Message);
                return (false, $"Message not sent error={snsException.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Error occured in PushNotificationToAwsTopic,ErrorCode:{Code},ERROR:{Message}", className, methodName,StatusCodes.Status500InternalServerError, ex.Message);
                return (false, $"Message not sent error={ex.Message}");
            }
        }

        public async Task<(bool, string)> PushNotificationBatchToAwsTopic(
                List<AwsSnsMessage> snsMessages,
                string topicName,
                bool isFifo = false,
                string messageGroupId = "",
                string deDuplicationPrefix = "")
        {
            const string methodName = nameof(PushNotificationBatchToAwsTopic);

            try
            {
                string awsAccessKey = await GetAwsAccessKey();
                string awsSecretKey = await GetAwsSecretKey();

                var credentials = new BasicAWSCredentials(awsAccessKey, awsSecretKey);
                using var snsClient = new AmazonSimpleNotificationServiceClient(credentials, RegionEndpoint.USEast2);

                string topicArn = GetAwsSnsTopic(topicName);

                // Build the batch request
                var batchRequest = new PublishBatchRequest
                {
                    TopicArn = topicArn,
                    PublishBatchRequestEntries = snsMessages
                        .Select((m, i) => new PublishBatchRequestEntry
                        {
                            Id = $"{Guid.NewGuid():N}-{i}",
                            Message = m.Message,
                            MessageGroupId = isFifo ? messageGroupId : null,
                            MessageDeduplicationId = isFifo ? $"{deDuplicationPrefix}-{Guid.NewGuid():N}" : null
                        }).ToList()
                };

                var response = await snsClient.PublishBatchAsync(batchRequest);

                // Handle partial failures
                if (response.Failed.Any())
                {
                    var failed = string.Join(", ", response.Failed.Select(f => $"{f.Id}:{f.Message}"));
                    _logger.LogError(
                        "{ClassName}.{MethodName}: Some messages failed to publish: {FailedIds}",
                        className, methodName, failed);

                    return (false, $"Partial failure: {failed}");
                }

                return (true, $"{response.Successful.Count} messages published successfully.");
            }
            catch (AmazonSimpleNotificationServiceException snsException)
            {
                _logger.LogError(snsException,
                    "{ClassName}.{MethodName}: SNS batch publish failed. Code:{Code}, Message:{Message}",
                    className, methodName, StatusCodes.Status500InternalServerError, snsException.Message);

                return (false, $"Batch publish failed: {snsException.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "{ClassName}.{MethodName}: Exception in batch publish. Code:{Code}, Message:{Message}",
                    className, methodName, StatusCodes.Status500InternalServerError, ex.Message);

                return (false, $"Batch publish failed: {ex.Message}");
            }
        }

    }
}
