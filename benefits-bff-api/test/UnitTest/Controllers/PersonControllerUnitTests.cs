using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using Sunny.Benefits.Bff.Api.Controllers;
using Sunny.Benefits.Bff.Infrastructure.HttpClients.Interfaces;
using Sunny.Benefits.Bff.Infrastructure.Repositories.Interfaces;
using Sunny.Benefits.Bff.Infrastructure.Services.Interfaces;
using Sunny.Benefits.Bff.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using Xunit;

namespace Sunny.Benefits.Bff.UnitTest.Controllers
{
    public class PersonControllerUnitTests
    {
        private readonly Mock<ILogger<PersonService>> _personServiceLogger;
        private readonly Mock<ILogger<PersonController>> _personControllerLogger;
        private readonly IPersonService _personService;
        private readonly PersonController _personController;
        private readonly Mock<IMapper> _mapper;
        private readonly Mock<IUserClient> _userClient;
        public PersonControllerUnitTests()
        {
            _userClient = new Mock<IUserClient>();
            _personControllerLogger = new Mock<ILogger<PersonController>>();
            _personServiceLogger = new Mock<ILogger<PersonService>>();
            _mapper = new Mock<IMapper>();
            _personService = new PersonService(_personServiceLogger.Object, _userClient.Object, _mapper.Object);
            _personController = new PersonController(_personControllerLogger.Object, _personService);
        }

        [Fact]
        public async Task UpdatePersonData_ReturnsOk_WhenUpdatedSuccessfully()
        {
            // Arrange
            var updateRequest = new UpdatePersonRequestDto
            {
                ConsumerCode = "ABC123",
                PhoneNumber = "1234567890",
                UpdateUser = "test-user"
            };

            var expectedResponse = new PersonResponseDto
            {
                Person = new PersonDto { PersonId = 1, PhoneNumber = "1234567890" },
                ErrorCode = null
            };

            _userClient.Setup(x => x.Put<PersonResponseDto>("person/update-person", updateRequest))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _personController.UpdatePersonData(updateRequest);
            var okResult = result.Result as OkObjectResult;

            // Assert
            Assert.NotNull(okResult);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
            Assert.NotNull(okResult.Value);
            Assert.Equal("1234567890", ((PersonResponseDto)okResult.Value).Person.PhoneNumber);
        }

        [Fact]
        public async Task UpdatePersonData_ReturnsErrorStatus_WhenErrorCodePresent()
        {
            // Arrange
            var updateRequest = new UpdatePersonRequestDto
            {
                ConsumerCode = "ABC123",
                UpdateUser = "test-user"
            };

            var errorResponse = new PersonResponseDto
            {
                ErrorCode = StatusCodes.Status400BadRequest,
                ErrorMessage = "Invalid request"
            };

            _userClient.Setup(x => x.Put<PersonResponseDto>("person/update-person", updateRequest))
                .ReturnsAsync(errorResponse);

            // Act
            var result = await _personController.UpdatePersonData(updateRequest);
            var objectResult = Assert.IsType<ObjectResult>(result.Result);

            // Assert
            Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
            Assert.Equal("Invalid request", ((PersonResponseDto)objectResult.Value).ErrorMessage);
        }

        [Fact]
        public async Task UpdatePersonData_ReturnsServerError_WhenExceptionThrown()
        {
            // Arrange
            var updateRequest = new UpdatePersonRequestDto
            {
                ConsumerCode = "ABC123",
                UpdateUser = "test-user"
            };

            _userClient.Setup(x => x.Put<PersonResponseDto>("person/update-person", updateRequest))
                .ThrowsAsync(new Exception("Simulated service exception"));

            // Act
            var result = await _personController.UpdatePersonData(updateRequest);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            var response = Assert.IsType<PersonResponseDto>(objectResult.Value);
            Assert.Equal(StatusCodes.Status500InternalServerError, response.ErrorCode);
            Assert.Equal("Simulated service exception", response.ErrorMessage);
        }

        [Fact]
        public async Task UpdatePersonData_ShouldReturnErrorResponse_WhenErrorCodeIsPresent()
        {
            // Arrange
            var updateRequest = new UpdatePersonRequestDto
            {
                ConsumerCode = "ABC123",
                UpdateUser = "test-user"
            };

            var errorResponse = new PersonResponseDto
            {
                ErrorCode = StatusCodes.Status422UnprocessableEntity,
                ErrorMessage = "Invalid data"
            };

            _userClient.Setup(x => x.Put<PersonResponseDto>("person/update-person", updateRequest))
                .ReturnsAsync(errorResponse);

            // Act
            var result = await _personService.UpdatePersonData(updateRequest);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status422UnprocessableEntity, result.ErrorCode);
            Assert.Equal("Invalid data", result.ErrorMessage);
        }
    }
}
