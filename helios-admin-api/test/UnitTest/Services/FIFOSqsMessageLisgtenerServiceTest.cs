using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using SunnyRewards.Helios.Admin.Core.Domain.Models;
using SunnyRewards.Helios.Admin.Infrastructure.Services;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Bff.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Tenant.Core.Domain.Models;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Models;
using System.Text.Json;
using Xunit;
using TaskA = System.Threading.Tasks.Task;


namespace SunnyRewards.Helios.Admin.UnitTest.Services
{
    public class FIFOSqsMessageListenerServiceTests
    {
        private readonly Mock<IAmazonSQS> _mockSqsClient;
        private readonly Mock<ILogger<BaseSqsMessageListenerService>> _mockLogger;
        private readonly Mock<IServiceScopeFactory> _mockScopeFactory;
        private readonly Mock<IServiceScope> _mockScope;
        private readonly Mock<IAwsQueueService> _mockQueueService;
        private readonly Mock<IEventProcessorFactory> _mockProcessorFactory;
        private readonly Mock<IEventDtoProcessor> _mockProcessor;

        public FIFOSqsMessageListenerServiceTests()
        {
            _mockSqsClient = new Mock<IAmazonSQS>();
            _mockLogger = new Mock<ILogger<BaseSqsMessageListenerService>>();
            _mockScopeFactory = new Mock<IServiceScopeFactory>();
            _mockScope = new Mock<IServiceScope>();
            _mockQueueService = new Mock<IAwsQueueService>();
            _mockProcessorFactory = new Mock<IEventProcessorFactory>();
            _mockProcessor = new Mock<IEventDtoProcessor>();

            var services = new ServiceCollection();
            services.AddLogging(); // registers ILoggerFactory + ILogger<T>
            services.AddSingleton(_mockQueueService.Object);
            services.AddSingleton(_mockProcessorFactory.Object);
            var provider = services.BuildServiceProvider();

            _mockScopeFactory.Setup(f => f.CreateScope()).Returns(_mockScope.Object);
            _mockScope.SetupGet(s => s.ServiceProvider).Returns(provider);
        }

