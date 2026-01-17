using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using SunnyRewards.Helios.ETL.Common.CustomException;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Services;
using SunnyRewards.Helios.NotificationService.Core.Domain.Dtos;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;

namespace SunnyRewards.Helios.ETL.UnitTests.Services
{
    public class NotificationRulesServiceTests
    {
        private readonly Mock<ILogger<NotificationRulesService>> _loggerMock;
        private readonly Mock<INotificationClient> _notificationClientMock;
        private readonly Mock<IAdminClient> _adminClientMock;
        private readonly Mock<INotificationRuleRepository> _notificationRulesRepoMock;
        private readonly Mock<IConsumerNotificationRepo> _consumerNotificationRepoMock;
        private readonly Mock<IHeliosEventPublisher<Dictionary<string, object>>> _eventPublisherMock;
        private readonly Mock<IMemoryCache> _memoryCacheMock;
        private readonly NotificationRulesService _service;

        public NotificationRulesServiceTests()
        {
            _loggerMock = new Mock<ILogger<NotificationRulesService>>();
            _notificationClientMock = new Mock<INotificationClient>();
            _adminClientMock = new Mock<IAdminClient>();
            _notificationRulesRepoMock = new Mock<INotificationRuleRepository>();
            _consumerNotificationRepoMock = new Mock<IConsumerNotificationRepo>();
            _eventPublisherMock = new Mock<IHeliosEventPublisher<Dictionary<string, object>>>();
            _memoryCacheMock = new Mock<IMemoryCache>();

            _service = new NotificationRulesService(
                _loggerMock.Object,
                _notificationClientMock.Object,
                _adminClientMock.Object,
                _notificationRulesRepoMock.Object,
                _eventPublisherMock.Object,
                _memoryCacheMock.Object,
                _consumerNotificationRepoMock.Object
            );
        }

        [Fact]
        public async System.Threading.Tasks.Task ProcessNotificationRulesAsync_should_throw_error_when_tenant_code_is_empty()
        {
            // Arrange
            var etlExecutionContext = new EtlExecutionContext { TenantCode = string.Empty };

            _adminClientMock.Setup(x => x.Get<TenantResponseDto>(It.IsAny<string>(), It.IsAny<Dictionary<string, long>>(), null))
                .ReturnsAsync(new TenantResponseDto()
                {
                    Tenant = new TenantDto()
                    {
                        TenantCode = "testTenant"
                    }
                });


            // Act
            await Assert.ThrowsAsync<ETLException>(() => _service.ProcessNotificationRulesAsync(etlExecutionContext));

        }

        [Fact]
        public async System.Threading.Tasks.Task ProcessNotificationRulesAsync_should_throw_error_when_tenant_api_return_error()
        {
            // Arrange
            var etlExecutionContext = new EtlExecutionContext { TenantCode = "testTenant" };

            _adminClientMock.Setup(x => x.Get<TenantResponseDto>(It.IsAny<string>(), It.IsAny<Dictionary<string, long>>(), null))
                .ReturnsAsync(new TenantResponseDto()
                {
                    ErrorCode = StatusCodes.Status400BadRequest
                });

            //_adminClientMock.Setup(x => x.Get<TenantResponseDto>(It.IsAny<string>(), It.IsAny<Dictionary<string, long>>()))
            //    .ReturnsAsync()

            // Actan
            //await _service.ProcessNotificationRulesAsync(etlExecutionContext);
            await Assert.ThrowsAsync<ETLException>(() => _service.ProcessNotificationRulesAsync(etlExecutionContext));

        }

        [Fact]
        public async System.Threading.Tasks.Task ProcessNotificationRulesAsync_when_tenant_api_return_valid_tenant()
        {
            // Arrange
            var etlExecutionContext = new EtlExecutionContext { TenantCode = "testTenant" };

            _adminClientMock.Setup(x => x.Get<TenantResponseDto>(It.IsAny<string>(), It.IsAny<Dictionary<string, long>>(), null))
                .ReturnsAsync(new TenantResponseDto()
                {
                    Tenant = new TenantDto()
                    {
                        TenantId = 2,
                        TenantCode = "12345"
                    }
                });

            // Act
            await _service.ProcessNotificationRulesAsync(etlExecutionContext);

            // Assert
            _adminClientMock.Verify(x => x.Get<TenantResponseDto>(It.IsAny<string>(), It.IsAny<IDictionary<string, long>>(), null), Times.Once);

        }

