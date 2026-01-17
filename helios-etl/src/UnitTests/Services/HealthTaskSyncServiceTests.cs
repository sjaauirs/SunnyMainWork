using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using SunnyRewards.Helios.ETL.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces;
using SunnyRewards.Helios.ETL.Infrastructure.Services;
using SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

public class HealthTaskSyncServiceTests
{
    private readonly Mock<ILogger<HealthTaskSyncService>> _loggerMock = new();
    private readonly Mock<IHealthClient> _healthClientMock = new();
    private readonly Mock<ITenantRepo> _tenantRepoMock = new();
    private readonly Mock<ITaskRewardRepo> _taskRewardRepoMock = new();
    private readonly Mock<IConsumerRepo> _consumerRepoMock = new();
    private readonly Mock<IHealthMetricRepo> _healthMetricRepoMock = new();
    private readonly Mock<IConsumerTaskRepo> _consumerTaskRepoMock = new();
    private readonly Mock<IConfiguration> _configurationMock = new();
    private readonly Mock<SunnyRewards.Helios.ETL.Common.Helpers.Interfaces.IJsonConvertWrapper> _jsonWrapperMock = new();
    private readonly Mock<IAdminClient> _adminClientMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<IProcessRecurringTasksService> _processRecurringTasksServiceMock = new();

    private HealthTaskSyncService CreateService()
    {
        return new HealthTaskSyncService(
            _loggerMock.Object,
            _healthClientMock.Object,
            _tenantRepoMock.Object,
            _taskRewardRepoMock.Object,
            _consumerRepoMock.Object,
            _healthMetricRepoMock.Object,
            _consumerTaskRepoMock.Object,
            _configurationMock.Object,
            _jsonWrapperMock.Object,
            _adminClientMock.Object,
            _mapperMock.Object,
            _processRecurringTasksServiceMock.Object
        );
    }

