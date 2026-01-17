using System;
using System.Threading.Tasks;
using Moq;
using Newtonsoft.Json;
using Xunit;
using SunnyRewards.Helios.Task.Infrastructure.Helpers;
using SunnyRewards.Helios.Task.Infrastructure.Helpers.Interface;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Constants;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using TaskAlias = System.Threading.Tasks.Task;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Task.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Task.Core.Domain.Models;
using System.Linq.Expressions;
using SunnyRewards.Helios.Task.UnitTest.Fixtures.MockRepositories;
using AutoMapper;
namespace SunnyRewards.Helios.Task.UnitTest.Helpers
{
    public class CommonTaskRewardServiceTests
    {
        private readonly CommonTaskRewardService _taskService;
        private readonly Mock<ITaskCommonHelper> _mockTaskCommonHelper;
        private readonly Mock<IConsumerTaskRepo> _mockConsumerTaskRepo;
        private readonly Mock<ILogger<CommonTaskRewardService>> _mockLogger;
        private readonly Mock<IMapper> _mockMapper;

        public CommonTaskRewardServiceTests()
        {
            _mockTaskCommonHelper = new Mock<ITaskCommonHelper>();
            _mockConsumerTaskRepo = new ConsumerTaskMockRepo();
            _mockLogger = new Mock<ILogger<CommonTaskRewardService>>();
            _mockMapper = new Mock<IMapper>();
            _taskService = new CommonTaskRewardService(_mockTaskCommonHelper.Object, _mockConsumerTaskRepo.Object, _mockLogger.Object, _mockMapper.Object);
        }

        [Fact]
        public async TaskAlias RecurrenceTaskProcess_ShouldReturn_WhenRecurrenceDefinitionJsonIsNull()
        {
            // Arrange
            var taskRewardDetailDto = new TaskRewardDetailDto { TaskReward = new TaskRewardDto { RecurrenceDefinitionJson = null } };

            // Act
            await _taskService.RecurrenceTaskProcess(taskRewardDetailDto);

            // Assert
            Assert.Null(taskRewardDetailDto.MinAllowedTaskCompleteTs);
            Assert.Null(taskRewardDetailDto.ComputedTaskExpiryTs);
        }

