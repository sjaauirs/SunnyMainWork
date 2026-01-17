using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using Sunny.Benefits.Bff.Api.Controllers;
using Sunny.Benefits.Bff.Infrastructure.Repositories.Interfaces;
using Sunny.Benefits.Bff.Infrastructure.Services.Interfaces;
using Sunny.Benefits.Bff.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using Xunit;
using Sunny.Benefits.Bff.Infrastructure.HttpClients.Interfaces;
using SunnyBenefits.Fis.Core.Domain.Dtos;

namespace Sunny.Benefits.Bff.UnitTest.Controllers
{
    public class PersonAddressControllerTests
    {
        private readonly Mock<ILogger<PersonAddressService>> _personAddressServiceLogger;
        private readonly Mock<ILogger<PersonAddressController>> _personAddressControllerLogger;
        private readonly IPersonAddressService _personAddressService;
        private readonly PersonAddressController _personAddressController;
        private readonly Mock<IMapper> _mapper;
        private readonly Mock<IUserClient> _userClient;
        private readonly Mock<IFisClient> _fisClient;
        public PersonAddressControllerTests()
        {
            _userClient = new Mock<IUserClient>();
            _personAddressControllerLogger = new Mock<ILogger<PersonAddressController>>();
            _personAddressServiceLogger = new Mock<ILogger<PersonAddressService>>();
            _mapper = new Mock<IMapper>();
            _fisClient = new Mock<IFisClient>();
            _personAddressService = new PersonAddressService(_personAddressServiceLogger.Object, _userClient.Object, _mapper.Object, _fisClient.Object);
            _personAddressController = new PersonAddressController(_personAddressControllerLogger.Object, _personAddressService);
        }

