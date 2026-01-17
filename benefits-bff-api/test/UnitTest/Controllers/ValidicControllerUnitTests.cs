using Microsoft.Extensions.Logging;
using Moq;
using Sunny.Benefits.Bff.Api.Controllers;
using Sunny.Benefits.Bff.Infrastructure.HttpClients.Interfaces;
using Sunny.Benefits.Bff.Infrastructure.Services.Interfaces;
using Sunny.Benefits.Bff.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sunny.Benefits.Bff.Core.Domain.Dtos;
using Xunit;
using Sunny.Benefits.Bff.Infrastructure.Helpers.Interface;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using Sunny.Benefits.Bff.Core.Constants;
using SunnyRewards.Helios.User.Core.Domain.Dtos;

namespace Sunny.Benefits.Bff.UnitTest.Controllers
{
    public class ValidicControllerUnitTests
    {
        private readonly Mock<ILogger<ValidicController>> _validicControllerLogger;
        private readonly Mock<ILogger<ValidicService>> _validicServiceLogger;
        private readonly Mock<IValidicClient> _validicClient;
        private readonly Mock<IConfiguration> _configuration;
        private readonly Mock<IValidicService> _validicServiceMock;
        private readonly Mock<IVault> _vault;

        private readonly IValidicService _validicService;
        private readonly ValidicController _validicController;

