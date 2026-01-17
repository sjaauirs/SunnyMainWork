using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using SunnyRewards.Helios.Admin.Infrastructure.Services;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using Xunit;

namespace SunnyRewards.Helios.Admin.UnitTest.Services
{
    public class AutoEnrollConsumerTaskServiceTests
    {
        private readonly Mock<ILogger<AutoEnrollConsumerTaskService>> _loggerMock;
        private readonly Mock<ITaskClient> _taskClientMock;
        private readonly Mock<IUserClient> _userClientMock;
        private readonly AutoEnrollConsumerTaskService _service;

        public AutoEnrollConsumerTaskServiceTests()
        {
            _loggerMock = new Mock<ILogger<AutoEnrollConsumerTaskService>>();
            _taskClientMock = new Mock<ITaskClient>();
            _userClientMock = new Mock<IUserClient>();

            _service = new AutoEnrollConsumerTaskService(_loggerMock.Object, _taskClientMock.Object, _userClientMock.Object);
        }

        [Fact]
        public void EnrollConsumerTask_MissingParameters_ReturnsBadRequest()
        {
            // Arrange
            var request = new AutoEnrollConsumerTaskRequestDto
            {
                TenantCode = string.Empty, // Missing TenantCode
                ConsumerCode = "Consumer123",
                TaskExternalCode = "Task123"
            };

            // Act
            var result = _service.EnrollConsumerTask(request);

            // Assert
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
            Assert.Equal("One or more required parameters are missing.", result.ErrorMessage);
        }

        [Fact]
        public async System.Threading.Tasks.Task EnrollConsumerTask_ConsumerNotFound_ReturnsNotFound()
        {
            // Arrange
            var request = new AutoEnrollConsumerTaskRequestDto
            {
                TenantCode = "Tenant123",
                ConsumerCode = "Consumer123",
                TaskExternalCode = "Task123"
            };

            _userClientMock.Setup(x => x.Post<GetConsumerResponseDto>("consumer/get-consumer", It.IsAny<GetConsumerRequestDto>()));
                           

            // Act
            var result = _service.EnrollConsumerTask(request);

            // Assert
            Assert.Equal(StatusCodes.Status404NotFound, result.ErrorCode);
            Assert.Equal("Consumer not found or invalid consumer code.", result.ErrorMessage);
        }

        [Fact]
        public async System.Threading.Tasks.Task EnrollConsumerTask_TenantCodeMismatch_ReturnsNotFound()
        {
            // Arrange
            var request = new AutoEnrollConsumerTaskRequestDto
            {
                TenantCode = "Tenant123",
                ConsumerCode = "Consumer123",
                TaskExternalCode = "Task123"
            };

            var consumerResponse = new GetConsumerResponseDto
            {
                Consumer = new ConsumerDto
                {
                    ConsumerCode = "Consumer123",
                    TenantCode = "DifferentTenant"
                }
            };

            _userClientMock.Setup(x => x.Post<GetConsumerResponseDto>("consumer/get-consumer", It.IsAny<GetConsumerRequestDto>()))
                           .ReturnsAsync(consumerResponse); // Simulating tenant mismatch

            // Act
            var result = _service.EnrollConsumerTask(request);

            // Assert
            Assert.Equal(StatusCodes.Status404NotFound, result.ErrorCode);
            Assert.Equal("Tenant code does not match the consumer's tenant.", result.ErrorMessage);
        }

        [Fact]
        public async System.Threading.Tasks.Task EnrollConsumerTask_TaskFetchFailed_ReturnsError()
        {
            // Arrange
            var request = new AutoEnrollConsumerTaskRequestDto
            {
                TenantCode = "Tenant123",
                ConsumerCode = "Consumer123",
                TaskExternalCode = "Task123"
            };

            var consumerResponse = new GetConsumerResponseDto
            {
                Consumer = new ConsumerDto
                {
                    ConsumerCode = "Consumer123",
                    TenantCode = "Tenant123"
                }
            };

            _userClientMock.Setup(x => x.Post<GetConsumerResponseDto>("consumer/get-consumer", It.IsAny<GetConsumerRequestDto>()))
                           .ReturnsAsync(consumerResponse); // Simulating consumer found

            _taskClientMock.Setup(x => x.Get<TaskRewardDetailsResponseDto>(It.IsAny<string>(), null))
                           .ReturnsAsync(new TaskRewardDetailsResponseDto
                           {
                               ErrorCode = StatusCodes.Status500InternalServerError,
                               ErrorMessage = "Task fetch error"
                           });

            // Act
            var result = _service.EnrollConsumerTask(request);

            // Assert
            Assert.Equal(StatusCodes.Status500InternalServerError, result.ErrorCode);
            Assert.Equal("Task fetch error", result.ErrorMessage);
        }

        [Fact]
        public async System.Threading.Tasks.Task EnrollConsumerTask_Success_ReturnsSuccess()
        {
            // Arrange
            var request = new AutoEnrollConsumerTaskRequestDto
            {
                TenantCode = "Tenant123",
                ConsumerCode = "Consumer123",
                TaskExternalCode = "Task123"
            };

            var consumerResponse = new GetConsumerResponseDto
            {
                Consumer = new ConsumerDto
                {
                    ConsumerCode = "Consumer123",
                    TenantCode = "Tenant123"
                }
            };

            var taskRewardDetailsResponse = new TaskRewardDetailsResponseDto
            {
                TaskRewardDetails = new List<TaskRewardDetailsDto>
                {
                    new TaskRewardDetailsDto
                    {
                        Task = new TaskDto
                        {
                            TaskId = 1
                        }
                    }
                }
            };

            _userClientMock.Setup(x => x.Post<GetConsumerResponseDto>("consumer/get-consumer", It.IsAny<GetConsumerRequestDto>()))
                           .ReturnsAsync(consumerResponse); // Simulating consumer found

            _taskClientMock.Setup(x => x.Get<TaskRewardDetailsResponseDto>(It.IsAny<string>(), null))
                           .ReturnsAsync(taskRewardDetailsResponse); // Simulating successful task fetch

            _taskClientMock.Setup(x => x.Post<ConsumerTaskResponseUpdateDto>(It.IsAny<string>(), It.IsAny<CreateConsumerTaskDto>()))
                           .ReturnsAsync(new ConsumerTaskResponseUpdateDto()); // Simulating successful task creation

            // Act
            var result = _service.EnrollConsumerTask(request);

            // Assert
            Assert.Null(result.ErrorCode);
        }
    }
}