        [Fact]
        public async System.Threading.Tasks.Task ProcessNotificationRulesAsync_when_notification_rule_has_context_config_as_null()
        {
            // Arrange
            var etlExecutionContext = new EtlExecutionContext { TenantCode = "testTenant" };

            _adminClientMock.Setup(x => x.Get<TenantResponseDto>(It.IsAny<string>(), It.IsAny<Dictionary<string, long>>(), null))
                .ReturnsAsync(new TenantResponseDto()
                {
                    Tenant = new TenantDto()
                    {
                        TenantId = 2,
                        TenantCode = "12345"
                    }
                });
            _notificationClientMock.Setup(x => x.Post<GetAllNotificationRuleResponseDto>(It.IsAny<string>(), It.IsAny<GetAllNotificationRulesRequestDto>(), null))
                .ReturnsAsync(new GetAllNotificationRuleResponseDto()
                {
                    NotificationRuleList = new List<NotificationRuleDto>()
                    {
                        new NotificationRuleDto()
                        {

                        }
                    }
                });

            // Act
            await _service.ProcessNotificationRulesAsync(etlExecutionContext);

            // Assert
            _adminClientMock.Verify(x => x.Get<TenantResponseDto>(It.IsAny<string>(), It.IsAny<IDictionary<string, long>>(), null), Times.Once);
        }

        [Fact]
        public async System.Threading.Tasks.Task ProcessNotificationRulesAsync_when_notification_rule_has_context_config_as_data()
        {
            // Arrange
            var etlExecutionContext = new EtlExecutionContext { TenantCode = "testTenant" };

            _adminClientMock.Setup(x => x.Get<TenantResponseDto>(It.IsAny<string>(), It.IsAny<Dictionary<string, long>>(), null))
                .ReturnsAsync(new TenantResponseDto()
                {
                    Tenant = new TenantDto()
                    {
                        TenantId = 2,
                        TenantCode = "12345"
                    }
                });
            _notificationClientMock.Setup(x => x.Post<GetAllNotificationRuleResponseDto>(It.IsAny<string>(), It.IsAny<GetAllNotificationRulesRequestDto>(), null))
                .ReturnsAsync(new GetAllNotificationRuleResponseDto()
                {
                    NotificationRuleList = new List<NotificationRuleDto>()
                    {
                        new NotificationRuleDto()
                        {
                            NotificationRuleCode = "nrc-df1570ea33bf43d291c8c082a864ded6",
                            NotificationRuleId = 1,
                            TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4",
                            NotificationEventTypeId = 4,
                            ContextConfig = "{\r\n  \"contextId\": \"123\",\r\n  \"contextType\": \"TASK\",\r\n  \"contextAttributes\": {\r\n    \"tenant\": \"Testing contextAttributes\"\r\n  }\r\n}",
                            FrequencyConfig = "{\r\n  \"day\": 0,\r\n  \"interval\": \"DAILY\"\r\n}"
                        }
                    }
                });

            // Act
            await _service.ProcessNotificationRulesAsync(etlExecutionContext);

            // Assert
            _adminClientMock.Verify(x => x.Get<TenantResponseDto>(It.IsAny<string>(), It.IsAny<IDictionary<string, long>>(), null), Times.Once);
        }

        [Fact]
        public async System.Threading.Tasks.Task ProcessNotificationRulesAsync_when_notification_rule_has_query()
        {
            // Arrange
            var etlExecutionContext = new EtlExecutionContext { TenantCode = "testTenant" };

            _adminClientMock.Setup(x => x.Get<TenantResponseDto>(It.IsAny<string>(), It.IsAny<Dictionary<string, long>>(), null))
                .ReturnsAsync(new TenantResponseDto()
                {
                    Tenant = new TenantDto()
                    {
                        TenantId = 2,
                        TenantCode = "12345"
                    }
                });
            _notificationClientMock.Setup(x => x.Post<GetAllNotificationRuleResponseDto>(It.IsAny<string>(), It.IsAny<GetAllNotificationRulesRequestDto>(), null))
                .ReturnsAsync(new GetAllNotificationRuleResponseDto()
                {
                    NotificationRuleList = new List<NotificationRuleDto>()
                    {
                        new NotificationRuleDto()
                        {
                            NotificationRuleCode = "nrc-df1570ea33bf43d291c8c082a864ded6",
                            NotificationRuleId = 1,
                            TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4",
                            NotificationEventTypeId = 4,
                            ContextConfig = "{\r\n  \"contextId\": \"123\",\r\n  \"contextType\": \"TASK\",\r\n  \"contextAttributes\": {\r\n    \"tenant\": \"Testing contextAttributes\"\r\n  }\r\n}",
                            FrequencyConfig = "{\r\n  \"day\": 0,\r\n  \"interval\": \"DAILY\"\r\n}",
                            EventRetrievalQuery = "select * from notification.notification_rule"
                        }
                    }
                });

            // Act
            await _service.ProcessNotificationRulesAsync(etlExecutionContext);

            // Assert
            _adminClientMock.Verify(x => x.Get<TenantResponseDto>(It.IsAny<string>(), It.IsAny<IDictionary<string, long>>(), null), Times.Once);
        }