        [Fact]
        public async Task Should_GetAllPersonAddresses_ByPersonId()
        {
            // Arrange
            long personId = 1;
            var expectedResponse = new GetAllPersonAddressesResponseDto
            {
                PersonAddressesList = new List<PersonAddressDto>(),
                ErrorCode = null
            };

            _userClient.Setup(x =>
                x.GetId<GetAllPersonAddressesResponseDto>(
                    It.IsAny<string>(),
                    It.IsAny<IDictionary<string, string>>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _personAddressController.GetAllPersonAddresses(personId);
            var okResult = result as OkObjectResult;

            // Assert
            Assert.NotNull(okResult?.Value);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Fact]
        public async Task GetAllPersonAddresses_Should_Return_ErrorResponse_When_ClientReturns404Response()
        {
            // Arrange
            long personId = 1;
            var errorResponse = new GetAllPersonAddressesResponseDto
            {
                ErrorCode = StatusCodes.Status404NotFound,
                ErrorMessage = "Address not found"
            };

            _userClient.Setup(x =>
                x.GetId<GetAllPersonAddressesResponseDto>(
                    It.IsAny<string>(),
                    It.IsAny<IDictionary<string, string>>()))
                .ReturnsAsync(errorResponse);

            // Act
            var result = await _personAddressController.GetAllPersonAddresses(personId);
            var objectResult = result as ObjectResult;

            // Assert
            Assert.NotNull(objectResult);
            Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);
            var responseDto = Assert.IsType<GetAllPersonAddressesResponseDto>(objectResult.Value);
            Assert.Equal(StatusCodes.Status404NotFound, responseDto.ErrorCode);
        }

        [Fact]
        public async Task GetAllPersonAddresses_Should_Handle_Exception_Thrown_By_Client()
        {
            // Arrange
            long personId = 1;
            var exceptionMessage = "Simulated client failure";

            _userClient.Setup(x =>
                x.GetId<GetAllPersonAddressesResponseDto>(
                    It.IsAny<string>(),
                    It.IsAny<IDictionary<string, string>>()))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await _personAddressController.GetAllPersonAddresses(personId);
            var objectResult = Assert.IsType<ObjectResult>(result);

            // Assert
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
            var responseDto = Assert.IsType<GetAllPersonAddressesResponseDto>(objectResult.Value);
            Assert.Equal(StatusCodes.Status500InternalServerError, responseDto.ErrorCode);
            Assert.Equal(exceptionMessage, responseDto.ErrorMessage);
        }

        [Fact]
        public async Task Should_CreatePersonAddress_Successfully_And_SetAsPrimary()
        {
            // Arrange
            var request = new CreatePersonAddressRequestDto
            {
                PersonId = 123,
                CreateUser = "user",
                AddressTypeId = 1,
                TenantCode = "tenant",
                ConsumerCode = "consumer"
            };

            var addressResponse = new PersonAddressResponseDto
            {
                ErrorCode = null,
                PersonAddress = new PersonAddressDto { PersonAddressId = 999 }
            };

            _userClient.Setup(x =>
                x.Post<PersonAddressResponseDto>(It.IsAny<string>(), It.IsAny<CreatePersonAddressRequestDto>()))
                .ReturnsAsync(addressResponse);

            _userClient.Setup(x =>
                x.Put<PersonAddressResponseDto>(It.IsAny<string>(), It.IsAny<UpdatePrimaryPersonAddressRequestDto>()))
                .ReturnsAsync(addressResponse);

            var mockFisResponse = new List<ConsumerAccountResponseDto>
            {
                new ConsumerAccountResponseDto
                {
                    ErrorMessage = null,
                    ErrorCode = null
                }
            };

            _fisClient.Setup(x =>
                x.Put<List<ConsumerAccountResponseDto>>(
                    It.IsAny<string>(),
                    It.IsAny<List<ConsumerAccountDto>>()))
                .ReturnsAsync(mockFisResponse);

            // Act
            var result = await _personAddressController.CreatePersonAddress(request);
            var okResult = result as OkObjectResult;

            // Assert
            Assert.NotNull(okResult);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
            var responseDto = Assert.IsType<PersonAddressResponseDto>(okResult.Value);
            Assert.Null(responseDto.ErrorCode);
        }

        [Fact]
        public async Task Should_CreatePersonAddress_Successfully_And_SetAsPrimary_FISNullResponse()
        {
            // Arrange
            var request = new CreatePersonAddressRequestDto
            {
                PersonId = 123,
                CreateUser = "user",
                AddressTypeId = 1,
                TenantCode = "tenant",
                ConsumerCode = "consumer"
            };

            var addressResponse = new PersonAddressResponseDto
            {
                ErrorCode = null,
                PersonAddress = new PersonAddressDto { PersonAddressId = 999 }
            };

            _userClient.Setup(x =>
                x.Post<PersonAddressResponseDto>(It.IsAny<string>(), It.IsAny<CreatePersonAddressRequestDto>()))
                .ReturnsAsync(addressResponse);

            _userClient.Setup(x =>
                x.Put<PersonAddressResponseDto>(It.IsAny<string>(), It.IsAny<UpdatePrimaryPersonAddressRequestDto>()))
                .ReturnsAsync(addressResponse);

            var mockFisResponse = new List<ConsumerAccountResponseDto>
            {
                new ConsumerAccountResponseDto
                {
                    ErrorMessage = "Update failed",
                    ErrorCode = 500
                }
            };

            _fisClient.Setup(x =>
                x.Put<List<ConsumerAccountResponseDto>>(
                    It.IsAny<string>(),
                    It.IsAny<List<ConsumerAccountDto>>()))
                .ReturnsAsync(mockFisResponse);

            // Act
            var result = await _personAddressController.CreatePersonAddress(request);
            var okResult = result as OkObjectResult;

            // Assert
            Assert.NotNull(okResult);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
            var responseDto = Assert.IsType<PersonAddressResponseDto>(okResult.Value);
            Assert.Null(responseDto.ErrorCode);
        }

        [Fact]
        public async Task CreatePersonAddress_Should_Return_Error_When_Create_Fails()
        {
            // Arrange
            var request = new CreatePersonAddressRequestDto
            {
                PersonId = 123,
                CreateUser = "user"
            };

            var errorResponse = new PersonAddressResponseDto
            {
                ErrorCode = StatusCodes.Status400BadRequest,
                ErrorMessage = "Create failed"
            };

            _userClient.Setup(x =>
                x.Post<PersonAddressResponseDto>(It.IsAny<string>(), It.IsAny<CreatePersonAddressRequestDto>()))
                .ReturnsAsync(errorResponse);

            // Act
            var result = await _personAddressController.CreatePersonAddress(request);
            var objectResult = result as ObjectResult;

            // Assert
            Assert.NotNull(objectResult);
            Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
            var responseDto = Assert.IsType<PersonAddressResponseDto>(objectResult.Value);
            Assert.Equal(StatusCodes.Status400BadRequest, responseDto.ErrorCode);
        }

        [Fact]
        public async Task CreatePersonAddress_Should_Log_When_SetPrimary_Fails()
        {
            // Arrange
            var request = new CreatePersonAddressRequestDto
            {
                PersonId = 123,
                CreateUser = "user",
                AddressTypeId = 1
            };

            var successResponse = new PersonAddressResponseDto
            {
                ErrorCode = null,
                PersonAddress = new PersonAddressDto { PersonAddressId = 999 }
            };

            var setPrimaryErrorResponse = new PersonAddressResponseDto
            {
                ErrorCode = StatusCodes.Status500InternalServerError,
                ErrorMessage = "Failed to set as primary"
            };

            _userClient.Setup(x =>
                x.Post<PersonAddressResponseDto>(It.IsAny<string>(), It.IsAny<CreatePersonAddressRequestDto>()))
                .ReturnsAsync(successResponse);

            _userClient.Setup(x =>
                x.Put<PersonAddressResponseDto>(It.IsAny<string>(), It.IsAny<UpdatePrimaryPersonAddressRequestDto>()))
                .ReturnsAsync(setPrimaryErrorResponse);

            // Act
            var result = await _personAddressController.CreatePersonAddress(request);
            var objectResult = result as ObjectResult;

            // Assert
            Assert.NotNull(objectResult);
            var responseDto = Assert.IsType<PersonAddressResponseDto>(objectResult.Value);
            Assert.Equal(null, responseDto.ErrorCode);
        }

        [Fact]
        public async Task CreatePersonAddress_Should_Handle_Exception_Thrown_By_Client()
        {
            // Arrange
            var request = new CreatePersonAddressRequestDto
            {
                PersonId = 123,
                CreateUser = "user"
            };

            _userClient.Setup(x =>
                x.Post<PersonAddressResponseDto>(It.IsAny<string>(), It.IsAny<CreatePersonAddressRequestDto>()))
                .ThrowsAsync(new Exception("Simulated client exception"));

            // Act
            var result = await _personAddressController.CreatePersonAddress(request);
            var objectResult = Assert.IsType<ObjectResult>(result);

            // Assert
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
            var responseDto = Assert.IsType<PersonAddressResponseDto>(objectResult.Value);
            Assert.Equal(StatusCodes.Status500InternalServerError, responseDto.ErrorCode);
        }

        [Fact]
        public async Task Should_UpdatePersonAddress_Without_Marking_As_Primary()
        {
            // Arrange
            var request = new UpdatePersonAddressRequestDto
            {
                PersonAddressId = 101,
                UpdateUser = "user"
            };

            var successResponse = new PersonAddressResponseDto
            {
                ErrorCode = null,
                PersonAddress = new PersonAddressDto { PersonAddressId = 101 }
            };

            _userClient.Setup(x =>
                x.Put<PersonAddressResponseDto>(It.IsAny<string>(), It.IsAny<UpdatePersonAddressRequestDto>()))
                .ReturnsAsync(successResponse);

            // Act
            var result = await _personAddressController.UpdatePersonAddress(request, false);
            var okResult = Assert.IsType<OkObjectResult>(result);

            // Assert
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
            Assert.Null(((PersonAddressResponseDto)okResult.Value!).ErrorCode);
        }

        [Fact]
        public async Task Should_UpdatePersonAddress_And_Mark_As_Primary()
        {
            // Arrange
            var request = new UpdatePersonAddressRequestDto
            {
                PersonAddressId = 101,
                UpdateUser = "user"
            };

            var updateResponse = new PersonAddressResponseDto
            {
                ErrorCode = null,
                PersonAddress = new PersonAddressDto { PersonAddressId = 101 }
            };

            _userClient.Setup(x =>
                x.Put<PersonAddressResponseDto>(It.IsAny<string>(), It.IsAny<UpdatePersonAddressRequestDto>()))
                .ReturnsAsync(updateResponse);

            _userClient.Setup(x =>
                x.Put<PersonAddressResponseDto>(It.IsAny<string>(), It.IsAny<UpdatePrimaryPersonAddressRequestDto>()))
                .ReturnsAsync(updateResponse);

            // Act
            var result = await _personAddressController.UpdatePersonAddress(request, true);
            var okResult = Assert.IsType<OkObjectResult>(result);

            // Assert
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
            Assert.Null(((PersonAddressResponseDto)okResult.Value!).ErrorCode);
        }

        [Fact]
        public async Task UpdatePersonAddress_Should_Return_Error_If_Update_Fails()
        {
            // Arrange
            var request = new UpdatePersonAddressRequestDto
            {
                PersonAddressId = 101,
                UpdateUser = "user"
            };

            var errorResponse = new PersonAddressResponseDto
            {
                ErrorCode = StatusCodes.Status400BadRequest,
                ErrorMessage = "Update failed"
            };

            _userClient.Setup(x =>
                x.Put<PersonAddressResponseDto>(It.IsAny<string>(), It.IsAny<UpdatePersonAddressRequestDto>()))
                .ReturnsAsync(errorResponse);

            // Act
            var result = await _personAddressController.UpdatePersonAddress(request, false);
            var objectResult = Assert.IsType<ObjectResult>(result);

            // Assert
            Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
            var responseDto = Assert.IsType<PersonAddressResponseDto>(objectResult.Value);
            Assert.Equal(StatusCodes.Status400BadRequest, responseDto.ErrorCode);
        }

        [Fact]
        public async Task UpdatePersonAddress_Should_Return_Error_If_SetPrimary_Fails()
        {
            // Arrange
            var request = new UpdatePersonAddressRequestDto
            {
                PersonAddressId = 101,
                UpdateUser = "user"
            };

            var updateResponse = new PersonAddressResponseDto
            {
                ErrorCode = null,
                PersonAddress = new PersonAddressDto { PersonAddressId = 101 }
            };

            var setPrimaryErrorResponse = new PersonAddressResponseDto
            {
                ErrorCode = StatusCodes.Status500InternalServerError,
                ErrorMessage = "Failed to set primary"
            };

            _userClient.Setup(x =>
                x.Put<PersonAddressResponseDto>(It.IsAny<string>(), It.IsAny<UpdatePersonAddressRequestDto>()))
                .ReturnsAsync(updateResponse);

            _userClient.Setup(x =>
                x.Put<PersonAddressResponseDto>(It.IsAny<string>(), It.IsAny<UpdatePrimaryPersonAddressRequestDto>()))
                .ReturnsAsync(setPrimaryErrorResponse);

            // Act
            var result = await _personAddressController.UpdatePersonAddress(request, true);
            var objectResult = Assert.IsType<ObjectResult>(result);

            // Assert
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
            var responseDto = Assert.IsType<PersonAddressResponseDto>(objectResult.Value);
            Assert.Equal(StatusCodes.Status500InternalServerError, responseDto.ErrorCode);
        }

        [Fact]
        public async Task UpdatePersonAddress_Should_Handle_Exception_Thrown_By_Client()
        {
            // Arrange
            var request = new UpdatePersonAddressRequestDto
            {
                PersonAddressId = 101,
                UpdateUser = "user"
            };

            _userClient.Setup(x =>
                x.Put<PersonAddressResponseDto>(It.IsAny<string>(), It.IsAny<UpdatePersonAddressRequestDto>()))
                .ThrowsAsync(new Exception("Simulated crash"));

            // Act
            var result = await _personAddressController.UpdatePersonAddress(request, false);
            var objectResult = Assert.IsType<ObjectResult>(result);

            // Assert
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
            var responseDto = Assert.IsType<PersonAddressResponseDto>(objectResult.Value);
            Assert.Equal(StatusCodes.Status500InternalServerError, responseDto.ErrorCode);
        }
    }
}
