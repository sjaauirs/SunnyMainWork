using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using SunnyRewards.Helios.Common.Core.Domain;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Constants;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Models;
using SunnyRewards.Helios.Task.Infrastructure.Helpers;
using SunnyRewards.Helios.Task.Infrastructure.Repositories.Interfaces;
using Xunit;
using Task1 = System.Threading.Tasks.Task;

namespace SunnyRewards.Helios.Task.UnitTest.Helpers
{
    public class TaskHelperTests
    {
        private readonly Mock<ILogger> _loggerMock;
        private readonly Mock<IConsumerTaskRepo> _consumerTaskRepoMock;

        public TaskHelperTests()
        {
            _loggerMock = new Mock<ILogger>();
            _consumerTaskRepoMock = new Mock<IConsumerTaskRepo>();
        }

        [Fact]
        public void ValidateParentTaskEligibility_Should_Return_False_When_ParentTaskAndReward_Is_Null()
        {
            // Arrange
            ConsumerTaskRewardModel? parentTaskAndReward = null;
            var consumerTasks = new List<ConsumerTaskModel>();
            var loggerMock = new Mock<ILogger>();

            // Act
            var result = TaskHelper.ValidateParentTaskEligibility(parentTaskAndReward, consumerTasks, loggerMock.Object);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ValidateParentTaskEligibility_Should_Return_False_When_ConsumerTasks_Is_Null()
        {
            // Arrange
            var parentTaskAndReward = new ConsumerTaskRewardModel
            {
                ConsumerTask = new ConsumerTaskModel { TenantCode = "test" },
                TaskReward = new TaskRewardModel { IsRecurring = true, RecurrenceDefinitionJson = "{\"recurrenceType\":\"Monthly\"}" }
            };
            IList<ConsumerTaskModel>? consumerTasks = null;
            var loggerMock = new Mock<ILogger>();

            // Act
            var result = TaskHelper.ValidateParentTaskEligibility(parentTaskAndReward, consumerTasks, loggerMock.Object);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ValidateParentTaskEligibility_Should_Return_False_When_RecurrenceType_Is_Schedule()
        {
            // Arrange
            var parentTaskAndReward = new ConsumerTaskRewardModel
            {
                ConsumerTask = new ConsumerTaskModel { TenantCode = "test" },
                TaskReward = new TaskRewardModel { IsRecurring = true, RecurrenceDefinitionJson = "{\"recurrenceType\":\"Schedule\"}" }
            };
            var consumerTasks = new List<ConsumerTaskModel>();
            var loggerMock = new Mock<ILogger>();

            // Act
            var result = TaskHelper.ValidateParentTaskEligibility(parentTaskAndReward, consumerTasks, loggerMock.Object);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ValidateParentTaskEligibility_Should_Log_Error_When_PeriodRestartDate_Is_Invalid()
        {
            // Arrange
            var parentTaskAndReward = new ConsumerTaskRewardModel
            {
                ConsumerTask = new ConsumerTaskModel { TenantCode = "test", TaskId = 1 },
                TaskReward = new TaskRewardModel { IsRecurring = true, RecurrenceDefinitionJson = "{\"recurrenceType\":\"Monthly\",\"periodic\":{\"periodRestartDate\":30}}" }
            };
            var consumerTasks = new List<ConsumerTaskModel>();
            var loggerMock = new Mock<ILogger>();

            // Act
            var result = TaskHelper.ValidateParentTaskEligibility(parentTaskAndReward, consumerTasks, loggerMock.Object);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ValidateParentTaskEligibility_Should_Return_True_For_Valid_Monthly_Recurrence()
        {
            // Arrange
            var parentTaskAndReward = new ConsumerTaskRewardModel
            {
                ConsumerTask = new ConsumerTaskModel { TenantCode = "test", TaskCompleteTs = DateTime.UtcNow.AddMonths(-1), TaskId = 1, TaskStatus = "COMPLETED" },
                TaskReward = new TaskRewardModel { IsRecurring = true, RecurrenceDefinitionJson = "{\"recurrenceType\":\"PERIODIC\",\"periodic\":{\"period\":\"MONTH\", \"periodRestartDate\":15}}" }
            };
            var consumerTasks = new List<ConsumerTaskModel>
            {
                new() { TenantCode = "test", TaskCompleteTs = DateTime.UtcNow.AddMonths(-1), TaskId = 1, TaskStatus = "COMPLETED" }
            };
            var loggerMock = new Mock<ILogger>();

            // Act
            var result = TaskHelper.ValidateParentTaskEligibility(parentTaskAndReward, consumerTasks, loggerMock.Object);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ValidateParentTaskEligibility_Should_Return_True_For_Valid_Quarterly_Recurrence()
        {
            // Arrange
            var parentTaskAndReward = new ConsumerTaskRewardModel
            {
                ConsumerTask = new ConsumerTaskModel { TenantCode = "test", TaskCompleteTs = DateTime.UtcNow.AddMonths(-3), TaskId = 1, TaskStatus = "COMPLETED" },
                TaskReward = new TaskRewardModel { IsRecurring = true, RecurrenceDefinitionJson = "{\"recurrenceType\":\"PERIODIC\",\"periodic\":{\"period\":\"QUARTER\", \"periodRestartDate\":15}}" }
            };
            var consumerTasks = new List<ConsumerTaskModel>
            {
                new() { TenantCode = "test", TaskCompleteTs = DateTime.UtcNow.AddMonths(-3), TaskId = 1, TaskStatus = "COMPLETED" }
            };
            var loggerMock = new Mock<ILogger>();

            // Act
            var result = TaskHelper.ValidateParentTaskEligibility(parentTaskAndReward, consumerTasks, loggerMock.Object);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ValidateParentTaskEligibility_Should_Return_False_When_ParentTask_Is_Not_Recurring()
        {
            // Arrange
            var parentTaskAndReward = new ConsumerTaskRewardModel
            {
                ConsumerTask = new ConsumerTaskModel { TenantCode = "test" },
                TaskReward = new TaskRewardModel { IsRecurring = false, RecurrenceDefinitionJson = "{\"recurrenceType\":\"Monthly\"}" }
            };
            var consumerTasks = new List<ConsumerTaskModel>();
            var loggerMock = new Mock<ILogger>();

            // Act
            var result = TaskHelper.ValidateParentTaskEligibility(parentTaskAndReward, consumerTasks, loggerMock.Object);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ValidateParentTaskEligibility_ShouldReturnFalse_WhenParentTaskRewardIsNull()
        {
            // Arrange
            var parentTaskAndReward = new ConsumerTaskRewardModel
            {
                ConsumerTask = new ConsumerTaskModel(),
            };
            var consumerTasks = new List<ConsumerTaskModel> { new() };

            // Act
            var result = TaskHelper.ValidateParentTaskEligibility(parentTaskAndReward, consumerTasks, _loggerMock.Object);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ValidateParentTaskEligibility_ShouldReturnFalse_WhenRecurrenceTypeIsNull()
        {
            // Arrange
            var parentTaskAndReward = new ConsumerTaskRewardModel
            {
                ConsumerTask = new ConsumerTaskModel(),
                TaskReward = new TaskRewardModel { IsRecurring = true, RecurrenceDefinitionJson = JsonConvert.SerializeObject(new RecurringDto()) }
            };
            var consumerTasks = new List<ConsumerTaskModel> { new() };

            // Act
            var result = TaskHelper.ValidateParentTaskEligibility(parentTaskAndReward, consumerTasks, _loggerMock.Object);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ValidateParentTaskEligibility_ShouldReturnFalse_WhenPeriodicPeriodIsNull()
        {
            // Arrange
            var parentRecurrenceDetails = new RecurringDto
            {
                recurrenceType = Constant.Periodic,
                periodic = new PeriodicDto { period = null }
            };

            var parentTaskAndReward = new ConsumerTaskRewardModel
            {
                ConsumerTask = new ConsumerTaskModel(),
                TaskReward = new TaskRewardModel
                {
                    IsRecurring = true,
                    RecurrenceDefinitionJson = JsonConvert.SerializeObject(parentRecurrenceDetails)
                }
            };

            var consumerTasks = new List<ConsumerTaskModel> { new() };

            // Act
            var result = TaskHelper.ValidateParentTaskEligibility(parentTaskAndReward, consumerTasks, _loggerMock.Object);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ValidateParentTaskEligibility_ShouldReturnTrue_WhenAllConditionsAreMet()
        {
            // Arrange
            var parentRecurrenceDetails = new RecurringDto
            {
                recurrenceType = Constant.Periodic,
                periodic = new PeriodicDto { period = "MONTH" , periodRestartDate = 15 }
            };

            var parentTaskAndReward = new ConsumerTaskRewardModel
            {
                ConsumerTask = new ConsumerTaskModel
                {
                    TenantCode = "test",
                    TaskId = 1,
                    TaskStatus = "COMPLETED",
                    TaskCompleteTs = DateTime.UtcNow.AddMonths(-1)
                },
                TaskReward = new TaskRewardModel
                {
                    IsRecurring = true,
                    RecurrenceDefinitionJson = JsonConvert.SerializeObject(parentRecurrenceDetails)
                }
            };
            
            var consumerTasks = new List<ConsumerTaskModel> { new() };

            // Act
            var result = TaskHelper.ValidateParentTaskEligibility(parentTaskAndReward, consumerTasks, _loggerMock.Object);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ValidateParentTaskEligibility_ShouldReturnFalse_WhenRecurrenceTypeIsNotSchedule()
        {
            // Arrange
            var parentRecurrenceDetails = new RecurringDto
            {
                recurrenceType = "NonSchedule",
                periodic = new PeriodicDto { period = "Month" }
            };

            var parentTaskAndReward = new ConsumerTaskRewardModel
            {
                ConsumerTask = new ConsumerTaskModel
                {
                    TaskCompleteTs = DateTime.UtcNow.AddMonths(-1)
                },
                TaskReward = new TaskRewardModel
                {
                    IsRecurring = true,
                    RecurrenceDefinitionJson = JsonConvert.SerializeObject(parentRecurrenceDetails)
                }
            };
            var consumerTasks = new List<ConsumerTaskModel> { new() };

            // Act
            var result = TaskHelper.ValidateParentTaskEligibility(parentTaskAndReward, consumerTasks, _loggerMock.Object);

            // Assert
            Assert.False(result);
        }
        [Fact]
        public async Task1 FilterAvailableTasksAsync_NoConsumerTasks_ReturnsOriginalTaskRewardDetails()
        {
            // Arrange
            var now = DateTime.UtcNow;
            string currentStartDate = now.ToString("MM-dd");
            string currentExpiryDate = now.AddDays(5).ToString("MM-dd");

            var taskRewardDetails = new List<TaskRewardDetailDto>
            {
                new()
                {
                    Task = new TaskDto { TaskId = 1 },
                    TaskReward = new TaskRewardDto
                    {
                        TaskId = 1,
                        IsRecurring = false,
                        RecurrenceDefinitionJson = $@"{{
                            ""schedules"": [
                                {{""startDate"": ""{currentStartDate}"", ""expiryDate"": ""{currentExpiryDate}""}},
                                {{""startDate"": ""{now.AddMonths(1):MM-dd}"", ""expiryDate"": ""{now.AddMonths(1).AddDays(5):MM-dd}""}}
                            ],
                            ""recurrenceType"": ""SCHEDULE""
                        }}"
                    }
                }
            };

           
            var consumerTasks = new List<ConsumerTaskModel>();
            // Act
            var result = await TaskHelper.FilterAvailableTasksAsync(taskRewardDetails, consumerTasks, _consumerTaskRepoMock.Object, _loggerMock.Object);

            // Assert
            Assert.Equal(taskRewardDetails, result);
        }

        [Fact]
        public async Task1 FilterAvailableTasksAsync_NoConsumerTasks_ReturnsOriginalTaskRewardDetails_HasValidSchedule()
        {
            // Arrange
            var now = DateTime.UtcNow;
            string currentStartDate = now.ToString("MM-dd");
            string currentExpiryDate = now.AddDays(5).ToString("MM-dd");
            var taskRewardDetails = new List<TaskRewardDetailDto>
            {
                 new()
                {
                    Task = new TaskDto { TaskId = 1 },
                    TaskReward = new TaskRewardDto
                    {
                        TaskId = 1,
                        IsRecurring = true,
                        RecurrenceDefinitionJson = $@"{{
                            ""schedules"": [
                                {{""startDate"": ""{currentStartDate}"", ""expiryDate"": ""{currentExpiryDate}""}},
                                {{""startDate"": ""{now.AddMonths(1):MM-dd}"", ""expiryDate"": ""{now.AddMonths(1).AddDays(5):MM-dd}""}}
                            ],
                            ""recurrenceType"": ""SCHEDULE""
                        }}"
                    }
                }
            };
            var consumerTasks = new List<ConsumerTaskModel>();


            // Act
            var result = await TaskHelper.FilterAvailableTasksAsync(taskRewardDetails, consumerTasks, _consumerTaskRepoMock.Object, _loggerMock.Object);

            // Assert
            Assert.Equal(taskRewardDetails, result);
        }
        [Fact]
        public async Task1 FilterAvailableTasksAsync_RecurringTaskIsValid_ReturnsAvailableTasks()
        {
            // Arrange
            var taskRewardDetails = new List<TaskRewardDetailDto>
            {
                new() {
                    Task = new TaskDto { TaskId = 1 },
                    TaskReward = new TaskRewardDto { IsRecurring = true, RecurrenceDefinitionJson = "{ \"recurrenceType\": \"PERIODIC\", \"periodic\": { \"period\": \"MONTH\", \"periodRestartDate\": 5 } }" }
                }
            };
            var consumerTasks = new List<ConsumerTaskModel>
            {
                new() { TaskId = 1, TaskStatus = "COMPLETED", TaskCompleteTs = DateTime.UtcNow.AddDays(-30) }
            };

            // Act
            var result = await TaskHelper.FilterAvailableTasksAsync(taskRewardDetails, consumerTasks, _consumerTaskRepoMock.Object, _loggerMock.Object);

            // Assert
            Assert.Contains(result, t => t.Task.TaskId == 1);
        }

        [Fact]
        public async Task1 FilterAvailableTasksAsync_RecurringTask_InvalidPeriodRestartDate_LogsError()
        {
            // Arrange
            var taskRewardDetails = new List<TaskRewardDetailDto>
            {
                new() {
                    Task = new TaskDto { TaskId = 1 },
                    TaskReward = new TaskRewardDto { IsRecurring = true, RecurrenceDefinitionJson = "{ \"periodic\": { \"periodRestartDate\": 29 } }" }
                }
            };
            var consumerTasks = new List<ConsumerTaskModel>
            {
                new() { TaskId = 1, TaskStatus = "COMPLETED", TaskCompleteTs = DateTime.UtcNow.AddDays(-30) }
            };

            // Act
            var result = await TaskHelper.FilterAvailableTasksAsync(taskRewardDetails, consumerTasks, _consumerTaskRepoMock.Object, _loggerMock.Object);

            // Assert
            Assert.DoesNotContain(result, t => t.Task.TaskId == 1);
        }

        [Fact]
        public async Task1 FilterAvailableTasksAsync_QuarterlyRecurringTask_IsValid_ReturnsAvailableTasks()
        {
            // Arrange
            var taskRewardDetails = new List<TaskRewardDetailDto>
            {
                new() {
                    Task = new TaskDto { TaskId = 2 },
                    TaskReward = new TaskRewardDto { IsRecurring = true, RecurrenceDefinitionJson = "{ \"recurrenceType\": \"PERIODIC\", \"periodic\": { \"period\": \"QUARTER\", \"periodRestartDate\": 5} }" }
                }
            };
            var consumerTasks = new List<ConsumerTaskModel>
            {
                new() { TaskId = 2, TaskStatus = "COMPLETED", TaskCompleteTs = DateTime.UtcNow.AddMonths(-3) }
            };

            // Act
            var result = await TaskHelper.FilterAvailableTasksAsync(taskRewardDetails, consumerTasks, _consumerTaskRepoMock.Object, _loggerMock.Object);

            // Assert
            Assert.Contains(result, t => t.Task.TaskId == 2);
        }

        [Fact]
        public async Task1 FilterAvailableTasksAsync_ParentTaskValid_ReturnsAvailableTasks()
        {
            // Arrange
            var taskRewardDetails = new List<TaskRewardDetailDto>
            {
                new() {
                    Task = new TaskDto { TaskId = 3 },
                    TaskReward = new TaskRewardDto { IsRecurring = true, RecurrenceDefinitionJson = "{ \"recurrenceType\": \"PERIODIC\", \"periodic\": { \"period\": \"MONTH\", \"periodRestartDate\": 5 } }" }
                }
            };
            var consumerTasks = new List<ConsumerTaskModel>
            {
                new() { TaskId = 3, TaskStatus = "COMPLETED", ParentConsumerTaskId = 1, TaskCompleteTs = DateTime.UtcNow.AddMonths(-1) }
            };

            _consumerTaskRepoMock.Setup(repo => repo.GetConsumerTaskWithReward(It.IsAny<string>(), 1, It.IsAny<string>()))
                .ReturnsAsync(new ConsumerTaskRewardModel
                {
                    ConsumerTask = new ConsumerTaskModel { TaskId = 3 },
                    TaskReward = new TaskRewardModel { IsRecurring = true }
                });

            // Act
            var result = await TaskHelper.FilterAvailableTasksAsync(taskRewardDetails, consumerTasks, _consumerTaskRepoMock.Object, _loggerMock.Object);

            // Assert
            Assert.Contains(result, t => t.Task.TaskId == 3);
        }

        [Fact]
        public async Task1 FilterAvailableTasksAsync_Should_Add_Recurring_Child_Task_With_Valid_Parent()
        {
            // Arrange
            var taskRewardDetails = new List<TaskRewardDetailDto>
            {
                new()
                {
                    Task = new TaskDto { TaskId = 1 },
                    TaskReward = new() { IsRecurring = true, RecurrenceDefinitionJson = "{\"recurrenceType\":\"Month\"}" }
                }
            };

            var consumerTasks = new List<ConsumerTaskModel>
            {
                new()
                {
                    TaskId = 1,
                    TaskStatus = Constants.Completed,
                    ParentConsumerTaskId = 1234,
                    TaskCompleteTs = DateTime.UtcNow.AddDays(-1),
                    TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4"
                }
            };

            var parentTaskAndReward = new ConsumerTaskRewardModel
            {
                ConsumerTask = new ConsumerTaskModel { TaskId = 1234, TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4", TaskStatus = Constants.Completed },
                TaskReward = new TaskRewardModel { IsRecurring = true, RecurrenceDefinitionJson = "{ \"recurrenceType\": \"PERIODIC\", \"periodic\": { \"period\": \"MONTH\", \"periodRestartDate\": 5 } }" }
            };

            var loggerMock = new Mock<ILogger>();
            var consumerTaskRepoMock = new Mock<IConsumerTaskRepo>();

            // Set up mock to return the parent task and reward
            consumerTaskRepoMock.Setup(repo => repo.GetConsumerTaskWithReward(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<string>()))
                .ReturnsAsync(parentTaskAndReward);

            // Mock the ValidateParentTaskEligibility method to return true for this test
            var validateParentTaskEligibilityMock = new Mock<Func<ConsumerTaskRewardModel, IList<ConsumerTaskModel>, ILogger, bool>>();
            validateParentTaskEligibilityMock.Setup(func => func(It.IsAny<ConsumerTaskRewardModel>(), It.IsAny<IList<ConsumerTaskModel>>(), It.IsAny<ILogger>()))
                .Returns(true);

            // Act
            var result = await TaskHelper.FilterAvailableTasksAsync(taskRewardDetails, consumerTasks, consumerTaskRepoMock.Object, loggerMock.Object);

            // Assert
            Assert.NotNull(result);
            Assert.Contains(result, task => task.Task.TaskId == 1); // Ensure the task is added to the result
        }

        [Fact]
        public async Task1 FilterAvailableTasksAsync_Should_Add_Task_With_Valid_Monthly_Recurrence()
        {
            // Arrange
            var taskRewardDetails = new List<TaskRewardDetailDto>
            {
                new()
                {
                    Task = new TaskDto { TaskId = 1 },
                    TaskReward = new()
                    {
                        IsRecurring = true,
                        RecurrenceDefinitionJson = JsonConvert.SerializeObject(new RecurringDto
                        {
                            recurrenceType = Constant.Periodic,
                            periodic = new() { period = Constant.Month, periodRestartDate = 15 },
                        })
                    }
                }
            };

            var consumerTasks = new List<ConsumerTaskModel>
            {
                new()
                {
                    TaskId = 1,
                    TaskStatus = Constants.Completed,
                    TaskCompleteTs = DateTime.UtcNow.AddMonths(-1), // Task completed in the previous month
                    TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4"
                }
            };

            var loggerMock = new Mock<ILogger>();
            var consumerTaskRepoMock = new Mock<IConsumerTaskRepo>();

            // Act
            var result = await TaskHelper.FilterAvailableTasksAsync(taskRewardDetails, consumerTasks, consumerTaskRepoMock.Object, loggerMock.Object);

            // Assert
            Assert.NotNull(result);
            Assert.Contains(result, task => task.Task.TaskId == 1); // Ensures the task was added to the available tasks
        }

        [Fact]
        public async Task1 FilterAvailableTasksAsync_Should_Add_Task_With_Valid_Quarterly_Recurrence()
        {
            // Arrange
            var taskRewardDetails = new List<TaskRewardDetailDto>
            {
                new()
                {
                    Task = new TaskDto { TaskId = 1 },
                    TaskReward = new()
                    {
                        IsRecurring = true,
                        RecurrenceDefinitionJson = JsonConvert.SerializeObject(new RecurringDto
                        {
                            recurrenceType = Constant.Periodic,
                            periodic = new() { period = Constant.QuarterlyPeriod, periodRestartDate = 15 },
                        })
                    }
                }
            };

            var consumerTasks = new List<ConsumerTaskModel>
            {
                new()
                {
                    TaskId = 1,
                    TaskStatus = Constants.Completed,
                    TaskCompleteTs = DateTime.UtcNow.AddMonths(-4), // Task completed in the previous quarter
                    TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4"
                }
            };

            var loggerMock = new Mock<ILogger>();
            var consumerTaskRepoMock = new Mock<IConsumerTaskRepo>();

            // Act
            var result = await TaskHelper.FilterAvailableTasksAsync(taskRewardDetails, consumerTasks, consumerTaskRepoMock.Object, loggerMock.Object);

            // Assert
            Assert.NotNull(result);
            Assert.Contains(result, task => task.Task.TaskId == 1); // Ensures the task was added to the available tasks
        }

        [Fact]
        public void ReturnsTrue_WhenTaskCompletedOutsideSchedule_AndNowInsideSchedule()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var start = now.AddDays(-1); // started yesterday
            var end = now.AddDays(1);    // ends tomorrow

            var recurrenceDetails = new RecurringDto
            {
                recurrenceType = "SCHEDULE",
                Schedules = new[]
                {
                new ScheduleDto
                {
                    StartDate = start.ToString("MM-dd"),
                    ExpiryDate = end.ToString("MM-dd")
                }
            }
            };

            var consumerTask = new ConsumerTaskModel
            {
                TaskStatus = "COMPLETED",
                TaskCompleteTs = start.AddDays(-1) // completed before the schedule window
            };

            // Act
            var result = TaskHelper.IsValidScheduleRecurring(recurrenceDetails, true, consumerTask);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ReturnsFalse_WhenTaskCompletedWithinSchedule()
        {
            var now = DateTime.UtcNow;
            var start = now.AddDays(-2);
            var end = now.AddDays(2);

            var recurrenceDetails = new RecurringDto
            {
                recurrenceType = "SCHEDULE",
                Schedules = new[]
                {
                new ScheduleDto
                {
                    StartDate = start.ToString("MM-dd"),
                    ExpiryDate = end.ToString("MM-dd")
                }
            }
            };

            var consumerTask = new ConsumerTaskModel
            {
                TaskStatus = "COMPLETED",
                TaskCompleteTs = now
            };

            var result = TaskHelper.IsValidScheduleRecurring(recurrenceDetails, true, consumerTask);

            Assert.False(result);
        }

        [Fact]
        public void ReturnsFalse_WhenTaskStatusNotCompleted()
        {
            var now = DateTime.UtcNow;
            var start = now.AddDays(-1);
            var end = now.AddDays(1);

            var recurrenceDetails = new RecurringDto
            {
                recurrenceType = "SCHEDULE",
                Schedules = new[]
                {
                new ScheduleDto
                {
                    StartDate = start.ToString("MM-dd"),
                    ExpiryDate = end.ToString("MM-dd")
                }
            }
            };

            var consumerTask = new ConsumerTaskModel
            {
                TaskStatus = "IN_PROGRESS",
                TaskCompleteTs = now
            };

            var result = TaskHelper.IsValidScheduleRecurring(recurrenceDetails, true, consumerTask);

            Assert.False(result);
        }

        [Fact]
        public void ReturnsFalse_WhenNoSchedulesProvided()
        {
            var consumerTask = new ConsumerTaskModel
            {
                TaskStatus = "COMPLETED",
                TaskCompleteTs = DateTime.UtcNow
            };

            var recurrenceDetails = new RecurringDto
            {
                recurrenceType = "SCHEDULE",
                Schedules = null
            };

            var result = TaskHelper.IsValidScheduleRecurring(recurrenceDetails, true, consumerTask);

            Assert.False(result);
        }

        [Fact]
        public void ReturnsFalse_WhenRecurringTypeNotSchedule()
        {
            var now = DateTime.UtcNow;

            var recurrenceDetails = new RecurringDto
            {
                recurrenceType = "MONTHLY",
                Schedules = new[]
                {
                new ScheduleDto
                {
                    StartDate = now.ToString("MM-dd"),
                    ExpiryDate = now.AddDays(1).ToString("MM-dd")
                }
            }
            };

            var consumerTask = new ConsumerTaskModel
            {
                TaskStatus = "COMPLETED",
                TaskCompleteTs = now
            };

            var result = TaskHelper.IsValidScheduleRecurring(recurrenceDetails, true, consumerTask);

            Assert.False(result);
        }

        [Fact]
        public void ReturnsFalse_WhenRecurrenceDetailsIsNull()
        {
            var consumerTask = new ConsumerTaskModel
            {
                TaskStatus = "COMPLETED",
                TaskCompleteTs = DateTime.UtcNow
            };

            var result = TaskHelper.IsValidScheduleRecurring(null, true, consumerTask);

            Assert.False(result);
        }

        [Fact]
        public void ReturnsFalse_WhenSchedulesIsNull()
        {
            // Act
            var result = TaskHelper.HasValidSchedule(null);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ReturnsFalse_WhenSchedulesIsEmpty()
        {
            // Act
            var result = TaskHelper.HasValidSchedule(Array.Empty<ScheduleDto>());

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ReturnsFalse_WhenDateFormatIsInvalid()
        {
            var schedules = new[]
            {
            new ScheduleDto { StartDate = "invalid", ExpiryDate = "06-30" }
        };

            var result = TaskHelper.HasValidSchedule(schedules);

            Assert.False(result);
        }

        [Fact]
        public void ReturnsFalse_WhenCurrentDateIsOutsideSchedule()
        {
            var currentYear = DateTime.UtcNow.Year;
            var pastStart = new DateTime(currentYear, 1, 1);
            var pastEnd = new DateTime(currentYear, 1, 15);

            var schedules = new[]
            {
            new ScheduleDto
            {
                StartDate = pastStart.ToString("MM-dd"),
                ExpiryDate = pastEnd.ToString("MM-dd")
            }
        };

            var result = TaskHelper.HasValidSchedule(schedules);

            Assert.False(result);
        }

        [Fact]
        public void ReturnsTrue_WhenCurrentDateIsWithinSchedule()
        {
            var today = DateTime.UtcNow.Date;

            var schedules = new[]
            {
            new ScheduleDto
            {
                StartDate = today.AddDays(-1).ToString("MM-dd"),
                ExpiryDate = today.AddDays(1).ToString("MM-dd")
            }
        };

            var result = TaskHelper.HasValidSchedule(schedules);

            Assert.True(result);
        }

        [Fact]
        public void ReturnsTrue_WhenCurrentDateIsOnStartDate()
        {
            var today = DateTime.UtcNow.Date;

            var schedules = new[]
            {
            new ScheduleDto
            {
                StartDate = today.ToString("MM-dd"),
                ExpiryDate = today.AddDays(2).ToString("MM-dd")
            }
        };

            var result = TaskHelper.HasValidSchedule(schedules);

            Assert.True(result);
        }

        [Fact]
        public void ReturnsTrue_WhenCurrentDateIsOnExpiryDate()
        {
            var today = DateTime.UtcNow.Date;

            var schedules = new[]
            {
            new ScheduleDto
            {
                StartDate = today.AddDays(-2).ToString("MM-dd"),
                ExpiryDate = today.ToString("MM-dd")
            }
        };

            var result = TaskHelper.HasValidSchedule(schedules);

            Assert.True(result);
        }

        [Fact]
        public void ReturnsTrue_WhenMultipleSchedules_AtLeastOneIsValid()
        {
            var today = DateTime.UtcNow.Date;

            var schedules = new[]
            {
            new ScheduleDto { StartDate = "01-01", ExpiryDate = "01-02" },
            new ScheduleDto
            {
                StartDate = today.ToString("MM-dd"),
                ExpiryDate = today.AddDays(5).ToString("MM-dd")
            }
        };

            var result = TaskHelper.HasValidSchedule(schedules);

            Assert.True(result);
        }

        [Fact]
        public void ReturnsFalse_WhenAllSchedulesInvalidFormat()
        {
            var schedules = new[]
            {
            new ScheduleDto { StartDate = "13-01", ExpiryDate = "99-99" },
            new ScheduleDto { StartDate = null, ExpiryDate = null }
        };

            var result = TaskHelper.HasValidSchedule(schedules);

            Assert.False(result);
        }

    }
}

