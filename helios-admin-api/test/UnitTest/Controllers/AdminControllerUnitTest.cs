using AutoFixture;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Sunny.Benefits.Cms.Core.Domain.Dtos;
using SunnyBenefits.Fis.Core.Domain.Dtos;
using SunnyBenefits.Fis.Core.Domain.Models;
using SunnyRewards.Helios.Admin.Api.Controllers;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos.Constants;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos.Enums;
using SunnyRewards.Helios.Admin.Core.Domain.Models;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Admin.UnitTest.Fixtures.MockDtos;
using SunnyRewards.Helios.Admin.UnitTest.Helpers.HttpClientsMock;
using SunnyRewards.Helios.Bff.Infrastructure.Services;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using SunnyRewards.Helios.Sweepstakes.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;
using System.Linq.Expressions;
using Xunit;
using PostRedeemStartRequestDto = SunnyRewards.Helios.Wallet.Core.Domain.Dtos.PostRedeemStartRequestDto;

namespace SunnyRewards.Helios.Admin.UnitTest.Controllers
{
    public class AdminControllerUnitTest
    {
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
        private readonly IConsumerTaskService _consumerTaskService;
        private readonly IWalletService _walletService;
        private readonly IConsumerAccountService _consumerAccountService;
        private readonly ISweepstakesInstanceService _sweepsstakesService;
        private readonly Mock<NHibernate.ISession> _session;
        private readonly AdminController _adminController;
        private readonly Mock<IConsumerTaskService> _consumerTaskServiceMock;

        private readonly IFixture _fixture;
        public readonly Mock<ICohortClient> _cohortClient;
        public readonly Mock<ICmsClient> _cmsClient;
        public readonly Mock<ITaskCommonHelper> _taskCommonHelper;
        private readonly Mock<IWalletTypeService> _walletTypeService;
        private readonly Mock<IEventService> _eventService;


        public AdminControllerUnitTest()
        {
            _consumerTaskServiceMock = new Mock<IConsumerTaskService>();
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
            _cmsClient = new Mock<ICmsClient>();
            _taskCommonHelper = new Mock<ITaskCommonHelper>();
            _cohortConsumerTaskService = new Mock<ICohortConsumerTaskService>();           
            _session = new Mock<NHibernate.ISession>();
            _fixture = new Fixture();
            _eventService = new Mock<IEventService>();

            _walletTypeService = new Mock<IWalletTypeService>();
            _sweepsstakesService = new SweepstakesInstanceService(_sweepstakesInstanceServiceLogger.Object, _sweepstakesClient.Object);
            _consumerAccountService = new ConsumerAccountService(_consumerAccountServiceLogger.Object, _fisClient.Object);
            _consumerTaskService = new ConsumerTaskService(_consumerTaskServiceLogger.Object, _walletClient.Object, _taskClient.Object,
            _tenantClient.Object, _userClient.Object, _configuration.Object, _fisClient.Object, _cohortConsumerTaskService.Object,
            _session.Object, _cohortClient.Object, _taskCommonHelper.Object,_cmsClient.Object, _walletTypeService.Object,_eventService.Object);
            _walletService = new WalletService(_walletServiceLogger.Object, _walletClient.Object, _userClient.Object, _taskClient.Object);
            _adminController = new AdminController(_consumerTaskLogger.Object, _consumerTaskService, _walletService, _consumerAccountService, _sweepsstakesService);
        }

        [Fact]
        public async System.Threading.Tasks.Task Should_Consumer_Task_Update()
        {
            var taskUpdateRequestDto = new TaskUpdateRequestMockDto();
            taskUpdateRequestDto.TaskStatus = "IN_PROGRESS";
            _configuration.Setup(config => config.GetSection("Health_Actions_Reward_Wallet_Type_Code").Value);
            _taskClient.Setup(x => x.PutFormData<ConsumerTaskDto>(It.IsAny<string>(), It.IsAny<object>())).ReturnsAsync(new ConsumerTaskMockDto());
            var response = await _adminController.UpdateConsumerTask(taskUpdateRequestDto);
            var result = response.Result as OkObjectResult;
            Assert.True(result?.StatusCode == 200);
        }
        [Fact]
        public async System.Threading.Tasks.Task Consumer_Task_Should_Return_Ok_Response_When_Task_IsImage()
        {
            // Arrange
            var taskUpdateRequestDto = new TaskUpdateRequestMockDto();
            taskUpdateRequestDto.Image = CreateMockFile("image.jpg", "image/jpeg", "Sample Image Content");
            var cmsImageResponseDto = new UploadImageResponseDto() { ConsumerImage = new ConsumerImageMockDto() };
            cmsImageResponseDto.ErrorCode = 200;
            cmsImageResponseDto.ErrorMessage = "Image uploaded successfully";
            taskUpdateRequestDto.TaskStatus = "IN_PROGRESS";
            _configuration.Setup(config => config.GetSection("Health_Actions_Reward_Wallet_Type_Code").Value);
            _taskClient.Setup(x => x.PutFormData<ConsumerTaskDto>(It.IsAny<string>(), It.IsAny<object>())).ReturnsAsync(new ConsumerTaskMockDto());
            _cmsClient.Setup(x => x.PostFormData<UploadImageResponseDto>(It.IsAny<string>(), It.IsAny<UploadImageRequestDto>())).ReturnsAsync(cmsImageResponseDto);

            // Act 
            var response = await _adminController.UpdateConsumerTask(taskUpdateRequestDto);

            // Assert
            Assert.NotNull(response);
            var result = response.Result as OkObjectResult;
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }
       
