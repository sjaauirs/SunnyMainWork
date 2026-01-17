using Amazon;
using Amazon.SQS;
using Amazon.SQS.Model;
using Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Core.Domain.Models;
using SunnyRewards.Helios.Bff.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using System.Text.Json;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services
{
    public class AwsQueueService : IAwsQueueService
    {
        private readonly ILogger<AwsQueueService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IVault _vault;
        private const string className = nameof(AwsQueueService);
        public AwsQueueService(IVault vault, ILogger<AwsQueueService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _vault = vault;
            _configuration = configuration;
        }

        /// <summary>
        /// Push Event to Consumer Event Queue
        /// </summary>
        /// <param name="postEventRequestModel"></param>
        /// <returns></returns>
        public async Task<(bool, string)> PushEventToConsumerEventQueue(PostEventRequestModel postEventRequestModel)
        {
            const string methodName = nameof(PushEventToConsumerEventQueue);
            try
            {
                string queueUrl = await GetAwsConsumerEventQueueUrl();
                var awsMessage = new AwsQueueMessage()
                {
                    QueueUrl = queueUrl,
                    Message = JsonConvert.SerializeObject(postEventRequestModel)
                };
                return await PushMessage(awsMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}:PushToQueue Error:{Message}",className,methodName,ex.Message);
                return (false, $"An error occurred while sending the message: {ex.Message}");
            }
        }



        /// <summary>
        /// Push Event to Dead Letter Consumer Event Queue
        /// </summary>
        /// <param name="postEventRequestModel"></param>
        /// <returns></returns>
        public async Task<(bool, string)> PushEventToConsumerEventDeadLetterQueue(string eventMessage)
        {
            const string methodName = nameof(PushEventToConsumerEventDeadLetterQueue);
            try
            {
                _logger.LogInformation("Sending Message to Dead Letter");
                string queueUrl = await GetAwsConsumerEventDeadLetterQueueUrl();

                var awsMessage = new AwsQueueMessage()
                {
                    QueueUrl = queueUrl,
                    Message = eventMessage
                };
                return await PushMessage(awsMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}:PushToQueue Error:{Message}", className, methodName, ex.Message);
                return (false, $"An error occurred while sending the message: {ex.Message}");
            }
        }

        /// <summary>
        /// return Consumer Event Queue Name
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetAwsConsumerEventQueueUrl() => await _vault.GetSecret(_configuration.GetSection("AWS:AWS_CONSUMER_EVENT_QUEUE_URL_KEY_NAME").Value?.ToString() ?? "");


        public async Task<string> GetAwsErrorQueueUrl() => await _vault.GetSecret(_configuration.GetSection("AWS:AWS_ERROR_QUEUE_URL_KEY_NAME").Value?.ToString() ?? "");


        /// <summary>
        /// return consumer event dead letter queue name
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetAwsConsumerEventDeadLetterQueueUrl() => await _vault.GetSecret(_configuration.GetSection("AWS:AWS_CONSUMER_EVENT_DEAD_LETTER_QUEUE_URL_KEY_NAME").Value?.ToString() ?? "");
        /// <summary>
        ///  Method to push event in Queue
        /// </summary>
        /// <param name="awsQueueMessage"></param>
        /// <returns></returns>
        private async Task<(bool ok, string msg)> PushMessage(AwsQueueMessage awsQueueMessage,string? messageGroupId = null,string? messageDeduplicationId = null)
        {
            try
            {
                string awsAccessKey = await GetAwsAccessKey();
                string awsSecretKey = await GetAwsSecretKey();
                using var sqsClient = new AmazonSQSClient(awsAccessKey, awsSecretKey, RegionEndpoint.USEast2);

                var request = new SendMessageRequest
                {
                    QueueUrl = awsQueueMessage.QueueUrl,
                    MessageBody = awsQueueMessage.Message
                };

                bool isFifo = awsQueueMessage.QueueUrl.EndsWith(".fifo", StringComparison.OrdinalIgnoreCase);
                if (isFifo)
                {

                    request.MessageGroupId = string.IsNullOrWhiteSpace(messageGroupId) ? "default" : messageGroupId;

                    request.MessageDeduplicationId = string.IsNullOrWhiteSpace(messageDeduplicationId)
                        ? Guid.NewGuid().ToString("N")
                        : messageDeduplicationId;
                }

                var response = await sqsClient.SendMessageAsync(request);

                if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    _logger.LogInformation(
                        "PushMessage: Sent. MessageId={MessageId} SequenceNumber={SequenceNumber} GroupId={GroupId} DedupId={DedupId}",
                        response.MessageId, response.SequenceNumber, request.MessageGroupId, request.MessageDeduplicationId);

                    return (true, $"Sent: {response.MessageId}");
                }

                _logger.LogError("PushMessage: Not sent. Status={Status}. Response={Response}",
                    response.HttpStatusCode, JsonConvert.SerializeObject(response));
                return (false, $"Not sent, status {response.HttpStatusCode}");
            }
            catch (AmazonSQSException sqsEx)
            {
                _logger.LogError(sqsEx, "PushMessage: SQS error {ErrorCode}: {Message}", sqsEx.ErrorCode, sqsEx.Message);
                return (false, $"{sqsEx.ErrorCode}: {sqsEx.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PushMessage: Unexpected error.");
                return (false, $"Unexpected: {ex.Message}");
            }
        }


        private async Task<string> GetAwsAccessKey() => await _vault.GetSecret(_configuration.GetSection("AWS:AWS_ACCESS_KEY_NAME").Value?.ToString() ?? "");
        private async Task<string> GetAwsSecretKey() => await _vault.GetSecret(_configuration.GetSection("AWS:AWS_SECRET_KEY_NAME").Value?.ToString() ?? "");

        public int GetMaxNumberOfMessages()
        {
            var maxMessagesString = _configuration.GetSection("AWS:MAX_NUMBER_MSG").Value;

            // Try to parse the string to an integer, and return the result.
            if (int.TryParse(maxMessagesString, out int maxMessages))
            {
                return maxMessages;
            }

            // Return a default value if parsing fails (e.g., return 10 if no valid value is found)
            return 10; //  MAX Default value allowed 
        }
 
        public int GetWaitTimeSeconds()
        {
            var maxMessagesString = _configuration.GetSection("AWS:WAIT_TIME_SECONDS").Value;

            // Try to parse the string to an integer, and return the result.
            if (int.TryParse(maxMessagesString, out int maxMessages))
            {
                return maxMessages;
            }

            // Return a default value if parsing fails (e.g., return 10 if no valid value is found)
            return 20; //  MAX Default value allowed 
        }

        public async Task<string> GetAwsConsumerCohortQueueUrl() => await _vault.GetSecret(_configuration.GetSection("AWS:AWS_CONSUMER_COHORT_QUEUE_URL_KEY_NAME").Value?.ToString() ?? "");
        public async Task<string> GetAwsConsumerCohortDeadLetterQueueUrl() => await _vault.GetSecret(_configuration.GetSection("AWS:AWS_CONSUMER_COHORT_DEAD_LETTER_QUEUE_URL_KEY_NAME").Value?.ToString() ?? "");
        public async Task<(bool, string)> PushEventToConsumerCohortEventDeadLetterQueue(string postEventRequestModel)
        {
            const string methodName = nameof(PushEventToConsumerCohortEventDeadLetterQueue);
            try
            {
                string queueUrl = await GetAwsConsumerCohortDeadLetterQueueUrl();
                var awsMessage = new AwsQueueMessage()
                {
                    QueueUrl = queueUrl,
                    Message = JsonConvert.SerializeObject(postEventRequestModel)
                };
                return await PushMessage(awsMessage , "COHORT_EVENT");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}:PushToQueue Error:{Message}", className, methodName, ex.Message);
                return (false, $"An error occurred while sending the message: {ex.Message}");
            }
        }

        public async Task<(bool, string)> PushMessageToErrortQueue(ConsumerErrorEventDto consumerErrorEventDto)
        {
            const string methodName = nameof(PushMessageToErrortQueue);
            try
            {
                string queueUrl = await GetAwsErrorQueueUrl();
                var awsMessage = new AwsQueueMessage()
                {
                    QueueUrl = queueUrl,
                    Message = JsonConvert.SerializeObject(consumerErrorEventDto)
                };
                return await PushMessage(awsMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}:PushToQueue Error:{Message}", className, methodName, ex.Message);
                return (false, $"An error occurred while sending the message: {ex.Message}");
            }
        }
    }
}
