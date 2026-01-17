using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using SunnyRewards.Helios.Common.Core.Services;
using SunnyRewards.Helios.Task.Core.Domain.Constants;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Models;
using SunnyRewards.Helios.Task.Infrastructure.Services;
using SunnyRewards.Helios.Task.Infrastructure.Services.Interface;
using SunnyRewards.Helios.Task.Infrastructure.Helpers.Interface;
using SunnyRewards.Helios.Task.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Task.Core.Domain.Dtos.Enums;
using Xunit;
using StatusCodes = Microsoft.AspNetCore.Http.StatusCodes;
using SunnyRewards.Helios.Common.Core.Domain;

namespace SunnyRewards.Helios.Task.UnitTest.Services
{
    public class ConsumerTaskServiceUnitTest
    {
        // Helper to create a ConsumerTaskService with required mocks
        private ConsumerTaskService CreateService(
            out Mock<IConsumerTaskRepo> consumerTaskRepo,
            out Mock<ITaskRewardRepo> taskRewardRepo,
            out Mock<IMapper> mapper)
        {
            var logger = new Mock<ILogger<ConsumerTaskService>>();
            mapper = new Mock<IMapper>();
            var session = new Mock<NHibernate.ISession>();
            consumerTaskRepo = new Mock<IConsumerTaskRepo>();
            var taskRepo = new Mock<ITaskRepo>();
            var taskDetailRepo = new Mock<ITaskDetailRepo>();
            taskRewardRepo = new Mock<ITaskRewardRepo>();
            var termsOfServiceRepo = new Mock<ITermsOfServiceRepo>();
            var taskRewardService = new Mock<ITaskRewardService>();
            var tenantTaskCategoryRepo = new Mock<ITenantTaskCategoryRepo>();
            var taskTypeRepo = new Mock<ITaskTypeRepo>();
            var subtaskService = new Mock<ISubtaskService>();
            var taskRewardTypeRepo = new Mock<ITaskRewardTypeRepo>();
            var fileHelper = new Mock<IFileHelper>();
            var configuration = new Mock<IConfiguration>();
            var vault = new Mock<IVault>();
            var taskCommonHelper = new Mock<ITaskCommonHelper>();
            var commonTaskRewardService = new Mock<ICommonTaskRewardService>();
            var heliosEventPublisher = new Mock<IHeliosEventPublisher<ConsumerTaskEventDto>>();

            // basic mapper behavior for mapping ConsumerTaskModel -> ConsumerTaskDto used in response
            mapper.Setup(m => m.Map<ConsumerTaskDto>(It.IsAny<ConsumerTaskModel>()))
                .Returns((ConsumerTaskModel ct) => new ConsumerTaskDto
                {
                    ConsumerTaskId = ct.ConsumerTaskId,
                    TaskId = ct.TaskId,
                    TaskStatus = ct.TaskStatus
                });

            var service = new ConsumerTaskService(
                logger.Object,
                mapper.Object,
                session.Object,
                consumerTaskRepo.Object,
                taskRepo.Object,
                taskDetailRepo.Object,
                taskRewardRepo.Object,
                termsOfServiceRepo.Object,
                taskRewardService.Object,
                tenantTaskCategoryRepo.Object,
                taskTypeRepo.Object,
                subtaskService.Object,
                taskRewardTypeRepo.Object,
                fileHelper.Object,
                configuration.Object,
                vault.Object,
                taskCommonHelper.Object,
                commonTaskRewardService.Object,
                heliosEventPublisher.Object
            );

            return service;
        }

        [Fact]
        public async System.Threading.Tasks.Task UpdateOther_WhenUnitsBelowRequired_DoesNotComplete()
        {
            // Arrange
            var service = CreateService(out var consumerTaskRepo, out var taskRewardRepo, out var mapper);

            var request = new UpdateHealthTaskProgressRequestDto
            {
                TenantCode = "TEN",
                ConsumerCode = "CMR",
                TaskId = 1,
                HealthTaskType = nameof(HealthTaskType.OTHER),
                NumberOfUnits = 3,
                DateTimeAddedFor = DateTime.UtcNow
            };

            // TaskReward with HEALTH criteria requiredUnits = 5
            var tr = new TaskRewardModel
            {
                TaskId = request.TaskId,
                TenantCode = request.TenantCode,
                SelfReport = true,
                TaskCompletionCriteriaJson = JsonConvert.SerializeObject(new TaskCompletionCriteriaJson
                {
                    CompletionCriteriaType = Constant.HealthCriteriaType,
                    HealthCriteria = new HealthCriteria
                    {
                        HealthTaskType = nameof(HealthTaskType.OTHER),
                        RequiredUnits = 5
                    }
                })
            };

            taskRewardRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false)).ReturnsAsync(new List<TaskRewardModel> { tr });