        [Fact]
        public async System.Threading.Tasks.Task Consumer_Task_Should_Return_NotFound_When_Task_Reward_Is_Null()
        {
            // Arrange
            var taskUpdateRequestDto = new TaskUpdateRequestMockDto();
            taskUpdateRequestDto.Image = CreateMockFile("image.jpg", "image/jpeg", "Sample Image Content");
            var cmsImageResponseDto = new UploadImageResponseDto() { ConsumerImage = new ConsumerImageMockDto() };
            var taskResponseDto = new FindConsumerTasksByIdResponseMockDto();
            taskResponseDto.TaskRewardDetail.TaskReward = null;
            taskUpdateRequestDto.TaskStatus = "IN_PROGRESS";
            var taskClient = new Mock<TaskClient>();
            _taskClient.Setup(client => client.Post<FindConsumerTasksByIdResponseDto>(It.IsAny<string>(), It.IsAny<FindConsumerTasksByIdRequestDto>()))
              .ReturnsAsync(taskResponseDto);
            _configuration.Setup(config => config.GetSection("Health_Actions_Reward_Wallet_Type_Code").Value);
            _taskClient.Setup(x => x.PutFormData<ConsumerTaskDto>(It.IsAny<string>(), It.IsAny<object>())).ReturnsAsync(new ConsumerTaskMockDto());
            _cmsClient.Setup(x => x.PostFormData<UploadImageResponseDto>(It.IsAny<string>(), It.IsAny<UploadImageRequestDto>())).ReturnsAsync(cmsImageResponseDto);

            // Act 
            var response = await _adminController.UpdateConsumerTask(taskUpdateRequestDto);

            // Assert
            Assert.NotNull(response);
            var result = response.Result as ObjectResult;
            Assert.Equal(StatusCodes.Status422UnprocessableEntity, result.StatusCode);
        }
        [Fact]
        public async System.Threading.Tasks.Task Consumer_Task_Should_Return_NotFound_When_Task_Completion_Criteria_Is_Null()
        {
            // Arrange
            var taskUpdateRequestDto = new TaskUpdateRequestMockDto();
            taskUpdateRequestDto.Image = CreateMockFile("image.jpg", "image/jpeg", "Sample Image Content");
            var cmsImageResponseDto = new UploadImageResponseDto() { ConsumerImage = new ConsumerImageMockDto() };
            var taskResponseDto = new FindConsumerTasksByIdResponseMockDto();
            taskResponseDto.TaskRewardDetail.TaskReward.TaskCompletionCriteriaJson = null;
            taskUpdateRequestDto.TaskStatus = "IN_PROGRESS";
            var taskClient = new Mock<TaskClient>();
            _taskClient.Setup(client => client.Post<FindConsumerTasksByIdResponseDto>(It.IsAny<string>(), It.IsAny<FindConsumerTasksByIdRequestDto>()))
              .ReturnsAsync(taskResponseDto);
            _configuration.Setup(config => config.GetSection("Health_Actions_Reward_Wallet_Type_Code").Value);
            _taskClient.Setup(x => x.PutFormData<ConsumerTaskDto>(It.IsAny<string>(), It.IsAny<object>())).ReturnsAsync(new ConsumerTaskMockDto());
            _cmsClient.Setup(x => x.PostFormData<UploadImageResponseDto>(It.IsAny<string>(), It.IsAny<UploadImageRequestDto>())).ReturnsAsync(cmsImageResponseDto);

            // Act 
            var response = await _adminController.UpdateConsumerTask(taskUpdateRequestDto);

            // Assert
            Assert.NotNull(response);
            var result = response.Result as ObjectResult;
            Assert.Equal(StatusCodes.Status422UnprocessableEntity, result.StatusCode);
        }
        [Fact]
        public async System.Threading.Tasks.Task Consumer_Task_Should_Return_InternalServer_When_Cms_Client_Throws_Exception()
        {
            // Arrange
            var taskUpdateRequestDto = new TaskUpdateRequestMockDto();
            taskUpdateRequestDto.Image = CreateMockFile("image.jpg", "image/jpeg", "Sample Image Content");
            taskUpdateRequestDto.TaskStatus = "IN_PROGRESS";
            _configuration.Setup(config => config.GetSection("Health_Actions_Reward_Wallet_Type_Code").Value);
            _taskClient.Setup(x => x.PutFormData<ConsumerTaskDto>(It.IsAny<string>(), It.IsAny<object>())).ReturnsAsync(new ConsumerTaskMockDto());
            _cmsClient.Setup(x => x.PostFormData<UploadImageResponseDto>(It.IsAny<string>(), It.IsAny<UploadImageRequestDto>())).Throws(new Exception("testing"));

            // Act 
            var response = await _adminController.UpdateConsumerTask(taskUpdateRequestDto);

            // Assert
            Assert.NotNull(response);
            Assert.Equal("testing",response.Value.ErrorMessage);
        }
        // Helper Method to Create Mock IFormFile
        private static FormFile CreateMockFile(string fileName, string contentType, string content)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(content);
            writer.Flush();
            stream.Position = 0;

            return new FormFile(stream, 0, stream.Length, "file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = contentType
            };
        }

        [Fact]
        public async System.Threading.Tasks.Task UpdateConsumerTask_ExceptionHandling()
        {
            var adminLogger = new Mock<ILogger<AdminController>>();
            var consumerTaskService = new Mock<IConsumerTaskService>();
            var walletService = new Mock<IWalletService>();
            var consumerAccountService = new Mock<IConsumerAccountService>();
            var sweepstakesInstanceService = new Mock<ISweepstakesInstanceService>();
            var consumerTaskController = new AdminController(adminLogger.Object, consumerTaskService.Object, walletService.Object, consumerAccountService.Object, sweepstakesInstanceService.Object);
            var taskUpdateRequestDto = new TaskUpdateRequestMockDto();

            consumerTaskService.Setup(x => x.UpdateConsumerTask(It.IsAny<TaskUpdateRequestMockDto>()))
                .ThrowsAsync(new Exception("Test Exception"));
            var response = await consumerTaskController.UpdateConsumerTask(taskUpdateRequestDto);
            Assert.True(response.Value?.ErrorMessage == "Test Exception");
        }

        [Fact]
        public async System.Threading.Tasks.Task UpdateConsumerTask_InvalidArguments_ReturnsInvalidArgumentsResponse()
        {
            var request = new TaskUpdateRequestMockDto()
            {
                MemberId = null,
                PartnerCode = null,
                ConsumerCode = null,
            };
            var response = await _adminController.UpdateConsumerTask(request);
            var result = response.Result as ObjectResult;
            Assert.True(result?.StatusCode == 400);
        }

        [Fact]
        public async System.Threading.Tasks.Task UpdateConsumerTask_Should_Return_Null_Response_For_Invalid_TenantCode()
        {
            var request = new TaskUpdateRequestMockDto()
            {
                ConsumerCode = null,
            };
            _tenantClient.Setup(c => c.Post<GetTenantByPartnerCodeResponseDto>("tenant/get-by-partner-code", It.IsAny<GetTenantByPartnerCodeRequestDto>()))
            .ReturnsAsync(new GetTenantByPartnerCodeResponseMockDto() { Tenant = new TenantDto { TenantCode = null } });

            var response = await _adminController.UpdateConsumerTask(request);
            var result = response.Result as ObjectResult;
            Assert.True(result?.StatusCode == 404);
        }

