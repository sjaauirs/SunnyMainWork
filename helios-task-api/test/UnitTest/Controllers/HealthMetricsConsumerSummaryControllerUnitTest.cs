using AutoMapper;
using Google.Api.Gax;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Api.Controller;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Models;
using SunnyRewards.Helios.Task.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Task.Infrastructure.Services;
using SunnyRewards.Helios.Task.Infrastructure.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using StatusCodes = Microsoft.AspNetCore.Http.StatusCodes;
using Task = System.Threading.Tasks.Task;

namespace SunnyRewards.Helios.Task.UnitTest.Controllers
{
    public class HealthMetricsConsumerSummaryControllerUnitTest
    {
        private readonly Mock<ILogger<HealthMetricsConsumerSummaryController>> _mockcontrollerLogger;
        private readonly IHealthMetricsConsumerSummaryService _service;
        private readonly HealthMetricsConsumerSummaryController _controller;
        private readonly Mock<ILogger<HealthMetricsConsumerSummaryService>> _mockLogger;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ITaskRewardRepo> _mockTaskRewardRepo;
        private readonly Mock<NHibernate.ISession> _mockSession;

        public HealthMetricsConsumerSummaryControllerUnitTest()
        {
            _mockcontrollerLogger = new Mock<ILogger<HealthMetricsConsumerSummaryController>>();
            _mockLogger = new Mock<ILogger<HealthMetricsConsumerSummaryService>>();
            _mockMapper = new Mock<IMapper>();
            _mockTaskRewardRepo = new Mock<ITaskRewardRepo>();
            _mockSession = new Mock<NHibernate.ISession>();

            _service = new HealthMetricsConsumerSummaryService(_mockLogger.Object, _mockMapper.Object, _mockTaskRewardRepo.Object, _mockSession.Object);

            _controller = new HealthMetricsConsumerSummaryController(_mockcontrollerLogger.Object, _service);
        }
        [Fact]
        public async System.Threading.Tasks.Task GetHealthMetrics_ReturnsOkResult_WhenSchedulesHealthMetricsAreFound()
        {
            // Arrange
            HealthMetricsRequestDto healthMetricsRequestDto = new HealthMetricsRequestDto { tenantCode = "tenant123" };
            var taskRewardData = new List<TaskRewardModel>
            {
                new TaskRewardModel
                {
                    TenantCode = "tenant123",
                    TaskCompletionCriteriaJson = JsonConvert.SerializeObject(new TaskCompletionCriteriaJson
                    {
                        CompletionCriteriaType = "HEALTH",
                        HealthCriteria = new HealthCriteria { HealthTaskType = "STEPS", RequiredSteps = 1000 }
                    }),
                    RecurrenceDefinitionJson = JsonConvert.SerializeObject(new RecurringDto { Schedules = new Common.Core.Domain.Dtos.ScheduleDto[]
    {
        new Common.Core.Domain.Dtos.ScheduleDto
        {
            StartDate = "01-01",
            ExpiryDate = "12-31"
        }
    }})
                }
            };
            _mockTaskRewardRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false))
                               .ReturnsAsync(taskRewardData);


            // Act
            var result = await _controller.getHealthMetrics(healthMetricsRequestDto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<HealthMetricsSummaryDto>(okResult.Value);
            Assert.Null(response.ErrorCode);
            Assert.Equal(200, okResult.StatusCode);
        }
        [Fact]
        public async System.Threading.Tasks.Task GetHealthMetrics_ReturnsOkResult_WhenMonthlyHealthMetricsAreFound()
        {
            // Arrange
            HealthMetricsRequestDto healthMetricsRequestDto = new HealthMetricsRequestDto { tenantCode = "tenant123" };
            var taskRewardData = new List<TaskRewardModel>
            {
                new TaskRewardModel
                {
                    TenantCode = "tenant123",
                    TaskCompletionCriteriaJson = JsonConvert.SerializeObject(new TaskCompletionCriteriaJson
                    {
                        CompletionCriteriaType = "HEALTH",
                        HealthCriteria = new HealthCriteria { HealthTaskType = "SLEEP", RequiredSteps = 1000 }
                    }),
RecurrenceDefinitionJson = JsonConvert.SerializeObject(new RecurringDto
{
    recurrenceType = "Monthly", // Specify the type of recurrence
    periodic = new PeriodicDto
    {
        period = "MONTH",
        periodRestartDate = 1 // Start on the 1st of every month
    } })
                }
            };

            _mockTaskRewardRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false))
                               .ReturnsAsync(taskRewardData);


            // Act
            var result = await _controller.getHealthMetrics(healthMetricsRequestDto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<HealthMetricsSummaryDto>(okResult.Value);
            Assert.Null(response.ErrorCode);
            Assert.Equal(200, okResult.StatusCode);
        }
        [Fact]
        public async void GetHealthMetrics_ReturnsOkResult_WhenQuaterlyHealthMetricsAreFound()
        {
            // Arrange
            HealthMetricsRequestDto healthMetricsRequestDto = new HealthMetricsRequestDto { tenantCode = "tenant123" };

            var tenantCode = "tenant123";
            var taskRewardData = new List<TaskRewardModel>
            {
                new TaskRewardModel
                {
                    TenantCode = tenantCode,
                    TaskCompletionCriteriaJson = JsonConvert.SerializeObject(new TaskCompletionCriteriaJson
                    {
                        CompletionCriteriaType = "HEALTH",
                        HealthCriteria = new HealthCriteria { HealthTaskType = "SLEEP", RequiredSleep= new RequiredSleep{ 
                         MinSleepDuration=1000} }
                    }),
RecurrenceDefinitionJson = JsonConvert.SerializeObject(new RecurringDto
{
    recurrenceType = "Monthly", // Specify the type of recurrence
    periodic = new PeriodicDto
    {
        period = "QUARTER",
        periodRestartDate = 1 // Start on the 1st of every month
    } })
                }
            };
            _mockTaskRewardRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false))
                               .ReturnsAsync(taskRewardData);


            // Act
            var result = await _controller.getHealthMetrics(healthMetricsRequestDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<HealthMetricsSummaryDto>(okResult.Value);
            Assert.Null(response.ErrorCode);
            Assert.Equal(200, okResult.StatusCode);
           
        }



        [Fact]
        public async System.Threading.Tasks.Task GetHealthMetrics_ReturnsInternalServerError_WhenExceptionOccurs()
        {
            // Arrange
            HealthMetricsRequestDto healthMetricsRequestDto = new HealthMetricsRequestDto { tenantCode = "tenant123" };

            var tenantCode = "tenant123";
            var exceptionMessage = "An error occurred while fetching data.";
            _mockTaskRewardRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await _controller.getHealthMetrics(healthMetricsRequestDto);

            // Assert
            var actionResult = Assert.IsType<ObjectResult>(result);
            var returnValue = Assert.IsType<HealthMetricsSummaryDto>(actionResult.Value);
            Assert.Equal(StatusCodes.Status500InternalServerError, returnValue.ErrorCode);
        }
    }
}