        [Fact]
        public async TaskA ExecuteAsync_ProcessesMessagesSuccessfully()
        {
            // Arrange
            var queueUrl = "consumer-cohort-test-queue-url";

            var eventDto = new EventDto<CohortEventDto>()
            {
                Header = new EventHeaderDto
                {
                    EventId = Guid.NewGuid().ToString("N"),
                    EventType = "COHORT_EVENT",
                    EventSubtype = "COHORT_ASSIGNMENT",
                    PublishTs = DateTime.UtcNow,
                    TenantCode = "tem-010101",
                    ConsumerCode = "con-11111",
                    SourceModule = "UserService"
                },
                Data = new CohortEventDto()
                {

                    EventId = "req-etrte",
                    TenantCode = "ten-3432",
                    TriggeredBy = "TEST",
                    ConsumerCode = "con-11111",

                }
            };

            var messageBody = JsonSerializer.Serialize(eventDto);
            _mockQueueService.Setup(q => q.GetAwsConsumerCohortQueueUrl()).ReturnsAsync(queueUrl);
            _mockProcessorFactory.Setup(f => f.GetEventDtoProcessor("COHORT_EVENT")).Returns(_mockProcessor.Object);
            _mockProcessor.Setup(p => p.ProcessEvent(It.IsAny<EventDto<Dictionary<string, object>>>())).ReturnsAsync(true);

            _mockSqsClient.Setup(sqs => sqs.ReceiveMessageAsync(It.IsAny<ReceiveMessageRequest>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync(new ReceiveMessageResponse
                          {
                              Messages = new List<Message>
                              {
                              new Message { Body = messageBody, ReceiptHandle = "receipt-handle" }
                              }
                          });

            _mockSqsClient.Setup(sqs => sqs.DeleteMessageAsync(queueUrl, "receipt-handle", default))
                          .ReturnsAsync(new DeleteMessageResponse());

            var service = new FIFOSqsMessageListenerService(
                _mockSqsClient.Object,
                _mockLogger.Object,
                _mockScopeFactory.Object
            );

            var cancellationToken = new CancellationTokenSource().Token;

            // Act
            await service.StartAsync(cancellationToken);
            await TaskA.Delay(100); // Allow the service to process
            await service.StopAsync(cancellationToken);

            // Assert

            _mockProcessor.Verify(p => p.ProcessEvent(It.IsAny<EventDto<dynamic>>()), Times.Once);
            _mockSqsClient.Verify(sqs => sqs.DeleteMessageAsync(queueUrl, "receipt-handle", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async TaskA ExecuteAsync_SendsMessageToDeadLetterQueue()
        {
            // Arrange
            var queueUrl = "consumer-cohort-test-queue-url";

            var eventDto = new EventDto<Dictionary<string, object>>()
            {
                Header = new EventHeaderDto
                {
                    EventId = Guid.NewGuid().ToString("N"),
                    EventType = "UNKNOWN",
                    EventSubtype = "COHORT_ASSIGNMENT",
                    PublishTs = DateTime.UtcNow,
                    TenantCode = "tem-010101",
                    ConsumerCode = "con-11111",
                    SourceModule = "UserService"
                },
                Data = new Dictionary<string, object>()
                {
                    ["EventId"] = "Req-21321",
                    ["ConsumerCode"] = "con-11111",
                    ["TenantCode"] = "tem-010101",
                    ["TriggeredBy"] = "TEST"
                }
            };
            var messageBody = JsonSerializer.Serialize(eventDto);

            _mockQueueService.Setup(q => q.GetAwsConsumerCohortQueueUrl()).ReturnsAsync(queueUrl);
            _mockQueueService.Setup(q => q.PushEventToConsumerCohortEventDeadLetterQueue(It.IsAny<string>()))
                             .ReturnsAsync((true, "Sent to dead letter"));

            _mockProcessorFactory.Setup(f => f.GetEventProcessor("UnknownEvent"));

            _mockSqsClient.Setup(sqs => sqs.ReceiveMessageAsync(It.IsAny<ReceiveMessageRequest>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync(new ReceiveMessageResponse
                          {
                              Messages = new List<Message>
                              {
                              new Message { Body = messageBody, ReceiptHandle = "receipt-handle" }
                              }
                          });

            var service = new FIFOSqsMessageListenerService(
                _mockSqsClient.Object,
                _mockLogger.Object,
                _mockScopeFactory.Object
            );

            var cancellationToken = new CancellationTokenSource().Token;

            // Act
            await service.StartAsync(cancellationToken);
            await TaskA.Delay(100); // Allow the service to process
            await service.StopAsync(cancellationToken);

            // Assert
            _mockQueueService.Verify(q => q.PushEventToConsumerCohortEventDeadLetterQueue(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async TaskA ExecuteAsync_HandlesExceptionGracefully()
        {
            // Arrange
            var queueUrl = "consumer-cohort-test-queue-url";
            _mockQueueService.Setup(q => q.GetAwsConsumerCohortQueueUrl()).ReturnsAsync(queueUrl);

            _mockSqsClient.Setup(sqs => sqs.ReceiveMessageAsync(It.IsAny<ReceiveMessageRequest>(), It.IsAny<CancellationToken>()))
                          .ThrowsAsync(new Exception("SQS error"));

            var service = new FIFOSqsMessageListenerService(
                _mockSqsClient.Object,
                _mockLogger.Object,
                _mockScopeFactory.Object
            );

            var cancellationToken = new CancellationTokenSource().Token;

            // Act
            await service.StartAsync(cancellationToken);
            await TaskA.Delay(100); // Allow the service to process
            await service.StopAsync(cancellationToken);

            // Assert
            _mockSqsClient.Verify(sqs => sqs.DeleteMessageAsync(queueUrl, "receipt-handle", It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async TaskA ExecuteAsync_ShouldHandleDeserializationError()
        {
            var queueUrl = "consumer-cohort-test-queue-url";
            var eventDto = new EventDto<Dictionary<string, object>>()
            {
                Header = new EventHeaderDto
                {
                    EventId = Guid.NewGuid().ToString("N"),
                    EventType = "COHORT_EVENT",
                    EventSubtype = "COHORT_ASSIGNMENT",
                    PublishTs = DateTime.UtcNow,
                    TenantCode = "tem-010101",
                    ConsumerCode = "con-11111",
                    SourceModule = "UserService"
                },
                Data = new Dictionary<string, object>()
                {
                    ["EventId"] = "Req-21321",
                    ["ConsumerCode"] = "con-11111",
                    ["TenantCode"] = "tem-010101",
                    ["TriggeredBy"] = "TEST"
                }
            };

            var messageBody = JsonSerializer.Serialize(eventDto);

            _mockQueueService.Setup(q => q.GetAwsConsumerCohortQueueUrl()).ReturnsAsync(queueUrl);
            _mockProcessorFactory.Setup(f => f.GetEventDtoProcessor("TestEvent")).Returns(_mockProcessor.Object);
            _mockProcessor.Setup(p => p.ProcessEvent(It.IsAny<EventDto<Dictionary<string, object>>>())).ReturnsAsync(true);

            _mockSqsClient.Setup(sqs => sqs.ReceiveMessageAsync(It.IsAny<ReceiveMessageRequest>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync(new ReceiveMessageResponse
                          {
                              Messages = new List<Message>
                              {
                              new Message { Body = messageBody, ReceiptHandle = "receipt-handle" }
                              }
                          });

            _mockSqsClient.Setup(sqs => sqs.DeleteMessageAsync(queueUrl, "receipt-handle", default))
                          .ReturnsAsync(new DeleteMessageResponse());

            var service = new FIFOSqsMessageListenerService(
                _mockSqsClient.Object,
                _mockLogger.Object,
                _mockScopeFactory.Object
            );

            var cancellationToken = new CancellationTokenSource().Token;


            // Act
            await service.StartAsync(cancellationToken);

            // Assert
            _mockQueueService.Verify(q => q.GetAwsConsumerCohortQueueUrl(), Times.Once);
            _mockSqsClient.Verify(s => s.ReceiveMessageAsync(It.IsAny<ReceiveMessageRequest>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
            _mockSqsClient.Verify(s => s.DeleteMessageAsync("consumer-cohort-test-queue-url", "receipt-handle", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async TaskA ExecuteAsync_ShouldHandleEventProcessingException()
        {
            var queueUrl = "consumer-cohort-test-queue-url";
            var messageBody = JsonSerializer.Serialize(new PostEventRequestModel
            {
                EventType = "TestEvent",
                EventSubtype = "testsubtype",
                EventCode = "123",
                ConsumerCode = "ABC",
                EventData = "\"{\\n    \\\"healthEvent\\\": \\\"WALKING\\\"\\n  }\""
            });

            _mockQueueService.Setup(q => q.GetAwsConsumerCohortQueueUrl()).ReturnsAsync(queueUrl);
            _mockProcessorFactory.Setup(f => f.GetEventDtoProcessor("TestEvent")).Returns(_mockProcessor.Object);
            _mockProcessor.Setup(p => p.ProcessEvent(It.IsAny<EventDto<Dictionary<string, object>>>()))
                .ThrowsAsync(new InvalidDataException());

            _mockSqsClient.Setup(sqs => sqs.ReceiveMessageAsync(It.IsAny<ReceiveMessageRequest>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync(new ReceiveMessageResponse
                          {
                              Messages = new List<Message>
                              {
                              new Message { Body = messageBody, ReceiptHandle = "receipt-handle" }
                              }
                          });

            _mockSqsClient.Setup(sqs => sqs.DeleteMessageAsync(queueUrl, "receipt-handle", default))
                          .ReturnsAsync(new DeleteMessageResponse());

            var service = new FIFOSqsMessageListenerService(
                _mockSqsClient.Object,
                _mockLogger.Object,
                _mockScopeFactory.Object
            );

            var cancellationToken = new CancellationTokenSource().Token;

            // Act
            await service.StartAsync(cancellationToken);
            await TaskA.Delay(100); // Allow the service to process
            await service.StopAsync(cancellationToken);

            // Assert

            // Assert
            _mockQueueService.Verify(x => x.PushEventToConsumerCohortEventDeadLetterQueue(It.IsAny<string>()), Times.Once);
        }
    }
}