        [Fact]
        public async System.Threading.Tasks.Task UpdateConsumerTask_Should_Return_Null_Response_For_InValid_ConsumerCode()
        {
            var request = new TaskUpdateRequestMockDto()
            {
                ConsumerCode = null,
            };
            _userClient.Setup(client => client.Post<GetConsumerByMemIdResponseDto>("consumer/get-consumer-by-memid", It.IsAny<GetConsumerByMemIdRequestDto>()))
               .ReturnsAsync(new GetConsumerByMemNbrResponseMockDto() { Consumer = null });

            var response = await _adminController.UpdateConsumerTask(request);
            var result = response.Result as ObjectResult;
            Assert.True(result?.StatusCode == 404);
        }

        [Fact]
        public async System.Threading.Tasks.Task UpdateConsumerTask_Should_ValidCheck_For_Consumer_Task_State()
        {
            var request = new TaskUpdateRequestMockDto();
            _taskClient.Setup(client => client.Post<RewardTypeResponseDto>("reward-type", It.IsAny<RewardTypeRequestDto>()))
               .ReturnsAsync(new RewardTypeResponseMockDto());
            _taskClient.Setup(x => x.PutFormData<ConsumerTaskDto>(It.IsAny<string>(), It.IsAny<object>())).ReturnsAsync(new ConsumerTaskMockDto());
            _configuration.Setup(config => config.GetSection("Health_Actions_Reward_Wallet_Type_Code").Value);
            _configuration.Setup(config => config.GetSection("Health_Actions_Redemption_Wallet_Type_Code").Value);
            _configuration.Setup(config => config.GetSection("Health_Actions_Reward_Purse_Wallet_Type_Code").Value);
            _userClient.Setup(x => x.Post<GetConsumerResponseDto>("consumer/get-consumer", It.IsAny<GetConsumerRequestDto>()))
                .ReturnsAsync(new GetConsumerResponseMockDto());
            _walletClient.Setup(x => x.Post<PostRedeemStartResponseDto>(Constant.WalletRedeemStartAPIUrl, It.IsAny<Core.Domain.Dtos.PostRedeemStartRequestDto>()))
                .ReturnsAsync(new PostRedeemStartResponseDto());
            _fisClient.Setup(x => x.Post<LoadValueResponseDto>(Constant.FISValueLoadAPIUrl, It.IsAny<LoadValueRequestDto>()))
                .ReturnsAsync(new LoadValueResponseDto());
            _walletClient.Setup(x => x.Post<PostRedeemCompleteResponseDto>(Constant.WalletRedeemCompleteAPIUrl, It.IsAny<PostRedeemCompleteRequestDto>()))
                .ReturnsAsync(new PostRedeemCompleteResponseDto());
            var response = await _adminController.UpdateConsumerTask(request);
            var result = response.Result as OkObjectResult;
            Assert.True(result?.StatusCode == 200);
        }

        [Fact]
        public async System.Threading.Tasks.Task UpdateConsumerTask_Should_Return_Ok_When_Value_Load_Return_Error()
        {
            var request = new TaskUpdateRequestMockDto();
            _taskClient.Setup(client => client.Post<RewardTypeResponseDto>("reward-type", It.IsAny<RewardTypeRequestDto>()))
               .ReturnsAsync(new RewardTypeResponseMockDto());
            _taskClient.Setup(x => x.PutFormData<ConsumerTaskDto>(It.IsAny<string>(), It.IsAny<object>())).ReturnsAsync(new ConsumerTaskMockDto());
            _configuration.Setup(config => config.GetSection("Health_Actions_Reward_Wallet_Type_Code").Value);
            _configuration.Setup(config => config.GetSection("Health_Actions_Redemption_Wallet_Type_Code").Value);
            _configuration.Setup(config => config.GetSection("Health_Actions_Reward_Purse_Wallet_Type_Code").Value);
            _userClient.Setup(x => x.Post<GetConsumerResponseDto>("consumer/get-consumer", It.IsAny<GetConsumerRequestDto>()))
                .ReturnsAsync(new GetConsumerResponseMockDto());
            _walletClient.Setup(x => x.Post<PostRedeemStartResponseDto>(Constant.WalletRedeemStartAPIUrl, It.IsAny<Core.Domain.Dtos.PostRedeemStartRequestDto>()))
                .ReturnsAsync(new PostRedeemStartResponseDto());
            _fisClient.Setup(x => x.Post<LoadValueResponseDto>(Constant.FISValueLoadAPIUrl, It.IsAny<LoadValueRequestDto>()))
                .ReturnsAsync(new LoadValueResponseDto() { ErrorCode = 500 });
            _walletClient.Setup(x => x.Post<PostRedeemCompleteResponseDto>(Constant.WalletRedeemCompleteAPIUrl, It.IsAny<PostRedeemCompleteRequestDto>()))
                .ReturnsAsync(new PostRedeemCompleteResponseDto());
            var response = await _adminController.UpdateConsumerTask(request);
            var result = response.Result as OkObjectResult;
            Assert.True(result?.StatusCode == 200);
        }

