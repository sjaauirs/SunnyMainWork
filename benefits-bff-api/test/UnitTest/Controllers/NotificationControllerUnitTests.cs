using Microsoft.Extensions.Logging;
using Moq;
using Sunny.Benefits.Bff.Api.Controllers;
using Sunny.Benefits.Bff.Infrastructure.HttpClients.Interfaces;
using Sunny.Benefits.Bff.Infrastructure.Repositories.Interfaces;
using Sunny.Benefits.Bff.Infrastructure.Services.Interfaces;
using Sunny.Benefits.Bff.Infrastructure.Services;
using Sunny.Benefits.Bff.UnitTest.HttpClients;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sunny.Benefits.Bff.UnitTest.Fixtures.MockDtos;
using Xunit;
using Sunny.Benefits.Bff.Core.Domain.Dtos;
using System.Web;
using SunnyRewards.Helios.NotificationService.Core.Domain.Dtos;

namespace Sunny.Benefits.Bff.UnitTest.Controllers
{
    public class NotificationControllerUnitTests
    {
        private readonly Mock<ILogger<NotificationController>> _notificationControllerLogger;
        private readonly Mock<ILogger<NotificationService>> _notificationServiceLogger;
        private readonly Mock<INotificationClient> _notificationClient;
        private readonly Mock<INotificationService> _notificationServiceMock;
        private readonly INotificationService _notificationService;
        private readonly NotificationController _notificationController;

        public NotificationControllerUnitTests()
        {
            _notificationControllerLogger = new Mock<ILogger<NotificationController>>();
            _notificationServiceLogger = new Mock<ILogger<NotificationService>>();
            _notificationClient = new NotificationClientMock();
            _notificationServiceMock = new Mock<INotificationService>();
            _notificationService = new NotificationService(_notificationServiceLogger.Object, _notificationClient.Object);
            _notificationController = new NotificationController(_notificationControllerLogger.Object, _notificationServiceMock.Object);
        }

        [Fact]
        public async Task Should_GetNotificationCategoryByTenant()
        {
            // Arrange
            string tenantCode = "tnt-123";

            var expectedResponse = new GetTenantNotificationCategoryResponseMockDto();
            _notificationServiceMock.Setup(x => x.GetNotificationCategoryByTenant(tenantCode))
                .ReturnsAsync(expectedResponse);

            // Act
            var response = await _notificationController.GetNotificationCategoryByTenant(tenantCode);
            var result = response.Result as OkObjectResult;

            // Assert
            Assert.NotNull(result?.Value);
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }

        [Fact]
        public async Task GetNotificationCategoryByTenant_Should_Return_NotFound()
        {
            // Arrange
            string tenantCode = "tnt-123";

            _notificationServiceMock.Setup(x => x.GetNotificationCategoryByTenant(tenantCode))
                .ReturnsAsync((GetTenantNotificationCategoryResponseDto)null!); // Simulate not found

            // Act
            var response = await _notificationController.GetNotificationCategoryByTenant(tenantCode);
            var result = response.Result as NotFoundResult;

            // Assert
            Assert.Equal(StatusCodes.Status404NotFound, result?.StatusCode);
        }

        [Fact]
        public async Task GetNotificationCategoryByTenant_Should_Return_Exception_Catch_In_Controller()
        {
            // Arrange
            string tenantCode = "tnt-123";

            _notificationServiceMock.Setup(x => x.GetNotificationCategoryByTenant(tenantCode))
                .ThrowsAsync(new Exception("Simulated exception"));

            // Act
            var result = await _notificationController.GetNotificationCategoryByTenant(tenantCode);
            var errorResult = Assert.IsType<ObjectResult>(result.Result);

            // Assert
            Assert.Equal(StatusCodes.Status500InternalServerError, errorResult.StatusCode);
            var responseDto = Assert.IsType<GetTenantNotificationCategoryResponseDto>(errorResult.Value);
            Assert.Equal(StatusCodes.Status500InternalServerError, responseDto.ErrorCode);
        }

