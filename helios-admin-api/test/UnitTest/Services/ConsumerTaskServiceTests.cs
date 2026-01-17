using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using SunnyBenefits.Fis.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Bff.Infrastructure.Services;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SunnyRewards.Helios.Admin.UnitTest.Services
{
  
        public class ConsumerTaskServiceTests
    {
        private readonly Mock<ILogger<ConsumerTaskService>> _consumerTaskServiceLoggerMock;
        private readonly Mock<IWalletClient> _walletClientMock;
        private readonly Mock<ITaskClient> _taskClientMock;
        private readonly Mock<ITenantClient> _tenantClientMock;
        private readonly Mock<IUserClient> _userClientMock;
        private readonly Mock<IConfiguration> _configMock;
        private readonly Mock<IFisClient> _fisClientMock;
        private readonly Mock<ICohortConsumerTaskService> _cohortConsumerTaskServiceMock;
        private readonly Mock<NHibernate.ISession> _sessionMock;
        private readonly ConsumerTaskService _service;
        public readonly Mock<ICohortClient> _cohortClient;
        public readonly Mock<ICmsClient> _cmsClient;
        private readonly Mock<ITaskCommonHelper> _taskCommonHelper;
        private readonly Mock<IWalletTypeService> _walletTypeService;
        private readonly Mock<IEventService> _eventService; 

        public ConsumerTaskServiceTests()
            {
            _consumerTaskServiceLoggerMock = new Mock<ILogger<ConsumerTaskService>>();
            _walletClientMock = new Mock<IWalletClient>();
            _taskClientMock = new Mock<ITaskClient>();
            _tenantClientMock = new Mock<ITenantClient>();
            _userClientMock = new Mock<IUserClient>();
            _configMock = new Mock<IConfiguration>();
            _fisClientMock = new Mock<IFisClient>();
            _cohortClient = new Mock<ICohortClient>();
            _taskCommonHelper = new Mock<ITaskCommonHelper>();
            _cohortConsumerTaskServiceMock = new Mock<ICohortConsumerTaskService>();
            _sessionMock = new Mock<NHibernate.ISession>();
            _cmsClient = new Mock<ICmsClient>();
            _walletTypeService = new Mock<IWalletTypeService>();
            _eventService = new Mock<IEventService>();

            _service = new ConsumerTaskService(
                _consumerTaskServiceLoggerMock.Object,
                _walletClientMock.Object,
                _taskClientMock.Object,
                _tenantClientMock.Object,
                _userClientMock.Object,
                _configMock.Object,
                _fisClientMock.Object,
                _cohortConsumerTaskServiceMock.Object,
                _sessionMock.Object,
                _cohortClient.Object,
                _taskCommonHelper.Object,
                _cmsClient.Object,
                _walletTypeService.Object,
                _eventService.Object
            );
        }
        [Fact]
        public async System.Threading.Tasks.Task PostConsumerTasks_ShouldReturnEmptyDto_WhenConsumerNotFound()
        {
            // Arrange
            var consumerTaskDto = new CreateConsumerTaskDto { ConsumerCode = "123", TenantCode = "tenant123" };
            var getConsumerResponse = new GetConsumerResponseDto { Consumer = null }; // Simulating not found consumer

            // Mocking the Post method of _userClient
            _userClientMock.Setup(u => u.Post<GetConsumerResponseDto>("consumer/get-consumer", It.IsAny<GetConsumerRequestDto>()))
                .ReturnsAsync(getConsumerResponse);

            // Act
            var result = await _service.PostConsumerTasks(consumerTaskDto);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.ConsumerTask); // No consumer task should be returned
        }

        [Fact]
        public async System.Threading.Tasks.Task PostConsumerTasks_ShouldReturnConsumerTaskDto_WhenConsumerTaskCreatedSuccessfully()
        {
            // Arrange
            var consumerTaskDto = new CreateConsumerTaskDto { ConsumerCode = "123", TenantCode = "tenant123" };
            var getConsumerResponse = new GetConsumerResponseDto { Consumer = new ConsumerDto() }; // Simulating found consumer
            var consumerTaskResponse = new ConsumerTaskResponseUpdateDto { ConsumerTask = new ConsumerTaskDto { ConsumerTaskId = 1 } }; // Simulating successful consumer task creation

            // Mocking the Post method of _userClient
            _userClientMock.Setup(u => u.Post<GetConsumerResponseDto>("consumer/get-consumer", It.IsAny<GetConsumerRequestDto>()))
                .ReturnsAsync(getConsumerResponse);

            // Mocking the Post method of _taskClient
            _taskClientMock.Setup(t => t.Post<ConsumerTaskResponseUpdateDto>("consumer-task", consumerTaskDto))
                .ReturnsAsync(consumerTaskResponse);

            // Act
            var result = await _service.PostConsumerTasks(consumerTaskDto);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.ConsumerTask); // Consumer task should be returned
            Assert.Equal(1, result.ConsumerTask.ConsumerTaskId); // Consumer task should have the correct ID
        }

        [Fact]
        public async System.Threading.Tasks.Task PostConsumerTasks_ShouldLogErrorAndThrow_WhenExceptionOccurs()
        {
            // Arrange
            var consumerTaskDto = new CreateConsumerTaskDto { ConsumerCode = "123", TenantCode = "tenant123" };

            // Mocking the Post method of _userClient to throw an exception
            _userClientMock.Setup(u => u.Post<GetConsumerResponseDto>("consumer/get-consumer", It.IsAny<GetConsumerRequestDto>()))
                .ThrowsAsync(new Exception("Test exception"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _service.PostConsumerTasks(consumerTaskDto));
            Assert.Equal("Test exception", exception.Message);
        }

        [Fact]
        public async System.Threading.Tasks.Task GetConsumersByCompletedTask_ReturnsValidResponse()
        {
            // Arrange
            var request = new GetConsumerTaskByTaskId
            {
                TaskId = 123,
                TenantCode = "TENANT1"
            };

            _taskClientMock.Setup(x => x.Post<PageinatedCompletedConsumerTaskResponseDto>(
                It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(new PageinatedCompletedConsumerTaskResponseDto
                {
                    CompletedTasks = new List<ConsumerTaskDto> { new() { ConsumerCode = "C123" } },
                    TotalRecords = 1
                });

            var CandP = new List<ConsumersAndPersons>();
            CandP.Add(new ConsumersAndPersons() { Person = new PersonDto() , Consumer = new ConsumerDto()});

            _userClientMock.Setup(x => x.Post<ConsumersAndPersonsListResponseDto>(
                It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(new ConsumersAndPersonsListResponseDto
                {
                    ConsumerAndPersons = CandP
                });

            // Act
            var result = await _service.GetConsumersByCompletedTask(request);

            // Assert
            Assert.NotNull(result.consumerwithTask);
            Assert.Equal(1, result.totalconsumersTasks);
        }

        [Fact]
        public async System.Threading.Tasks.Task GetConsumersByCompletedTask_ReturnsNotFound_WhenNoCompletedTasks()
        {
            _taskClientMock.Setup(x => x.Post<PageinatedCompletedConsumerTaskResponseDto>(
                It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(new PageinatedCompletedConsumerTaskResponseDto
                {
                    CompletedTasks = null,
                    ErrorCode = 404
                });

            var result = await _service.GetConsumersByCompletedTask(new GetConsumerTaskByTaskId());

            Assert.Equal(404, result.ErrorCode);
        }

        [Fact]
        public async System.Threading.Tasks.Task GetConsumersByCompletedTask_ReturnsNotFound_WhenNoValidConsumerCodes()
        {
            _taskClientMock.Setup(x => x.Post<PageinatedCompletedConsumerTaskResponseDto>(
                It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(new PageinatedCompletedConsumerTaskResponseDto
                {
                    CompletedTasks = new List<ConsumerTaskDto> { new() { ConsumerCode = null } }
                });

            var result = await _service.GetConsumersByCompletedTask(new GetConsumerTaskByTaskId());

            Assert.Equal(404, result.ErrorCode);
        }

        [Fact]
        public async System.Threading.Tasks.Task GetConsumersByCompletedTask_ThrowsException_OnClientFailure()
        {
            _taskClientMock.Setup(x => x.Post<PageinatedCompletedConsumerTaskResponseDto>(
                It.IsAny<string>(), It.IsAny<object>()))
                .ThrowsAsync(new Exception("Client failure"));

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.GetConsumersByCompletedTask(new GetConsumerTaskByTaskId()));
        }

        [Fact]
        public async System.Threading.Tasks.Task IsValidConsumerAccount_ReturnsTrue_WhenAccountIsValid()
        {
            var request = new GetConsumerAccountRequestDto { ConsumerCode = "123" };
            var response = new GetConsumerAccountResponseDto
            {
                ConsumerAccount = new ConsumerAccountDto { ProxyNumber = "1234" }
            };

            _fisClientMock
                .Setup(x => x.Post<GetConsumerAccountResponseDto>(It.IsAny<string>(), request))
                .ReturnsAsync(response);

            var result = await _service.IsValidConsumerAccount(request);

            Assert.True(result);
        }

        [Fact]
        public async System.Threading.Tasks.Task IsValidConsumerAccount_ReturnsFalse_WhenResponseIsNull()
        {
            var request = new GetConsumerAccountRequestDto { ConsumerCode = "123" };

            _fisClientMock
                .Setup(x => x.Post<GetConsumerAccountResponseDto>(It.IsAny<string>(), request))
                .ReturnsAsync((GetConsumerAccountResponseDto)null!);

            var result = await _service.IsValidConsumerAccount(request);

            Assert.False(result);
        }

        [Fact]
        public async System.Threading.Tasks.Task IsValidConsumerAccount_ReturnsFalse_WhenErrorCodeExists()
        {
            var request = new GetConsumerAccountRequestDto { ConsumerCode = "123" };
            var response = new GetConsumerAccountResponseDto
            {
                ErrorCode = 400
            };

            _fisClientMock
                .Setup(x => x.Post<GetConsumerAccountResponseDto>(It.IsAny<string>(), request))
                .ReturnsAsync(response);

            var result = await _service.IsValidConsumerAccount(request);

            Assert.False(result);
        }

        [Fact]
        public async System.Threading.Tasks.Task IsValidConsumerAccount_ReturnsFalse_WhenAccountIsNull()
        {
            var request = new GetConsumerAccountRequestDto { ConsumerCode = "123" };
            var response = new GetConsumerAccountResponseDto
            {
                ConsumerAccount = null
            };

            _fisClientMock
                .Setup(x => x.Post<GetConsumerAccountResponseDto>(It.IsAny<string>(), request))
                .ReturnsAsync(response);

            var result = await _service.IsValidConsumerAccount(request);

            Assert.False(result);
        }

        [Fact]
        public async System.Threading.Tasks.Task IsValidConsumerAccount_ReturnsFalse_WhenProxyNumberIsDefault()
        {
            var request = new GetConsumerAccountRequestDto { ConsumerCode = "123" };
            var response = new GetConsumerAccountResponseDto
            {
                ConsumerAccount = new ConsumerAccountDto
                {
                    ProxyNumber = SunnyRewards.Helios.Admin.Core.Domain.Constants.Constant.ProxyNumberDefaultValue
                }
            };

            _fisClientMock
                .Setup(x => x.Post<GetConsumerAccountResponseDto>(It.IsAny<string>(), request))
                .ReturnsAsync(response);

            var result = await _service.IsValidConsumerAccount(request);

            Assert.False(result);
        }

    }
}