        [Fact]
        public async System.Threading.Tasks.Task UpdateConsumerTask_Should_Return_Ok_When_Value_Load_Return_Exception()
        {
            var request = new TaskUpdateRequestMockDto();
            _taskClient.Setup(client => client.Post<RewardTypeResponseDto>("reward-type", It.IsAny<RewardTypeRequestDto>()))
               .ReturnsAsync(new RewardTypeResponseMockDto());
            _taskClient.Setup(x => x.PutFormData<ConsumerTaskDto>(It.IsAny<string>(), It.IsAny<object>())).ReturnsAsync(new ConsumerTaskMockDto());
            _configuration.Setup(config => config.GetSection("Health_Actions_Reward_Wallet_Type_Code").Value);
            _configuration.Setup(config => config.GetSection("Health_Actions_Redemption_Wallet_Type_Code").Value);
            _configuration.Setup(config => config.GetSection("Health_Actions_Reward_Purse_Wallet_Type_Code").Value);
            _userClient.Setup(x => x.Post<GetConsumerResponseDto>("consumer/get-consumer", It.IsAny<GetConsumerRequestDto>()))
                .ReturnsAsync(new GetConsumerResponseMockDto());
            _walletClient.Setup(x => x.Post<PostRedeemStartResponseDto>(Constant.WalletRedeemStartAPIUrl, It.IsAny<Core.Domain.Dtos.PostRedeemStartRequestDto>()))
                .ReturnsAsync(new PostRedeemStartResponseDto());
            _fisClient.Setup(x => x.Post<LoadValueResponseDto>(Constant.FISValueLoadAPIUrl, It.IsAny<LoadValueRequestDto>()))
                .ThrowsAsync(new Exception("Test Exception"));
            _walletClient.Setup(x => x.Post<PostRedeemFailResponseDto>(Constant.WalletRedeemFailAPIUrl, It.IsAny<PostRedeemFailRequestDto>()))
                .ThrowsAsync(new Exception("Test Exception"));
            var response = await _adminController.UpdateConsumerTask(request);
            var result = response.Result as OkObjectResult;
            Assert.True(result?.StatusCode == 200);
        }

        [Fact]
        public async System.Threading.Tasks.Task UpdateConsumerTask_Should_NullCheck_For_Consumer_Task_State()
        {
            var request = new TaskUpdateRequestMockDto()
            {
                ConsumerCode = "null"
            };
            _configuration.Setup(config => config.GetSection("Health_Actions_Reward_Wallet_Type_Code").Value);
            _userClient.Setup(x => x.Post<GetConsumerResponseDto>("consumer/get-consumer", It.IsAny<GetConsumerRequestDto>()))
                .ReturnsAsync(new GetConsumerResponseMockDto() { Consumer = null });
            var response = await _adminController.UpdateConsumerTask(request);
            var result = response.Result as ObjectResult;
            Assert.True(result?.StatusCode == 404);
        }


        [Fact]
        public async System.Threading.Tasks.Task UpdateConsumerTask_Should_Return_Error()
        {
            var request = new TaskUpdateRequestMockDto();
            _taskClient.Setup(client => client.Post<FindConsumerTasksByIdResponseDto>("get-consumer-task-by-task-id", It.IsAny<FindConsumerTasksByIdRequestDto>()))
                .ReturnsAsync(new FindConsumerTasksByIdResponseMockDto() { ConsumerTask = null });
            _taskClient.Setup(x => x.PutFormData<ConsumerTaskDto>(It.IsAny<string>(), It.IsAny<object>())).ReturnsAsync(new ConsumerTaskMockDto());
            var response = await _adminController.UpdateConsumerTask(request);
            var result = response.Result as ObjectResult;
            Assert.True(result?.StatusCode == 404);
        }


        [Fact]
        public async System.Threading.Tasks.Task UpdateConsumerTask_Should_Return_NotFoundResponse()
        {
            var ServiceMock = new Mock<IConsumerTaskService>();
            var walletService = new Mock<IWalletService>();
            var consumerAccountService = new Mock<IConsumerAccountService>();
            var sweepstakesInstanceService = new Mock<ISweepstakesInstanceService>();
            var task = new Mock<ITaskClient>();
            var wallet = new Mock<IWalletClient>();
            var request = new TaskUpdateRequestMockDto();
            ServiceMock.Setup(service => service.UpdateConsumerTask(It.IsAny<TaskUpdateRequestMockDto>()))
                                 .ReturnsAsync(new ConsumerTaskUpdateResponseDto() { ErrorCode = 404 });
            task.Setup(client => client.Post<FindConsumerTasksByIdResponseDto>("get-consumer-task-by-task-id",
                It.IsAny<FindConsumerTasksByIdRequestDto>())).ReturnsAsync(new FindConsumerTasksByIdResponseMockDto());
            wallet.Setup(client => client.Post<PostRewardResponseDto>("wallet/reward", It.IsAny<PostRewardRequestDto>()))
                .ReturnsAsync(new PostRewardResponseMockDto());
            var controller = new AdminController(_consumerTaskLogger.Object, ServiceMock.Object, walletService.Object, consumerAccountService.Object, sweepstakesInstanceService.Object);

            var response = await controller.UpdateConsumerTask(request);
            var result = response.Result as ObjectResult;
            Assert.True(result?.StatusCode == 404);
        }

        [Fact]
        public async System.Threading.Tasks.Task UpdateConsumerTask_Should_Return_NotFoundResponse_When_TaskCompletedDate_Is_Invalid()
        {
            var ServiceMock = new Mock<IConsumerTaskService>();
            var walletService = new Mock<IWalletService>();
            var consumerAccountService = new Mock<IConsumerAccountService>();
            var sweepstakesInstanceService = new Mock<ISweepstakesInstanceService>();
            var task = new Mock<ITaskClient>();
            var wallet = new Mock<IWalletClient>();
            var request = new TaskUpdateRequestMockDto();
            var taskresponse = new FindConsumerTasksByIdResponseMockDto();
            taskresponse.TaskRewardDetail.TaskReward.IsRecurring = false;
            request.TaskStatus = "COMPLETED";
            request.TaskCompletedTs = DateTime.UtcNow.AddMonths(1);
            task.Setup(client => client.Post<FindConsumerTasksByIdResponseDto>("get-consumer-task-by-task-id",
                It.IsAny<FindConsumerTasksByIdRequestDto>())).ReturnsAsync(taskresponse);
            wallet.Setup(client => client.Post<PostRewardResponseDto>("wallet/reward", It.IsAny<PostRewardRequestDto>()))
                .ReturnsAsync(new PostRewardResponseMockDto());

            var response = await _adminController.UpdateConsumerTask(request);
            var result = response.Result as ObjectResult;
            Assert.True(result?.StatusCode == 422);
        }

