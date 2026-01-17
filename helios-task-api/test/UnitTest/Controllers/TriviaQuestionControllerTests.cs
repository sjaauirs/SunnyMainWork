using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SunnyRewards.Helios.Task.Api.Controller;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Models;
using SunnyRewards.Helios.Task.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Task.Infrastructure.Services;
using SunnyRewards.Helios.Task.Infrastructure.Services.Interface;
using SunnyRewards.Helios.Task.UnitTest.Fixtures.MockRepositories;
using System.Linq.Expressions;
using Xunit;
using TaskAlias = System.Threading.Tasks.Task;
namespace SunnyRewards.Helios.Task.UnitTest.Controllers
{
    public class TriviaQuestionControllerTests
    {
        private readonly TriviaQuestionService _triviaQuestionService;
        private readonly Mock<ILogger<TriviaQuestionController>> _mockLogger;
        private readonly Mock<ILogger<TriviaQuestionService>> _triviaServiceLogger;
        private readonly Mock<IMapper> _mapper;
        private readonly Mock<ITriviaQuestionRepo> _triviaQuestionRepo;
        private readonly TriviaQuestionController _controller;
        private readonly Mock<ITriviaService> _triviaServiceMock;

        public TriviaQuestionControllerTests()
        {
            _mockLogger = new Mock<ILogger<TriviaQuestionController>>();
            _triviaServiceLogger = new Mock<ILogger<TriviaQuestionService>>();
            _triviaQuestionRepo = new TriviaQuestionMockRepo();
            _mapper = new Mock<IMapper>();
            _triviaServiceMock = new Mock<ITriviaService>();
            _triviaQuestionService = new TriviaQuestionService(_triviaQuestionRepo.Object, _mapper.Object, _triviaServiceLogger.Object, _triviaServiceMock.Object);
            _controller = new TriviaQuestionController(_triviaQuestionService, _mockLogger.Object);
        }

        [Fact]
        public async TaskAlias GetAllTriviaQuestions_ReturnsOkResult_WithTriviaQuestions()
        {
            string languageCode = "en-us";
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
            _triviaQuestionRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<TriviaQuestionModel, bool>>>(), false)).ReturnsAsync(new List<TriviaQuestionModel>
            {
                new TriviaQuestionModel() {
                        TriviaQuestionId = 1,
                        TriviaQuestionCode = "Q123",
                        TriviaJson = "{}",
                        QuestionExternalCode = "EXT123"
                    }
            });
            _mapper.Setup(x => x.Map<IList<TriviaQuestionData>>(It.IsAny<IList<TriviaQuestionModel>>()))
                .Returns(triviaQuestions);

            // Act
            var result = await _controller.GetAllTriviaQuestions(languageCode);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<TriviaQuestionResponseDto>(okResult.Value);
            Assert.NotNull(response.TriviaQuestions);
            Assert.Single(response.TriviaQuestions);
        }

        [Fact]
        public async TaskAlias GetAllTriviaQuestions_ReturnsInternalServerError_OnException()
        {
            // Arrange
            string languageCode = "en-us";
            var _mockTriviaService = new Mock<ITriviaQuestionService>();
            _mockTriviaService
                .Setup(s => s.GetAllTriviaQuestions(languageCode))
                .ThrowsAsync(new Exception("Service Error"));
            var _controller = new TriviaQuestionController(_mockTriviaService.Object, _mockLogger.Object);
            _mockTriviaService.Setup(service => service.GetAllTriviaQuestions(languageCode)).ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _controller.GetAllTriviaQuestions(languageCode);

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
            _triviaQuestionRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TriviaQuestionModel, bool>>>(), false)).ReturnsAsync(
                new TriviaQuestionModel()
                {
                    TriviaQuestionId = 1,
                    TriviaQuestionCode = "Q123",
                    TriviaJson = "{}",
                    QuestionExternalCode = "EXT123"
                }
                );

            // Act
            var result = await _controller.UpdateTriviaQuestion("Q123", mockRequest);

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

            // Act
            var result = await _controller.UpdateTriviaQuestion("Q123", mockRequest);

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

            _triviaQuestionRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TriviaQuestionModel, bool>>>(), false)).ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _controller.UpdateTriviaQuestion("Q123", mockRequest);

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

            _triviaQuestionRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TriviaQuestionModel, bool>>>(), false));

            // Act
            var result = await _controller.UpdateTriviaQuestion("Q123", mockRequest);

            // Assert
            var statusResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status404NotFound, statusResult.StatusCode);
        }
    }
}
