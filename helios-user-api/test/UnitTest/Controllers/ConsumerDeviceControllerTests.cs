using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using SunnyRewards.Helios.Common.Core.Services;
using SunnyRewards.Helios.Common.Core.Services.Interfaces;
using SunnyRewards.Helios.User.Api.Controllers;
using SunnyRewards.Helios.User.Core.Domain.Constant;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Models;
using SunnyRewards.Helios.User.Infrastructure.Services;
using SunnyRewards.Helios.User.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.User.UnitTest.Fixtures.MockModels;
using SunnyRewards.Helios.User.UnitTest.Fixtures.MockRepositories;
using System.Linq.Expressions;
using Xunit;

namespace SunnyRewards.Helios.User.UnitTest.Controllers
{
    public class ConsumerDeviceControllerTests
    {
        private readonly IConsumerDeviceService _consumerDeviceService;
        private readonly ConsumerDeviceController _consumerDeviceController;
        private readonly Mock<IVault> _vaultMock;
        private readonly Mock<IEncryptionHelper> _encryptionHelper;
        private readonly ConsumerDeviceMockRepo _consumerDeviceMockRepo;
        private readonly Mock<ILogger<ConsumerDeviceController>> _controllerLogger;
        private readonly Mock<ILogger<ConsumerDeviceService>> _serviceLogger;
        private readonly Mock<IMapper> _mapper;
        private readonly Mock<IHashingService> _hashingService;

        public ConsumerDeviceControllerTests()
        {
            _serviceLogger = new Mock<ILogger<ConsumerDeviceService>>();
            _controllerLogger = new Mock<ILogger<ConsumerDeviceController>>();
            _vaultMock = new Mock<IVault>();
            _encryptionHelper = new Mock<IEncryptionHelper>();
            _consumerDeviceMockRepo = new ConsumerDeviceMockRepo();
            _mapper = new Mock<IMapper>();
            _hashingService = new Mock<IHashingService>();
            _consumerDeviceService = new ConsumerDeviceService(_serviceLogger.Object, _consumerDeviceMockRepo.Object,
                 _encryptionHelper.Object, _vaultMock.Object, _mapper.Object, _hashingService.Object);
            _consumerDeviceController = new ConsumerDeviceController(_controllerLogger.Object, _consumerDeviceService);
        }
        [Fact]
        public async Task Create_ConsumerDevice_Should_Return_Ok_Response()
        {
            // Arrange
            var requestDto = GetPostConsumerDeviceRequestDto();
            var symmetricEncryptionKey = "Test Symmetric Key";
            var decryptedDeviceId = "vnOphbTxJOEhU/tAPe/U2zSbagWVRj4yOKXEwEgdGJp39irc9IB8cWM";
            _consumerDeviceMockRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ConsumerDeviceModel, bool>>>(), false));
            _vaultMock.Setup(x => x.GetTenantSecret(requestDto.TenantCode, SecretName.SymmetricEncryptionKey)).ReturnsAsync(symmetricEncryptionKey);
            _encryptionHelper.Setup(x => x.Encrypt(requestDto.DeviceId, Convert.FromBase64String(symmetricEncryptionKey))).Returns(decryptedDeviceId);

            // Act 
            var response = await _consumerDeviceController.CreateConsumerDevice(requestDto);

