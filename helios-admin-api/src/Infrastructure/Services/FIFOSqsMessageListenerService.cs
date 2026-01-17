using Amazon.SQS;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Admin.Infrastructure.Services;
using SunnyRewards.Helios.Bff.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using System;
using System.Text.Json;

public class FIFOSqsMessageListenerService : BaseSqsMessageListenerService
{
    public FIFOSqsMessageListenerService(IAmazonSQS sqsClient, ILogger<BaseSqsMessageListenerService> logger, IServiceScopeFactory factory)
        : base(sqsClient, logger, factory, nameof(FIFOSqsMessageListenerService)) { }

    protected override Task<string> GetQueueUrl(IAwsQueueService queueService) =>
        queueService.GetAwsConsumerCohortQueueUrl();

    protected override Func<string, Task<(bool, string)>> GetDeadLetterFunc(IAwsQueueService queueService) =>
        queueService.PushEventToConsumerCohortEventDeadLetterQueue;

    protected override async Task HandleMessage(string body, IServiceProvider services)
    {
        var logger = services.GetRequiredService<ILogger<BaseSqsMessageListenerService>>();
        logger.LogInformation($"Message Received: {body}");
        var eventProcessorFactory = services.GetRequiredService<IEventProcessorFactory>();
        var postedEvent = JsonSerializer.Deserialize<EventDto<dynamic>>(body);


        var processor = eventProcessorFactory.GetEventDtoProcessor(postedEvent?.Header.EventType ?? string.Empty);
        if (processor == null)
            throw new InvalidOperationException("Processor not found.");

        logger.LogInformation($" Processing Message: {body}");
        await processor.ProcessEvent(postedEvent!);
    }
}

