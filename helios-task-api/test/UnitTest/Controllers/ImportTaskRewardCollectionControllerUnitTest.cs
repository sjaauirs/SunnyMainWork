using AutoMapper;
using Azure;
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SunnyRewards.Helios.Task.UnitTest.Controllers
{
    public class ImportTaskRewardCollectionControllerUnitTest
    {
        private readonly Mock<ILogger<ImportTaskRewardCollectionController>> _mockLogger;
        private readonly Mock<ILogger<ImportTaskRewardCollectionService>> _mockServiceLogger;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ITaskRewardRepo> _mockTaskRewardRepo;
        private readonly Mock<ITaskRewardCollectionRepo> _mockTaskRewardCollectionRepo;
        private readonly ImportTaskRewardCollectionService _importTaskRewardCollectionService;
        private readonly ImportTaskRewardCollectionController _controller;

        public ImportTaskRewardCollectionControllerUnitTest()
        {
            _mockLogger = new Mock<ILogger<ImportTaskRewardCollectionController>>();
            _mockServiceLogger = new Mock<ILogger<ImportTaskRewardCollectionService>>();
            _mockMapper = new Mock<IMapper>();
            _mockTaskRewardRepo = new Mock<ITaskRewardRepo>();
            _mockTaskRewardCollectionRepo = new Mock<ITaskRewardCollectionRepo>();
            _importTaskRewardCollectionService = new ImportTaskRewardCollectionService(_mockServiceLogger.Object, _mockTaskRewardRepo.Object
                ,_mockTaskRewardCollectionRepo.Object,_mockMapper.Object);
            _controller = new ImportTaskRewardCollectionController(_mockLogger.Object, _importTaskRewardCollectionService);

        }

        [Fact]
        public async void ImportTask_ShouldReturnSuccessWhenTaskRewardCollectionIsCreated()
        {
            // Arrange
            var requestDto = new ImportTaskRewardCollectionRequestDto
            {
                TaskRewardCollections = new List<ExportTaskRewardCollectionDto>
                {
                    new ExportTaskRewardCollectionDto
                    {
                        ChildTaskRewardCode = "child-Reward-code",
                        ParentTaskRewardCode = "parent-Reward-code",
                        ChildTaskRewardId = 101,
                        ParentTaskRewardId = 1,
                        ConfigJson = "{}",
                        TaskRewardCollectionId = 1,
                        UniqueChildCode = "unique-child-code"
                    }
                }

            };

            var parentTaskReward = new TaskRewardModel
            {
                TaskRewardId = 1,
                TaskRewardCode = "parent-Reward-code",
                IsCollection = true,
                DeleteNbr = 0
            };

            var childTaskReward = new TaskRewardModel
            {
                TaskRewardId = 101,
                TaskRewardCode = "child-Reward-code",
                DeleteNbr = 0
            };

            var mappedModel = new TaskRewardCollectionModel
            {
                ParentTaskRewardId = parentTaskReward.TaskRewardId,
                ChildTaskRewardId = childTaskReward.TaskRewardId,
                UniqueChildCode = "unique-child-code",
                ConfigJson = "{}"
            };

            _mockTaskRewardRepo.SetupSequence(repo =>
                repo.FindOneAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false))
                .ReturnsAsync(parentTaskReward)
                .ReturnsAsync(childTaskReward);

            // Setup Mapper
            _mockMapper
                .Setup(m => m.Map<TaskRewardCollectionModel>(It.IsAny<TaskRewardCollectionDto>()))
                .Returns(mappedModel);

            // Setup CreateAsync
            _mockTaskRewardCollectionRepo
                .Setup(repo => repo.CreateAsync(It.IsAny<TaskRewardCollectionModel>()))
                .ReturnsAsync(new TaskRewardCollectionModel { TaskRewardCollectionId = 1});
            // Act
            var result = await _controller.ImportTaskRewardCollection(requestDto);

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status200OK, objectResult.StatusCode);
        }

        [Fact]
        public async void ImportTask_ShouldReturnSuccess_When_ParentOrChildRewardNotThere()
        {
            // Arrange
            var requestDto = new ImportTaskRewardCollectionRequestDto
            {
                TaskRewardCollections = new List<ExportTaskRewardCollectionDto>
                {
                    new ExportTaskRewardCollectionDto
                    {
                        ChildTaskRewardCode = "child-Reward-code",
                        ParentTaskRewardCode = "parent-Reward-code",
                        ChildTaskRewardId = 101,
                        ParentTaskRewardId = 1,
                        ConfigJson = "{}",
                        TaskRewardCollectionId = 1,
                        UniqueChildCode = "unique-child-code"
                    }
                }

            };

            // Act
            var result = await _controller.ImportTaskRewardCollection(requestDto);

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status200OK, objectResult.StatusCode);
        }
        [Fact]
        public async void ImportTask_ShouldReturnSuccessWhenTaskRewardCollectionIsUpdated()
        {
            // Arrange
            var requestDto = new ImportTaskRewardCollectionRequestDto
            {
                TaskRewardCollections = new List<ExportTaskRewardCollectionDto>
                {
                    new ExportTaskRewardCollectionDto
                    {
                        ChildTaskRewardCode = "child-Reward-code",
                        ParentTaskRewardCode = "parent-Reward-code",
                        ChildTaskRewardId = 101,
                        ParentTaskRewardId = 1,
                        ConfigJson = "{}",
                        TaskRewardCollectionId = 1,
                        UniqueChildCode = "unique-child-code"
                    }
                }

            };

            var parentTaskReward = new TaskRewardModel
            {
                TaskRewardId = 1,
                TaskRewardCode = "parent-Reward-code",
                IsCollection = true,
                DeleteNbr = 0
            };

            var childTaskReward = new TaskRewardModel
            {
                TaskRewardId = 101,
                TaskRewardCode = "child-Reward-code",
                DeleteNbr = 0
            };

            var mappedModel = new TaskRewardCollectionModel
            {
                ParentTaskRewardId = parentTaskReward.TaskRewardId,
                ChildTaskRewardId = childTaskReward.TaskRewardId,
                UniqueChildCode = "unique-child-code",
                ConfigJson = "{}"
            };

            _mockTaskRewardRepo.SetupSequence(repo =>
                repo.FindOneAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false))
                .ReturnsAsync(parentTaskReward)
                .ReturnsAsync(childTaskReward);
            _mockTaskRewardCollectionRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<TaskRewardCollectionModel, bool>>>(), false))
                .ReturnsAsync(new TaskRewardCollectionModel { TaskRewardCollectionId = 1});

            // Setup Mapper
            _mockMapper
                .Setup(m => m.Map<TaskRewardCollectionModel>(It.IsAny<TaskRewardCollectionDto>()))
                .Returns(mappedModel);

            // Setup CreateAsync
            _mockTaskRewardCollectionRepo
                .Setup(repo => repo.UpdateAsync(It.IsAny<TaskRewardCollectionModel>()))
                .ReturnsAsync(new TaskRewardCollectionModel { TaskRewardCollectionId = 1 });
            // Act
            var result = await _controller.ImportTaskRewardCollection(requestDto);

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status200OK, objectResult.StatusCode);
        }


    }
}
