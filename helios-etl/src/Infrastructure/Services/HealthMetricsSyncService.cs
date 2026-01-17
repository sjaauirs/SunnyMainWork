using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SQS;
using Amazon.SQS.Model;
using CsvHelper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NHibernate;
using SunnyRewards.Helios.ETL.Common.Domain.Models;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using SunnyRewards.Helios.ETL.Infrastructure.AwsConfig;
using SunnyRewards.Helios.ETL.Infrastructure.Helpers.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.User.Core.Domain.Constant;
using System.Text;
using Microsoft.AspNetCore.Http;
using ISession = NHibernate.ISession;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services
{
    public class HealthMetricsSyncService : AwsConfiguration, IHealtMetricsSyncService
    {
        private readonly IHealthMetricRepo _healthMetricRepo;
        private readonly IHealthMetricTypeRepo _healthMetricTypeRepo;
        private readonly ILogger<HealthMetricsSyncService> _logger;
        private readonly ISession _session;
        private const string className=nameof(HealthMetricsSyncService);
        public HealthMetricsSyncService(IVault vault, ILogger<HealthMetricsSyncService> logger, IConfiguration configuration,
             IHealthMetricRepo healthMetricRepo, IHealthMetricTypeRepo healthMetricTypeRepo, ISession session)
            : base(vault, configuration)
        {
            _logger = logger;
            _healthMetricRepo = healthMetricRepo;
            _healthMetricTypeRepo = healthMetricTypeRepo;
            _session = session;
        }

        /// <summary>
        /// Processes messages from the Retail product AWS SQS queue and generate FIS APL file.
        /// </summary>
        /// <param name="etlExecutionContext"></param>
        /// <returns></returns>
        public async Task ProcessQueueMessages(EtlExecutionContext etlExecutionContext)
        {
            const string methodName = nameof(ProcessQueueMessages);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName} - Starting to process queue messages...", className, methodName);
                string queueUrl = await GetHealthMetricQueueUrl();
                var maxNumberOfMessages = 10;
                var records = new List<HealthMetricModel>();
                var processedMessages = new List<Message>();
                using (var sqsClient = new AmazonSQSClient(GetAwsAccessKey().Result, GetAwsSecretKey().Result, RegionEndpoint.USEast2))
                {
                    while (true)
                    {
                        var receiveRequest = new ReceiveMessageRequest
                        {
                            QueueUrl = queueUrl,
                            MaxNumberOfMessages = maxNumberOfMessages
                        };

                        var receiveResponse = await sqsClient.ReceiveMessageAsync(receiveRequest);

                        if (!receiveResponse.Messages.Any())
                        {
                            _logger.LogInformation("{ClassName}.{MethodName} - No messages found in the queue. Exiting processing loop.", className, methodName);
                            break;
                        }

                        var healthMetricModels = await ConvertToHealthMetricModels(receiveResponse.Messages, processedMessages);
                        records.AddRange(healthMetricModels);
                        await DeleteMessages(sqsClient, queueUrl, processedMessages);
                        _logger.LogInformation("{ClassName}.{MethodName} - Processed and deleted messages from the queue.", className, methodName);
                    }
                }

                // Write all records to CSV file
                if (records.Any())
                {
                    await PersistHealthModels(records);
                    _logger.LogInformation("{ClassName}.{MethodName} - health models persisted in DB.", className, methodName);
                }
                else
                {
                    _logger.LogInformation("{ClassName}.{MethodName} - No records to write to CSV file.", className, methodName);
                }

                _logger.LogInformation("{ClassName}.{MethodName} - Finished processing queue messages.", className, methodName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Error occurred while processing queue messages, ErrorCode:{Code},ERROR: {Message}", className, methodName,StatusCodes.Status500InternalServerError,ex.Message);
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="records"></param>
        /// <returns></returns>
        private async Task PersistHealthModels(List<HealthMetricModel> records)
        {
            const string methodName=nameof(PersistHealthModels);
            var transaction = _session.BeginTransaction();
            try
            {
                foreach (var model in records)
                {
                    await _session.SaveAsync(model);
                }
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "{ClassName}.{MethodName} - Error occurred while processing queue messages, ErrorCode:{Code},ERROR: {Message}", className, methodName, StatusCodes.Status500InternalServerError,ex.Message);
                throw;
            }
        }

        private async Task<List<HealthMetricModel>> ConvertToHealthMetricModels(List<Message> messages, List<Message> processedMessages)
        {
            
            var records = new List<HealthMetricModel>();
            foreach (var message in messages)
            {
                var messageBody = JsonConvert.DeserializeObject<HealthMetricMessageDto>(message.Body);
                if (messageBody != null)
                {
                    var record = await MapMessageToHealthMetricRecord(messageBody);
                    if (record != null)
                    {
                        records.Add(record);
                        processedMessages.Add(message);
                    }
                }
            }
            return records;
        }

        private async Task DeleteMessages(AmazonSQSClient sqsClient, string queueUrl, List<Message> messages)
        {
            const string methodName=nameof(DeleteMessages);
            var deleteRequests = messages.Select(message => new DeleteMessageBatchRequestEntry
            {
                Id = message.MessageId,
                ReceiptHandle = message.ReceiptHandle
            }).ToList();

            _logger.LogInformation("{ClassName}.{MethodName} - Deleting messages from the queue...", className, methodName);
            await sqsClient.DeleteMessageBatchAsync(new DeleteMessageBatchRequest
            {
                QueueUrl = queueUrl,
                Entries = deleteRequests
            });
            _logger.LogInformation("{ClassName}.{MethodName} - Messages deleted successfully.", className, methodName);
        }

        private async Task<HealthMetricModel?> MapMessageToHealthMetricRecord(HealthMetricMessageDto message)
        {
            var now = DateTime.UtcNow;
            var healtMetricTypeData = await _healthMetricTypeRepo.FindOneAsync(x => x.HealthMetricTypeCode == message.HealthMetricTypeCode && x.DeleteNbr == 0);
            var healthMetricModel = new HealthMetricModel
            {
                HealthMetricTypeId = healtMetricTypeData.HealthMetricTypeId,
                ConsumerCode = message.ConsumerCode,
                CaptureTs = message.CaptureTs,
                DataJson = message.DataJson,
                TenantCode = message.TenantCode,
                CreateTs = now,
                CreateUser = "ETL",
                OsMetricTs = message.OsMetricTs
            };

            return healthMetricModel;
        }
    }
}