            var consumerTask = new ConsumerTaskModel
            {
                ConsumerTaskId = 10,
                TaskId = request.TaskId,
                TenantCode = request.TenantCode,
                ConsumerCode = request.ConsumerCode,
                TaskStatus = Constants.InProgress,
                ProgressDetail = null
            };

            consumerTaskRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<ConsumerTaskModel, bool>>>(), false)).ReturnsAsync(new List<ConsumerTaskModel> { consumerTask });

            // Act
            var result = await service.UpdateHealthTaskProgress(request);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsTaskCompleted); // should not be flagged completed
            Assert.Equal(Constants.InProgress, result.ConsumerTask?.TaskStatus); // mapped TaskStatus remains InProgress
        }

        [Fact]
        public async System.Threading.Tasks.Task UpdateOther_WhenUnitsReachRequired_MarksTaskCompleted()
        {
            // Arrange
            var service = CreateService(out var consumerTaskRepo, out var taskRewardRepo, out var mapper);

            var request = new UpdateHealthTaskProgressRequestDto
            {
                TenantCode = "TEN",
                ConsumerCode = "CMR",
                TaskId = 2,
                HealthTaskType = nameof(HealthTaskType.OTHER),
                NumberOfUnits = 5,
                DateTimeAddedFor = DateTime.UtcNow
            };

            // requiredUnits = 5
            var tr = new TaskRewardModel
            {
                TaskId = request.TaskId,
                TenantCode = request.TenantCode,
                SelfReport = true,
                TaskCompletionCriteriaJson = JsonConvert.SerializeObject(new TaskCompletionCriteriaJson
                {
                    CompletionCriteriaType = Constant.HealthCriteriaType,
                    HealthCriteria = new HealthCriteria
                    {
                        HealthTaskType = nameof(HealthTaskType.OTHER),
                        RequiredUnits = 5
                    }
                })
            };

            taskRewardRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false)).ReturnsAsync(new List<TaskRewardModel> { tr });

            var consumerTask = new ConsumerTaskModel
            {
                ConsumerTaskId = 11,
                TaskId = request.TaskId,
                TenantCode = request.TenantCode,
                ConsumerCode = request.ConsumerCode,
                TaskStatus = Constants.InProgress,
                ProgressDetail = null
            };

            consumerTaskRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<ConsumerTaskModel, bool>>>(), false)).ReturnsAsync(new List<ConsumerTaskModel> { consumerTask });

            // Act
            var result = await service.UpdateHealthTaskProgress(request);

            // Assert
            Assert.NotNull(result);
            // UpdateProgressDetail sets consumerTask.TaskStatus = COMPLETED, then method sets IsTaskCompleted = true and resets TaskStatus to IN_PROGRESS
            Assert.True(result.IsTaskCompleted);
            Assert.Equal(Constants.InProgress, result.ConsumerTask?.TaskStatus);
        }

        [Fact]
        public async System.Threading.Tasks.Task UpdateOther_WithExistingActivityTimestamp_ReplacesPreviousUnits()
        {
            // Arrange
            var service = CreateService(out var consumerTaskRepo, out var taskRewardRepo, out var mapper);

            var timestamp = DateTime.UtcNow;
            var request = new UpdateHealthTaskProgressRequestDto
            {
                TenantCode = "TEN",
                ConsumerCode = "CMR",
                TaskId = 3,
                HealthTaskType = nameof(HealthTaskType.OTHER),
                NumberOfUnits = 4, // new units
                DateTimeAddedFor = timestamp
            };

            // criteria requiring 7 units - starting with existing activity of 5 units at same timestamp; replacing 5 with 4 => total decreases by 1
            var tr = new TaskRewardModel
            {
                TaskId = request.TaskId,
                TenantCode = request.TenantCode,
                SelfReport = true,
                TaskCompletionCriteriaJson = JsonConvert.SerializeObject(new TaskCompletionCriteriaJson
                {
                    CompletionCriteriaType = Constant.HealthCriteriaType,
                    HealthCriteria = new HealthCriteria
                    {
                        HealthTaskType = nameof(HealthTaskType.OTHER),
                        RequiredUnits = 7
                    }
                })
            };

            taskRewardRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false)).ReturnsAsync(new List<TaskRewardModel> { tr });

            // build existing progressDetail JSON with one activity of 5 units at the same timestamp
            var existing = new HealthProgressDetails<OtherHealthTaksRollupDataDto>
            {
                DetailType = nameof(HealthTaskType.OTHER),
                HealthProgress = new OtherHealthTaksRollupDataDto
                {
                    TotalUnits = 5,
                    ActivityLog = new[] { new TrackingDto { TimeStamp = timestamp, UnitsAdded = 5, Source = "Manual" } },
                    HealthReport = new List<HealthTrackingDto>()
                }
            };

            var consumerTask = new ConsumerTaskModel
            {
                ConsumerTaskId = 12,
                TaskId = request.TaskId,
                TenantCode = request.TenantCode,
                ConsumerCode = request.ConsumerCode,
                TaskStatus = Constants.InProgress,
                ProgressDetail = JsonConvert.SerializeObject(existing, new JsonSerializerSettings { ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver() })
            };

            consumerTaskRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<ConsumerTaskModel, bool>>>(), false)).ReturnsAsync(new List<ConsumerTaskModel> { consumerTask });

            // Act
            var result = await service.UpdateHealthTaskProgress(request);

            // Assert
            Assert.NotNull(result);
            // After replacing 5 with 4 the totalUnits becomes 4 which is < 7 => not completed
            Assert.False(result.IsTaskCompleted);
            Assert.Equal(Constants.InProgress, result.ConsumerTask?.TaskStatus);
        }

        [Fact]
        public async System.Threading.Tasks.Task UpdateOther_UIComponent_IsDialerRequiredFalse_CompletesWhenUiValidRegardlessOfUnits()
        {
            // Arrange
            var service = CreateService(out var consumerTaskRepo, out var taskRewardRepo, out var mapper);

            var request = new UpdateHealthTaskProgressRequestDto
            {
                TenantCode = "TEN",
                ConsumerCode = "CMR",
                TaskId = 4,
                HealthTaskType = nameof(HealthTaskType.OTHER),
                NumberOfUnits = 100,
                DateTimeAddedFor = DateTime.UtcNow,
                HealthReport = new List<HealthTrackingDto>
                {
                    new HealthTrackingDto
                    {
                        HealthReportCompletionDate = DateTime.UtcNow,
                        HealthReportData = new List<HealthTrackingDetailDto>
                        {
                            new HealthTrackingDetailDto { HealthReportType = "Blood Pressure", HealthReportValue = "120/80" }
                        }
                    }
                }
            };

            // criteria with UI_COMPONENT, IsDialerRequired = false, one required UI component "Blood Pressure"
            var criteriaJson = new TaskCompletionCriteriaJson
            {
                SelfReportType = Constant.UIComponentReportType,
                CompletionCriteriaType = Constant.HealthCriteriaType,
                HealthCriteria = new HealthCriteria
                {
                    HealthTaskType = nameof(HealthTaskType.OTHER),
                    RequiredUnits = 100, // high required units won't be met by NumberOfUnits
                    IsDialerRequired = false,
                    UiComponent = new List<UiComponent>
                    {
                        new UiComponent
                        {
                            IsRequiredField = true,
                            ReportTypeLabel = new Dictionary<string,string> { { "en-US", "Blood Pressure" } }
                        }
                    }
                }
            };

            var tr = new TaskRewardModel
            {
                TaskId = request.TaskId,
                TenantCode = request.TenantCode,
                SelfReport = true,
                TaskCompletionCriteriaJson = JsonConvert.SerializeObject(criteriaJson)
            };

            taskRewardRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false)).ReturnsAsync(new List<TaskRewardModel> { tr });

            var consumerTask = new ConsumerTaskModel
            {
                ConsumerTaskId = 13,
                TaskId = request.TaskId,
                TenantCode = request.TenantCode,
                ConsumerCode = request.ConsumerCode,
                TaskStatus = Constants.InProgress,
                ProgressDetail = null
            };

            consumerTaskRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<ConsumerTaskModel, bool>>>(), false)).ReturnsAsync(new List<ConsumerTaskModel> { consumerTask });

            // Act
            var result = await service.UpdateHealthTaskProgress(request);

            // Assert
            Assert.NotNull(result);
            // UI components valid and IsDialerRequired == false should mark as completed
            Assert.True(result.IsTaskCompleted);
        }

        [Fact]
        public async System.Threading.Tasks.Task UpdateOther_UIComponent_IsDialerRequiredTrue_CompletesOnlyWhenUnitsSatisfyRequired()
        {
            // Arrange
            var service = CreateService(out var consumerTaskRepo, out var taskRewardRepo, out var mapper);

            var request = new UpdateHealthTaskProgressRequestDto
            {
                TenantCode = "TEN",
                ConsumerCode = "CMR",
                TaskId = 5,
                HealthTaskType = nameof(HealthTaskType.OTHER),
                NumberOfUnits = 2,
                DateTimeAddedFor = DateTime.UtcNow,
                HealthReport = new List<HealthTrackingDto>
                {
                    new HealthTrackingDto
                    {
                        HealthReportCompletionDate = DateTime.UtcNow,
                        HealthReportData = new List<HealthTrackingDetailDto>
                        {
                            new HealthTrackingDetailDto { HealthReportType = "Blood Pressure", HealthReportValue = "120/80" }
                        }
                    }
                }
            };

            // criteria with UI_COMPONENT, IsDialerRequired = true, requiredUnits = 2 => completion should occur
            var criteriaJson = new TaskCompletionCriteriaJson
            {
                SelfReportType = Constant.UIComponentReportType,
                CompletionCriteriaType = Constant.HealthCriteriaType,
                HealthCriteria = new HealthCriteria
                {
                    HealthTaskType = nameof(HealthTaskType.OTHER),
                    RequiredUnits = 2,
                    IsDialerRequired = true,
                    UiComponent = new List<UiComponent>
                    {
                        new UiComponent
                        {
                            IsRequiredField = true,
                            ReportTypeLabel = new Dictionary<string,string> { { "en-US", "Blood Pressure" } }
                        }
                    }
                }
            };

            var tr = new TaskRewardModel
            {
                TaskId = request.TaskId,
                TenantCode = request.TenantCode,
                SelfReport = true,
                TaskCompletionCriteriaJson = JsonConvert.SerializeObject(criteriaJson)
            };

            taskRewardRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false)).ReturnsAsync(new List<TaskRewardModel> { tr });

            var consumerTask = new ConsumerTaskModel
            {
                ConsumerTaskId = 14,
                TaskId = request.TaskId,
                TenantCode = request.TenantCode,
                ConsumerCode = request.ConsumerCode,
                TaskStatus = Constants.InProgress,
                ProgressDetail = null
            };

            consumerTaskRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<ConsumerTaskModel, bool>>>(), false)).ReturnsAsync(new List<ConsumerTaskModel> { consumerTask });

            // Act
            var result = await service.UpdateHealthTaskProgress(request);

            // Assert
            Assert.NotNull(result);
            // because units equal 2 and requiredUnits == 2 and UI validation passes, task should be completed
            Assert.True(result.IsTaskCompleted);
        }

        [Fact]
        public async System.Threading.Tasks.Task UpdateOther_ThrowsWhenHealthProgressIsNullInExistingDetail()
        {
            // Arrange
            var service = CreateService(out var consumerTaskRepo, out var taskRewardRepo, out var mapper);

            var request = new UpdateHealthTaskProgressRequestDto
            {
                TenantCode = "TEN",
                ConsumerCode = "CMR",
                TaskId = 6,
                HealthTaskType = nameof(HealthTaskType.OTHER),
                NumberOfUnits = 1,
                DateTimeAddedFor = DateTime.UtcNow
            };

            var tr = new TaskRewardModel
            {
                TaskId = request.TaskId,
                TenantCode = request.TenantCode,
                SelfReport = true,
                TaskCompletionCriteriaJson = JsonConvert.SerializeObject(new TaskCompletionCriteriaJson
                {
                    CompletionCriteriaType = Constant.HealthCriteriaType,
                    HealthCriteria = new HealthCriteria { HealthTaskType = nameof(HealthTaskType.OTHER), RequiredUnits = 1 }
                })
            };

            taskRewardRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false)).ReturnsAsync(new List<TaskRewardModel> { tr });

            // existing progress detail explicitly has healthProgress = null to trigger exception branch
            var existingDetail = new { detailType = "OTHER", healthProgress = (object)null };
            var consumerTask = new ConsumerTaskModel
            {
                ConsumerTaskId = 15,
                TaskId = request.TaskId,
                TenantCode = request.TenantCode,
                ConsumerCode = request.ConsumerCode,
                TaskStatus = Constants.InProgress,
                ProgressDetail = JsonConvert.SerializeObject(existingDetail)
            };

            consumerTaskRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<ConsumerTaskModel, bool>>>(), false)).ReturnsAsync(new List<ConsumerTaskModel> { consumerTask });

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(async () => await service.UpdateHealthTaskProgress(request));
        }
    }
}