        [Fact]
        public async System.Threading.Tasks.Task UpdateConsumerTask_Should_Return_Null_Response_For_Error()
        {
            var request = new TaskUpdateRequestMockDto();
            _taskClient.Setup(client => client.Post<RewardTypeResponseDto>("reward-type", It.IsAny<RewardTypeRequestDto>()))
                .ReturnsAsync(new RewardTypeResponseMockDto());
            _taskClient.Setup(x => x.PutFormData<ConsumerTaskDto>(It.IsAny<string>(), It.IsAny<object>())).ReturnsAsync(new ConsumerTaskMockDto());
            _configuration.Setup(config => config.GetSection("Health_Actions_Reward_Wallet_Type_Code").Value).Returns("wat-2d62dcaf2aa4424b9ff6c2ddb5895077");
            _walletClient.Setup(client => client.Post<PostRewardResponseDto>("wallet/reward", It.IsAny<PostRewardRequestDto>()))
                .ReturnsAsync(new PostRewardResponseMockDto() { ErrorCode = 404 });

            var response = await _adminController.UpdateConsumerTask(request);
            var result = response.Result as ObjectResult;
            Assert.True(result?.StatusCode == 404);
        }

        [Fact]
        public async System.Threading.Tasks.Task UpdateConsumerTask_Should_Return_Null_Response_If_TransactionDetail_Is_Null()
        {
            var request = new TaskUpdateRequestMockDto();
            _taskClient.Setup(client => client.Post<RewardTypeResponseDto>("reward-type", It.IsAny<RewardTypeRequestDto>()))
               .ReturnsAsync(new RewardTypeResponseMockDto());
            _configuration.Setup(config => config.GetSection("Health_Actions_Reward_Wallet_Type_Code").Value);
            _walletClient.Setup(client => client.Post<PostRewardResponseDto>("wallet/reward", It.IsAny<PostRewardRequestDto>()))
                .ReturnsAsync(new PostRewardResponseMockDto() { TransactionDetail = null });
            _taskClient.Setup(x => x.PutFormData<ConsumerTaskDto>(It.IsAny<string>(), It.IsAny<object>())).ReturnsAsync(new ConsumerTaskMockDto());
            var response = await _adminController.UpdateConsumerTask(request);
            var result = response.Result as NotFoundObjectResult;
            Assert.True(((ConsumerTaskUpdateResponseDto?)result.Value)?.ConsumerTask == null);
        }

        [Fact]
        public async System.Threading.Tasks.Task UpdateConsumerTask_Should_Return_Error_Result_When_Auto_Enrolled_Failed()
        {
            var request = new TaskUpdateRequestMockDto();
            request.IsAutoEnrollEnabled = true;
            _taskClient.Setup(client => client.Post<FindConsumerTasksByIdResponseDto>("get-consumer-task-by-task-id", It.IsAny<FindConsumerTasksByIdRequestDto>()));
            _taskClient.Setup(client => client.Post<RewardTypeResponseDto>("reward-type", It.IsAny<RewardTypeRequestDto>()))
               .ReturnsAsync(new RewardTypeResponseMockDto());
            _taskClient.Setup(x => x.PutFormData<ConsumerTaskDto>(It.IsAny<string>(), It.IsAny<object>())).ReturnsAsync(new ConsumerTaskMockDto());
           
            var response = await _adminController.UpdateConsumerTask(request);
            var result = response.Result as ObjectResult;
            Assert.True(result?.StatusCode == 422);
        }

        [Fact]
        public async System.Threading.Tasks.Task UpdateConsumerTask_Should_Work_When_Auto_Enrolled_Success()
        {
            var request = new TaskUpdateRequestMockDto();
            request.IsAutoEnrollEnabled = true;
            _taskClient.Setup(client => client.Post<FindConsumerTasksByIdResponseDto>("get-consumer-task-by-task-id", It.IsAny<FindConsumerTasksByIdRequestDto>()));
            var consumerTaskResponse = new ConsumerTaskResponseUpdateDto { ConsumerTask = new ConsumerTaskDto { ConsumerTaskId = 1 } };
            
            _taskClient.Setup(t => t.Post<ConsumerTaskResponseUpdateDto>("consumer-task", It.IsAny<CreateConsumerTaskDto>()))
                .ReturnsAsync(consumerTaskResponse);
           
            var response = await _adminController.UpdateConsumerTask(request);
            var result = response.Result as ObjectResult;
            Assert.True(result?.StatusCode == 422);
        }


        [Fact]
        public async System.Threading.Tasks.Task UpdateConsumerTask_Should_Return_Exception_Catch_In_Service()
        {
            var taskUpdateRequestMockDto = new TaskUpdateRequestMockDto();
            _taskClient.Setup(client => client.Post<FindConsumerTasksByIdResponseDto>(It.IsAny<string>(),
                It.IsAny<FindConsumerTasksByIdRequestDto>())).ThrowsAsync(new Exception("Simulated exception"));

            var response = await _adminController.UpdateConsumerTask(taskUpdateRequestMockDto);
            Assert.True(response.Value?.ErrorMessage == "Simulated exception");
        }

        [Fact]
        public async System.Threading.Tasks.Task Wallet_ClearEntriesWallet_Should_Returns_Bad_Request_Result()
        {
            // Arrange
            var requestDto = new ClearEntriesWalletRequestMockDto();
            requestDto.TenantCode = string.Empty;
            var expectedResponseDto = new BaseResponseDto
            {
                ErrorCode = StatusCodes.Status400BadRequest,
                ErrorMessage = "No tenant code supplied"
            };

            // Act
            var result = await _adminController.ClearEntriesWallet(requestDto);

            // Assert
            Assert.IsType<ObjectResult>(result.Result);
            var objectResult = result.Result as ObjectResult;
            var responseDto = objectResult?.Value as BaseResponseDto;
            Assert.Equal(expectedResponseDto.ErrorCode, responseDto?.ErrorCode);
            Assert.Equal(expectedResponseDto.ErrorMessage, responseDto?.ErrorMessage);
        }

        [Fact]
        public async System.Threading.Tasks.Task Wallet_ClearEntriesWallet_Should_Returns_Not_Found_Result()
        {
            // Arrange
            var requestDto = new ClearEntriesWalletRequestMockDto();
            var expectedResponseDto = new BaseResponseDto
            {
                ErrorCode = StatusCodes.Status404NotFound,
                ErrorMessage = "Wallet type not found"
            };
            _walletClient.Setup(client => client.Post<BaseResponseDto>(It.IsAny<string>(),
               It.IsAny<ClearEntriesWalletRequestDto>())).ReturnsAsync(expectedResponseDto);

            // Act
            var result = await _adminController.ClearEntriesWallet(requestDto);

            // Assert
            Assert.IsType<ObjectResult>(result.Result);
            var objectResult = result.Result as ObjectResult;
            var responseDto = objectResult?.Value as BaseResponseDto;
            Assert.Equal(expectedResponseDto.ErrorCode, responseDto?.ErrorCode);
            Assert.Equal(expectedResponseDto.ErrorMessage, responseDto?.ErrorMessage);
        }

