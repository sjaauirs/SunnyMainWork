using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using SunnyRewards.Helios.Admin.Core.Domain.Models;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services;
using SunnyRewards.Helios.Bff.Infrastructure.Services.Interfaces;
using System.Text.Json;
using Xunit;
using TaskA = System.Threading.Tasks.Task;

namespace SunnyRewards.Helios.Admin.UnitTest.Services
{
    public class SqsMessageListenerServiceTests
    {
        private readonly Mock<IAmazonSQS> _mockSqsClient;
        private readonly Mock<ILogger<BaseSqsMessageListenerService>> _mockLogger;
        private readonly Mock<IServiceScopeFactory> _mockScopeFactory;
        private readonly Mock<IServiceScope> _mockScope;
        private readonly Mock<IAwsQueueService> _mockQueueService;
        private readonly Mock<IEventProcessorFactory> _mockProcessorFactory;
        private readonly Mock<IEventProcessor> _mockProcessor;

        public SqsMessageListenerServiceTests()
        {
            _mockSqsClient = new Mock<IAmazonSQS>();
            _mockLogger = new Mock<ILogger<BaseSqsMessageListenerService>>();
            _mockScopeFactory = new Mock<IServiceScopeFactory>();
            _mockScope = new Mock<IServiceScope>();
            _mockQueueService = new Mock<IAwsQueueService>();
            _mockProcessorFactory = new Mock<IEventProcessorFactory>();
            _mockProcessor = new Mock<IEventProcessor>();

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
            var queueUrl = "test-queue-url";
            var messageBody = JsonSerializer.Serialize(new PostEventRequestModel
            {
                EventType = "TestEvent",
                EventSubtype = "testsubtype",
                EventCode = "123",
                ConsumerCode = "ABC",
                EventData = "\"{\\n    \\\"healthEvent\\\": \\\"WALKING\\\"\\n  }\""
            });

            _mockQueueService.Setup(q => q.GetAwsConsumerEventQueueUrl()).ReturnsAsync(queueUrl);
            _mockProcessorFactory.Setup(f => f.GetEventProcessor("TestEvent")).Returns(_mockProcessor.Object);
            _mockProcessor.Setup(p => p.ProcessEvent(It.IsAny<PostEventRequestModel>())).ReturnsAsync(true);

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

            var service = new SqsMessageListenerService(
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

            _mockProcessor.Verify(p => p.ProcessEvent(It.IsAny<PostEventRequestModel>()), Times.Once);
            _mockSqsClient.Verify(sqs => sqs.DeleteMessageAsync(queueUrl, "receipt-handle", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async TaskA ExecuteAsync_SendsMessageToDeadLetterQueue()
        {
            // Arrange
            var queueUrl = "test-queue-url";
            var messageBody = JsonSerializer.Serialize(new PostEventRequestModel
            {
                EventType = "UnknownEvent",
                EventCode = "456",
                ConsumerCode = "XYZ",
                EventSubtype = "testsubtype",
                EventData = "\"{\\n    \\\"healthEvent\\\": \\\"WALKING\\\"\\n  }\""
            });

            _mockQueueService.Setup(q => q.GetAwsConsumerEventQueueUrl()).ReturnsAsync(queueUrl);
            _mockQueueService.Setup(q => q.PushEventToConsumerEventDeadLetterQueue(It.IsAny<string>()))
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

            var service = new SqsMessageListenerService(
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
            _mockQueueService.Verify(q => q.PushEventToConsumerEventDeadLetterQueue(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async TaskA ExecuteAsync_HandlesExceptionGracefully()
        {
            // Arrange
            var queueUrl = "test-queue-url";
            _mockQueueService.Setup(q => q.GetAwsConsumerEventQueueUrl()).ReturnsAsync(queueUrl);

            _mockSqsClient.Setup(sqs => sqs.ReceiveMessageAsync(It.IsAny<ReceiveMessageRequest>(), It.IsAny<CancellationToken>()))
                          .ThrowsAsync(new Exception("SQS error"));

            var service = new SqsMessageListenerService(
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
            var queueUrl = "test-queue-url";
            var messageBody = JsonSerializer.Serialize(new PostEventRequestModel
            {
                EventType = "TestEvent",
                EventCode = "123",
                ConsumerCode = "ABC",
                EventData = "\"{\\n    \\\"healthEvent\\\": \\\"WALKING\\\"\\n  }\""
            });
            // Arrange
            var invalidMessage = new Message
            {
                Body = "invalid-json",
                ReceiptHandle = "receipt-handle"
            };

            _mockQueueService.Setup(q => q.GetAwsConsumerEventQueueUrl()).ReturnsAsync(queueUrl);
            _mockProcessorFactory.Setup(f => f.GetEventProcessor("TestEvent")).Returns(_mockProcessor.Object);
            _mockProcessor.Setup(p => p.ProcessEvent(It.IsAny<PostEventRequestModel>())).ReturnsAsync(true);

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

            var service = new SqsMessageListenerService(
                _mockSqsClient.Object,
                _mockLogger.Object,
                _mockScopeFactory.Object
            );

            var cancellationToken = new CancellationTokenSource().Token;


            // Act
            await service.StartAsync(cancellationToken);

            // Assert
            _mockQueueService.Verify(q => q.GetAwsConsumerEventQueueUrl(), Times.Once);
            _mockSqsClient.Verify(s => s.ReceiveMessageAsync(It.IsAny<ReceiveMessageRequest>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
            _mockSqsClient.Verify(s => s.DeleteMessageAsync("test-queue-url", "receipt-handle", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async TaskA ExecuteAsync_ShouldHandleEventProcessingException()
        {
            var queueUrl = "test-queue-url";
            var messageBody = JsonSerializer.Serialize(new PostEventRequestModel
            {
                EventType = "TestEvent",
                EventSubtype = "testsubtype",
                EventCode = "123",
                ConsumerCode = "ABC",
                EventData = "\"{\\n    \\\"healthEvent\\\": \\\"WALKING\\\"\\n  }\""
            });

            _mockQueueService.Setup(q => q.GetAwsConsumerEventQueueUrl()).ReturnsAsync(queueUrl);
            _mockProcessorFactory.Setup(f => f.GetEventProcessor("TestEvent")).Returns(_mockProcessor.Object);
            _mockProcessor.Setup(p => p.ProcessEvent(It.IsAny<PostEventRequestModel>()))
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

            var service = new SqsMessageListenerService(
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
            _mockQueueService.Verify(x => x.PushEventToConsumerEventDeadLetterQueue(It.IsAny<string>()), Times.Once);
        }
    }
}
