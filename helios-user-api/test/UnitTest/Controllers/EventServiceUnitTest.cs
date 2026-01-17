using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.User.Infrastructure.Services;
using System.Runtime.Serialization;
using Xunit;

namespace SunnyRewards.Helios.User.UnitTest.Services
{
    public class EventServiceTests
    {
        private readonly Mock<IAdminClient> _adminClientMock;
        private readonly Mock<ILogger<EventService>> _loggerMock;
        private readonly EventService _eventService;
        private readonly Mock<IHeliosEventPublisher<CohortEventDto>> _heliosEventPublisher;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessor;


        public EventServiceTests()
        {
            _adminClientMock = new Mock<IAdminClient>();
            _loggerMock = new Mock<ILogger<EventService>>();
            _heliosEventPublisher = new Mock<IHeliosEventPublisher<CohortEventDto>>();
            _httpContextAccessor = new Mock<IHttpContextAccessor>();
            _eventService = new EventService(_loggerMock.Object, _adminClientMock.Object,_heliosEventPublisher.Object,_httpContextAccessor.Object);
        }

        [Fact]
        public async Task PostEvent_ReturnsResponse_WhenSuccessful()
        {
            // Arrange
            var request = new PostEventRequestDto
            {
                ConsumerCode = "C123",
                TenantCode = "T1",
                EventType = "CONSUMER_HISTORY",
                EventSubtype = "UPDATE",
                EventSource = "test",
                EventData = new List<ConsumerDto>()
            };

            var expectedResponse = new BaseResponseDto() { ErrorCode = null, ErrorDescription = "" , ErrorDescriptionType = null , ErrorMessage = ""};

            _adminClientMock
                .Setup(client => client.Post<BaseResponseDto>("post-event", It.IsAny<PostEventRequestDto>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var response = await _eventService.PostEvent(request);

            // Assert
            Assert.Null(response.ErrorCode);
            _adminClientMock.Verify(x => x.Post<BaseResponseDto>("post-event", It.IsAny<PostEventRequestDto>()), Times.Once);
        }

        [Fact]
        public async Task PostEvent_ThrowsException_WhenClientFails()
        {
            // Arrange
            var request = new PostEventRequestDto
            {
                ConsumerCode = "C123",
                TenantCode = "T1",
                EventType = "CONSUMER_HISTORY",
                EventSubtype = "UPDATE",
                EventSource = "test",
                EventData = new List<ConsumerDto>()
            };

            _adminClientMock
                .Setup(client => client.Post<BaseResponseDto>("post-event", request))
                .ThrowsAsync(new Exception("Admin failure"));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _eventService.PostEvent(request));
            Assert.Equal("Admin failure", ex.Message);
        }

        [Fact]
        public async Task CreateConsumerHistoryEvent_CallsPostEvent_WithnullPayload()
        {

            PostEventRequestDto capturedRequest = null;

            _adminClientMock
                .Setup(client => client.Post<BaseResponseDto>(
                    "post-event", It.IsAny<PostEventRequestDto>()))
                .Callback<string, object>((_, req) => capturedRequest = (PostEventRequestDto)req)
                .ReturnsAsync(new BaseResponseDto());

            // Act
            var result = await _eventService.CreateConsumerHistoryEvent(null);

            // Assert
   
            Assert.Null(result.ErrorCode);
        }

        [Fact]
        public async Task CreateConsumerHistoryEvent_CallsPostEvent_WithCorrectPayload()
        {
            // Arrange
            var consumers = new List<ConsumerDto>
            {
                new ConsumerDto
                {
                    ConsumerCode = "C001",
                    TenantCode = "T001"
                }
            };

            PostEventRequestDto capturedRequest = null;

            _adminClientMock
                .Setup(client => client.Post<BaseResponseDto>(
                    "post-event", It.IsAny<PostEventRequestDto>()))
                .Callback<string, object>((_, req) => capturedRequest = (PostEventRequestDto)req)
                .ReturnsAsync(new BaseResponseDto());

            // Act
            var result = await _eventService.CreateConsumerHistoryEvent(consumers);

            // Assert
            Assert.NotNull(capturedRequest);
            Assert.Equal("C001", capturedRequest.ConsumerCode);
            Assert.Equal("T001", capturedRequest.TenantCode);
            Assert.Equal("CONSUMER_HISTORY", capturedRequest.EventType);
            Assert.Equal("UPDATE", capturedRequest.EventSubtype);
            Assert.Equal("test", capturedRequest.EventSource);
            Assert.Null(result.ErrorCode);
            Assert.Equal(consumers, capturedRequest.EventData);
        }
        [Fact]
        public async Task PublishCohortEventToSNSTopic_ShouldNotPublish_WhenInputInvalid()
        {
            // Arrange: invalid tenantCode
            string tenantCode = "";
            string consumerCode = "cmr-00a4b30b3df844f1a04dd21b5f3b9e8e";

            // Act
            await _eventService.PublishCohortEventToSNSTopic(consumerCode, tenantCode);

            // Assert
            _heliosEventPublisher.Verify(p => p.PublishMessage(It.IsAny<EventHeaderDto>(), It.IsAny<CohortEventDto>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async Task PublishCohortEventToSNSTopic_ShouldPublish_WhenInputValid()
        {
            // Arrange
            string tenantCode = "ten-ecada21e57154928a2bb959e8365b8b4";
            string consumerCode = "cmr-00a4b30b3df844f1a04dd21b5f3b9e8e";
            _heliosEventPublisher.Setup(p => p.PublishMessage(It.IsAny<EventHeaderDto>(), It.IsAny<CohortEventDto>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(new PublishResultDto { Published = true });

            var context = new DefaultHttpContext();
            context.Request.Headers["X-HELIOS-REQUEST-ID"] = "req-123";
            _httpContextAccessor.Setup(a => a.HttpContext).Returns(context);

            // Act
            await _eventService.PublishCohortEventToSNSTopic(tenantCode, consumerCode);

            // Assert
            _heliosEventPublisher.Verify(p => p.PublishMessage(It.IsAny<EventHeaderDto>(), It.IsAny<CohortEventDto>(),It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Once);
        }

        [Fact]
        public async Task PublishCohortEventToSNSTopic_ShouldRetryAndFail_WhenNotPublished()
        {
            // Arrange: always fail
            string tenantCode = "ten-ecada21e57154928a2bb959e8365b8b4";
            string consumerCode = "cmr-00a4b30b3df844f1a04dd21b5f3b9e8e";
            _heliosEventPublisher.Setup(p => p.PublishMessage(It.IsAny<EventHeaderDto>(), It.IsAny<CohortEventDto>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(new PublishResultDto { Published = false });

            _httpContextAccessor.Setup(a => a.HttpContext).Returns(new DefaultHttpContext());

            // Act
            await _eventService.PublishCohortEventToSNSTopic(tenantCode, consumerCode);

            // Assert
            _heliosEventPublisher.Verify(p => p.PublishMessage(It.IsAny<EventHeaderDto>(), It.IsAny<CohortEventDto>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.AtLeast(2));
        }

        [Fact]
        public async Task PublishCohortEventToSNSTopic_ShouldHandleExceptionDuringPublish()
        {
            // Arrange: throw exception
            string tenantCode = "ten-ecada21e57154928a2bb959e8365b8b4";
            string consumerCode = "cmr-00a4b30b3df844f1a04dd21b5f3b9e8e";
            _heliosEventPublisher.Setup(p => p.PublishMessage(It.IsAny<EventHeaderDto>(), It.IsAny<CohortEventDto>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ThrowsAsync(new InvalidOperationException("Test error"));

            _httpContextAccessor.Setup(a => a.HttpContext).Returns(new DefaultHttpContext());

            // Act
            await _eventService.PublishCohortEventToSNSTopic(tenantCode, consumerCode);

            // Assert
            _heliosEventPublisher.Verify(p => p.PublishMessage(It.IsAny<EventHeaderDto>(), It.IsAny<CohortEventDto>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.AtLeast(1));
        }
        [Fact]
        public void GetObjectData_ShouldAddValuesToSerializationInfo()
        {
            // Arrange
            var dto = new CohortEventDto
            {
                EventId = "ecada21e57154928a2bb959e8365b8b4",
                TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4",
                ConsumerCode = "cmr-00a4b30b3df844f1a04dd21b5f3b9e8e",
                TriggeredBy = "Import"
            };
#pragma warning disable SYSLIB0051
            var info = new SerializationInfo(typeof(CohortEventDto), new FormatterConverter());
#pragma warning restore SYSLIB0051
            var context = new StreamingContext();

            // Act
            dto.GetObjectData(info, context);

            // Assert
            Assert.Equal("ecada21e57154928a2bb959e8365b8b4", info.GetString("EventId"));
            Assert.Equal("ten-ecada21e57154928a2bb959e8365b8b4", info.GetString("TenantCode"));
            Assert.Equal("cmr-00a4b30b3df844f1a04dd21b5f3b9e8e", info.GetString("ConsumerCode"));
            Assert.Equal("Import", info.GetString("TriggeredBy"));
        }
    }   
}
