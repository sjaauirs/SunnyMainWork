using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Sunny.Benefits.Bff.Api.Controllers;
using Sunny.Benefits.Bff.Core.Constants;
using Sunny.Benefits.Bff.Core.Domain.Dtos;
using Sunny.Benefits.Bff.Infrastructure.Helpers.Interface;
using Sunny.Benefits.Bff.Infrastructure.HttpClients.Interfaces;
using Sunny.Benefits.Bff.Infrastructure.Services;
using Sunny.Benefits.Bff.Infrastructure.Services.Interfaces;
using Sunny.Benefits.Bff.UnitTest.HttpClients;
using SunnyBenefits.Fis.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;
using SunnyRewards.Helios.Tenant.Core.Domain.Models;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Models;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;
using System.Net;
using Xunit;

namespace Sunny.Benefits.Bff.UnitTest.Controllers
{
    public class ConsumerSummaryControllerUnitTest
    {
        private readonly Mock<IAuth0Helper> _mockAuth0Helper;
        private readonly Mock<ILoginService> _mockLoginService;
        private readonly Mock<ITenantService> _mockTenantService;
        private readonly Mock<IWalletService> _mockWalletService;
        private readonly Mock<ILogger<ConsumerSummaryService>> _mockLogger;
        private readonly ConsumerSummaryService _consumerSummaryService;
        private readonly Mock<ITaskClient> _taskClient;
        private readonly Mock<IFisClient> _fisClient;
        private readonly Mock<ILogger<ConsumerSummaryController>> _mockControllerLogger;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessor;
        private ConsumerSummaryController _controller;
        private readonly Mock<ITaskService> _mockTaskService; private readonly Mock<ITenantAccountService> _mockTenantAccountService;
        private readonly Mock<IConsumerSummaryService> _mockConsumerSummaryService;

        public ConsumerSummaryControllerUnitTest()
        {
            _mockAuth0Helper = new Mock<IAuth0Helper>();
            _mockLoginService = new Mock<ILoginService>();
            _mockTenantService = new Mock<ITenantService>();
            _mockWalletService = new Mock<IWalletService>();
            _taskClient = new TaskClientMock();
            _fisClient = new FisClientMock();
            _mockLogger = new Mock<ILogger<ConsumerSummaryService>>();
            _mockControllerLogger = new Mock<ILogger<ConsumerSummaryController>>();
            _mockTaskService = new Mock<ITaskService>();
            _mockTenantAccountService = new Mock<ITenantAccountService>();
            _mockConsumerSummaryService = new Mock<IConsumerSummaryService>();
            _httpContextAccessor = new Mock<IHttpContextAccessor>();

            _consumerSummaryService = new ConsumerSummaryService(
           _mockLogger.Object,
           _mockAuth0Helper.Object, _taskClient.Object,
           _mockLoginService.Object,
           _mockTenantService.Object,
           _mockWalletService.Object,
           _mockTaskService.Object,
           _mockTenantAccountService.Object, _fisClient.Object, _httpContextAccessor.Object
       );

            _controller = new ConsumerSummaryController(_mockControllerLogger.Object, _consumerSummaryService);
        }

        [Fact]
        public async Task GetConsumerSummary_ReturnsBadRequest_WhenInputIsInvalid()
        {
            // Arrange
            var requestDto = new ConsumerSummaryRequestDto { email = "", consumerCode = "" };

            // Act
            var result = await _controller.ConsumerSummary(requestDto);

            // Assert
            Assert.NotNull(result);
            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
            var responseDto = Assert.IsType<ConsumerSummaryResponseDto>(objectResult.Value);
            Assert.Equal("Invalid Input Data, ConsumerCode is Required", responseDto.ErrorMessage);
        }