        [Fact]
        public async System.Threading.Tasks.Task ProcessNotificationRulesAsync_when_notification_rule_has_query_return_results()
        {
            // Arrange
            var etlExecutionContext = new EtlExecutionContext { TenantCode = "testTenant" };

            _adminClientMock.Setup(x => x.Get<TenantResponseDto>(It.IsAny<string>(), It.IsAny<Dictionary<string, long>>(), null))
                .ReturnsAsync(new TenantResponseDto()
                {
                    Tenant = new TenantDto()
                    {
                        TenantId = 2,
                        TenantCode = "12345"
                    }
                });
            _notificationClientMock.Setup(x => x.Post<GetAllNotificationRuleResponseDto>(It.IsAny<string>(), It.IsAny<GetAllNotificationRulesRequestDto>(), null))
                .ReturnsAsync(new GetAllNotificationRuleResponseDto()
                {
                    NotificationRuleList = new List<NotificationRuleDto>()
                    {
                        new NotificationRuleDto()
                        {
                            NotificationRuleCode = "nrc-df1570ea33bf43d291c8c082a864ded6",
                            NotificationRuleId = 1,
                            TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4",
                            NotificationEventTypeId = 4,
                            ContextConfig = "{\r\n  \"contextId\": \"123\",\r\n  \"contextType\": \"TASK\",\r\n  \"contextAttributes\": {\r\n    \"tenant\": \"Testing contextAttributes\"\r\n  }\r\n}",
                            FrequencyConfig = "{\r\n  \"day\": 0,\r\n  \"interval\": \"DAILY\"\r\n}",
                            EventRetrievalQuery = "select * from notification.notification_rule"
                        }
                    }
                });
            _notificationRulesRepoMock.Setup(x => x.ExecuteQuery(It.IsAny<string>())).Returns(
                new List<Dictionary<string, object>>()
                {
                    new Dictionary<string, object>()
                    {
                        { "consumerCode", "1234"}
                    }
                });
            // Act
            await _service.ProcessNotificationRulesAsync(etlExecutionContext);

            // Assert
            _adminClientMock.Verify(x => x.Get<TenantResponseDto>(It.IsAny<string>(), It.IsAny<IDictionary<string, long>>(), null), Times.Once);
        }

        [Fact]
        public async System.Threading.Tasks.Task ProcessNotificationRulesAsync_when_notification_event_type_has_data()
        {
            // Arrange
            var etlExecutionContext = new EtlExecutionContext { TenantCode = "testTenant" };

            _adminClientMock.Setup(x => x.Get<TenantResponseDto>(It.IsAny<string>(), It.IsAny<Dictionary<string, long>>(), null))
                .ReturnsAsync(new TenantResponseDto()
                {
                    Tenant = new TenantDto()
                    {
                        TenantId = 2,
                        TenantCode = "12345"
                    }
                });
            _notificationClientMock.Setup(x => x.Post<GetAllNotificationRuleResponseDto>(It.IsAny<string>(), It.IsAny<GetAllNotificationRulesRequestDto>(), null))
                .ReturnsAsync(new GetAllNotificationRuleResponseDto()
                {
                    NotificationRuleList = new List<NotificationRuleDto>()
                    {
                        new NotificationRuleDto()
                        {
                            NotificationRuleCode = "nrc-df1570ea33bf43d291c8c082a864ded6",
                            NotificationRuleId = 1,
                            TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4",
                            NotificationEventTypeId = 4,
                            ContextConfig = "{\r\n  \"contextId\": \"123\",\r\n  \"contextType\": \"TASK\",\r\n  \"contextAttributes\": {\r\n    \"tenant\": \"Testing contextAttributes\"\r\n  }\r\n}",
                            FrequencyConfig = "{\r\n  \"day\": 0,\r\n  \"interval\": \"DAILY\"\r\n}",
                            EventRetrievalQuery = "select * from notification.notification_rule"
                        }
                    }
                });
            _notificationRulesRepoMock.Setup(x => x.ExecuteQuery(It.IsAny<string>())).Returns(
                new List<Dictionary<string, object>>()
                {
                    new Dictionary<string, object>()
                    {
                        { "consumerCode", "1234"}
                    }
                });
            _notificationClientMock.Setup(x => x.Get<NotificationEventTypeResponseDto>(It.IsAny<string>(), It.IsAny<Dictionary<string, long>>(), null)).
                ReturnsAsync(new NotificationEventTypeResponseDto()
                {
                    NotificationEventType = new NotificationEventTypeDto()
                    {
                        NotificationEventName = "TASK"
                    }
                });

            // Act
            await _service.ProcessNotificationRulesAsync(etlExecutionContext);

            // Assert
            _adminClientMock.Verify(x => x.Get<TenantResponseDto>(It.IsAny<string>(), It.IsAny<IDictionary<string, long>>(), null), Times.Once);
        }
    }
}
