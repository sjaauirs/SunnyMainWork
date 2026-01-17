using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SunnyRewards.Helios.Common.Core.Repositories;
using SunnyRewards.Helios.Task.Api.Controller;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Models;
using SunnyRewards.Helios.Task.Infrastructure.Repositories;
using SunnyRewards.Helios.Task.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Task.Infrastructure.Services;
using SunnyRewards.Helios.Task.UnitTest.Fixtures.MockDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using TaskAlias = System.Threading.Tasks.Task;

namespace SunnyRewards.Helios.Task.UnitTest.Controllers
{
    public class TaskRewardCollectionControllerUnitTests
    {
        private readonly TaskRewardCollectionService _service;
        private readonly Mock<ILogger<TaskRewardCollectionService>> _serviceLogger;
        private readonly Mock<ILogger<TaskRewardCollectionController>> _controllerLogger;
        private readonly TaskRewardCollectionController _controller;
        private readonly Mock<ITaskRewardCollectionRepo> _taskRewardCollectionRepo;
        public TaskRewardCollectionControllerUnitTests()
        {
            _taskRewardCollectionRepo = new Mock<ITaskRewardCollectionRepo>();  
            _serviceLogger = new Mock<ILogger<TaskRewardCollectionService>>();
            _controllerLogger = new Mock<ILogger<TaskRewardCollectionController>>();
            _service = new TaskRewardCollectionService(_taskRewardCollectionRepo.Object, _serviceLogger.Object);
            _controller = new TaskRewardCollectionController(_controllerLogger.Object, _service);
        }

        [Fact]
        public async TaskAlias ExportTaskRewardCollection_Should_Return_Ok_Response()
        {
            // Arrange 
            var requestDto = new ExportTaskRewardCollectionRequestDto();
            var taskRewardCollections = new ExportTaskRewardCollectionResponseDto()
            {
                TaskRewardCollections = new List<ExportTaskRewardCollectionDto>() { new ExportTaskRewardCollectionMockDto() },
            };
            _taskRewardCollectionRepo.Setup(x => x.GetTaskRewardCollections(It.IsAny<string>())).ReturnsAsync(taskRewardCollections);

            // Act 
            var response = await _controller.ExportTaskRewardCollection(requestDto);

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(response);
            Assert.NotNull(response);
            Assert.Equal(StatusCodes.Status200OK, objectResult.StatusCode);

        }

        [Fact]
        public async TaskAlias ExportTaskRewardCollection_Should_Return_NotFound_Response()
        {
            // Arrange 
            var requestDto = new ExportTaskRewardCollectionRequestDto();
            var taskRewardCollections = new ExportTaskRewardCollectionResponseDto()
            {
                TaskRewardCollections = new List<ExportTaskRewardCollectionDto>()
            };
            _taskRewardCollectionRepo.Setup(x => x.GetTaskRewardCollections(It.IsAny<string>())).ReturnsAsync(taskRewardCollections);

            // Act 
            var response = await _controller.ExportTaskRewardCollection(requestDto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(response);
            Assert.NotNull(response);
            Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);

        }
        [Fact]
        public async TaskAlias ExportTaskRewardCollection_Should_Throw_Exception()
        {
            // Arrange 
            var requestDto = new ExportTaskRewardCollectionRequestDto();
            _taskRewardCollectionRepo.Setup(x => x.GetTaskRewardCollections(It.IsAny<string>())).ThrowsAsync(new Exception());

            // Act 
            var response = await _controller.ExportTaskRewardCollection(requestDto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(response);
            Assert.NotNull(response);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);

        }
        [Fact]
        public async TaskAlias GetTaskRewardCollections_ReturnsCorrectData()
        {
            // Arrange
            var mockSession = new Mock<NHibernate.ISession>();
            var logger = new Mock<ILogger<BaseRepo<TaskRewardCollectionModel>>>();
            var parentReward = new TaskRewardModel
            {
                TaskRewardId = 1,
                TaskRewardCode = "PARENT-CODE",
                TenantCode = "TEST_TENANT",
                IsCollection = true,
                DeleteNbr = 0
            };

            var childReward = new TaskRewardModel
            {
                TaskRewardId = 2,
                TaskRewardCode = "CHILD-CODE",
                TenantCode = "TEST_TENANT",
                DeleteNbr = 0
            };

            var collectionModel = new TaskRewardCollectionModel
            {
                TaskRewardCollectionId = 100,
                ParentTaskRewardId = 1,
                ChildTaskRewardId = 2,
                UniqueChildCode = "UNIQUE-CODE",
                ConfigJson = "{\"config\":\"value\"}",
                DeleteNbr = 0
            };

            var parentQuery = new List<TaskRewardModel> { parentReward }.AsQueryable();
            var childQuery = new List<TaskRewardModel> { childReward }.AsQueryable();
            var collectionQuery = new List<TaskRewardCollectionModel> { collectionModel }.AsQueryable();

            mockSession.Setup(s => s.Query<TaskRewardModel>()).Returns(parentQuery.Concat(childQuery).AsQueryable());
            mockSession.Setup(s => s.Query<TaskRewardCollectionModel>()).Returns(collectionQuery);

            var service = new TaskRewardCollectionRepo(logger.Object, mockSession.Object);

            // Act
            var result = await service.GetTaskRewardCollections("tenant-code");

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.TaskRewardCollections);
        }



    }
}
