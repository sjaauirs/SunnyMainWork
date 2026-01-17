using Microsoft.Extensions.Logging;
using Moq;
using SunnyRewards.Helios.Admin.Api.Controllers;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaskAlias = System.Threading.Tasks.Task;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using Xunit;
using SunnyRewards.Helios.Admin.Core.Domain.Constants;

namespace SunnyRewards.Helios.Admin.UnitTest.Controllers
{
    public class TriviaControllerUnitTest
    {
        private readonly Mock<ILogger<TriviaController>> _controllerLogger;
        private readonly Mock<ILogger<TriviaService>> _TriviaServiceLogger;
        private readonly Mock<ITaskClient> _taskClient;
        private readonly ITriviaService _triviaService;
        private readonly TriviaController _triviaController;

        public TriviaControllerUnitTest()
        {
            // Initialize mocks
            _controllerLogger = new Mock<ILogger<TriviaController>>();
            _TriviaServiceLogger = new Mock<ILogger<TriviaService>>();
            _taskClient = new Mock<ITaskClient>();
            _triviaService = new TriviaService(_TriviaServiceLogger.Object, _taskClient.Object);

            _triviaController = new TriviaController(_controllerLogger.Object, _triviaService);
        }
        [Fact]
        public async TaskAlias CreateTrivia_ShouldReturnOkResult()
        {
            var requestDto = CreateTriviaRequestMockDto();

            // Arrange
            _taskClient.Setup(x => x.Post<BaseResponseDto>(Constant.CreateTriviaRequest, It.IsAny<TriviaRequestDto>()))
                .ReturnsAsync(new BaseResponseDto() { ErrorCode = null });
            // Act
            var result = await _triviaController.CreateTrivia(requestDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Fact]
        public async TaskAlias CreateTrivia_ShouldReturnErrorResult_WhenCreateTaskReturnsError()
        {
            // Arrange
            var requestDto = CreateTriviaRequestMockDto();

            // Arrange
            _taskClient.Setup(x => x.Post<BaseResponseDto>(Constant.CreateTriviaRequest, It.IsAny<TriviaRequestDto>()))
                .ReturnsAsync(new BaseResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError });

            // Act
            var result = await _triviaController.CreateTrivia(requestDto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }
        [Fact]
        public async TaskAlias CreateTrivia_ShouldThrow_Exception_ErrorResult_WhenCreateTaskReturnsException()
        {
            // Arrange
            var requestDto = CreateTriviaRequestMockDto();

            // Arrange
            _taskClient.Setup(x => x.Post<BaseResponseDto>(Constant.CreateTriviaRequest, It.IsAny<TriviaRequestDto>()))
                 .ThrowsAsync(new Exception("An error occurred while Creating the task."));

            // Act
            var result = await _triviaController.CreateTrivia(requestDto);
            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        [Fact]
        public async TaskAlias GetAllTrivia_ReturnsOk_WhenServiceReturnsSuccess()
        {
            // Arrange
            var expectedResponse = new TriviaResponseDto
            {
                TriviaList = new List<TriviaDataDto> { new TriviaDataDto() },
                ErrorCode = null
            };
            _taskClient.Setup(x => x.Get<TriviaResponseDto>(Constant.GetAllTriviaAPIUrl, It.IsAny<Dictionary<string, long>> ()))
                 .ReturnsAsync(expectedResponse);

            // Act
            var result = await _triviaController.GetAllTrivia();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
            Assert.Equal(expectedResponse, okResult.Value);
        }

        [Fact]
        public async TaskAlias GetAllTrivia_ReturnsError_WhenServiceReturnsErrorCode()
        {
            // Arrange
            var expectedResponse = new TriviaResponseDto
            {
                ErrorCode = StatusCodes.Status400BadRequest,
                ErrorMessage = "Error occurred"
            };
            _taskClient.Setup(x => x.Get<TriviaResponseDto>(Constant.GetAllTriviaAPIUrl, It.IsAny<Dictionary<string, long>>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _triviaController.GetAllTrivia();

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status400BadRequest, statusCodeResult.StatusCode);
            Assert.Equal(expectedResponse, statusCodeResult.Value);
        }

        [Fact]
        public async TaskAlias GetAllTrivia_ReturnsInternalServerError_OnException()
        {
            // Arrange
            _taskClient.Setup(x => x.Get<TriviaResponseDto>(Constant.GetAllTriviaAPIUrl, It.IsAny<Dictionary<string, long>>()))
                .ThrowsAsync(new Exception("Something went wrong"));

            // Act
            var result = await _triviaController.GetAllTrivia();

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);

            var response = Assert.IsType<TriviaResponseDto>(statusCodeResult.Value);
            Assert.Equal(StatusCodes.Status500InternalServerError, response.ErrorCode);
            Assert.Equal("An unexpected error occurred while retrieving trivia data. Please try again later.", response.ErrorMessage);
        }

        private static TriviaRequestDto CreateTriviaRequestMockDto()
        {
            return new TriviaRequestDto
            {
               
                TaskRewardCode = "tsk-90399d4b7682458cbc9a93206967",
                trivia= new Trivia
                {
                    TriviaId=1,
                    CreateUser = "per-915325069cdb42c783dd4601e1d27704",
                     CtaTaskExternalCode="test-123",
                      ConfigJson="",
                      TriviaCode="code-234"
                }
            };
        }
      
       

    }
}
