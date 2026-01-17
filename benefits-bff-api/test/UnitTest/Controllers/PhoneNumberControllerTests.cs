using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Sunny.Benefits.Bff.Api.Controllers;
using Sunny.Benefits.Bff.Infrastructure.Repositories.Interfaces;
using Sunny.Benefits.Bff.Infrastructure.Services.Interfaces;
using Sunny.Benefits.Bff.Infrastructure.Services;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using Xunit;

namespace Sunny.Benefits.Bff.UnitTest.Controllers
{
    public class PhoneNumberControllerTests
    {
        private readonly Mock<ILogger<PhoneNumberService>> _serviceLogger;
        private readonly Mock<ILogger<PhoneNumberController>> _controllerLogger;
        private readonly Mock<IUserClient> _userClient;
        private readonly Mock<IMapper> _mapper;
        private readonly IPhoneNumberService _phoneNumberService;
        private readonly PhoneNumberController _phoneNumberController;

        public PhoneNumberControllerTests()
        {
            _serviceLogger = new Mock<ILogger<PhoneNumberService>>();
            _controllerLogger = new Mock<ILogger<PhoneNumberController>>();
            _userClient = new Mock<IUserClient>();
            _mapper = new Mock<IMapper>();
            _phoneNumberService = new PhoneNumberService(_serviceLogger.Object, _userClient.Object, _mapper.Object);
            _phoneNumberController = new PhoneNumberController(_controllerLogger.Object, _phoneNumberService);
        }

        [Fact]
        public async Task Should_GetAllPhoneNumbers_ByPersonId()
        {
            long personId = 1;
            var expectedResponse = new GetAllPhoneNumbersResponseDto
            {
                PhoneNumbersList = new List<PhoneNumberDto>(),
                ErrorCode = null
            };

            _userClient.Setup(x =>
                x.GetId<GetAllPhoneNumbersResponseDto>(It.IsAny<string>(), It.IsAny<IDictionary<string, string>>()))
                .ReturnsAsync(expectedResponse);

            var result = await _phoneNumberController.GetAllPhoneNumbers(personId);
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Fact]
        public async Task GetAllPhoneNumbers_Should_Return_Error_When_ClientReturns404()
        {
            long personId = 1;
            var response = new GetAllPhoneNumbersResponseDto { ErrorCode = StatusCodes.Status404NotFound, ErrorMessage = "Not found" };

            _userClient.Setup(x =>
                x.GetId<GetAllPhoneNumbersResponseDto>(It.IsAny<string>(), It.IsAny<IDictionary<string, string>>()))
                .ReturnsAsync(response);

            var result = await _phoneNumberController.GetAllPhoneNumbers(personId);
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);
        }

        [Fact]
        public async Task CreatePhoneNumber_Should_SetAsPrimary_IfMobile()
        {
            var request = new CreatePhoneNumberRequestDto { PersonId = 1, PhoneTypeId = 2, CreateUser = "admin" };
            var response = new PhoneNumberResponseDto
            {
                PhoneNumber = new PhoneNumberDto { PhoneNumberId = 101 },
                ErrorCode = null
            };

            _userClient.Setup(x => x.Post<PhoneNumberResponseDto>(It.IsAny<string>(), request))
                .ReturnsAsync(response);
            _userClient.Setup(x => x.Put<PhoneNumberResponseDto>(It.IsAny<string>(), It.IsAny<UpdatePrimaryPhoneNumberRequestDto>()))
                .ReturnsAsync(response);

            var result = await _phoneNumberController.CreatePhoneNumber(request);
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Fact]
        public async Task UpdatePhoneNumber_Should_SetAsPrimary_IfFlagTrue()
        {
            var request = new UpdatePhoneNumberRequestDto { PhoneNumberId = 1, UpdateUser = "admin" };
            var response = new PhoneNumberResponseDto
            {
                PhoneNumber = new PhoneNumberDto { PhoneNumberId = 1 },
                ErrorCode = null
            };

            _userClient.Setup(x => x.Put<PhoneNumberResponseDto>("phone-number/update-number", request))
                .ReturnsAsync(response);
            _userClient.Setup(x => x.Put<PhoneNumberResponseDto>("phone-number/set-primary", It.IsAny<UpdatePrimaryPhoneNumberRequestDto>()))
                .ReturnsAsync(response);

            var result = await _phoneNumberController.UpdatePhoneNumber(request, true);
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Fact]
        public async Task CreatePhoneNumber_Should_ReturnError_WhenClientFails()
        {
            var request = new CreatePhoneNumberRequestDto { PersonId = 1 };
            var response = new PhoneNumberResponseDto { ErrorCode = StatusCodes.Status400BadRequest, ErrorMessage = "Bad request" };

            _userClient.Setup(x => x.Post<PhoneNumberResponseDto>(It.IsAny<string>(), request))
                .ReturnsAsync(response);

            var result = await _phoneNumberController.CreatePhoneNumber(request);
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
        }

