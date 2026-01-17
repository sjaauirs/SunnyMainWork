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
using SunnyRewards.Helios.Task.Core.Domain.Models;

namespace SunnyRewards.Helios.Admin.UnitTest.Controllers
{
    public class TriviaQuestionControllerUnitTest
    {
        private readonly Mock<ILogger<TriviaQuestionController>> _controllerLogger;
        private readonly Mock<ILogger<TriviaQuestionService>> _TriviaServiceLogger;
        private readonly Mock<ITaskClient> _taskClient;
        private readonly ITriviaQuestionService _triviaService;
        private readonly TriviaQuestionController _triviaController;

        public TriviaQuestionControllerUnitTest()
        {
            // Initialize mocks
            _controllerLogger = new Mock<ILogger<TriviaQuestionController>>();
            _TriviaServiceLogger = new Mock<ILogger<TriviaQuestionService>>();
            _taskClient = new Mock<ITaskClient>();
            _triviaService = new TriviaQuestionService(_TriviaServiceLogger.Object, _taskClient.Object);

            _triviaController = new TriviaQuestionController(_controllerLogger.Object, _triviaService);
        }
        [Fact]
        public async TaskAlias CreateTriviaQuestion_ShouldReturnOkResult()
        {
            var requestDto = CreateTriviaQuestionMockDto();

            // Arrange
            _taskClient.Setup(x => x.Post<BaseResponseDto>(Constant.CreateTriviaQuestionRequest, It.IsAny<TriviaQuestionRequestDto>()))
                .ReturnsAsync(new BaseResponseDto() { ErrorCode = null });
            // Act
            var result = await _triviaController.CreateTriviaQuestion(requestDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Fact]
        public async TaskAlias CreateTriviaQuestion_ShouldReturnErrorResult_WhenCreateTaskReturnsError()
        {
            // Arrange
            var requestDto = CreateTriviaQuestionMockDto();

            // Arrange
            _taskClient.Setup(x => x.Post<BaseResponseDto>(Constant.CreateTriviaQuestionRequest, It.IsAny<TriviaQuestionRequestDto>()))
                .ReturnsAsync(new BaseResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError });

            // Act
            var result = await _triviaController.CreateTriviaQuestion(requestDto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }
        [Fact]
        public async TaskAlias CreateTriviaQuestion_ShouldThrow_Exception_ErrorResult_WhenCreateTaskReturnsException()
        {
            // Arrange
            var requestDto = CreateTriviaQuestionMockDto();

            // Arrange
            _taskClient.Setup(x => x.Post<BaseResponseDto>(Constant.CreateTriviaQuestionRequest, It.IsAny<TriviaQuestionRequestDto>()))
                .ThrowsAsync(new Exception("An error occurred while Creating the task."));

            // Act
            var result = await _triviaController.CreateTriviaQuestion(requestDto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }


        [Fact]
        public async TaskAlias GetAllTriviaQuestions_ReturnsOkResult_WithTriviaQuestions()
        {
            var languageCode = "en-US";

            // Arrange
            var triviaQuestions = new List<TriviaQuestionData>
                {
                    new TriviaQuestionData
                    {
                        TriviaQuestionId = 1,
                        TriviaQuestionCode = "Q123",
                        TriviaJson = "{}",
                        QuestionExternalCode = "EXT123"
                    }
                };
            var mockResponse = new TriviaQuestionResponseDto
            {
                TriviaQuestions = triviaQuestions
            };
           
            _taskClient.Setup(x => x.Get<TriviaQuestionResponseDto>(Constant.TriviaQuestionsAPIUrl, It.IsAny<Dictionary<string, long>>()))
                .ReturnsAsync(mockResponse);

            // Act
            var result = await _triviaController.GetAllTriviaQuestions(languageCode);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<TriviaQuestionResponseDto>(okResult.Value);
            Assert.NotNull(response.TriviaQuestions);
            Assert.Single(response.TriviaQuestions);
        }

        [Fact]
        public async TaskAlias GetAllTriviaQuestions_ReturnsInternalServerError_OnException()
        {
            var languageCode = "en-US";
            // Arrange
            _taskClient.Setup(x => x.Get<TriviaQuestionResponseDto>(Constant.TriviaQuestionsAPIUrl, It.IsAny<Dictionary<string, long>>()))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _triviaController.GetAllTriviaQuestions(languageCode);

            // Assert
            var statusResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusResult.StatusCode);
        }

        [Fact]
        public async TaskAlias UpdateTriviaQuestion_ReturnsOkResult_OnSuccessfulUpdate()
        {
            // Arrange
            var mockRequest = new TriviaQuestionData
            {
                TriviaQuestionId = 1,
                TriviaQuestionCode = "Q123",
                TriviaJson = "{}",
                QuestionExternalCode = "EXT123"
            };
          

            _taskClient.Setup(x => x.Put<TriviaQuestionUpdateResponseDto>($"{Constant.TriviaQuestionsAPIUrl}/Q123", mockRequest))
                .ReturnsAsync(new TriviaQuestionUpdateResponseDto()
                {
                    IsSuccess = true
                });

            // Act
            var result = await _triviaController.UpdateTriviaQuestion("Q123", mockRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Fact]
        public async TaskAlias UpdateTriviaQuestion_ReturnsBadRequest_IfCodeMismatch()
        {
            // Arrange
            var mockRequest = new TriviaQuestionData
            {
                TriviaQuestionId = 1,
                TriviaQuestionCode = "Q456",
                TriviaJson = "{}",
                QuestionExternalCode = "EXT123"
            };

            _taskClient.Setup(x => x.Put<TriviaQuestionUpdateResponseDto>(It.IsAny<string>(), mockRequest))
               .ReturnsAsync(new TriviaQuestionUpdateResponseDto()
               {
                   IsSuccess = false,
                   ErrorCode = StatusCodes.Status400BadRequest
               });

            // Act
            var result = await _triviaController.UpdateTriviaQuestion("Q123", mockRequest);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status400BadRequest, statusCodeResult.StatusCode);
        }

        [Fact]
        public async TaskAlias UpdateTriviaQuestion_ReturnsInternalServerError_OnException()
        {
            // Arrange
            var mockRequest = new TriviaQuestionData
            {
                TriviaQuestionId = 1,
                TriviaQuestionCode = "Q123",
                TriviaJson = "{}",
                QuestionExternalCode = "EXT123"
            };

            _taskClient.Setup(x => x.Put<TriviaQuestionUpdateResponseDto>(It.IsAny<string>(), mockRequest))
               .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _triviaController.UpdateTriviaQuestion("Q123", mockRequest);

            // Assert
            var statusResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusResult.StatusCode);
        }

        [Fact]
        public async TaskAlias UpdateTriviaQuestion_ReturnsInternalServerError_WhenQuestionNotFound()
        {
            // Arrange
            var mockRequest = new TriviaQuestionData
            {
                TriviaQuestionId = 1,
                TriviaQuestionCode = "Q123",
                TriviaJson = "{}",
                QuestionExternalCode = "EXT123"
            };

            _taskClient.Setup(x => x.Put<TriviaQuestionUpdateResponseDto>(It.IsAny<string>(), mockRequest))
              .ReturnsAsync(new TriviaQuestionUpdateResponseDto()
              {
                  IsSuccess = false,
                  ErrorCode = StatusCodes.Status404NotFound
              });

            // Act
            var result = await _triviaController.UpdateTriviaQuestion("Q123", mockRequest);

            // Assert
            var statusResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status404NotFound, statusResult.StatusCode);
        }

        private static TriviaQuestionRequestDto CreateTriviaQuestionMockDto()
        {
            return new TriviaQuestionRequestDto
            {
                TriviaQuestionCode = "tsk-90399d4b7682458cbc9a93206967",
                QuestionExternalCode = "code-233",
                CreateUser = "per-915325069cdb42c783dd4601e1d27704"
            };
        }
    }
}