        public ValidicControllerUnitTests()
        {
            _validicControllerLogger = new Mock<ILogger<ValidicController>>();
            _validicServiceLogger = new Mock<ILogger<ValidicService>>();
            _validicClient = new Mock<IValidicClient>();
            _configuration = new Mock<IConfiguration>();
            _validicServiceMock = new Mock<IValidicService>();
            _vault = new Mock<IVault>();

            // Setup for configuration values
            _vault.Setup(v => v.GetTenantSecret(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync("dummy-token");

            _validicService = new ValidicService(_validicServiceLogger.Object, _validicClient.Object, _configuration.Object, _vault.Object);
            _validicController = new ValidicController(_validicControllerLogger.Object, _validicServiceMock.Object);
        }

        [Fact]
        public async Task CreateValidicUser_Should_Return_Ok()
        {
            // Arrange
            var request = new CreateValidicUserRequestDto
            {
                ConsumerCode = "cmr-602a9f22-f62a-449b-b2b6-b638558777c5",
                TenantCode = "ten-602a9f22-f62a-449b-b2b6-b638558777c5"
            };

            var response = new CreateValidicUserResponseDto
            {
                Id = "test-id",
                Uid = "cmr-602a9f22-f62a-449b-b2b6-b638558777c5",
                Status = "active",
                Created_at = DateTime.UtcNow,
                Updated_at = DateTime.UtcNow,
                Marketplace = new Marketplace
                {
                    Token = "test-marketplace-token",
                    Url = "https://test-url.com"
                },
                Mobile = new Mobile
                {
                    Token = "test-mobile-token"
                }
            };

            _validicServiceMock.Setup(x => x.CreateValidicUser(request)).ReturnsAsync(response);

            // Act
            var result = await _validicController.CreateValidicUser(request);
            var okResult = result.Result as OkObjectResult;

            // Assert
            Assert.NotNull(okResult);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
            var dto = Assert.IsType<CreateValidicUserResponseDto>(okResult.Value);
            Assert.Equal("test-id", dto.Id);
        }

        [Fact]
        public async Task CreateValidicUser_Should_Return_422_When_Id_IsNull()
        {
            // Arrange
            var request = new CreateValidicUserRequestDto
            {
                ConsumerCode = "cmr-602a9f22-f62a-449b-b2b6-b638558777c5",
                TenantCode = "ten-602a9f22-f62a-449b-b2b6-b638558777c5"
            };

            var response = new CreateValidicUserResponseDto
            {
                Id = null,
                ErrorCode = StatusCodes.Status422UnprocessableEntity,
                Status = "error"
            };

            _validicServiceMock.Setup(x => x.CreateValidicUser(request)).ReturnsAsync(response);

            // Act
            var result = await _validicController.CreateValidicUser(request);
            var objectResult = Assert.IsType<ObjectResult>(result.Result);

            // Assert
            Assert.Equal(StatusCodes.Status422UnprocessableEntity, objectResult.StatusCode);
            var dto = Assert.IsType<CreateValidicUserResponseDto>(objectResult.Value);
            Assert.Equal(StatusCodes.Status422UnprocessableEntity, dto.ErrorCode);
        }

        [Fact]
        public async Task CreateValidicUser_Should_Return_500_On_Exception()
        {
            // Arrange
            var request = new CreateValidicUserRequestDto
            {
                ConsumerCode = "cmr-602a9f22-f62a-449b-b2b6-b638558777c5",
                TenantCode = "ten-602a9f22-f62a-449b-b2b6-b638558777c5"
            };

            _validicServiceMock.Setup(x => x.CreateValidicUser(request))
                .ThrowsAsync(new Exception("Simulated service failure"));

            // Act
            var result = await _validicController.CreateValidicUser(request);

            // Assert
            var responseDto = Assert.IsType<CreateValidicUserResponseDto>(result.Value);
            Assert.Equal(StatusCodes.Status500InternalServerError, responseDto.ErrorCode);
        }

        [Fact]
        public async Task CreateValidicUser_Should_Return_Response_When_Successful()
        {
            // Arrange
            var request = new CreateValidicUserRequestDto
            {
                ConsumerCode = "cmr-602a9f22-f62a-449b-b2b6-b638558777c5",
                TenantCode = "ten-602a9f22-f62a-449b-b2b6-b638558777c5"
            };
            var expectedGetResponse = new CreateValidicUserResponseDto
            {
                Id = null
            };
            var expectedResponse = new CreateValidicUserResponseDto
            {
                Id = "test-id",
                Uid = "cmr-602a9f22-f62a-449b-b2b6-b638558777c5",
                Status = "active",
                Created_at = DateTime.UtcNow,
                Updated_at = DateTime.UtcNow,
                Marketplace = new Marketplace
                {
                    Token = "test-marketplace-token",
                    Url = "https://test-url.com"
                },
                Mobile = new Mobile
                {
                    Token = "test-mobile-token"
                }
            };
            var expectedUrl = "organizations/dummy-token/users?token=dummy-token";

            _validicClient.Setup(client =>
                client.GetId<CreateValidicUserResponseDto>(
                    It.IsAny<string>(),
                    It.IsAny<IDictionary<string, string>>()))
                .ReturnsAsync(expectedGetResponse);

            _validicClient.Setup(client =>
                client.Post<CreateValidicUserResponseDto>(expectedUrl, It.IsAny<ValidicCreateUserRequestPayloadDto>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var response = await _validicService.CreateValidicUser(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal("test-id", response.Id);
            Assert.Null(response.ErrorCode);
        }

        [Fact]
        public async Task CreateValidicUser_Service_Should_Return_422_When_Id_IsNull()
        {
            // Arrange
            var request = new CreateValidicUserRequestDto
            {
                ConsumerCode = "cmr-602a9f22-f62a-449b-b2b6-b638558777c5",
                TenantCode = "ten-602a9f22-f62a-449b-b2b6-b638558777c5"
            };

            var failedResponse = new CreateValidicUserResponseDto
            {
                Id = null,
                Uid = "cmr-invalid"
            };

            _validicClient.Setup(client => client.Post<CreateValidicUserResponseDto>(
                It.IsAny<string>(), It.IsAny<ValidicCreateUserRequestPayloadDto>())).ReturnsAsync(failedResponse);

            // Act
            var response = await _validicService.CreateValidicUser(request);

            // Assert
            Assert.Equal(StatusCodes.Status422UnprocessableEntity, response.ErrorCode);
        }

        [Fact]
        public async Task CreateValidicUser_Should_Throw_Exception_When_ClientFails()
        {
            // Arrange
            var request = new CreateValidicUserRequestDto
            {
                ConsumerCode = "cmr-602a9f22-f62a-449b-b2b6-b638558777c5",
                TenantCode = "ten-602a9f22-f62a-449b-b2b6-b638558777c5"
            };

            _validicClient.Setup(client => client.Post<CreateValidicUserResponseDto>(
                It.IsAny<string>(), It.IsAny<ValidicCreateUserRequestPayloadDto>())).ThrowsAsync(new Exception("Simulated failure"));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _validicService.CreateValidicUser(request));
            Assert.Equal("Simulated failure", ex.Message);
        }

        [Fact]
        public async Task CreateValidicUser_Service_Should_Return_400_When_Token_Is_Missing()
        {
            // Arrange
            var request = new CreateValidicUserRequestDto
            {
                ConsumerCode = "cmr-602a9f22-f62a-449b-b2b6-b638558777c5",
                TenantCode = "ten-602a9f22-f62a-449b-b2b6-b638558777c5"
            };

            // Simulate missing token
            _vault.Setup(v => v.GetTenantSecret(request.TenantCode, CommonConstants.ValidicToken))
                .ReturnsAsync(string.Empty);

            // Still need to mock OrgId so the method doesn't crash before returning response
            _vault.Setup(v => v.GetTenantSecret(request.TenantCode, CommonConstants.ValidicOrgId))
                .ReturnsAsync("dummy-org-id");

            var service = new ValidicService(
                _validicServiceLogger.Object,
                _validicClient.Object,
                _configuration.Object,
                _vault.Object
            );

            // Act
            var response = await service.CreateValidicUser(request);

            // Assert
            Assert.Equal(StatusCodes.Status400BadRequest, response.ErrorCode);
            Assert.Equal("Validic token not found for the tenant.", response.ErrorMessage);
        }

        [Fact]
        public async Task CreateValidicUser_Service_Should_Return_400_When_OrgId_Is_Missing()
        {
            // Arrange
            var request = new CreateValidicUserRequestDto
            {
                ConsumerCode = "cmr-602a9f22-f62a-449b-b2b6-b638558777c5",
                TenantCode = "ten-602a9f22-f62a-449b-b2b6-b638558777c5"
            };

            // Simulate missing token
            _vault.Setup(v => v.GetTenantSecret(request.TenantCode, CommonConstants.ValidicToken))
                .ReturnsAsync("dummy-token");

            // Still need to mock OrgId so the method doesn't crash before returning response
            _vault.Setup(v => v.GetTenantSecret(request.TenantCode, CommonConstants.ValidicOrgId))
                .ReturnsAsync(string.Empty);

            var service = new ValidicService(
                _validicServiceLogger.Object,
                _validicClient.Object,
                _configuration.Object,
                _vault.Object
            );

            // Act
            var response = await service.CreateValidicUser(request);

            // Assert
            Assert.Equal(StatusCodes.Status400BadRequest, response.ErrorCode);
            Assert.Equal("Validic org id not found for the tenant.", response.ErrorMessage);
        }

        [Fact]
        public async Task CreateValidicUser_Should_Return_SuccessResponse_When_User_Exists()
        {
            // Arrange
            var request = new CreateValidicUserRequestDto
            {
                ConsumerCode = "cmr-602a9f22-f62a-449b-b2b6-b638558777c5",
                TenantCode = "ten-602a9f22-f62a-449b-b2b6-b638558777c5"
            };
            var expectedResponse = new CreateValidicUserResponseDto
            {
                Id = "test-id",
                Uid = "cmr-602a9f22-f62a-449b-b2b6-b638558777c5",
                Status = "active",
                Created_at = DateTime.UtcNow,
                Updated_at = DateTime.UtcNow,
                Marketplace = new Marketplace
                {
                    Token = "test-marketplace-token",
                    Url = "https://test-url.com"
                },
                Mobile = new Mobile
                {
                    Token = "test-mobile-token"
                }
            };
            var expectedUrl = "organizations/dummy-token/users?token=dummy-token";

            _validicClient.Setup(client =>
                client.GetId<CreateValidicUserResponseDto>(
                    It.IsAny<string>(),
                    It.IsAny<IDictionary<string, string>>()))
                .ReturnsAsync(expectedResponse);

            _validicClient.Setup(client =>
                client.Post<CreateValidicUserResponseDto>(expectedUrl, It.IsAny<ValidicCreateUserRequestPayloadDto>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var response = await _validicService.CreateValidicUser(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal("test-id", response.Id);
            Assert.Null(response.ErrorCode);
        }
    }
}