        [Fact]
        public async Task GetConsumerSummary_ReturnsInternalServerError_WhenExceptionOccurs()
        {
            // Arrange
            var requestDto = new ConsumerSummaryRequestDto { email = "test@example.com", consumerCode = "testCode" };
            _mockAuth0Helper.Setup(x => x.GetUserById(It.IsAny<GetUserRequestDto>()))
                            .ThrowsAsync(new Exception("Some error in API service call"));

            // Act
            var result = await _controller.ConsumerSummary(requestDto);

            // Assert
            Assert.NotNull(result);
            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetConsumerSummary_ReturnsData_WhenAllServicesSucceed()
        {
            // Arrange
            var requestDto = new ConsumerSummaryRequestDto { email = "test@example.com", consumerCode = "testCode" };
            var consumerByEmail = new GetConsumerByEmailResponseDto { };
            _mockLoginService.Setup(x => x.GetPersonAndConsumerDetails(It.IsAny<string>()))
                             .ReturnsAsync(consumerByEmail);
            _mockAuth0Helper.Setup(x => x.GetUserById(It.IsAny<GetUserRequestDto>()))
                            .ReturnsAsync(new UserGetResponseDto() { });
            _mockTenantService.Setup(x => x.GetTenantByTenantCode(It.IsAny<string>()))
                              .ReturnsAsync(new TenantDto { TenantCode = "tenant123" });
            _mockWalletService.Setup(x => x.GetWallets(It.IsAny<FindConsumerWalletRequestDto>(), null))
                              .ReturnsAsync(new WalletResponseDto { });
            _mockTenantService.Setup(x=>x.CheckCostcoMemberhipSupport(It.IsAny<TenantDto>()))
                              .Returns(true);
            _mockTaskService.Setup(x => x.GetConsumerTasks(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TenantDto>(), It.IsAny<string>()))
                .ReturnsAsync(new ConsumerTaskResponseDto() { });
            _mockTenantAccountService.Setup(x => x.GetTenantAccount(It.Is<TenantAccountCreateRequestDto>(dto => dto.TenantCode == "tenant123")))
                                     .ReturnsAsync(new TenantAccountDto());
            _taskClient.Setup(client => client.Post<HealthMetricsSummaryDto>(CommonConstants.GetHealthMetrics, It.IsAny<string>()))
           .ReturnsAsync(new HealthMetricsSummaryDto
           {
               HealthMetricsQueryStartTsMap = new Dictionary<string, DateTime?>  {
        { "SLEEP", DateTime.Now }
   }
           });


            // Act
            var result = await _controller.ConsumerSummary(requestDto);

            // Assert
            Assert.NotNull(result);
            var objectResult = Assert.IsType<OkObjectResult>(result.Result);
            var responseDto = Assert.IsType<ConsumerSummaryResponseDto>(objectResult.Value);
            Assert.NotNull(responseDto);
        }


        [Fact]
        public async Task GetConsumerSummary_ReturnsCachedData_WhenAllServicesSucceed()
        {
            // Arrange
            var requestDto = new ConsumerSummaryRequestDto { email = "test@example.com", consumerCode = "testCode" };
            var consumerByEmail = new GetConsumerByEmailResponseDto { };
            _mockLoginService.Setup(x => x.GetPersonAndConsumerDetails(It.IsAny<string>()))
                             .ReturnsAsync(consumerByEmail);
            _mockAuth0Helper.Setup(x => x.GetUserById(It.IsAny<GetUserRequestDto>()))
                            .ReturnsAsync(new UserGetResponseDto() { });
            _mockTenantService.Setup(x => x.GetTenantByTenantCode(It.IsAny<string>()))
                              .ReturnsAsync(new TenantDto { TenantCode = "tenant123" });
            _mockWalletService.Setup(x => x.GetWallets(It.IsAny<FindConsumerWalletRequestDto>(), null))
                              .ReturnsAsync(new WalletResponseDto { });
            _mockTenantService.Setup(x => x.CheckCostcoMemberhipSupport(It.IsAny<TenantDto>()))
                              .Returns(true);
            _mockTaskService
     .Setup(x => x.GetConsumerTasks(
         It.IsAny<string>(),
         It.IsAny<string>(),
         It.IsAny<TenantDto>(),
         It.IsAny<string>()))
     .ReturnsAsync(new ConsumerTaskResponseDto
     {
         PendingTasks = new List<TaskRewardDetailDto>
         {
            new TaskRewardDetailDto
            {
                TaskReward = new TaskRewardDto
                {
                    Reward = @"{""rewardType"":""MEMBERSHIP_DOLLARS"",""rewardAmount"":""65"",""membershipType"":""COSTCO""}"
                }
            },
            new TaskRewardDetailDto
            {
                TaskReward = new TaskRewardDto
                {
                }
            },
         },
         CompletedTasks = new List<TaskRewardDetailDto>
         {
            new TaskRewardDetailDto
            {
                TaskReward = new TaskRewardDto
                {
                    Reward = @"{""rewardType"":""MEMBERSHIP_DOLLARS"",""rewardAmount"":""65"",""membershipType"":""COSTCO""}"
                }
            },new TaskRewardDetailDto
            {
                TaskReward = new TaskRewardDto
                {
                }
            }
         },
         AvailableTasks = new List<TaskRewardDetailDto>
         {
            new TaskRewardDetailDto
            {
                TaskReward = new TaskRewardDto
                {
                    Reward = @"{""rewardType"":""MEMBERSHIP_DOLLARS"",""rewardAmount"":""65"",""membershipType"":""COSTCO""}"
                }
            },new TaskRewardDetailDto
            {
                TaskReward = new TaskRewardDto
                {
                }
            }
         }
     }
    );
            _mockTenantAccountService.Setup(x => x.GetTenantAccount(It.Is<TenantAccountCreateRequestDto>(dto => dto.TenantCode == "tenant123")))
                                     .ReturnsAsync(new TenantAccountDto());
            _taskClient.Setup(client => client.Post<HealthMetricsSummaryDto>(CommonConstants.GetHealthMetrics, It.IsAny<string>()))
           .ReturnsAsync(new HealthMetricsSummaryDto
           {
               HealthMetricsQueryStartTsMap = new Dictionary<string, DateTime?>  {
        { "SLEEP", DateTime.Now }
   }
           });


            DefaultHttpContext _httpContext = new DefaultHttpContext();
            var consumerResponse = new GetConsumerByPersonUniqueIdentifierResponseDto
            {
                Person = new PersonDto { PersonUniqueIdentifier = "unique-123" },
                Consumer = new[] { new ConsumerDto() }
            };
            _httpContext.Items["ConsumerInfo"] = consumerResponse;
            _httpContext.Items["TenantInfo"] = new TenantDto { TenantCode = "tenant123" };
            _httpContextAccessor.Setup(a => a.HttpContext)
                .Returns(_httpContext);
            // Act
            var result = await _controller.ConsumerSummary(requestDto);

            // Assert
            Assert.NotNull(result);
            var objectResult = Assert.IsType<OkObjectResult>(result.Result);
            var responseDto = Assert.IsType<ConsumerSummaryResponseDto>(objectResult.Value);
            Assert.NotNull(responseDto);
        }

     

        [Fact]
        public async Task GetConsumerSummary_ReturnsData_WhenAllServicesForHealthMetricsSucceed()
        {
            // Arrange
            var requestDto = new ConsumerSummaryRequestDto { email = "test@example.com", consumerCode = "testCode" };

            var consumerByEmail = new GetConsumerByEmailResponseDto { };
            _mockLoginService.Setup(x => x.GetPersonAndConsumerDetails(It.IsAny<string>()))
                             .ReturnsAsync(consumerByEmail);
            _mockAuth0Helper.Setup(x => x.GetUserById(It.IsAny<GetUserRequestDto>()))
                            .ReturnsAsync(new UserGetResponseDto() { });
            _mockTenantService.Setup(x => x.GetTenantByTenantCode(It.IsAny<string>()))
                              .ReturnsAsync(new TenantDto { TenantCode = "tenant123" });
            _mockWalletService.Setup(x => x.GetWallets(It.IsAny<FindConsumerWalletRequestDto>(), null))
                              .ReturnsAsync(new WalletResponseDto { });
            _mockTenantService.Setup(x => x.CheckCostcoMemberhipSupport(It.IsAny<TenantDto>()))
                              .Returns(true);
            _mockTaskService.Setup(x => x.GetConsumerTasks(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TenantDto>(), It.IsAny<string>()))
                .ReturnsAsync(new ConsumerTaskResponseDto() { });
            _mockTenantAccountService.Setup(x => x.GetTenantAccount(It.Is<TenantAccountCreateRequestDto>(dto => dto.TenantCode == "tenant123")))
                                     .ReturnsAsync(new TenantAccountDto());
            HealthMetricsRequestDto healthMetricsRequestDto = new HealthMetricsRequestDto { tenantCode = "tenant123" };
            _taskClient.Setup(client => client.Post<HealthMetricsSummaryDto>(CommonConstants.GetHealthMetrics, It.IsAny<HealthMetricsRequestDto>()))
           .ReturnsAsync(new HealthMetricsSummaryDto
           {
               HealthMetricsQueryStartTsMap = new Dictionary<string, DateTime?>{
        { "SLEEP", DateTime.Now } }
           });

            // Act
            var result = await _controller.ConsumerSummary(requestDto);

            // Assert
            Assert.NotNull(result);
            var objectResult = Assert.IsType<OkObjectResult>(result.Result);
            var responseDto = Assert.IsType<ConsumerSummaryResponseDto>(objectResult.Value);
            Assert.NotNull(responseDto);
        }
        [Fact]
        public async Task GetConsumerSummary_ReturnsData_Whenhealthmetricexceptionoccour()
        {
            // Arrange
            var requestDto = new ConsumerSummaryRequestDto { email = "test@example.com", consumerCode = "testCode" };

            _mockAuth0Helper.Setup(x => x.GetUserById(It.IsAny<GetUserRequestDto>()))
                            .ReturnsAsync(new UserGetResponseDto() { });
            _mockLoginService.Setup(x => x.GetPersonAndConsumerDetails(It.IsAny<string>()))
                             .ReturnsAsync(new GetConsumerByEmailResponseDto { });
            _mockTenantService.Setup(x => x.GetTenantByTenantCode(It.IsAny<string>()))
                              .ReturnsAsync(new TenantDto { TenantCode = "tenant123" });
            _mockWalletService.Setup(x => x.GetWallets(It.IsAny<FindConsumerWalletRequestDto>(), null))
                              .ReturnsAsync(new WalletResponseDto { });
            _mockTenantService.Setup(x => x.CheckCostcoMemberhipSupport(It.IsAny<TenantDto>()))
                              .Returns(true);
            _mockTaskService.Setup(x => x.GetConsumerTasks(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TenantDto>(), It.IsAny<string>()))
                .ReturnsAsync(new ConsumerTaskResponseDto() { });
            _mockTenantAccountService.Setup(x => x.GetTenantAccount(It.Is<TenantAccountCreateRequestDto>(dto => dto.TenantCode == "tenant123")))
                                     .ReturnsAsync(new TenantAccountDto());
            HealthMetricsRequestDto healthMetricsRequestDto = new HealthMetricsRequestDto { tenantCode = "tenant123" };

            _taskClient.Setup(client => client.Post<HealthMetricsSummaryDto>(CommonConstants.GetHealthMetrics, It.IsAny<HealthMetricsRequestDto>()))
.ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _controller.ConsumerSummary(requestDto);

            // Assert
            Assert.NotNull(result);

        }
        [Fact]
        public async Task GetConsumerSummary_ReturnsData_When_TenantAccountNull()
        {
            // Arrange
            var requestDto = new ConsumerSummaryRequestDto { email = "test@example.com", consumerCode = "testCode" };

            var consumerByEmail = new GetConsumerByEmailResponseDto { };
            _mockLoginService.Setup(x => x.GetPersonAndConsumerDetails(It.IsAny<string>()))
                             .ReturnsAsync(consumerByEmail);
            _mockAuth0Helper.Setup(x => x.GetUserById(It.IsAny<GetUserRequestDto>()))
                            .ReturnsAsync(new UserGetResponseDto() { });
            _mockTenantService.Setup(x => x.GetTenantByTenantCode(It.IsAny<string>()))
                              .ReturnsAsync(new TenantDto { TenantCode = "tenant123" });
            _mockWalletService.Setup(x => x.GetWallets(It.IsAny<FindConsumerWalletRequestDto>(), null))
                              .ReturnsAsync(new WalletResponseDto { });
            _mockTenantService.Setup(x => x.CheckCostcoMemberhipSupport(It.IsAny<TenantDto>()))
                              .Returns(true);
            _mockTaskService.Setup(x => x.GetConsumerTasks(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TenantDto>(), It.IsAny<string>()))
                .ReturnsAsync(new ConsumerTaskResponseDto() { });
            _mockTenantAccountService.Setup(x => x.GetTenantAccount(It.IsAny<TenantAccountCreateRequestDto>()));

            // Act
            var result = await _controller.ConsumerSummary(requestDto);

            // Assert
            Assert.NotNull(result);
            var objectResult = Assert.IsType<OkObjectResult>(result.Result);
            var responseDto = Assert.IsType<ConsumerSummaryResponseDto>(objectResult.Value);
            Assert.NotNull(responseDto);
        }
        [Fact]
        public async Task GetConsumerSummary_ThrowsInvalidDataException_WhenTenantCodeIsNull()
        {
            // Arrange
            var requestDto = new ConsumerSummaryRequestDto { email = "test@example.com", consumerCode = "testCode" };

            _mockAuth0Helper.Setup(x => x.GetUserById(It.IsAny<GetUserRequestDto>()))
                            .ReturnsAsync(new UserGetResponseDto { });
            _mockLoginService.Setup(x => x.GetPersonAndConsumerDetails(It.IsAny<string>()))
                             .ReturnsAsync(new GetConsumerByEmailResponseDto { });
            _mockTenantService.Setup(x => x.GetTenantByTenantCode(It.IsAny<string>()))
                              .ReturnsAsync(new TenantDto()); // Null Tenant

            var result = await _controller.ConsumerSummary(requestDto);

            // Act & Assert
            Assert.NotNull(result);
            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
            var responseDto = Assert.IsType<BaseResponseDto>(objectResult.Value);
            Assert.Equal("Tenant not found for Consumer with tenant code: ", responseDto.ErrorMessage);
        }

        [Fact]
        public async Task GetConsumerSummary_WhenAnyServices_Return_ErrorCode()
        {
            // Arrange
            var requestDto = new ConsumerSummaryRequestDto { email = "test@example.com", consumerCode = "testCode" };

            _mockAuth0Helper.Setup(x => x.GetUserById(It.IsAny<GetUserRequestDto>()))
                            .ReturnsAsync(new UserGetResponseDto() { });
            _mockLoginService.Setup(x => x.GetPersonAndConsumerDetails(It.IsAny<string>()))
                             .ReturnsAsync(new GetConsumerByEmailResponseDto { ErrorCode = 500, ErrorMessage = "Error" });
            _mockTenantService.Setup(x => x.GetTenantByTenantCode(It.IsAny<string>()))
                              .ReturnsAsync(new TenantDto { TenantCode = "tenant123" });
            _mockWalletService.Setup(x => x.GetWallets(It.IsAny<FindConsumerWalletRequestDto>(), null))
                              .ReturnsAsync(new WalletResponseDto { });
            _mockTenantService.Setup(x => x.CheckCostcoMemberhipSupport(It.IsAny<TenantDto>()))
                              .Returns(true);

            _mockTaskService.Setup(x => x.GetConsumerTasks(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TenantDto>(), It.IsAny<string>()))
                .ReturnsAsync(new ConsumerTaskResponseDto() { });

            // Act
            var result = await _controller.ConsumerSummary(requestDto);

            // Assert
            Assert.NotNull(result);
            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            var responseDto = Assert.IsType<BaseResponseDto>(objectResult.Value);
            Assert.Equal("Error", responseDto.ErrorMessage);
        }

        [Fact]
        public async Task GetConsumerSummary_ReturnsData_ForCardIssueStatusFails()
        {
            // Arrange
            var requestDto = new ConsumerSummaryRequestDto { email = "test@example.com", consumerCode = "testCode" };

            var consumerByEmail = new GetConsumerByEmailResponseDto { };
            _mockLoginService.Setup(x => x.GetConsumerByEmail(It.IsAny<string>()))
                             .ReturnsAsync(consumerByEmail);
            _mockAuth0Helper.Setup(x => x.GetUserById(It.IsAny<GetUserRequestDto>()))
                            .ReturnsAsync(new UserGetResponseDto() { });
            _mockTenantService.Setup(x => x.GetTenantByTenantCode(It.IsAny<string>()))
                              .ReturnsAsync(new TenantDto { TenantCode = "tenant123" });
            _mockWalletService.Setup(x => x.GetWallets(It.IsAny<FindConsumerWalletRequestDto>(), null))
                              .ReturnsAsync(new WalletResponseDto { });
            _mockTenantService.Setup(x => x.CheckCostcoMemberhipSupport(It.IsAny<TenantDto>()))
                              .Returns(true);
            _mockTaskService.Setup(x => x.GetConsumerTasks(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TenantDto>(), It.IsAny<string>()))
                .ReturnsAsync(new ConsumerTaskResponseDto() { });
            _mockTenantAccountService.Setup(x => x.GetTenantAccount(It.Is<TenantAccountCreateRequestDto>(dto => dto.TenantCode == "tenant123")))
                                     .ReturnsAsync(new TenantAccountDto());
            HealthMetricsRequestDto healthMetricsRequestDto = new HealthMetricsRequestDto { tenantCode = "tenant123" };
            _taskClient.Setup(client => client.Post<HealthMetricsSummaryDto>(CommonConstants.GetHealthMetrics, It.IsAny<HealthMetricsRequestDto>()))
           .ReturnsAsync(new HealthMetricsSummaryDto
           {
               HealthMetricsQueryStartTsMap = new Dictionary<string, DateTime?>{
               { "SLEEP", DateTime.Now } }
           });

            _fisClient.Setup(client => client.Post<GetConsumerAccountResponseDto>(It.IsAny<string>(), It.IsAny<GetConsumerAccountRequestDto>()))
                .ReturnsAsync(new GetConsumerAccountResponseDto());

            // Act
            var result = await _controller.ConsumerSummary(requestDto);

            // Assert
            Assert.NotNull(result);
            var objectResult = Assert.IsType<OkObjectResult>(result.Result);
            var responseDto = Assert.IsType<ConsumerSummaryResponseDto>(objectResult.Value);
            Assert.NotNull(responseDto);
        }

        [Fact]
        public async Task GetConsumerSummary_ReturnsData_ForCardIssueStatusException()
        {
            // Arrange
            var requestDto = new ConsumerSummaryRequestDto { email = "test@example.com", consumerCode = "testCode" };

            var consumerByEmail = new GetConsumerByEmailResponseDto { };
            _mockLoginService.Setup(x => x.GetConsumerByEmail(It.IsAny<string>()))
                             .ReturnsAsync(consumerByEmail);
            _mockAuth0Helper.Setup(x => x.GetUserById(It.IsAny<GetUserRequestDto>()))
                            .ReturnsAsync(new UserGetResponseDto() { });
            _mockTenantService.Setup(x => x.GetTenantByTenantCode(It.IsAny<string>()))
                              .ReturnsAsync(new TenantDto { TenantCode = "tenant123" });
            _mockWalletService.Setup(x => x.GetWallets(It.IsAny<FindConsumerWalletRequestDto>(), null))
                              .ReturnsAsync(new WalletResponseDto { });
            _mockTenantService.Setup(x => x.CheckCostcoMemberhipSupport(It.IsAny<TenantDto>()))
                              .Returns(true);
            _mockTaskService.Setup(x => x.GetConsumerTasks(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TenantDto>(), It.IsAny<string>()))
                .ReturnsAsync(new ConsumerTaskResponseDto() { });
            _mockTenantAccountService.Setup(x => x.GetTenantAccount(It.Is<TenantAccountCreateRequestDto>(dto => dto.TenantCode == "tenant123")))
                                     .ReturnsAsync(new TenantAccountDto());
            HealthMetricsRequestDto healthMetricsRequestDto = new HealthMetricsRequestDto { tenantCode = "tenant123" };
            _taskClient.Setup(client => client.Post<HealthMetricsSummaryDto>(CommonConstants.GetHealthMetrics, It.IsAny<HealthMetricsRequestDto>()))
           .ReturnsAsync(new HealthMetricsSummaryDto
           {
               HealthMetricsQueryStartTsMap = new Dictionary<string, DateTime?>{
               { "SLEEP", DateTime.Now } }
           });

            _fisClient.Setup(client => client.Post<GetConsumerAccountResponseDto>(It.IsAny<string>(), It.IsAny<GetConsumerAccountRequestDto>()))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _controller.ConsumerSummary(requestDto);

            // Assert
            Assert.NotNull(result);
            var objectResult = Assert.IsType<OkObjectResult>(result.Result);
            var responseDto = Assert.IsType<ConsumerSummaryResponseDto>(objectResult.Value);
            Assert.NotNull(responseDto);
        }

        [Fact]
        public async Task ConsumerDetail_WithValidRequest_ReturnsOkResponse()
        {
            // Arrange
            _controller = new ConsumerSummaryController(_mockControllerLogger.Object, _mockConsumerSummaryService.Object);

            var consumerDetailRequestDto = new ConsumerSummaryRequestDto { consumerCode = "CMR-12345" };
            var expectedResponse = new GetConsumerByEmailResponseDto { Consumer = new ConsumerDto[] { new ConsumerDto { ConsumerCode = "CMR-12345" } } };

            _mockConsumerSummaryService.Setup(s => s.GetConsumerDetails(consumerDetailRequestDto))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.ConsumerDetail(consumerDetailRequestDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
            Assert.Equal(expectedResponse, okResult.Value);
            _mockConsumerSummaryService.Verify(s => s.GetConsumerDetails(consumerDetailRequestDto), Times.Once);
        }

        [Fact]
        public async Task ConsumerDetail_WithErrorResponse_ReturnsBadRequest()
        {
            // Arrange
            _controller = new ConsumerSummaryController(_mockControllerLogger.Object, _mockConsumerSummaryService.Object);
            var consumerDetailRequestDto = new ConsumerSummaryRequestDto { consumerCode = "CMR-12345" };
            var errorResponse = new GetConsumerByEmailResponseDto
            {
                ErrorCode = (int)HttpStatusCode.BadRequest,
                ErrorMessage = "Invalid Consumer Code"
            };

            _mockConsumerSummaryService.Setup(s => s.GetConsumerDetails(consumerDetailRequestDto))
                .ReturnsAsync(errorResponse);

            // Act
            var result = await _controller.ConsumerDetail(consumerDetailRequestDto);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status400BadRequest, statusCodeResult.StatusCode);
            Assert.Equal(errorResponse, statusCodeResult.Value);
        }

        [Fact]
        public async Task ConsumerDetail_WithNotFoundError_ReturnsNotFound()
        {
            // Arrange
            _controller = new ConsumerSummaryController(_mockControllerLogger.Object, _mockConsumerSummaryService.Object);
            var consumerDetailRequestDto = new ConsumerSummaryRequestDto { consumerCode = "INVALID" };
            var errorResponse = new GetConsumerByEmailResponseDto
            {
                ErrorCode = (int)HttpStatusCode.NotFound,
                ErrorMessage = "Consumer not found"
            };

            _mockConsumerSummaryService.Setup(s => s.GetConsumerDetails(consumerDetailRequestDto))
                .ReturnsAsync(errorResponse);

            // Act
            var result = await _controller.ConsumerDetail(consumerDetailRequestDto);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status404NotFound, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task ConsumerDetail_WithServiceException_ReturnsInternalServerError()
        {
            // Arrange
            _controller = new ConsumerSummaryController(_mockControllerLogger.Object, _mockConsumerSummaryService.Object);
            var consumerDetailRequestDto = new ConsumerSummaryRequestDto { consumerCode = "CMR-12345" };
            var exceptionMessage = "Database connection failed";

            _mockConsumerSummaryService.Setup(s => s.GetConsumerDetails(consumerDetailRequestDto))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await _controller.ConsumerDetail(consumerDetailRequestDto);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
            var response = Assert.IsType<BaseResponseDto>(statusCodeResult.Value);
            Assert.Equal(StatusCodes.Status500InternalServerError, response.ErrorCode);
            Assert.Equal(exceptionMessage, response.ErrorMessage);
        }

        [Fact]
        public async Task ConsumerDetail_LogsInformationOnSuccess()
        {
            // Arrange
            _controller = new ConsumerSummaryController(_mockControllerLogger.Object, _mockConsumerSummaryService.Object);
            var consumerDetailRequestDto = new ConsumerSummaryRequestDto { consumerCode = "CMR-12345" };
            var expectedResponse = new GetConsumerByEmailResponseDto { Consumer = new ConsumerDto[] { new ConsumerDto { ConsumerCode = "CMR-12345" } } };

            _mockConsumerSummaryService.Setup(s => s.GetConsumerDetails(consumerDetailRequestDto))
                .ReturnsAsync(expectedResponse);

            // Act
            await _controller.ConsumerDetail(consumerDetailRequestDto);

            // Assert
            _mockControllerLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Started processing ConsumerDetail")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task ConsumerDetail_LogsErrorOnServiceError()
        {
            // Arrange
            _controller = new ConsumerSummaryController(_mockControllerLogger.Object, _mockConsumerSummaryService.Object);
            var consumerDetailRequestDto = new ConsumerSummaryRequestDto { consumerCode = "CMR-12345" };
            var errorResponse = new GetConsumerByEmailResponseDto
            {
                ErrorCode = (int)HttpStatusCode.BadRequest,
                ErrorMessage = "Invalid request"
            };

            _mockConsumerSummaryService.Setup(s => s.GetConsumerDetails(consumerDetailRequestDto))
                .ReturnsAsync(errorResponse);

            // Act
            await _controller.ConsumerDetail(consumerDetailRequestDto);

            // Assert
            _mockControllerLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error occurred while fetching ConsumerDetail")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task ConsumerDetail_WithEmptyConsumerCode_ReturnsError()
        {
            // Arrange
            _controller = new ConsumerSummaryController(_mockControllerLogger.Object, _mockConsumerSummaryService.Object);
            var consumerDetailRequestDto = new ConsumerSummaryRequestDto { consumerCode = "" };
            var errorResponse = new GetConsumerByEmailResponseDto
            {
                ErrorCode = (int)HttpStatusCode.BadRequest,
                ErrorMessage = "Consumer code is required"
            };

            _mockConsumerSummaryService.Setup(s => s.GetConsumerDetails(It.IsAny<ConsumerSummaryRequestDto>()))
                .ReturnsAsync(errorResponse);

            // Act
            var result = await _controller.ConsumerDetail(consumerDetailRequestDto);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status400BadRequest, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task ConsumerDetail_CallsServiceOnce()
        {
            // Arrange
            _controller = new ConsumerSummaryController(_mockControllerLogger.Object, _mockConsumerSummaryService.Object);
            var consumerDetailRequestDto = new ConsumerSummaryRequestDto { consumerCode = "CMR-12345" };
            var expectedResponse = new GetConsumerByEmailResponseDto { Consumer = new ConsumerDto[] { new ConsumerDto { ConsumerCode = "CMR-12345" } } };

            _mockConsumerSummaryService.Setup(s => s.GetConsumerDetails(consumerDetailRequestDto))
                .ReturnsAsync(expectedResponse);

            // Act
            await _controller.ConsumerDetail(consumerDetailRequestDto);

            // Assert
            _mockConsumerSummaryService.Verify(s => s.GetConsumerDetails(consumerDetailRequestDto), Times.Once);
        }        
    }
}