    [Fact]
    public async Task UpdateHealthTaskProgress_ValidStepsTask_UpdatesProgress()
    {
        // Arrange
        var service = CreateService();
        var consumerTask = new ETLConsumerTaskModel
        {
            ConsumerCode = "C1",
            TenantCode = "T1",
            TaskId = 1,
            TaskStatus = "inprogress",
            ProgressDetail = null
        };
        var healthMetricRollupResponseDto = new HealthMetriRollupResponseDto
        {
            HealthMetricRollUpData = new HealthMetricRollUpDto()
        };
        var etlTaskReward = new ETLTaskRewardModel
        {
            TenantCode = "T1",
            //TaskCompletionCriteria = new TaskCompletionCriteria { CompletionCriteriaType = "health" }
        };

        var taskRewardDetails = new List<TaskRewardDetailsDto>
        {
            new TaskRewardDetailsDto
            {
                TaskReward = new ETLTaskRewardModel { TaskId = 1, TaskCompletionCriteriaJson = "{\"healthCriteria\": {\"requiredSteps\": 200000, \"healthTaskType\": \"STEPS\"}, \"selfReportType\": \"INPUT\", \"completionPeriodType\": \"MONTH\", \"completionCriteriaType\": \"HEALTH\"}", SelfReport = true },
                Task = new TaskDto { TaskName = "STEPS", SelfReport = true }
            }
        };
        _adminClientMock.Setup(a => a.Get<TaskRewardDetailsResponseDto>(It.IsAny<string>(), It.IsAny<IDictionary<string, long>>(), null))
            .ReturnsAsync(new TaskRewardDetailsResponseDto { TaskRewardDetails = taskRewardDetails });
        _adminClientMock.Setup(a => a.Post<GetTasksAndTaskRewardsResponseDto>(It.IsAny<string>(), It.IsAny<GetTasksAndTaskRewardsRequestDto>(), null))
    .ReturnsAsync(new GetTasksAndTaskRewardsResponseDto
    {
        taskAndTaskRewardDtos = new List<TaskAndTaskRewardDto>() {new TaskAndTaskRewardDto {
        TaskReward = new TaskRewardDto { TaskId = 1, TaskCompletionCriteriaJson = "{}"},
        Task = new SunnyRewards.Helios.Task.Core.Domain.Dtos.TaskDto { TaskName = "STEPS", SelfReport = true }
        } }
    });

        _consumerTaskRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ETLConsumerTaskModel, bool>>>(), false))
            .ReturnsAsync(new List<ETLConsumerTaskModel> { consumerTask });
        _consumerTaskRepoMock.Setup(r => r.UpdateAsync(It.IsAny<ETLConsumerTaskModel>()))
            .ReturnsAsync(consumerTask);

        // Act
        await service.UpdateHealthTaskProgress(consumerTask, healthMetricRollupResponseDto, etlTaskReward);

        // Assert
        _consumerTaskRepoMock.Verify(r => r.UpdateAsync(It.IsAny<ETLConsumerTaskModel>()), Times.Once);
        _loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task UpdateHealthTaskProgress_InvalidHealthTaskType_ReturnsEarly()
    {
        // Arrange
        var service = CreateService();
        var consumerTask = new ETLConsumerTaskModel
        {
            ConsumerCode = "C1",
            TenantCode = "T1",
            TaskId = 1,
            TaskStatus = "inprogress",
            ProgressDetail = null
        };
        var healthMetricRollupResponseDto = new HealthMetriRollupResponseDto
        {
            HealthMetricRollUpData = new HealthMetricRollUpDto()
        };
        var etlTaskReward = new ETLTaskRewardModel
        {
            TenantCode = "T1",
            //TaskCompletionCriteria = new TaskCompletionCriteria { CompletionCriteriaType = "health" }
        };

        var taskRewardDetails = new List<TaskRewardDetailsDto>
        {
            new TaskRewardDetailsDto
            {
                TaskReward = new ETLTaskRewardModel { TaskId = 1, TaskCompletionCriteriaJson = "{}" },
                Task = new TaskDto { TaskName = "INVALID", SelfReport = true }
            }
        };
        _adminClientMock.Setup(a => a.Get<TaskRewardDetailsResponseDto>(It.IsAny<string>(), It.IsAny<IDictionary<string, long>>(), null))
            .ReturnsAsync(new TaskRewardDetailsResponseDto { TaskRewardDetails = taskRewardDetails });
        _consumerTaskRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ETLConsumerTaskModel, bool>>>(), false))
            .ReturnsAsync(new List<ETLConsumerTaskModel> { consumerTask });

        // Act
        await service.UpdateHealthTaskProgress(consumerTask, healthMetricRollupResponseDto, etlTaskReward);

        // Assert
        _consumerTaskRepoMock.Verify(r => r.UpdateAsync(It.IsAny<ETLConsumerTaskModel>()), Times.Never);
    }

    [Fact]
    public async Task UpdateHealthTaskProgress_NonHealthCriteriaType_ReturnsEarly()
    {
        // Arrange
        var service = CreateService();
        var consumerTask = new ETLConsumerTaskModel
        {
            ConsumerCode = "C1",
            TenantCode = "T1",
            TaskId = 1,
            TaskStatus = "inprogress",
            ProgressDetail = null
        };
        var healthMetricRollupResponseDto = new HealthMetriRollupResponseDto
        {
            HealthMetricRollUpData = new HealthMetricRollUpDto()
        };
        var etlTaskReward = new ETLTaskRewardModel
        {
            TenantCode = "T1",
            //TaskCompletionCriteria = new TaskCompletionCriteria { CompletionCriteriaType = "other" }
        };

        var taskRewardDetails = new List<TaskRewardDetailsDto>
        {
            new TaskRewardDetailsDto
            {
                TaskReward = new ETLTaskRewardModel { TaskId = 1, TaskCompletionCriteriaJson = "{}" },
                Task = new TaskDto { TaskName = "STEPS", SelfReport = true }
            }
        };
        _adminClientMock.Setup(a => a.Get<TaskRewardDetailsResponseDto>(It.IsAny<string>(), It.IsAny<IDictionary<string, long>>(), null))
            .ReturnsAsync(new TaskRewardDetailsResponseDto { TaskRewardDetails = taskRewardDetails });
        _consumerTaskRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ETLConsumerTaskModel, bool>>>(), false))
            .ReturnsAsync(new List<ETLConsumerTaskModel> { consumerTask });

        // Act
        await service.UpdateHealthTaskProgress(consumerTask, healthMetricRollupResponseDto, etlTaskReward);

        // Assert
        _consumerTaskRepoMock.Verify(r => r.UpdateAsync(It.IsAny<ETLConsumerTaskModel>()), Times.Never);
    }
}

// Dummy DTOs for test compilation
public class TaskRewardDetailsResponseDto
{
    public List<TaskRewardDetailsDto> TaskRewardDetails { get; set; }
}
public class TaskRewardDetailsDto
{
    public ETLTaskRewardModel TaskReward { get; set; }
    public TaskDto Task { get; set; }
}
public class TaskDto
{
    public string TaskName { get; set; }
    public bool SelfReport { get; set; }
}
