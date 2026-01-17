using Amazon.SQS;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Core.Domain.Models;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Bff.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using System.Text.Json;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services
{
    public class SqsMessageListenerService : BaseSqsMessageListenerService
    {
        public SqsMessageListenerService(
            IAmazonSQS sqsClient,
            ILogger<BaseSqsMessageListenerService> logger,
            IServiceScopeFactory scopeFactory)
            : base(sqsClient, logger, scopeFactory, nameof(SqsMessageListenerService)) { }

        protected override async Task<string> GetQueueUrl(IAwsQueueService queueService)
            => await queueService.GetAwsConsumerEventQueueUrl();

        protected override Func<string, Task<(bool, string)>> GetDeadLetterFunc(IAwsQueueService queueService)
            => queueService.PushEventToConsumerEventDeadLetterQueue;

        protected override async System.Threading.Tasks.Task HandleMessage(string messageBody, IServiceProvider serviceProvider)
        {

            var eventProcessorFactory = serviceProvider.GetRequiredService<IEventProcessorFactory>();
            var logger = serviceProvider.GetRequiredService<ILogger<BaseSqsMessageListenerService>>();        

            PostEventRequestModel? postedEvent = null;
            EventDto<ConsumerTaskEventDto>? taskEvent = null;
            EventDto<AgreementsVerifiedEventDto>? agreementsEvent = null;
            bool isConsumerTask = false;
            bool isAgreementsVerified = false;

            try
            {
                postedEvent = JsonSerializer.Deserialize<PostEventRequestModel>(messageBody);
                if (postedEvent?.EventType == null || postedEvent?.EventSubtype == null)
                {
                    var snsMessage = JsonSerializer.Deserialize<SnsMessage<string>>(messageBody);
                    if (snsMessage?.Message == null)
                        throw new InvalidOperationException("Invalid SNS message format.");
                    
                    // Try to deserialize as ConsumerTaskEventDto first
                    try
                    {
                        taskEvent = JsonSerializer.Deserialize<EventDto<ConsumerTaskEventDto>>(snsMessage.Message);
                        if (taskEvent?.Header == null || taskEvent?.Data == null)
                            throw new InvalidOperationException("Invalid ConsumerTask event payload.");
                        isConsumerTask = true;
                    }
                    catch
                    {
                        // If ConsumerTaskEventDto fails, try AgreementsVerifiedEventDto
                        try
                        {
                            agreementsEvent = JsonSerializer.Deserialize<EventDto<AgreementsVerifiedEventDto>>(snsMessage.Message);
                            if (agreementsEvent?.Header == null || agreementsEvent?.Data == null)
                                throw new InvalidOperationException("Invalid AgreementsVerified event payload.");
                            isAgreementsVerified = true;
                        }
                        catch
                        {
                            throw new InvalidOperationException("Unknown event type in SNS message.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Deserialization failed. Raw message: {Body}", messageBody);
                throw;
            }

            if (isConsumerTask)
            {
                var processor = eventProcessorFactory.GetConsumerTaskEventProcesser();
                if (processor == null) throw new InvalidOperationException("No processor for consumer task event.");
                var success = await processor.ProcessEvent(taskEvent!);
                logger.LogInformation("ConsumerTask Event processed:   STATUS: {Status} - {EventId}", success, taskEvent?.Header.EventId);
            }
            else if (isAgreementsVerified)
            {
                var processor = eventProcessorFactory.GetAgreementsVerifiedEventProcessor();
                if (processor == null) throw new InvalidOperationException("No processor for agreements verified event.");
                var success = await processor.ProcessEvent(agreementsEvent!);
                logger.LogInformation("AgreementsVerified Event processed: STATUS: {Status} - {EventId}", success, agreementsEvent?.Header.EventId);
            }
            else
            {
                var processor = eventProcessorFactory.GetEventProcessor(postedEvent?.EventType ?? string.Empty);
                if (processor == null) throw new InvalidOperationException("No processor for generic event.");
                var success = await processor.ProcessEvent(postedEvent!);
                logger.LogInformation("Event processed: STATUS:{Status} - {EventCode}", success, postedEvent?.EventCode);
            }
        }
    }
}