        [Fact]
        public async System.Threading.Tasks.Task Wallet_ClearEntriesWallet_Should_Return_Success_Result()
        {
            // Arrange
            var requestDto = new ClearEntriesWalletRequestMockDto();
            var expectedResponseDto = new BaseResponseDto();

            _walletClient.Setup(client => client.Post<BaseResponseDto>(It.IsAny<string>(),
               It.IsAny<ClearEntriesWalletRequestDto>())).ReturnsAsync(expectedResponseDto);

            // Act
            var result = await _adminController.ClearEntriesWallet(requestDto);

            // Assert
            Assert.IsType<ActionResult<BaseResponseDto>>(result);
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.True(actionResult.StatusCode == 200);
        }

        [Fact]
        public async System.Threading.Tasks.Task Wallet_ClearEntriesWallet_Should_Returns_Internal_Server_Error()
        {
            // Arrange
            var requestDto = new ClearEntriesWalletRequestMockDto();
            var exceptionMessage = "Test Exception";


            _walletClient.Setup(client => client.Post<BaseResponseDto>(It.IsAny<string>(),
               It.IsAny<ClearEntriesWalletRequestDto>())).ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await _adminController.ClearEntriesWallet(requestDto);

            // Assert
            Assert.IsType<ObjectResult>(result.Result);
            var objectResult = result.Result as ObjectResult;
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult?.StatusCode);
            Assert.IsType<BaseResponseDto>(objectResult?.Value);
            var responseDto = objectResult.Value as BaseResponseDto;
            Assert.Equal(exceptionMessage, responseDto?.ErrorMessage);
        }

        [Fact]
        public async System.Threading.Tasks.Task Wallet_ClearEntriesWallet_Should_Returns_Internal_Server_Error_When_Client_Return_500()
        {
            // Arrange
            var requestDto = new ClearEntriesWalletRequestMockDto();
            var expectedResponseDto = new BaseResponseDto
            {
                ErrorCode = StatusCodes.Status500InternalServerError,
                ErrorMessage = "Test Exception"
            };


            _walletClient.Setup(client => client.Post<BaseResponseDto>(It.IsAny<string>(),
               It.IsAny<ClearEntriesWalletRequestDto>())).ReturnsAsync(expectedResponseDto);

            // Act
            var result = await _adminController.ClearEntriesWallet(requestDto);

            // Assert
            Assert.IsType<ObjectResult>(result.Result);
            var objectResult = result.Result as ObjectResult;
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult?.StatusCode);
            Assert.IsType<BaseResponseDto>(objectResult?.Value);
            var responseDto = objectResult.Value as BaseResponseDto;
            Assert.Equal(expectedResponseDto.ErrorMessage, responseDto?.ErrorMessage);
        }

        [Fact]
        public async System.Threading.Tasks.Task PostConsumerTasks_ShouldReturnOk_WhenConsumerTaskIsFound()
        {
            // Arrange
            var requestDto = new CreateConsumerTaskDto { TaskId = 1 };
            var consumerTaskResponse = new ConsumerTaskResponseUpdateDto
            {
                ConsumerTask = new ConsumerTaskDto { ConsumerTaskId = 1 }
            };

            // Mocking the service method to return a valid response
            _consumerTaskServiceMock.Setup(service => service.PostConsumerTasks(It.IsAny<CreateConsumerTaskDto>()))
                .ReturnsAsync(consumerTaskResponse);
          var  _controller = new AdminController(_consumerTaskLogger.Object, _consumerTaskServiceMock.Object, _walletService, _consumerAccountService, _sweepsstakesService);

            // Act
            var result = await _controller.PostConsumerTasks(requestDto);

            // Assert
            Assert.NotNull(result); // Ensure the ConsumerTask is returned
        }

        [Fact]
        public async System.Threading.Tasks.Task PostConsumerTasks_ShouldReturnNotFound_WhenConsumerTaskIsNull()
        {
            // Arrange
            var requestDto = new CreateConsumerTaskDto { TaskId = 1 };
            var consumerTaskResponse = new ConsumerTaskResponseUpdateDto
            {
                ConsumerTask = null // Simulating no consumer task found
            };

            // Mocking the service method to return a response with no ConsumerTask
            _consumerTaskServiceMock.Setup(service => service.PostConsumerTasks(It.IsAny<CreateConsumerTaskDto>()))
                .ReturnsAsync(consumerTaskResponse);

            // Act
            var result = await _adminController.PostConsumerTasks(requestDto);

            // Assert
            var returnValue = Assert.IsType<ConsumerTaskResponseUpdateDto>(result.Value);
            Assert.Null(returnValue.ConsumerTask); // Ensure no consumer task is returned
        }

        #region RevertAllTransactionAndTasks Tests
        [Fact]
        public async System.Threading.Tasks.Task RevertAllTransactionAndTasks_Should_Return_Bad_Request_Result()
        {
            // Arrange
            var requestDto = new RevertTransactionsRequestMockDto
            {
                ConsumerCode = string.Empty
            };

            // Act
            var result = await _adminController.RevertAllTransactionAndTasks(requestDto);

            // Assert
            Assert.IsType<ObjectResult>(result.Result);
            var objectResult = result.Result as ObjectResult;
            Assert.Equal(StatusCodes.Status400BadRequest, objectResult?.StatusCode);
        }

        [Fact]
        public async System.Threading.Tasks.Task RevertAllTransactionAndTasks_Should_Return_Not_Found_Result()
        {
            // Arrange
            var requestDto = new RevertTransactionsRequestMockDto();
            var consumerMockData = new GetConsumerResponseMockDto();
            consumerMockData.Consumer.ConsumerCode = null;

            _userClient.Setup(client => client.Post<GetConsumerResponseDto>(It.IsAny<string>(),
               It.IsAny<GetConsumerRequestDto>())).ReturnsAsync(consumerMockData);

            // Act
            var result = await _adminController.RevertAllTransactionAndTasks(requestDto);

            // Assert
            Assert.IsType<ObjectResult>(result.Result);
            var objectResult = result.Result as ObjectResult;
            Assert.Equal(StatusCodes.Status404NotFound, objectResult?.StatusCode);
        }

        [Fact]
        public async System.Threading.Tasks.Task RevertAllTransactionAndTasks_Should_Return_Ok_Result()
        {
            // Arrange
            var requestDto = new RevertTransactionsRequestMockDto();
            _walletClient.Setup(client => client.Post<BaseResponseDto>(It.IsAny<string>(),
               It.IsAny<RevertTransactionsRequestDto>())).ReturnsAsync(new BaseResponseDto());
            _taskClient.Setup(client => client.Post<BaseResponseDto>(It.IsAny<string>(),
               It.IsAny<RevertAllConsumerTasksRequestDto>())).ReturnsAsync(new BaseResponseDto());

            // Act
            var result = await _adminController.RevertAllTransactionAndTasks(requestDto);

            // Assert
            Assert.IsType<OkObjectResult>(result.Result);
            var okObjectResult = result.Result as OkObjectResult;
            Assert.Equal(StatusCodes.Status200OK, okObjectResult?.StatusCode);
        }

        [Fact]
        public async System.Threading.Tasks.Task RevertAllTransactionAndTasks_Should_Return_Internal_Server_Error_Result_When_Exception_Occurred()
        {
            // Arrange
            var requestDto = new RevertTransactionsRequestMockDto();
            _walletClient.Setup(client => client.Post<BaseResponseDto>(It.IsAny<string>(),
               It.IsAny<RevertTransactionsRequestDto>())).ReturnsAsync(new BaseResponseDto());
            _taskClient.Setup(client => client.Post<BaseResponseDto>(It.IsAny<string>(),
               It.IsAny<RevertTransactionsRequestDto>())).ThrowsAsync(new Exception("Test Exception"));

            // Act
            var result = await _adminController.RevertAllTransactionAndTasks(requestDto);

            // Assert
            Assert.IsType<ObjectResult>(result.Result);
            var objectResult = result.Result as ObjectResult;
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult?.StatusCode);
        }

        [Fact]
        public async System.Threading.Tasks.Task RevertAllTransactionAndTasks_Should_Return_Task_Api_Error_Response()
        {
            // Arrange
            var requestDto = new RevertTransactionsRequestMockDto();
            _walletClient.Setup(client => client.Post<BaseResponseDto>(It.IsAny<string>(),
               It.IsAny<RevertTransactionsRequestDto>())).ReturnsAsync(new BaseResponseDto());
            _taskClient.Setup(client => client.Post<BaseResponseDto>(It.IsAny<string>(), It.IsAny<RevertTransactionsRequestDto>()))
                .ReturnsAsync(new BaseResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError });

            // Act
            var result = await _adminController.RevertAllTransactionAndTasks(requestDto);

            // Assert
            Assert.IsType<ObjectResult>(result.Result);
            var objectResult = result.Result as ObjectResult;
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult?.StatusCode);
        }

        [Fact]
        public async System.Threading.Tasks.Task RevertAllTransactionAndTasks_Should_Return_Wallet_Api_Error_Response()
        {
            // Arrange
            var requestDto = new RevertTransactionsRequestMockDto();
            _walletClient.Setup(client => client.Post<BaseResponseDto>(It.IsAny<string>(), It.IsAny<RevertTransactionsRequestDto>()))
                .ReturnsAsync(new BaseResponseDto() { ErrorCode = StatusCodes.Status404NotFound });

            // Act
            var result = await _adminController.RevertAllTransactionAndTasks(requestDto);

            // Assert
            Assert.IsType<ObjectResult>(result.Result);
            var objectResult = result.Result as ObjectResult;
            Assert.Equal(StatusCodes.Status404NotFound, objectResult?.StatusCode);
        }

        #endregion

        #region Redeem Tests
        [Fact]
        public async System.Threading.Tasks.Task Redeem_Should_Return_Bad_Request_Result()
        {
            // Arrange
            var requestDto = new PostRedeemStartRequestDto()
            {
                ConsumerCode = string.Empty
            };
            _walletClient.Setup(client => client.Post<PostRedeemStartResponseDto>(It.IsAny<string>(),
              It.IsAny<PostRedeemStartRequestDto>())).ReturnsAsync(new PostRedeemStartResponseDto()
              {
                  ErrorCode = StatusCodes.Status400BadRequest
              });

            // Act
            var result = await _adminController.Redeem(requestDto);

            // Assert
            Assert.IsType<ObjectResult>(result.Result);
            var objectResult = result.Result as ObjectResult;
            Assert.Equal(StatusCodes.Status400BadRequest, objectResult?.StatusCode);
        }

        [Fact]
        public async System.Threading.Tasks.Task Redeem_Should_Return_Ok_Result()
        {
            // Arrange
            var requestDto = new PostRedeemStartRequestDto();
            _walletClient.Setup(client => client.Post<PostRedeemStartResponseDto>(It.IsAny<string>(),
              It.IsAny<PostRedeemStartRequestDto>())).ReturnsAsync(new PostRedeemStartResponseDto());
            _walletClient.Setup(client => client.Post<PostRedeemCompleteResponseDto>(It.IsAny<string>(),
              It.IsAny<PostRedeemCompleteRequestDto>())).ReturnsAsync(new PostRedeemCompleteResponseDto());

            // Act
            var result = await _adminController.Redeem(requestDto);

            // Assert
            Assert.IsType<OkObjectResult>(result.Result);
            var okObjectResult = result.Result as OkObjectResult;
            Assert.Equal(StatusCodes.Status200OK, okObjectResult?.StatusCode);
        }

        [Fact]
        public async System.Threading.Tasks.Task Redeem_Should_Return_Internal_Server_Error_Result_When_Exception_Occurred()
        {
            // Arrange
            var requestDto = new PostRedeemStartRequestDto();
            _walletClient.Setup(client => client.Post<PostRedeemStartResponseDto>(It.IsAny<string>(),
              It.IsAny<PostRedeemStartRequestDto>())).ThrowsAsync(new Exception("Test Exception"));

            // Act
            var result = await _adminController.Redeem(requestDto);

            // Assert
            Assert.IsType<ObjectResult>(result.Result);
            var objectResult = result.Result as ObjectResult;
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult?.StatusCode);
        }
        #endregion

        #region CreateConsumerAccount Unit Tests
        [Fact]
        public async System.Threading.Tasks.Task CreateConsumerAccount_Should_Create_Consumer_Account()
        {
            // Arrange
            var createConsumerAccountRequestDto = new CreateConsumerAccountRequestMockDto();


            // Act
            var result = await _adminController.CreateConsumerAccount(createConsumerAccountRequestDto);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<OkObjectResult>(result.Result);
            var okObjectResult = result.Result as OkObjectResult;
            Assert.Equal(StatusCodes.Status200OK, okObjectResult?.StatusCode);

        }

        [Fact]
        public async System.Threading.Tasks.Task CreateConsumerAccount_Should_Throw_Internal_Server_Error()
        {
            // Arrange
            var createConsumerAccountRequestDto = new CreateConsumerAccountRequestMockDto();
            _fisClient.Setup(client => client.Post<ConsumerAccountDto>("create-consumer-account", It.IsAny<CreateConsumerAccountRequestDto>()))
               .ReturnsAsync(new ConsumerAccountDto()
               {
                   ConsumerAccountId = 0
               });

            // Act
            var result = await _adminController.CreateConsumerAccount(createConsumerAccountRequestDto);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ObjectResult>(result.Result);
            var objectResult = result.Result as ObjectResult;
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult?.StatusCode);

        }
        #endregion

        #region GetConsumerAccount Unit Tests

        [Fact]
        public async System.Threading.Tasks.Task GetConsumerAccount_Should_Return_Consumer_Account()
        {
            // Arrange
            var getConsumerAccountRequestDto = new GetConsumerAccountRequestMockDto();


            // Act
            var result = await _adminController.GetConsumerAccount(getConsumerAccountRequestDto);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<OkObjectResult>(result.Result);
            var okObjectResult = result.Result as OkObjectResult;
            Assert.Equal(StatusCodes.Status200OK, okObjectResult?.StatusCode);
        }

        [Fact]
        public async System.Threading.Tasks.Task GetConsumerAccount_Should_Return_Not_Found_Result()
        {
            // Arrange
            var getConsumerAccountRequestDto = new GetConsumerAccountRequestMockDto();
            _fisClient.Setup(client => client.Post<GetConsumerAccountResponseDto>("get-consumer-account", It.IsAny<GetConsumerAccountRequestDto>()))
                .ReturnsAsync(new GetConsumerAccountResponseDto()
                {
                    ErrorCode = 404
                });

            // Act
            var result = await _adminController.GetConsumerAccount(getConsumerAccountRequestDto);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ObjectResult>(result.Result);
            var objectResult = result.Result as ObjectResult;
            Assert.Equal(StatusCodes.Status404NotFound, objectResult?.StatusCode);
        }

        [Fact]
        public async System.Threading.Tasks.Task GetConsumerAccount_Should_Throw_Internal_Server_Error()
        {
            // Arrange
            var getConsumerAccountRequestDto = new GetConsumerAccountRequestMockDto();
            _fisClient.Setup(client => client.Post<GetConsumerAccountResponseDto>("get-consumer-account", It.IsAny<GetConsumerAccountRequestDto>()))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _adminController.GetConsumerAccount(getConsumerAccountRequestDto);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ObjectResult>(result.Result);
            var objectResult = result.Result as ObjectResult;
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult?.StatusCode);
        }
        #endregion
        #region sweepstakesInstance    
        [Fact]
        public async System.Threading.Tasks.Task CreateSweepstakesInstance_Should_Create_Sweepstakes_Instance()
        {
            // Arrange
            var createRequestDto = new SweepstakesInstanceRequestMockDto();


            // Act
            var result = await _adminController.CreateSweepstakesInstance(createRequestDto);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<OkObjectResult>(result.Result);
            var okObjectResult = result.Result as OkObjectResult;
            Assert.Equal(StatusCodes.Status200OK, okObjectResult?.StatusCode);

        }

        [Fact]
        public async System.Threading.Tasks.Task CreateSweepstakesInstance_Should_Throw_Internal_Server_Error()
        {
            // Arrange
            var createRequestDto = new SweepstakesInstanceRequestMockDto();
            _sweepstakesClient.Setup(client => client.Post<SweepstakesInstanceResponseDto>("sweepstakes/create-sweepstakes-instance", It.IsAny<SweepstakesInstanceRequestDto>())).Throws(new Exception("Testing exception")); ;


            // Act
            var result = await _adminController.CreateSweepstakesInstance(createRequestDto);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ObjectResult>(result.Result);
            var objectResult = result.Result as ObjectResult;
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult?.StatusCode);

        }

        [Fact]
        public async System.Threading.Tasks.Task Get_sweepstakes_instance_Success_ReturnsOkResult()
        {
            // Arrange
            var sweepstakesInstanceCode = _fixture.Create<string>();
            var sweepstakesInstance = new SweepstakesInstanceMockDto();
            sweepstakesInstance.ErrorCode = null;
            var url = $"{Core.Domain.Constants.Constant.SweepstakesInstanceGetUrl}?sweepstakesInstanceCode={sweepstakesInstanceCode}";
            _sweepstakesClient.Setup(client => client.Get<SweepstakesInstanceDto>(url, null))
                .ReturnsAsync(sweepstakesInstance);

            // Act
            var result = await _adminController.GetSweepstakesInstance(sweepstakesInstanceCode);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<OkObjectResult>(result.Result);
            var okObjectResult = result.Result as OkObjectResult;
            Assert.Equal(StatusCodes.Status200OK, okObjectResult?.StatusCode);

        }

        [Fact]
        public async System.Threading.Tasks.Task Get_sweepstakes_instance_throw_error()
        {
            // Arrange
            var sweepstakesInstanceCode = _fixture.Create<string>();
            var sweepstakesInstance = new SweepstakesInstanceMockDto();
            var url = $"{Core.Domain.Constants.Constant.SweepstakesInstanceGetUrl}?sweepstakesInstanceCode={sweepstakesInstanceCode}";
            _sweepstakesClient.Setup(client => client.Get<SweepstakesInstanceDto>(url, null))
                .ReturnsAsync(sweepstakesInstance);

            // Act
            var result = await _adminController.GetSweepstakesInstance(It.IsAny<string>());

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ObjectResult>(result.Result);
            var objectResult = result.Result as ObjectResult;
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult?.StatusCode);

        }
        #endregion



    }
}