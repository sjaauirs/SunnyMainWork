using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SunnyRewards.Helios.Admin.Api.Controllers;
using SunnyRewards.Helios.Admin.Core.Domain.Constants;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Models;
using Xunit;
using TaskAlias = System.Threading.Tasks.Task;


namespace SunnyRewards.Helios.Admin.UnitTest.Controllers
{
    public class TriviaQuestionGroupControllerUnitTest
    {
        private readonly Mock<ILogger<TriviaQuestionGroupController>> _controllerLogger;
        private readonly Mock<ILogger<TriviaQuestionGroupService>> _TriviaServiceLogger;
        private readonly Mock<ITaskClient> _taskClient;
        private readonly ITriviaQuestionGroupService _triviaService;
        private readonly TriviaQuestionGroupController _triviaController;

        public TriviaQuestionGroupControllerUnitTest()
        {
            // Initialize mocks
            _controllerLogger = new Mock<ILogger<TriviaQuestionGroupController>>();
            _TriviaServiceLogger = new Mock<ILogger<TriviaQuestionGroupService>>();
            _taskClient = new Mock<ITaskClient>();
            _triviaService = new TriviaQuestionGroupService(_TriviaServiceLogger.Object, _taskClient.Object);

            _triviaController = new TriviaQuestionGroupController(_controllerLogger.Object, _triviaService);
        }
        [Fact]
        public async TaskAlias CreateTriviaQuestionGroup_ShouldReturnOkResult()
        {
            var requestDto = CreateTriviaQuestionGroupMockDto();

            // Arrange
            _taskClient.Setup(x => x.Post<BaseResponseDto>(Constant.CreateTriviaQuestionGroupRequest, It.IsAny<TriviaQuestionGroupRequestDto>()))
                .ReturnsAsync(new BaseResponseDto() { ErrorCode = null });
            // Act
            var result = await _triviaController.CreateTriviaQuestionGroup(requestDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Fact]
        public async TaskAlias CreateTriviaQuestionGroup_ShouldReturnErrorResult_WhenCreateTaskReturnsError()
        {
            // Arrange
            var requestDto = CreateTriviaQuestionGroupMockDto();

            // Arrange
            _taskClient.Setup(x => x.Post<BaseResponseDto>(Constant.CreateTriviaQuestionGroupRequest, It.IsAny<TriviaQuestionGroupRequestDto>()))
                .ReturnsAsync(new BaseResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError });

            // Act
            var result = await _triviaController.CreateTriviaQuestionGroup(requestDto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }
        [Fact]
        public async TaskAlias CreateTriviaQuestionGroup_ShouldThrow_Exception_ErrorResult_WhenCreateTaskReturnsException()
        {
            // Arrange
            var requestDto = CreateTriviaQuestionGroupMockDto();

            // Arrange
            _taskClient.Setup(x => x.Post<BaseResponseDto>(Constant.CreateTriviaQuestionGroupRequest, It.IsAny<TriviaQuestionGroupRequestDto>()))
                .ThrowsAsync(new Exception("An error occurred while Creating the task."));

            // Act
            var result = await _triviaController.CreateTriviaQuestionGroup(requestDto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
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
            
            _taskClient.Setup(x => x.GetById<TriviaQuestionGroupResponseDto>(Constant.TriviaQuestionGroupsAPIUrl + "/", triviaId))
                .ReturnsAsync(new TriviaQuestionGroupResponseDto() { TriviaQuestionGroupList = triviaQuestionGroupList });

            // Act
            var result = await _triviaController.GetTriviaQuestionGroupsByTriviaId(triviaId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Fact]
        public async TaskAlias GetTriviaQuestionGroupsByTriviaId_ReturnsInternalServerError_WhenExceptionOccurs()
        {
            // Arrange
            long triviaId = 1;
            _taskClient.Setup(x => x.GetById<TriviaQuestionGroupResponseDto>(Constant.TriviaQuestionGroupsAPIUrl + "/", triviaId))
                .ThrowsAsync(new Exception("Test Exception"));

            // Act
            var result = await _triviaController.GetTriviaQuestionGroupsByTriviaId(triviaId);

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
            _taskClient.Setup(x => x.Put<TriviaQuestionGroupUpdateResponseDto>($"{Constant.TriviaQuestionGroupsAPIUrl}/{triviaQuestionGroupId.ToString()}", updateRequest))
               .ReturnsAsync(mockResponse);
            // Act
            var result = await _triviaController.UpdateTriviaQuestionGroup(triviaQuestionGroupId, updateRequest);

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
            _taskClient.Setup(x => x.Put<TriviaQuestionGroupUpdateResponseDto>($"{Constant.TriviaQuestionGroupsAPIUrl}/{triviaQuestionGroupId.ToString()}", updateRequest))
              .ReturnsAsync(new TriviaQuestionGroupUpdateResponseDto()
              {
                  ErrorCode = StatusCodes.Status400BadRequest
              });

            // Act
            var result = await _triviaController.UpdateTriviaQuestionGroup(triviaQuestionGroupId, updateRequest);

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


            _taskClient.Setup(x => x.Put<TriviaQuestionGroupUpdateResponseDto>($"{Constant.TriviaQuestionGroupsAPIUrl}/{triviaQuestionGroupId.ToString()}", updateRequest))
              .ThrowsAsync(new Exception("Test Exception"));

            // Act
            var result = await _triviaController.UpdateTriviaQuestionGroup(triviaQuestionGroupId, updateRequest);

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

            _taskClient.Setup(x => x.Delete<BaseResponseDto>($"{Constant.TriviaQuestionGroupsAPIUrl}/{triviaQuestionGroupId.ToString()}"))
              .ReturnsAsync(new BaseResponseDto());

            // Act
            var result = await _triviaController.DeleteTriviaQuestionGroup(triviaQuestionGroupId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

        }

        [Fact]
        public async TaskAlias DeleteTriviaQuestionGroup_ReturnsNotFound_WhenTriviaQuestionGroupNotFound()
        {
            // Arrange
            long triviaQuestionGroupId = 1;

            _taskClient.Setup(x => x.Delete<BaseResponseDto>($"{Constant.TriviaQuestionGroupsAPIUrl}/{triviaQuestionGroupId.ToString()}"))
              .ReturnsAsync(new BaseResponseDto()
              {
                  ErrorCode = StatusCodes.Status404NotFound
              });

            // Act
            var result = await _triviaController.DeleteTriviaQuestionGroup(triviaQuestionGroupId);

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

            _taskClient.Setup(x => x.Delete<BaseResponseDto>($"{Constant.TriviaQuestionGroupsAPIUrl}/{triviaQuestionGroupId.ToString()}"))
              .ThrowsAsync(new Exception("Test Exception"));

            // Act
            var result = await _triviaController.DeleteTriviaQuestionGroup(triviaQuestionGroupId);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);

            var response = Assert.IsType<BaseResponseDto>(statusCodeResult.Value);
            Assert.Equal(StatusCodes.Status500InternalServerError, response.ErrorCode);
            Assert.Equal("An unexpected error occurred while deleting the trivia question group.", response.ErrorMessage);
        }

        #endregion

        private static TriviaQuestionGroupRequestDto CreateTriviaQuestionGroupMockDto()
        {
            return new TriviaQuestionGroupRequestDto
            {

                TriviaQuestionCode = "qsk-90399d4b7682458cbc9a93206967",
                TriviaCode = "tsk-90399d4b7682458cbc9a93206967",
                TriviaQuestionGroup = new TriviaQuestionGroupPostRequestDto { SequenceNbr = 1, ValidEndTs = DateTime.UtcNow.AddDays(3), ValidStartTs = DateTime.Now, CreateUser = "SYSTEM" },
            };
        }
    }
}
