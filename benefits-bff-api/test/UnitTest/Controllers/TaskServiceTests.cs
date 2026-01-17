using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Sunny.Benefits.Bff.Infrastructure.HttpClients.Interfaces;
using Sunny.Benefits.Bff.Infrastructure.Repositories.Interfaces;
using Sunny.Benefits.Bff.Infrastructure.Services;
using Sunny.Benefits.Bff.Infrastructure.Services.Interfaces;
using Sunny.Benefits.Bff.UnitTest.HttpClients;
using SunnyRewards.Helios.Cohort.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;
using Xunit;

namespace Sunny.Benefits.Bff.Tests.Infrastructure.Services
{
    public class TaskServiceTests
    {
        private readonly Mock<ILogger<TaskService>> _loggerMock;
        private readonly Mock<ITaskClient> _taskClientMock;
        private readonly TaskService _taskService;
        private readonly Mock<ICohortClient> _cohortClient;
        private readonly IConfiguration _configuration;

        public TaskServiceTests()
        {
            _loggerMock = new Mock<ILogger<TaskService>>();
            _taskClientMock = new Mock<ITaskClient>();
            _cohortClient = new Mock<ICohortClient>();
            _configuration = new ConfigurationBuilder()
          .SetBasePath(Directory.GetCurrentDirectory())
          .AddJsonFile("appsettings.json")
          .Build();
            _taskService = new TaskService(_loggerMock.Object, _taskClientMock.Object,
                  _cohortClient.Object, _configuration);
        }

        [Fact]
        public async Task GetConsumerTasks_ShouldReturnNotFound_WhenTasksFoundandRecommended()
        {
            // Arrange
            var consumerCode = "consumer123";
            var tenantCode = "tenant456";
            var languageCode = "en-US";

            var tenant = new TenantDto { RecommendedTask = true, TenantCode = tenantCode, TenantAttribute = "{\"ux\":{\"themeColors\":{\"accent1\":\"#3B6676\",\"accent2\":\"#9CD4C6\",\"accent3\":\"#FFFFFF\",\"accent4\":\"#293A40\",\"accent5\":\"#949C99\",\"accent6\":\"#212121\",\"accent7\":\"#3B66767F\",\"accent8\":\"#151E23\",\"primaryaccent\":\"#151E23\"}},\"trivia\":{\"startupTrivia\":true},\"consumerWallet\":{\"ownerMaximum\":500,\"walletMaximum\":500,\"contributorMaximum\":200},\"nonMonetaryOnly\":false,\"spinwheelTaskEnabled\":false,\"monetarySplashScreens\":[\"/assets/images/ftue-swiper1.svg\"],\"nonMonetaryFTUEFileName\":\"/assets/images/bf_nonmononly_ftue_new.svg\",\"bf_monetarySplashScreens\":[\"/assets/images/bf_ftue-swiper1.svg\",\"/assets/images/bf_ftue-swiper2.svg\"],\"nonMonetarySplashScreens\":[\"/assets/images/ftue-swiper2.svg\"],\"nonmonetaryPrizesEnabled\":true,\"bf_nonMonetarySplashScreens\":[\"/assets/images/bf_ftue-swiper2.svg\"],\"supportLiveTransferToRewardsPurse\":true, \"costcoMemberShipSupport\":false}" };

            var availableTasks = new List<TaskRewardDetailDto>
        {
            new TaskRewardDetailDto { TaskReward = new TaskRewardDto { TaskRewardCode = "R1", Priority = 1 } },
            new TaskRewardDetailDto { TaskReward = new TaskRewardDto { TaskRewardCode = "R2", Priority = 2 } },
            new TaskRewardDetailDto { TaskReward = new TaskRewardDto { TaskRewardCode = "Tas567565kb54", Priority =  5} }
            };

            var recommendedTasksResponse = 

            _taskClientMock
                .Setup(client => client.Post<ConsumerTaskResponseDto>("get-all-consumer-tasks", It.IsAny<ConsumerTaskRequestDto>()))
                .ReturnsAsync(new ConsumerTaskResponseDto
                {
                    CompletedTasks = new List<TaskRewardDetailDto>(),
                    PendingTasks = new List<TaskRewardDetailDto>(),
                    AvailableTasks = availableTasks
                });

            var rewardTypeResponse = new RewardTypeResponseDto()
            {
                RewardTypeDto = new TaskRewardTypeDto()
                {
                    RewardTypeId = 1,
                    RewardTypeCode = "rtc-a5a943d3fc2a4506ab12218204d60805",
                    RewardTypeDescription = "Description",
                    RewardTypeName = "Reward Name"
                }

            };

            _taskClientMock.Setup(client => client.Post<RewardTypeResponseDto>("reward-type-code", It.IsAny<RewardTypeCodeRequestDto>()))
              .ReturnsAsync(rewardTypeResponse);

            _cohortClient
            .Setup(c => c.Post<GetConsumerRecommendedTasksResponseDto>("cohort/consumer-recommended-tasks", It.IsAny<GetConsumerRecommendedTasksRequestDto>()))
            .ReturnsAsync(new GetConsumerRecommendedTasksResponseMockDto());


            // Act
            var result = await _taskService.GetConsumerTasks(consumerCode, tenantCode, tenant, languageCode);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.AvailableTasks.Count);
            Assert.Equal(5, result.AvailableTasks[0].TaskReward.Priority);
            Assert.Empty(result.PendingTasks);
            Assert.Empty(result.CompletedTasks);
        }

        [Fact]
        public async Task GetConsumerTasks_ShouldReturnTasks_WhenTasksFound_notRecommended()
        {
            // Arrange
            var consumerCode = "consumer123";
            var tenantCode = "tenant456";
            var languageCode = "en-US";
            var expectedTasks = new ConsumerTaskResponseDto
            {
                CompletedTasks = new List<TaskRewardDetailDto> { new TaskRewardDetailDto { Task = new TaskDto() { } , TaskReward= new TaskRewardDto() { } } },
                PendingTasks = new List<TaskRewardDetailDto> { new TaskRewardDetailDto { Task = new TaskDto() { }, TaskReward = new TaskRewardDto() { } } },
            };

            var tenant = new TenantDto { RecommendedTask = false, TenantCode = tenantCode };

            _taskClientMock
                .Setup(client => client.Post<ConsumerTaskResponseDto>("get-all-consumer-tasks", It.IsAny<ConsumerTaskRequestDto>()))
                .ReturnsAsync(expectedTasks);

            _cohortClient
        .Setup(c => c.Post<GetConsumerRecommendedTasksResponseDto>("cohort/consumer-recommended-tasks", It.IsAny<GetConsumerRecommendedTasksRequestDto>()))
        .ReturnsAsync(new GetConsumerRecommendedTasksResponseMockDto());


            // Act
            var result = await _taskService.GetConsumerTasks(consumerCode, tenantCode, tenant, languageCode);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedTasks.CompletedTasks.Count, result.CompletedTasks?.Count);
            Assert.Equal(expectedTasks.PendingTasks.Count, result.PendingTasks?.Count);
            Assert.Null(result.ErrorCode);
        }


    }
    public class GetConsumerRecommendedTasksResponseMockDto : GetConsumerRecommendedTasksResponseDto
    {
        public GetConsumerRecommendedTasksResponseMockDto()
        {
            TaskRewards = new List<TaskRewardPriorityDto>
            {
                new TaskRewardPriorityDto()
            {

                Priority = 5,
                TaskRewardCode="Tas567565kb54"
            }
            };
        }
    }
}