        [Fact]
        public async Task GetNotificationCategoryByTenant_Should_Throw_In_Service_When_ClientFails()
        {
            // Arrange
            string tenantCode = "tnt-123";
            var parameters = new Dictionary<string, string> { { "tenantCode", HttpUtility.UrlEncode(tenantCode) } };

            _notificationClient.Setup(x => x.GetId<GetTenantNotificationCategoryResponseDto>(
                It.IsAny<string>(), parameters))
                .ThrowsAsync(new Exception("Simulated failure in HTTP call"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _notificationService.GetNotificationCategoryByTenant(tenantCode));
        }

        [Fact]
        public async Task Should_Map_NotificationCategoryNames_To_TenantList()
        {
            // Arrange
            var tenantCode = "tnt-123";
            var parameters = new Dictionary<string, string> { { "tenantCode", HttpUtility.UrlEncode(tenantCode) } };

            var tenantNotificationList = new GetTenantNotificationCategoryResponseDto
            {
                TenantNotificationCategoryList = new List<Core.Domain.Dtos.TenantNotificationCategoryDto>
                {
                    new Core.Domain.Dtos.TenantNotificationCategoryDto
                    {
                        NotificationCategoryId = 1,
                        NotificationCategoryName = null
                    },
                    new Core.Domain.Dtos.TenantNotificationCategoryDto
                    {
                        NotificationCategoryId = 2,
                        NotificationCategoryName = null
                    }
                }
            };

            var allCategories = new GetAllNotificationCategoriesResponseDto
            {
                NotificationCategoriesList = new List<NotificationCategoryDto>
                {
                    new NotificationCategoryDto { NotificationCategoryId = 1, CategoryName = "REWARDS" },
                    new NotificationCategoryDto { NotificationCategoryId = 2, CategoryName = "DEPOSITS" }
                }
            };

            _notificationClient.Setup(x => x.GetId<GetTenantNotificationCategoryResponseDto>(
                It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
                .ReturnsAsync(tenantNotificationList);

            _notificationClient.Setup(x => x.GetId<GetAllNotificationCategoriesResponseDto>(
                It.Is<string>(s => s.Contains("notification-category/all-categories")),
                It.IsAny<Dictionary<string, string>>()))
                .ReturnsAsync(allCategories);

            // Act
            var result = await _notificationService.GetNotificationCategoryByTenant(tenantCode);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("REWARDS", result.TenantNotificationCategoryList[0].NotificationCategoryName);
            Assert.Equal("DEPOSITS", result.TenantNotificationCategoryList[1].NotificationCategoryName);
        }

        [Fact]
        public async Task Should_Leave_CategoryName_Null_If_NotFound()
        {
            // Arrange
            var tenantCode = "tnt-123";

            var tenantResponse = new GetTenantNotificationCategoryResponseDto
            {
                TenantNotificationCategoryList = new List<Core.Domain.Dtos.TenantNotificationCategoryDto>
                {
                    new Core.Domain.Dtos.TenantNotificationCategoryDto { NotificationCategoryId = 99 }
                }
            };

            var categories = new GetAllNotificationCategoriesResponseDto
            {
                NotificationCategoriesList = new List<NotificationCategoryDto>
                {
                    new NotificationCategoryDto { NotificationCategoryId = 1, CategoryName = "REWARDS" }
                }
            };

            _notificationClient.Setup(x => x.GetId<GetTenantNotificationCategoryResponseDto>(
                It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
                .ReturnsAsync(tenantResponse);

            _notificationClient.Setup(x => x.GetId<GetAllNotificationCategoriesResponseDto>(
                It.Is<string>(s => s.Contains("notification-category/all-categories")),
                It.IsAny<Dictionary<string, string>>()))
                .ReturnsAsync(categories);

            // Act
            var result = await _notificationService.GetNotificationCategoryByTenant(tenantCode);

            // Assert
            Assert.NotNull(result.TenantNotificationCategoryList);
            Assert.Null(result.TenantNotificationCategoryList[0].NotificationCategoryName);
        }

        [Fact]
        public async Task Should_GetConsumerNotificationPref()
        {
            // Arrange
            string tenantCode = "tnt-123";
            string consumerCode = "cmr-456";

            var expectedResponse = new GetConsumerNotificationPrefResponseMockDto();
            _notificationServiceMock.Setup(x => x.GetConsumerNotificationPref(tenantCode, consumerCode))
                .ReturnsAsync(expectedResponse);

            // Act
            var response = await _notificationController.GetConsumerNotificationPref(tenantCode, consumerCode);
            var result = response.Result as OkObjectResult;

            // Assert
            Assert.NotNull(result?.Value);
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }


        [Fact]
        public async Task GetConsumerNotificationPref_Should_Return_NotFound()
        {
            // Arrange
            string tenantCode = "tnt-123";
            string consumerCode = "cmr-456";

            _notificationServiceMock.Setup(x => x.GetConsumerNotificationPref(tenantCode, consumerCode))
                .ReturnsAsync((ConsumerNotificationPrefResponseDto)null!); // Simulate not found

            // Act
            var response = await _notificationController.GetConsumerNotificationPref(tenantCode, consumerCode);
            var result = response.Result as NotFoundResult;

            // Assert
            Assert.Equal(StatusCodes.Status404NotFound, result?.StatusCode);
        }


        [Fact]
        public async Task GetConsumerNotificationPref_Should_Return_Exception_Catch_In_Controller()
        {
            // Arrange
            string tenantCode = "tnt-123";
            string consumerCode = "cmr-456";

            _notificationServiceMock.Setup(x => x.GetConsumerNotificationPref(tenantCode, consumerCode))
                .ThrowsAsync(new Exception("Simulated failure"));

            // Act
            var result = await _notificationController.GetConsumerNotificationPref(tenantCode, consumerCode);
            var errorResult = Assert.IsType<ObjectResult>(result.Result);

            // Assert
            Assert.Equal(StatusCodes.Status500InternalServerError, errorResult.StatusCode);

            var responseDto = Assert.IsType<ConsumerNotificationPrefResponseDto>(errorResult.Value);
            Assert.Equal(StatusCodes.Status500InternalServerError, responseDto.ErrorCode);
        }

        [Fact]
        public async Task GetConsumerNotificationPref_Should_Throw_In_Service_When_ClientFails()
        {
            // Arrange
            string tenantCode = "tnt-123";
            string consumerCode = "cmr-456";
            var endpoint = $"consumer-notication-pref?consumerCode={consumerCode}&tenantCode={tenantCode}";

            _notificationClient.Setup(x => x.GetId<ConsumerNotificationPrefResponseDto>(
                endpoint, It.IsAny<Dictionary<string, string>>()))
                .ThrowsAsync(new Exception("Simulated client error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _notificationService.GetConsumerNotificationPref(tenantCode, consumerCode));
        }

        [Fact]
        public async Task Should_CreateConsumerNotificationPref()
        {
            // Arrange
            var requestDto = new CreateConsumerNotificationPrefRequestDto
            {
                ConsumerNotificationPreferenceCode = "nct-001",
                TenantCode = "tnt-123",
                ConsumerCode = "cmr-456",
                CreateUser = "test-user",
                UseDefault = false,
                NotificationConfig = new ConsumerPrefConfigDto
                {
                    CategoryConfig = new List<CategoryConfig>
                    {
                        new CategoryConfig
                        {
                            CategoryName = "Rewards",
                            ChannelConfig = new List<ChannelConfig>
                            {
                                new ChannelConfig { ChannelType = "Email", Enabled = true },
                                new ChannelConfig { ChannelType = "SMS", Enabled = false }
                            }
                        }
                    },
                    MasterChannelConfig = new List<ChannelConfig>
                    {
                        new ChannelConfig { ChannelType = "Push", Enabled = true }
                    }
                }
            };

            var expectedResponse = new GetConsumerNotificationPrefResponseMockDto();

            _notificationServiceMock.Setup(x => x.CreateConsumerNotificationPref(requestDto))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _notificationController.CreateConsumerNotificationPref(requestDto);
            var okResult = result.Result as OkObjectResult;

            // Assert
            Assert.NotNull(okResult);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Fact]
        public async Task CreateConsumerNotificationPref_Should_Return_ErrorCodeStatus()
        {
            // Arrange
            var requestDto = new CreateConsumerNotificationPrefRequestDto
            {
                TenantCode = "tnt-123",
                ConsumerCode = "cmr-456",
                CreateUser = "test-user",
                UseDefault = true
            };

            var errorResponse = new ConsumerNotificationPrefResponseDto
            {
                ErrorCode = StatusCodes.Status400BadRequest,
                ErrorMessage = "Invalid request"
            };

            _notificationServiceMock.Setup(x => x.CreateConsumerNotificationPref(requestDto))
                .ReturnsAsync(errorResponse);

            // Act
            var result = await _notificationController.CreateConsumerNotificationPref(requestDto);
            var objectResult = Assert.IsType<ObjectResult>(result.Result);

            // Assert
            Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
            Assert.Equal("Invalid request", ((ConsumerNotificationPrefResponseDto)objectResult.Value).ErrorMessage);
        }

        [Fact]
        public async Task CreateConsumerNotificationPref_Should_Return_Exception_Catch_In_Controller()
        {
            // Arrange
            var requestDto = new CreateConsumerNotificationPrefRequestDto
            {
                TenantCode = "tnt-123",
                ConsumerCode = "cmr-456",
                CreateUser = "test-user",
                UseDefault = false
            };

            _notificationServiceMock.Setup(x => x.CreateConsumerNotificationPref(requestDto))
                .ThrowsAsync(new Exception("Simulated failure"));

            // Act
            var result = await _notificationController.CreateConsumerNotificationPref(requestDto);

            // Assert
            var errorResponse = Assert.IsType<ConsumerNotificationPrefResponseDto>(result.Value);
            Assert.Equal(StatusCodes.Status500InternalServerError, errorResponse.ErrorCode);
        }

        [Fact]
        public async Task CreateConsumerNotificationPref_Should_Throw_In_Service_When_ClientFails()
        {
            // Arrange
            var requestDto = new CreateConsumerNotificationPrefRequestDto
            {
                TenantCode = "tnt-123",
                ConsumerCode = "cmr-456",
                CreateUser = "test-user",
                UseDefault = true
            };

            _notificationClient.Setup(x => x.Post<ConsumerNotificationPrefResponseDto>(
                "consumer-notication-pref/create-consumer-notification-pref", requestDto))
                .ThrowsAsync(new Exception("Client failed"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _notificationService.CreateConsumerNotificationPref(requestDto));
        }

        [Fact]
        public async Task CreateConsumerNotificationPref_Should_Return_ErrorResponse_When_ErrorCode_IsNotNull()
        {
            // Arrange
            var requestDto = new CreateConsumerNotificationPrefRequestDto
            {
                TenantCode = "tnt-123",
                ConsumerCode = "cmr-456",
                CreateUser = "test-user",
                UseDefault = true
            };

            var errorResponse = new ConsumerNotificationPrefResponseDto
            {
                ErrorCode = StatusCodes.Status400BadRequest,
                ErrorMessage = "Invalid config"
            };

            _notificationClient.Setup(x => x.Post<ConsumerNotificationPrefResponseDto>(
                "consumer-notication-pref/create-consumer-notification-pref", requestDto))
                .ReturnsAsync(errorResponse);

            // Act
            var result = await _notificationService.CreateConsumerNotificationPref(requestDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
            Assert.Equal("Invalid config", result.ErrorMessage);
        }

        [Fact]
        public async Task Should_UpdateConsumerNotificationPref()
        {
            // Arrange
            var requestDto = new UpdateConsumerNotificationPrefRequestDto
            {
                ConsumerNotificationPreferenceId = 1,
                UpdateUser = "test-user",
                UseDefault = false,
                NotificationConfig = new ConsumerPrefConfigDto
                {
                    CategoryConfig = new List<CategoryConfig>
                    {
                        new CategoryConfig
                        {
                            CategoryName = "Rewards",
                            ChannelConfig = new List<ChannelConfig>
                            {
                                new ChannelConfig { ChannelType = "Email", Enabled = true },
                                new ChannelConfig { ChannelType = "SMS", Enabled = false }
                            }
                        }
                    },
                    MasterChannelConfig = new List<ChannelConfig>
                    {
                        new ChannelConfig { ChannelType = "Push", Enabled = true }
                    }
                }
            };

            var expectedResponse = new GetConsumerNotificationPrefResponseMockDto();

            _notificationServiceMock.Setup(x => x.UpdateCustomerNotificationPref(requestDto))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _notificationController.UpdateCustomerNotificationPref(requestDto);
            var okResult = result.Result as OkObjectResult;

            // Assert
            Assert.NotNull(okResult);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task UpdateConsumerNotificationPref_Should_Return_ErrorCodeStatus()
        {
            // Arrange
            var requestDto = new UpdateConsumerNotificationPrefRequestDto
            {
                ConsumerNotificationPreferenceId = 1,
                UpdateUser = "test-user",
                UseDefault = true,
                NotificationConfig = null
            };

            var errorResponse = new ConsumerNotificationPrefResponseDto
            {
                ErrorCode = StatusCodes.Status400BadRequest,
                ErrorMessage = "Update failed"
            };

            _notificationServiceMock.Setup(x => x.UpdateCustomerNotificationPref(requestDto))
                .ReturnsAsync(errorResponse);

            // Act
            var result = await _notificationController.UpdateCustomerNotificationPref(requestDto);
            var objectResult = Assert.IsType<ObjectResult>(result.Result);

            // Assert
            Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
            Assert.Equal("Update failed", ((ConsumerNotificationPrefResponseDto)objectResult.Value).ErrorMessage);
        }

        [Fact]
        public async Task UpdateConsumerNotificationPref_Should_Return_Exception_Catch_In_Controller()
        {
            // Arrange
            var requestDto = new UpdateConsumerNotificationPrefRequestDto
            {
                ConsumerNotificationPreferenceId = 9876,
                UpdateUser = "test-user",
                UseDefault = false,
                NotificationConfig = null
            };

            _notificationServiceMock.Setup(x => x.UpdateCustomerNotificationPref(requestDto))
                .ThrowsAsync(new Exception("Simulated controller exception"));

            // Act
            var result = await _notificationController.UpdateCustomerNotificationPref(requestDto);

            // Assert
            var errorResponse = Assert.IsType<ConsumerNotificationPrefResponseDto>(result.Value);
            Assert.Equal(StatusCodes.Status500InternalServerError, errorResponse.ErrorCode);
        }

        [Fact]
        public async Task UpdateConsumerNotificationPref_Should_Throw_In_Service_When_ClientFails()
        {
            // Arrange
            var requestDto = new UpdateConsumerNotificationPrefRequestDto
            {
                ConsumerNotificationPreferenceId = 9876,
                UpdateUser = "test-user",
                UseDefault = true,
                NotificationConfig = null
            };

            _notificationClient.Setup(x => x.Put<ConsumerNotificationPrefResponseDto>(
                "consumer-notication-pref/update-consumer-notification-pref", requestDto))
                .ThrowsAsync(new Exception("Client failure"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _notificationService.UpdateCustomerNotificationPref(requestDto));
        }

        [Fact]
        public async Task UpdateConsumerNotificationPref_Should_Return_ErrorResponse_When_ErrorCode_IsNotNull()
        {
            // Arrange
            var requestDto = new UpdateConsumerNotificationPrefRequestDto
            {
                ConsumerNotificationPreferenceId = 101,
                UpdateUser = "test-user",
                UseDefault = false
            };

            var errorResponse = new ConsumerNotificationPrefResponseDto
            {
                ErrorCode = StatusCodes.Status422UnprocessableEntity,
                ErrorMessage = "Update failed"
            };

            _notificationClient.Setup(x => x.Put<ConsumerNotificationPrefResponseDto>(
                "consumer-notication-pref/update-consumer-notification-pref", requestDto))
                .ReturnsAsync(errorResponse);

            // Act
            var result = await _notificationService.UpdateCustomerNotificationPref(requestDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status422UnprocessableEntity, result.ErrorCode);
            Assert.Equal("Update failed", result.ErrorMessage);
        }
    }
}
