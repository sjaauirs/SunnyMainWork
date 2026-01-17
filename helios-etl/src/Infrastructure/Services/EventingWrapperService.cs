using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces;
using System.Security.Cryptography;
using System.Collections.Concurrent;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Common.Constants;
using SunnyRewards.Helios.ETL.Core.Domain.Constants;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services
{
    public class EventingWrapperService : IEventingWrapperService
    {
        private readonly ILogger<EventingWrapperService> _logger;
        private readonly IAwsNotificationService _awsNotificationService;
        private const string ClassName = nameof(EventingWrapperService);

        public EventingWrapperService(
            ILogger<EventingWrapperService> logger,
            IAwsNotificationService awsNotificationService)
        {
            _logger = logger;
            _awsNotificationService = awsNotificationService;
        }

        // Common utility methods that can be used by the eventing services
        public async Task<List<(long RowId, bool Published, string Error)>> PublishMessagesInParallelAsync(
            List<(string EventMessage, long RowId, string EventId, string EventType, string PersonUniqueIdentifier)> messages,
            string jobId,
            string topicName)
        {
            var results = new ConcurrentBag<(long RowId, bool Published, string Error)>();

            await Parallel.ForEachAsync(messages, new ParallelOptions { MaxDegreeOfParallelism = 20 }, async (msg, _) =>
            {
                try
                {
                    var publishResult = await PublishMessageWithRetries(
                        msg.EventMessage, 
                        msg.PersonUniqueIdentifier, 
                        msg.RowId.ToString(), 
                        topicName);
                    results.Add((msg.RowId, publishResult.Published, publishResult.ErrorMessage));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error publishing message for RowId: {RowId}", msg.RowId);
                    results.Add((msg.RowId, false, ex.Message));
                }
            });

            return results.ToList();
        }

        private async Task<PublishResultDto> PublishMessageWithRetries(
            string snsMessage, 
            string messageGroupId, 
            string deDuplicationId, 
            string topicName)
        {
            int maxTries = NotificationConstants.MaxTries;
            var publishResult = new PublishResultDto
            {
                Published = false,
                ErrorMessage = string.Empty
            };
            var message = new AwsSnsMessage(snsMessage);

            while (maxTries > 0)
            {
                try
                {
                    (publishResult.Published, publishResult.ErrorMessage) = await _awsNotificationService.PushNotificationToAwsTopic(
                        message, topicName, true, messageGroupId, deDuplicationId);

                    if (publishResult.Published)
                    {
                        _logger.LogInformation("{ClassName}.{MethodName}: Message published successfully. Retries left: {MaxTries}, EventData: {EventData}",
                            ClassName, nameof(PublishMessageWithRetries), maxTries, snsMessage);
                        break;
                    }

                    _logger.LogWarning("{ClassName}.{MethodName}: Message publishing failed. Retries left: {MaxTries}, ErrorMessage: {ErrorMessage}, EventData: {EventData}",
                        ClassName, nameof(PublishMessageWithRetries), maxTries, publishResult.ErrorMessage, snsMessage);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "{ClassName}.{MethodName}: Exception occurred while publishing message. Retries left: {MaxTries}, EventData: {EventData}",
                        ClassName, nameof(PublishMessageWithRetries), maxTries, snsMessage);
                }

                maxTries--;
                if (maxTries > 0)
                {
                    await Task.Delay(GetSecureRandomNumber(NotificationConstants.RetryMinWaitMS, NotificationConstants.RetryMaxWaitMS));
                }
            }

            return publishResult;
        }

        private static int GetSecureRandomNumber(int minValue, int maxValue)
        {
            var buffer = new byte[4];
            RandomNumberGenerator.Fill(buffer);
            int result = BitConverter.ToInt32(buffer, 0) & int.MaxValue;
            return minValue + (result % (maxValue - minValue));
        }
    }
}