        [Fact]
        public async Task UpdatePhoneNumber_Should_ReturnError_IfPrimaryFails()
        {
            var request = new UpdatePhoneNumberRequestDto { PhoneNumberId = 1, UpdateUser = "admin" };
            var updateResponse = new PhoneNumberResponseDto { PhoneNumber = new PhoneNumberDto { PhoneNumberId = 1 }, ErrorCode = null };
            var failResponse = new PhoneNumberResponseDto { ErrorCode = StatusCodes.Status500InternalServerError, ErrorMessage = "Primary update failed" };

            _userClient.Setup(x => x.Put<PhoneNumberResponseDto>("phone-number/update-number", request))
                .ReturnsAsync(updateResponse);
            _userClient.Setup(x => x.Put<PhoneNumberResponseDto>("phone-number/set-primary", It.IsAny<UpdatePrimaryPhoneNumberRequestDto>()))
                .ReturnsAsync(failResponse);

            var result = await _phoneNumberController.UpdatePhoneNumber(request, true);
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        [Fact]
        public async Task UpdatePhoneNumber_Should_Handle_Exception_Thrown_By_Service()
        {
            // Arrange
            var request = new UpdatePhoneNumberRequestDto { PhoneNumberId = 1, UpdateUser = "admin" };
            var exceptionMessage = "Simulated controller catch test";

            var mockService = new Mock<IPhoneNumberService>();
            var logger = new Mock<ILogger<PhoneNumberController>>();
            var controller = new PhoneNumberController(logger.Object, mockService.Object);

            mockService.Setup(s => s.UpdatePhoneNumber(It.IsAny<UpdatePhoneNumberRequestDto>(), It.IsAny<bool>()))
                       .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await controller.UpdatePhoneNumber(request, markAsPrimary: false);
            var objectResult = Assert.IsType<ObjectResult>(result);

            // Assert
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
            var responseDto = Assert.IsType<PhoneNumberResponseDto>(objectResult.Value);
            Assert.Equal(StatusCodes.Status500InternalServerError, responseDto.ErrorCode);
        }

        [Fact]
        public async Task CreatePhoneNumber_Should_Handle_Exception_Thrown_By_Service()
        {
            // Arrange
            var request = new CreatePhoneNumberRequestDto { PersonId = 1, PhoneTypeId = 1, CreateUser = "admin" };
            var exceptionMessage = "Simulated controller catch test";

            var mockService = new Mock<IPhoneNumberService>();
            var logger = new Mock<ILogger<PhoneNumberController>>();
            var controller = new PhoneNumberController(logger.Object, mockService.Object);

            mockService.Setup(s => s.CreatePhoneNumber(It.IsAny<CreatePhoneNumberRequestDto>()))
                       .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await controller.CreatePhoneNumber(request);
            var objectResult = Assert.IsType<ObjectResult>(result);

            // Assert
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
            var responseDto = Assert.IsType<PhoneNumberResponseDto>(objectResult.Value);
            Assert.Equal(StatusCodes.Status500InternalServerError, responseDto.ErrorCode);
        }

        [Fact]
        public async Task GetAllPhoneNumbers_Should_Handle_Exception_Thrown_By_Service()
        {
            // Arrange
            long personId = 1;
            var exceptionMessage = "Simulated controller catch test";

            var mockService = new Mock<IPhoneNumberService>();
            var logger = new Mock<ILogger<PhoneNumberController>>();
            var controller = new PhoneNumberController(logger.Object, mockService.Object);

            mockService.Setup(s => s.GetAllPhoneNumbers(It.IsAny<long>()))
                       .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await controller.GetAllPhoneNumbers(personId);
            var objectResult = Assert.IsType<ObjectResult>(result);

            // Assert
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
            var responseDto = Assert.IsType<GetAllPhoneNumbersResponseDto>(objectResult.Value);
            Assert.Equal(StatusCodes.Status500InternalServerError, responseDto.ErrorCode);
        }

    }
}
