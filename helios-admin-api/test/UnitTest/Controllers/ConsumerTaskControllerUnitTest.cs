using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SunnyRewards.Helios.ConsumerTask.Api.Controllers;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using Xunit;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Infrastructure.Services;
using AutoFixture;
using SunnyRewards.Helios.Admin.Api.Controllers;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.UnitTest.Helpers.HttpClientsMock;
using Microsoft.Extensions.Configuration;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.UnitTest.Fixtures.MockDtos;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients;

namespace ConsumerTask.Tests
{
    public class ConsumerTaskControllerTests
    {
        private readonly Mock<ILogger<ConsumerTaskController>> _mockLogger;
        private readonly IConsumerTaskService _consumerTaskService;
        private readonly ConsumerTaskController _controller;


        private readonly Mock<ILogger<AdminController>> _consumerTaskLogger;
        private readonly Mock<ILogger<ConsumerTaskService>> _consumerTaskServiceLogger;
        private readonly Mock<ILogger<WalletService>> _walletServiceLogger;
        private readonly Mock<ILogger<ConsumerAccountService>> _consumerAccountServiceLogger;
        private readonly Mock<ILogger<SweepstakesInstanceService>> _sweepstakesInstanceServiceLogger;
        private readonly Mock<IConfiguration> _configuration;
        private readonly Mock<IUserClient> _userClient;
        private readonly Mock<IWalletClient> _walletClient;
        private readonly Mock<ITaskClient> _taskClient;
        private readonly Mock<ITenantClient> _tenantClient;
        private readonly Mock<IFisClient> _fisClient;
        private readonly Mock<ICohortConsumerTaskService> _cohortConsumerTaskService;
        private readonly Mock<ISweepstakesClient> _sweepstakesClient;
        private readonly IWalletService _walletService;
        private readonly IConsumerAccountService _consumerAccountService;
        private readonly ISweepstakesInstanceService _sweepsstakesService;
        private readonly Mock<NHibernate.ISession> _session;
        private readonly AdminController _adminController;
        private readonly IFixture _fixture;
        public readonly Mock<ICohortClient> _cohortClient;
        private readonly Mock<ITaskCommonHelper> _taskCommonHelper;
        private readonly Mock<ICmsClient> _cmsClient;
        private readonly Mock<IWalletTypeService> _walletTypeService;
        private readonly Mock<IEventService> _eventService;

        public ConsumerTaskControllerTests()
        {
            _mockLogger = new Mock<ILogger<ConsumerTaskController>>();

            _configuration = new Mock<IConfiguration>();
            _consumerTaskLogger = new Mock<ILogger<AdminController>>();
            _consumerTaskServiceLogger = new Mock<ILogger<ConsumerTaskService>>();
            _walletServiceLogger = new Mock<ILogger<WalletService>>();
            _consumerAccountServiceLogger = new Mock<ILogger<ConsumerAccountService>>();
            _sweepstakesInstanceServiceLogger = new Mock<ILogger<SweepstakesInstanceService>>();
            _userClient = new UserClientMock();
            _walletClient = new WalletClientMock();
            _fisClient = new FisClientMock();
            _taskClient = new TaskClientMock();
            _sweepstakesClient = new SweepstakesClientMock();
            _tenantClient = new TenantClientMock();
            _cohortClient = new CohortMockClient();
            _taskCommonHelper = new Mock<ITaskCommonHelper>();
            _cohortConsumerTaskService = new Mock<ICohortConsumerTaskService>();
            _session = new Mock<NHibernate.ISession>();
            _fixture = new Fixture();
            _sweepsstakesService = new SweepstakesInstanceService(_sweepstakesInstanceServiceLogger.Object, _sweepstakesClient.Object);
            _consumerAccountService = new ConsumerAccountService(_consumerAccountServiceLogger.Object, _fisClient.Object);
            _cmsClient = new Mock<ICmsClient>();
            _walletTypeService = new Mock<IWalletTypeService>();
            _eventService = new Mock<IEventService>();
            _consumerTaskService = new ConsumerTaskService(_consumerTaskServiceLogger.Object, _walletClient.Object, _taskClient.Object,
            _tenantClient.Object, _userClient.Object, _configuration.Object, _fisClient.Object, 
            _cohortConsumerTaskService.Object, _session.Object, _cohortClient.Object, _taskCommonHelper.Object,_cmsClient.Object, _walletTypeService.Object, _eventService.Object);
            _walletService = new WalletService(_walletServiceLogger.Object, _walletClient.Object, _userClient.Object, _taskClient.Object);

            _controller = new ConsumerTaskController(_mockLogger.Object, _consumerTaskService);
        }

        [Fact]
        public async Task GetAvailableRecurringTask_ReturnsOkResult_WhenTasksAreAvailable()
        {
            // Arrange
            var requestDto = new AvailableRecurringTasksRequestDto
            {
                TenantCode = "TestCode",
                ConsumerCode = "Consumer456",
                TaskAvailabilityTs = DateTime.UtcNow
            };

           

            _userClient.Setup(client => client.Post<GetConsumerResponseDto>("consumer/get-consumer", It.IsAny<BaseRequestDto>()))
               .ReturnsAsync(new GetConsumerResponseMockDto());

            // Act
            var result = await _controller.GetAvailableRecurringTask(requestDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<AvailableRecurringTaskResponseDto>(okResult.Value);

            Assert.NotNull(response.AvailableTasks);
        }
    }
}
