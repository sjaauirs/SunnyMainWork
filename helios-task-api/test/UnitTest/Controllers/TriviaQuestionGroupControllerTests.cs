using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
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


    public class TriviaQuestionGroupControllerTests
    {
        private readonly Mock<ILogger<TriviaQuestionGroupController>> _mockLogger;
        private readonly Mock<ILogger<TriviaQuestionGroupService>> _triviaServiceLogger;
        private readonly Mock<ITriviaQuestionGroupRepo> _triviaQuestionGroupRepo;
        private readonly TriviaQuestionGroupService _triviaQuestionGroupService;
        private readonly TriviaQuestionGroupController _controller;
        private readonly Mock<IMapper> _mapper;

        public TriviaQuestionGroupControllerTests()
        {
            _triviaQuestionGroupRepo = new TriviaQuestionGroupMockRepo();
            _mockLogger = new Mock<ILogger<TriviaQuestionGroupController>>();
            _triviaServiceLogger = new Mock<ILogger<TriviaQuestionGroupService>>();
            _mapper = new Mock<IMapper>();
            _triviaQuestionGroupService = new TriviaQuestionGroupService(_triviaQuestionGroupRepo.Object, _mapper.Object, _triviaServiceLogger.Object);
            _controller = new TriviaQuestionGroupController(_triviaQuestionGroupService, _mockLogger.Object);
        }

        #region GetTriviaQuestionGroupsByTriviaId Tests

        [Fact]
        public async TaskAlias GetTriviaQuestionGroupsByTriviaId_ReturnsOkResult_WhenServiceReturnsValidResponse()
        {
            // Arrange
            long triviaId = 1;
            var triviaQuestionGroupList = new List<TriviaQuestionGroupDto>
            {
                new TriviaQuestionGroupDto { TriviaQuestionGroupId = 1, TriviaId = triviaId, TriviaQuestionId = 100, SequenceNbr = 1, ValidStartTs = DateTime.Now, ValidEndTs = DateTime.Now.AddDays(1) }
            };

            _triviaQuestionGroupRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<TriviaQuestionGroupModel, bool>>>(), false)).ReturnsAsync(new List<TriviaQuestionGroupModel>
            {
                new TriviaQuestionGroupModel() { TriviaQuestionGroupId = 1, TriviaId = triviaId, TriviaQuestionId = 100, SequenceNbr = 1, ValidStartTs = DateTime.Now, ValidEndTs = DateTime.Now.AddDays(1) }
            });
            _mapper.Setup(x => x.Map<IList<TriviaQuestionGroupDto>>(It.IsAny<IList<TriviaQuestionGroupModel>>()))
                .Returns(triviaQuestionGroupList);

            // Act
            var result = await _controller.GetTriviaQuestionGroupsByTriviaId(triviaId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Fact]
        public async TaskAlias GetTriviaQuestionGroupsByTriviaId_ReturnsInternalServerError_WhenExceptionOccurs()
        {
            // Arrange
            long triviaId = 1;

            var _mockTriviaService = new Mock<ITriviaQuestionGroupService>();
            var _controller = new TriviaQuestionGroupController(_mockTriviaService.Object, _mockLogger.Object);
            _mockTriviaService.Setup(service => service.GetTriviaQuestionGroupsByTriviaId(triviaId))
              .ThrowsAsync(new Exception("Test Exception"));

            // Act
            var result = await _controller.GetTriviaQuestionGroupsByTriviaId(triviaId);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);

            var response = Assert.IsType<TriviaQuestionGroupResponseDto>(statusCodeResult.Value);
            Assert.Equal(StatusCodes.Status500InternalServerError, response.ErrorCode);
        }

        #endregion

        #region UpdateTriviaQuestionGroup Tests

        [Fact]
        public async TaskAlias UpdateTriviaQuestionGroup_ReturnsOkResult_WhenServiceReturnsSuccess()
        {
            // Arrange
            long triviaQuestionGroupId = 1;
            var updateRequest = new TriviaQuestionGroupDto
            {
                TriviaQuestionGroupId = triviaQuestionGroupId,
                TriviaId = 1,
                TriviaQuestionId = 100,
                SequenceNbr = 1,
                ValidStartTs = DateTime.Now,
                ValidEndTs = DateTime.Now.AddDays(1)
            };
            var mockResponse = new TriviaQuestionGroupUpdateResponseDto { IsSuccess = true };
            _triviaQuestionGroupRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TriviaQuestionGroupModel, bool>>>(), false)).ReturnsAsync(
                new TriviaQuestionGroupModel()
                {
                    TriviaQuestionGroupId = triviaQuestionGroupId,
                    TriviaId = 1,
                    TriviaQuestionId = 100,
                    SequenceNbr = 1,
                    ValidStartTs = DateTime.Now,
                    ValidEndTs = DateTime.Now.AddDays(1)
                });

            // Act
            var result = await _controller.UpdateTriviaQuestionGroup(triviaQuestionGroupId, updateRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Fact]
        public async TaskAlias UpdateTriviaQuestionGroup_ReturnsBadRequest_WhenIdsMismatch()
        {
            // Arrange
            long triviaQuestionGroupId = 1;
            var updateRequest = new TriviaQuestionGroupDto
            {
                TriviaQuestionGroupId = 2, // Mismatch with path ID
                TriviaId = 1,
                TriviaQuestionId = 100,
                SequenceNbr = 1,
                ValidStartTs = DateTime.Now,
                ValidEndTs = DateTime.Now.AddDays(1)
            };

            // Act
            var result = await _controller.UpdateTriviaQuestionGroup(triviaQuestionGroupId, updateRequest);

            // Assert
            var badRequestResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);

            var response = Assert.IsType<TriviaQuestionGroupUpdateResponseDto>(badRequestResult.Value);
            Assert.False(response.IsSuccess);
            Assert.Equal(StatusCodes.Status400BadRequest, response.ErrorCode);
        }

        [Fact]
        public async TaskAlias UpdateTriviaQuestionGroup_ReturnsInternalServerError_WhenExceptionOccurs()
        {
            // Arrange
            long triviaQuestionGroupId = 1;
            var updateRequest = new TriviaQuestionGroupDto
            {
                TriviaQuestionGroupId = triviaQuestionGroupId,
                TriviaId = 1,
                TriviaQuestionId = 100,
                SequenceNbr = 1,
                ValidStartTs = DateTime.Now,
                ValidEndTs = DateTime.Now.AddDays(1)
            };

           
            _triviaQuestionGroupRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TriviaQuestionGroupModel, bool>>>(), false))
                .ThrowsAsync(new Exception("Test Exception"));

            // Act
            var result = await _controller.UpdateTriviaQuestionGroup(triviaQuestionGroupId, updateRequest);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);

            var response = Assert.IsType<TriviaQuestionGroupUpdateResponseDto>(statusCodeResult.Value);
            Assert.False(response.IsSuccess);
            Assert.Equal(StatusCodes.Status500InternalServerError, response.ErrorCode);
        }

        #endregion

        #region DeleteTriviaQuestionGroup Tests

        [Fact]
        public async TaskAlias DeleteTriviaQuestionGroup_ReturnsOkResult_WhenServiceReturnsSuccess()
        {
            // Arrange
            long triviaQuestionGroupId = 1;
            

            _triviaQuestionGroupRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TriviaQuestionGroupModel, bool>>>(), false)).ReturnsAsync(
                new TriviaQuestionGroupModel()
                {
                    TriviaQuestionGroupId = triviaQuestionGroupId,
                    TriviaId = 1,
                    TriviaQuestionId = 100,
                    SequenceNbr = 1,
                    ValidStartTs = DateTime.Now,
                    ValidEndTs = DateTime.Now.AddDays(1)
                });

            // Act
            var result = await _controller.DeleteTriviaQuestionGroup(triviaQuestionGroupId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

        }

        [Fact]
        public async TaskAlias DeleteTriviaQuestionGroup_ReturnsNotFound_WhenTriviaQuestionGroupNotFound()
        {
            // Arrange
            long triviaQuestionGroupId = 1;
            
            _triviaQuestionGroupRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TriviaQuestionGroupModel, bool>>>(), false));

            // Act
            var result = await _controller.DeleteTriviaQuestionGroup(triviaQuestionGroupId);

            // Assert
            var notFoundResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);

            var response = Assert.IsType<BaseResponseDto>(notFoundResult.Value);
            Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
        }

        [Fact]
        public async TaskAlias DeleteTriviaQuestionGroup_ReturnsInternalServerError_WhenExceptionOccurs()
        {
            // Arrange
            long triviaQuestionGroupId = 1;

            _triviaQuestionGroupRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TriviaQuestionGroupModel, bool>>>(), false))
                .ThrowsAsync(new Exception("Test Exception"));

            // Act
            var result = await _controller.DeleteTriviaQuestionGroup(triviaQuestionGroupId);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);

            var response = Assert.IsType<BaseResponseDto>(statusCodeResult.Value);
            Assert.Equal(StatusCodes.Status500InternalServerError, response.ErrorCode);
            Assert.Equal("An unexpected error occurred while deleting the trivia question group.", response.ErrorMessage);
        }

        #endregion

    }

}