            // Assert
            Assert.NotNull(response);
            var okObjectResponse = response.Result as OkObjectResult;
            Assert.IsType<OkObjectResult>(response.Result);
            Assert.Equal(StatusCodes.Status200OK, okObjectResponse.StatusCode);
        }
        [Fact]
        public async Task Create_ConsumerDevice_Should_Return_Conflict_Response()
        {
            // Arrange
            var requestDto = GetPostConsumerDeviceRequestDto();
            _consumerDeviceMockRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ConsumerDeviceModel, bool>>>(), false)).ReturnsAsync(new ConsumerDeviceModel());

            // Act 
            var response = await _consumerDeviceController.CreateConsumerDevice(requestDto);

            // Assert
            Assert.NotNull(response);
            var objectResult = response.Result as ObjectResult;
            Assert.IsType<ObjectResult>(response.Result);
            Assert.Equal(StatusCodes.Status409Conflict, objectResult.StatusCode);
        }
        [Fact]
        public async Task Create_ConsumerDevice_Should_Return_BadRequest_Response()
        {
            // Arrange
            var requestDto = GetPostConsumerDeviceRequestDto();
            _consumerDeviceMockRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ConsumerDeviceModel, bool>>>(), false));
            _vaultMock.Setup(x => x.InvalidSecret).Returns("<SECRET_NOT_FOUND>");
            _vaultMock.Setup(x => x.GetTenantSecret(requestDto.TenantCode, SecretName.SymmetricEncryptionKey)).ReturnsAsync("<SECRET_NOT_FOUND>");

            // Act 
            var response = await _consumerDeviceController.CreateConsumerDevice(requestDto);

            // Assert
            Assert.NotNull(response);
            var objectResult = response.Result as ObjectResult;
            Assert.IsType<ObjectResult>(response.Result);
            Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
        }
        [Fact]
        public async Task Create_ConsumerDevice_Should_Return_InternalServer_Error()
        {
            // Arrange
            var requestDto = GetPostConsumerDeviceRequestDto();
            _consumerDeviceMockRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ConsumerDeviceModel, bool>>>(), false))
                .Throws(new Exception("Some thing went wrong"));

            // Act 
            var response = await _consumerDeviceController.CreateConsumerDevice(requestDto);

            // Assert
            Assert.NotNull(response);
            var objectResult = response.Result as ObjectResult;
            Assert.IsType<ObjectResult>(response.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }
        [Fact]
        public async Task Get_ConsumerDevices_Should_Return_Ok_Response()
        {
            // Arrange
            var requestDto = new GetConsumerDeviceRequestDto()
            {
                TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4",
                ConsumerCode = "cmr-58b6d93e24284a0db8bbe7fd782362ed"
            };
            _consumerDeviceMockRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<ConsumerDeviceModel, bool>>>(), false))
                .ReturnsAsync(new List<ConsumerDeviceModel>() { new ConsumerDeviceMockModel() });

            // Act 
            var response = await _consumerDeviceController.GetConsumerDevices(requestDto);

            // Assert
            Assert.NotNull(response);
            var objectResult = response.Result as OkObjectResult;
            Assert.IsType<OkObjectResult>(response.Result);
            Assert.Equal(StatusCodes.Status200OK, objectResult.StatusCode);
        }
        [Fact]
        public async Task Get_ConsumerDevices_Should_Return_NotFound_Response()
        {
            // Arrange
            var requestDto = new GetConsumerDeviceRequestDto()
            {
                TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4",
                ConsumerCode = "cmr-58b6d93e24284a0db8bbe7fd782362ed"
            };
            _consumerDeviceMockRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<ConsumerDeviceModel, bool>>>(), false));

            // Act 
            var response = await _consumerDeviceController.GetConsumerDevices(requestDto);

            // Assert
            Assert.NotNull(response);
            var objectResult = response.Result as ObjectResult;
            Assert.IsType<ObjectResult>(response.Result);
            Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);
        }
        [Fact]
        public async Task Get_ConsumerDevices_Should_Return_InternalServer_Error()
        {
            // Arrange
            var requestDto = new GetConsumerDeviceRequestDto()
            {
                TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4",
                ConsumerCode = "cmr-58b6d93e24284a0db8bbe7fd782362ed"
            };
            _consumerDeviceMockRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<ConsumerDeviceModel, bool>>>(), false))
                .Throws(new Exception("Some thing went wrong"));

            // Act 
            var response = await _consumerDeviceController.GetConsumerDevices(requestDto);

            // Assert
            Assert.NotNull(response);
            var objectResult = response.Result as ObjectResult;
            Assert.IsType<ObjectResult>(response.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        private PostConsumerDeviceRequestDto GetPostConsumerDeviceRequestDto()
        {
            return new PostConsumerDeviceRequestDto()
            {
                ConsumerCode = "cmr-58b6d93e24284a0db8bbe7fd782362ed",
                TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4",
                DeviceId = "8d14f6e4-2a4e-4c9f-a7cb-b739e81d4e90",
                DeviceType = "PHONE",
                DeviceAttrJson = "{\"screen_size\": \"6.7 inches\", \"screen_resolution\": \"2560x1440\", \"device_description\": \"Flagship smartphone with advanced camera\", \"device_platform\": \"Android\", \"platform_version\": \"14.0\"}"
            };
        }
    }
}
