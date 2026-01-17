using Amazon;
using Amazon.SQS;
using Amazon.SQS.Model;
using Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SunnyRewards.Helios.Common.Core.Helpers;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Infrastructure.AwsConfig;
using SunnyRewards.Helios.ETL.Infrastructure.Logs.Interface;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services
{
    public class AwsQueueService : AwsConfiguration, IAwsQueueService
    {
        private readonly ILogger<AwsQueueService> _logger;

        private const string className=nameof(AwsQueueService);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="vault"></param>
        public AwsQueueService(IVault vault, ILogger<AwsQueueService> logger, IConfiguration configuration)
            : base(vault, configuration)
        {
            _logger = logger;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="awsQueueMessage"></param>
        /// <returns></returns>
        private async Task<(bool, string)> PushMessage(AwsQueueMessage awsQueueMessage)
        {
            const string methodName=nameof(PushMessage);
            try
            {
                string awsAccessKey = await GetAwsAccessKey();
                string awsSecretKey = await GetAwsSecretKey();

                // TODO :  push to amazon queue
                var sqsClient = new AmazonSQSClient(awsAccessKey, awsSecretKey, RegionEndpoint.USEast2);
                var request = new SendMessageRequest
                {
                    QueueUrl = awsQueueMessage.QueueUrl,
                    MessageBody = awsQueueMessage.Message
                };

                var response = await sqsClient.SendMessageAsync(request);

                if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    _logger.LogInformation("{ClassName}.{MethodName} - Message sent sucessfully with QueueUrl:{Url}", className, methodName,awsQueueMessage.QueueUrl);
                    return (true, "Message Sent!");
                }
                else
                {
                    _logger.LogError("{ClassName}.{MethodName} - Error occured while sending message with QueueUrl:{Url},ErrorCode:{Code}", className, methodName, awsQueueMessage.QueueUrl,response.HttpStatusCode);
                    return (false, $"Message not sent status code = {response.HttpStatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"{ClassName}.{MethodName} - Error occured while sending message with QueueUrl:{Url},ErrorCode:{Code},ERROR:{Msg}", className, methodName, awsQueueMessage.QueueUrl,StatusCodes.Status500InternalServerError,ex.Message);
                return (false, $"Message not sent error={ex.Message}");
            }
        }

        /// <summary>
        /// Send message to aws queue service
        /// </summary>
        /// <param name="taskUpdateDto"></param>
        /// <returns>bool = true/false for result, message = information message</returns>
        public async Task<(bool, string)> PushToTaskUpdateQueue(EtlTaskUpdateDto taskUpdateDto)
        {
            string queueUrl = await GetAwsTaskUpdateQueueUrl();
            const string methodName=nameof(PushToTaskUpdateQueue);
            try
            {
                var awsMessage = new AwsQueueMessage()
                {
                    QueueUrl = queueUrl,
                    Message = JsonConvert.SerializeObject(taskUpdateDto)
                };
                return await PushMessage(awsMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Error occured while sending message with QueueUrl:{Url},ErrorCode:{Code},ERROR:{Msg}", className, methodName, queueUrl, StatusCodes.Status500InternalServerError, ex.Message);
                return (false, $"Message not sent error={ex.Message}");
            }
        }

        public async Task<(bool, string)> PushToMemberImportEventDlqQueue(MemberEnrollmentDetailDto memberEnrollmentDetailDto)
        {
            string queueUrl = GetAwsMemberImportDlqQueueUrl();
            const string methodName = nameof(PushToMemberImportEventDlqQueue);
            try
            {
                var awsMessage = new AwsQueueMessage()
                {
                    QueueUrl = queueUrl,
                    Message = JsonConvert.SerializeObject(memberEnrollmentDetailDto)
                };
                return await PushMessage(awsMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Error occured while sending message with QueueUrl:{Url},ErrorCode:{Code},ERROR:{Msg}", className, methodName, queueUrl, StatusCodes.Status500InternalServerError, ex.Message);
                return (false, $"Message not sent error={ex.Message}");
            }
        }
        public async Task<(bool, string)> PushToBatchJobRecordQueue(ETLBatchJobRecordQueueRequestDto requestDto)
        {
            try
            {
               

                string queueUrl = await GetAwsBatchJobReportQueueUrl();

                var awsMessage = new AwsQueueMessage()
                {
                    QueueUrl = queueUrl,
                    Message = JsonConvert.SerializeObject(requestDto)
                };
                _logger.LogInformation("Sending below message to Queue: ");
                _logger.LogInformation(awsMessage.Message);
                return await PushMessage(awsMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"PushToQueue Error={ex.Message}");
                return (false, $"Message not sent error={ex.Message}");
            }
        }
    }
}