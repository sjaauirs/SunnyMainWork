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
using Xunit;
using TaskAlias = System.Threading.Tasks.Task;


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Admin.UnitTest.Controllers
{
    public class SubSubtaskControllerUnitTest
    {

        private readonly Mock<ILogger<SubtaskController>> _controllerLogger;
        private readonly Mock<ILogger<SubTaskService>> _taskServiceLogger;
        private readonly Mock<ITaskClient> _taskClient;
        private readonly ISubtaskService _taskService;
        private readonly SubtaskController _SubtaskController;

        public SubSubtaskControllerUnitTest()
        {
            // Initialize mocks
            _controllerLogger = new Mock<ILogger<SubtaskController>>();
            _taskServiceLogger = new Mock<ILogger<SubTaskService>>();
            _taskClient = new Mock<ITaskClient>();
            _taskService = new SubTaskService(_taskServiceLogger.Object, _taskClient.Object);

            _SubtaskController = new SubtaskController(_controllerLogger.Object, _taskService);
        }

        [Fact]
        public async TaskAlias CreateSubTask_ShouldReturnOkResult()
        {
            var requestDto = CreateSubTaskMockDto();

            // Arrange
            _taskClient.Setup(x => x.Post<BaseResponseDto>(Constant.CreateSubTaskAPIUrl, It.IsAny<SubtaskRequestDto>()))
                .ReturnsAsync(new BaseResponseDto() { ErrorCode = null });
            // Act
            var result = await _SubtaskController.CreateSubTask(requestDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Fact]
        public async TaskAlias CreateSubTask_ShouldReturnErrorResult_WhenCreateTaskReturnsError()
        {
            // Arrange
            var requestDto = CreateSubTaskMockDto();
            _taskClient.Setup(x => x.Post<BaseResponseDto>(Constant.CreateSubTaskAPIUrl, It.IsAny<SubtaskRequestDto>()))
                .ReturnsAsync(new BaseResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError });

            // Act
            var result = await _SubtaskController.CreateSubTask(requestDto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }
        [Fact]
        public async TaskAlias CreateSubTask_ShouldThrow_Exception_ErrorResult_WhenCreateTaskReturnsException()
        {
            // Arrange
            var requestDto = CreateSubTaskMockDto();

            _taskClient.Setup(x => x.Post<BaseResponseDto>(Constant.CreateSubTaskAPIUrl, It.IsAny<SubtaskRequestDto>()))
                    .ThrowsAsync(new Exception("An error occurred while Creating the task."));

            // Act
            var result = await _SubtaskController.CreateSubTask(requestDto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }
        private static SubtaskRequestDto CreateSubTaskMockDto()
        {
            return new SubtaskRequestDto
            {

                ParentTaskRewardCode = "tsk-90399d4b7682458cbc9a93206967",
                ChildTaskRewardCode = "tsk-90399d4b7682458cbc9a93206967",
                Subtask = new PostSubTaskDto
                {
                    ConfigJson = "",
                    CreateUser = "test"
                }
            };
        }
    }

}
