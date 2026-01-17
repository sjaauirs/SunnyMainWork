using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Bff.Infrastructure.Services.Interfaces;

public abstract class BaseSqsMessageListenerService : BackgroundService
{
    private readonly IAmazonSQS _sqsClient;
    private readonly ILogger<BaseSqsMessageListenerService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly string _className;
    private Func<string, Task<(bool, string)>>? _sendToDeadLetter;
    private string _queueUrl = string.Empty;
    private int _maxMessages = 10;
    private int _waitTimeSeconds = 10;

    protected BaseSqsMessageListenerService(
        IAmazonSQS sqsClient,
        ILogger<BaseSqsMessageListenerService> logger,
        IServiceScopeFactory scopeFactory,
        string className)
    {
        _sqsClient = sqsClient;
        _logger = logger;
        _scopeFactory = scopeFactory;
        _className = className;   // injecting class Name
    }

    protected abstract Task<string> GetQueueUrl(IAwsQueueService queueService);
    protected abstract Func<string, Task<(bool, string)>> GetDeadLetterFunc(IAwsQueueService queueService);
    protected abstract Task HandleMessage(string messageBody, IServiceProvider serviceProvider);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        const string methodName = nameof(ExecuteAsync);
        _logger.LogInformation("{Class}.{Method}: Starting SQS listener", _className, methodName);

        using (var scope = _scopeFactory.CreateScope())
        {
            var queueService = scope.ServiceProvider.GetRequiredService<IAwsQueueService>();
            _queueUrl = await GetQueueUrl(queueService);
            _sendToDeadLetter = GetDeadLetterFunc(queueService);
            _maxMessages = queueService.GetMaxNumberOfMessages();
            _waitTimeSeconds = queueService.GetWaitTimeSeconds();
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            var request = new ReceiveMessageRequest
            {
                QueueUrl = _queueUrl,
                MaxNumberOfMessages = _maxMessages,
                WaitTimeSeconds = _waitTimeSeconds
            };

            try
            {
                var response = await _sqsClient.ReceiveMessageAsync(request, stoppingToken);

                foreach (var message in response.Messages)
                {
                    _logger.LogInformation("{Class}.{Method}: Received message: {Body}", _className, methodName, message.Body);

                    try
                    {
                        using var scope = _scopeFactory.CreateScope();
                        await HandleMessage(message.Body, scope.ServiceProvider);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "{Class}.{Method}: Message handling failed", _className, methodName);
                        if (_sendToDeadLetter != null)
                        {
                            var (result, msg) = await _sendToDeadLetter(message.Body);
                            if (result)
                            {
                                _logger.LogInformation("{Class}.{Method}: Moved to DLQ", _className, methodName);
                            }
                            else
                            {
                                _logger.LogError("{Class}.{Method}: DLQ failed - {Msg}", _className, methodName, msg);
                            }
                        }
                    }

                    await _sqsClient.DeleteMessageAsync(_queueUrl, message.ReceiptHandle, stoppingToken);
                    _logger.LogInformation("{Class}.{Method}: Message deleted", _className, methodName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Class}.{Method}: Error receiving messages", _className, methodName);
            }

            await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
        }

        _logger.LogInformation("{Class}.{Method}: Stopping SQS listener", _className, methodName);
    }
}