        [Fact]
        public async TaskAlias RecurrenceTaskProcess_ShouldProcessPeriodicRecurrence()
        {
            // Arrange
            var recurrenceDetails = new RecurringDto
            {
                recurrenceType = Constant.Periodic,
                periodic = new PeriodicDto { periodRestartDate = 1, period = "monthly" }
            };
            var taskRewardDetailDto = new TaskRewardDetailDto
            {
                TaskReward = new TaskRewardDto { RecurrenceDefinitionJson = JsonConvert.SerializeObject(recurrenceDetails) }
            };

            var expectedStartDate = DateTime.UtcNow.AddDays(-5);
            var expectedEndDate = DateTime.UtcNow.AddDays(25);

            _mockTaskCommonHelper
                .Setup(x => x.GetPeriodStartAndEndDatesAsync(It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync((expectedStartDate, expectedEndDate));

            // Act
            await _taskService.RecurrenceTaskProcess(taskRewardDetailDto);

            // Assert
            Assert.Equal(expectedStartDate, taskRewardDetailDto.MinAllowedTaskCompleteTs);
            Assert.Equal(expectedEndDate, taskRewardDetailDto.ComputedTaskExpiryTs);
        }

        [Fact]
        public async TaskAlias RecurrenceTaskProcess_ShouldProcessScheduleRecurrence()
        {
            // Arrange
            var recurrenceDetails = new RecurringDto
            {
                recurrenceType = Constant.Schedule,
                Schedules = new[] { new ScheduleDto { StartDate = DateTime.UtcNow.ToString(), ExpiryDate = DateTime.UtcNow.AddDays(7).ToString() } }
            };
            var taskRewardDetailDto = new TaskRewardDetailDto
            {
                TaskReward = new TaskRewardDto { RecurrenceDefinitionJson = JsonConvert.SerializeObject(recurrenceDetails) }
            };

            var expectedStartDate = DateTime.UtcNow.AddDays(-2);
            var expectedExpiryDate = DateTime.UtcNow.AddDays(5);

            _mockTaskCommonHelper
                .Setup(x => x.FindMatchingScheduleStartDateAndExpiryDateAsync(It.IsAny<ScheduleDto[]>()))
                .ReturnsAsync((expectedStartDate, expectedExpiryDate));

            // Act
            await _taskService.RecurrenceTaskProcess(taskRewardDetailDto);

            // Assert
            Assert.Equal(expectedStartDate, taskRewardDetailDto.MinAllowedTaskCompleteTs);
            Assert.Equal(expectedExpiryDate, taskRewardDetailDto.ComputedTaskExpiryTs);
        }

        [Fact]
        public async TaskAlias RecurrenceTaskProcess_ShouldHandleMinValueStartDate()
        {
            // Arrange
            var recurrenceDetails = new RecurringDto { recurrenceType = Constant.Schedule };
            var taskRewardDetailDto = new TaskRewardDetailDto
            {
                TaskReward = new TaskRewardDto { RecurrenceDefinitionJson = JsonConvert.SerializeObject(recurrenceDetails) },
                ConsumerTask = new ConsumerTaskStatTSDto { TaskStartTs = DateTime.UtcNow.AddDays(-10) }
            };

            _mockTaskCommonHelper
                .Setup(x => x.FindMatchingScheduleStartDateAndExpiryDateAsync(It.IsAny<ScheduleDto[]>()))
                .ReturnsAsync((DateTime.MinValue, DateTime.UtcNow.AddDays(5)));

            // Act
            await _taskService.RecurrenceTaskProcess(taskRewardDetailDto);

            // Assert
            Assert.Equal(taskRewardDetailDto.ConsumerTask.TaskStartTs, taskRewardDetailDto.MinAllowedTaskCompleteTs);
            Assert.NotNull(taskRewardDetailDto.ComputedTaskExpiryTs);
        }

        [Fact]
        public async TaskAlias RecurrenceTaskProcess_ShouldSetMaxAllowedTaskCompletionTs()
        {
            // Arrange
            var recurrenceDetails = new RecurringDto { recurrenceType = Constant.Periodic, periodic = new PeriodicDto { period = "weekly", periodRestartDate = 7 } };
            var taskRewardDetailDto = new TaskRewardDetailDto
            {
                TaskReward = new TaskRewardDto { RecurrenceDefinitionJson = JsonConvert.SerializeObject(recurrenceDetails) }
            };

            _mockTaskCommonHelper
                .Setup(x => x.GetPeriodStartAndEndDatesAsync(It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync((DateTime.UtcNow.AddDays(-7), DateTime.UtcNow.AddDays(7)));

            // Act
            await _taskService.RecurrenceTaskProcess(taskRewardDetailDto);

            // Assert
            Assert.Equal(DateTime.UtcNow.Date, taskRewardDetailDto.TaskReward!.MaxAllowedTaskCompletionTs?.Date);
        }
        [Fact]
        public async TaskAlias GetAvailableTasksAsync_WhenConsumerHasTasks_ReturnsFilteredTasks()
        {
            // Arrange
            var requestDto = new TaskRewardCollectionRequestDto()
            {
                TaskRewardCode = "trw-8a154edc602c49efb210d67a7bfe22b4",
                TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4",
                ConsumerCode ="consumerCode"
            };
            var taskRewardDetailDtos = new List<TaskRewardDetailDto>
            {
                new TaskRewardDetailDto { TaskReward = new TaskRewardDto { TaskId = 1 } },
                new TaskRewardDetailDto { TaskReward = new TaskRewardDto { TaskId = 2 } }
            };

            var filteredTasks = await TaskHelper.FilterAvailableTasksAsync(taskRewardDetailDtos, new List<ConsumerTaskModel>(), _mockConsumerTaskRepo.Object, _mockLogger.Object);

            // Act
            var result = await _taskService.GetAvailableTasksAsync(taskRewardDetailDtos, requestDto);

            // Assert
            Assert.NotNull(result);
            Assert.All(filteredTasks, task => Assert.Contains(task, result));
        }
        [Fact]
        public async TaskAlias GetAvailableTasksAsync_WhenConsumerHasNoTasks_ReturnsFilteredTasks()
        {
            // Arrange
            var requestDto = new TaskRewardCollectionRequestDto()
            {
                TaskRewardCode = "trw-8a154edc602c49efb210d67a7bfe22b4",
                TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4",
                ConsumerCode = "consumerCode",
                IsIncludeCompletedTask = true
            };
            var taskRewardDetailDtos = new List<TaskRewardDetailDto>
            {
                new TaskRewardDetailDto { TaskReward = new TaskRewardDto { TaskId = 1 } },
            };

            var filteredTasks = await TaskHelper.FilterAvailableTasksAsync(taskRewardDetailDtos, new List<ConsumerTaskModel>(), _mockConsumerTaskRepo.Object, _mockLogger.Object);

            // Act
            var result = await _taskService.GetAvailableTasksAsync(taskRewardDetailDtos, requestDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(filteredTasks.Count, result.Count);
            Assert.All(filteredTasks, task => Assert.Contains(task, result));
        }
    }
}