using AutoMapper;
using Azure;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using NHibernate;
using NHibernate.Cfg;
using SunnyRewards.Helios.Common.Core.Domain;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Helpers;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using SunnyRewards.Helios.Common.Core.Repositories;
using SunnyRewards.Helios.Task.Api.Controller;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Dtos.Enums;
using SunnyRewards.Helios.Task.Core.Domain.Models;
using SunnyRewards.Helios.Task.Infrastructure.Helpers;
using SunnyRewards.Helios.Task.Infrastructure.Helpers.Interface;
using SunnyRewards.Helios.Task.Infrastructure.Mappings;
using SunnyRewards.Helios.Task.Infrastructure.Repositories;
using SunnyRewards.Helios.Task.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Task.Infrastructure.Services;
using SunnyRewards.Helios.Task.Infrastructure.Services.Interface;
using SunnyRewards.Helios.Task.UnitTest.Fixtures.MockDtos;
using SunnyRewards.Helios.Task.UnitTest.Fixtures.MockModel;
using SunnyRewards.Helios.Task.UnitTest.Fixtures.MockRepositories;
using SunnyRewards.Helios.Task.UnitTest.Helpers;
using System.Linq.Expressions;
using System.Text;
using Xunit;
using static Google.Apis.Requests.BatchRequest;
using StatusCodes = Microsoft.AspNetCore.Http.StatusCodes;
using Task1 = System.Threading.Tasks.Task;

namespace SunnyRewards.Helios.Task.UnitTest.Controllers
{
    public class ConsumerTaskControllerUnitTest
    {
        private readonly Mock<ILogger<ConsumerTaskController>> _consumerTaskLogger;
        private readonly IConsumerTaskService _consumerTaskService;
        private readonly IMapper _mapper;
        private readonly Mock<NHibernate.ISession> _session;
        private readonly Mock<ILogger<ConsumerTaskService>> _consumerTaskServiceLogger;
        private readonly Mock<IConsumerTaskRepo> _consumerTaskRepo;
        private readonly Mock<ITaskRepo> _taskRepository;
        private readonly Mock<ITaskRewardRepo> _taskRewardRepository;
        private readonly Mock<ITaskDetailRepo> _taskDetailRepository;
        private readonly Mock<ITermsOfServiceRepo> _termsOfServiceRepository;
        private readonly Mock<ITaskTypeRepo> _taskTypeRepository;
        private readonly ITaskRewardService _taskRewardService;
        private readonly Mock<ILogger<TaskRewardService>> _taskRewardServicelogger;
        private readonly Mock<ITenantTaskCategoryRepo> _tenantTaskCategoryRepo;
        private readonly Mock<ISubTaskRepo> _subTaskRepo;
        private readonly ISubtaskService _subTaskService;
        private readonly Mock<ILogger<SubTaskService>> _subtaskServiceLogger;
        private readonly ConsumerTaskController _consumerTaskController;
        private readonly Mock<ITaskRewardTypeRepo> _taskRewardTypeRepository;
        private readonly Mock<IFileHelper> _fileHelper;
        private readonly Mock<IConfiguration> _config;
        private readonly Mock<IVault> _vault;
        private readonly Mock<ITaskCommonHelper> _taskCommonHelper;
        private readonly Mock<ITaskRewardCollectionRepo> _taskRewardCollectionRepo;
        private readonly Mock<ICommonTaskRewardService> _commonTaskRewardService;
        private readonly Mock<IHeliosEventPublisher<ConsumerTaskEventDto>> _heliosEventPublisher;
        private readonly Mock<IAdventureRepo> _adventureRepo;

        public ConsumerTaskControllerUnitTest()
        {
            _session = new Mock<NHibernate.ISession>();
            _consumerTaskServiceLogger = new Mock<ILogger<ConsumerTaskService>>();
            _consumerTaskLogger = new Mock<ILogger<ConsumerTaskController>>();
            _consumerTaskRepo = new ConsumerTaskMockRepo();
            _taskRepository = new TaskMockRepo();
            _taskRewardServicelogger = new Mock<ILogger<TaskRewardService>>();
            _taskRewardRepository = new TaskRewardMockRepo();
            _taskDetailRepository = new TaskDetailMockRepo();
            _taskTypeRepository = new TaskTypeMockRepo();
            _taskRewardCollectionRepo = new Mock<ITaskRewardCollectionRepo>();
            _termsOfServiceRepository = new TermsOfServiceMockRepo();
            _tenantTaskCategoryRepo = new TenantTaskCategoryMockRepo();
            _subTaskRepo = new SubTaskMockRepo();
            _taskRewardTypeRepository = new TaskRewardTypeMockRepo();
            _subtaskServiceLogger = new Mock<ILogger<SubTaskService>>();
            _fileHelper = new FileHelperMock();
            _config = new ConfigurationMock();
            _vault = new VaultMock();
            _taskCommonHelper = new Mock<ITaskCommonHelper>();
            _commonTaskRewardService = new Mock<ICommonTaskRewardService>();
            _heliosEventPublisher = new Mock<IHeliosEventPublisher<ConsumerTaskEventDto>>();
            _adventureRepo = new Mock<IAdventureRepo>();
            _mapper = new Mapper(new MapperConfiguration(
                configure =>
                {
                    configure.AddMaps(typeof(Infrastructure.Mappings.MappingProfile.TaskMapping).Assembly.FullName);
                    configure.AddMaps(typeof(Infrastructure.Mappings.MappingProfile.TaskDetailMapping).Assembly.FullName);
                    configure.AddMaps(typeof(Infrastructure.Mappings.MappingProfile.SubTaskMapping).Assembly.FullName);
                    configure.AddMaps(typeof(Infrastructure.Mappings.MappingProfile.ConsumerTaskMapping).Assembly.FullName);
                }));
            _taskRewardService = new TaskRewardService(_taskRewardServicelogger.Object, _mapper, _taskRewardRepository.Object,
                 _taskRepository.Object, _taskDetailRepository.Object, _termsOfServiceRepository.Object, _consumerTaskRepo.Object, _tenantTaskCategoryRepo.Object, _taskTypeRepository.Object,
                 _taskRewardTypeRepository.Object, _taskRewardCollectionRepo.Object, _commonTaskRewardService.Object, _adventureRepo.Object);
            _subTaskService = new SubTaskService(_subtaskServiceLogger.Object, _taskRepository.Object, _taskRewardRepository.Object, _taskTypeRepository.Object,
                  _subTaskRepo.Object, _consumerTaskRepo.Object, _mapper);
            _consumerTaskService = new ConsumerTaskService(_consumerTaskServiceLogger.Object, _mapper, _session.Object, _consumerTaskRepo.Object,
              _taskRepository.Object, _taskDetailRepository.Object, _taskRewardRepository.Object, _termsOfServiceRepository.Object, _taskRewardService,
              _tenantTaskCategoryRepo.Object, _taskTypeRepository.Object, _subTaskService, _taskRewardTypeRepository.Object, _fileHelper.Object, _config.Object, _vault.Object, _taskCommonHelper.Object, _commonTaskRewardService.Object
              , _heliosEventPublisher.Object);

            _consumerTaskController = new ConsumerTaskController(_consumerTaskLogger.Object, _consumerTaskService, _subTaskService);

        }
        //#1
        [Fact]
        public async void Should_Get_Consumer_Task_By_Id()
        {
            var findConsumerTasksByIdRequestMockDto = new FindConsumerTasksByIdRequestMockDto();
            findConsumerTasksByIdRequestMockDto.LanguageCode = "es";
            var response = await _consumerTaskController.GetConsumerTaskById(findConsumerTasksByIdRequestMockDto);
            var result = response?.Result as OkObjectResult;
            Assert.True(result?.StatusCode == 200);
        }

        [Fact]
        public async void Should_Get_Consumer_Task_By_Id_Zero()
        {
            var findConsumerTasksByIdRequestMockDto = new FindConsumerTasksByIdRequestMockDto()
            {
                ConsumerCode = "cmr-c69905fc68ce4f36851f877bae38f22e",
                TaskStatus = "completed",
                TaskId = 0,
                TaskCode = "tsk-210ddf7876234c11b64668d4246f0b44"
            };
            var response = await _consumerTaskController.GetConsumerTaskById(findConsumerTasksByIdRequestMockDto);
            var result = response?.Result as OkObjectResult;
            Assert.True(result?.StatusCode == 200);
        }


        [Fact]
        public async void Should_Get_Null_Response_For_Given_Request()
        {
            var findConsumerTasksByIdRequestMockDto = new FindConsumerTasksByIdRequestMockDto()
            {
                TaskStatus = "Pending",
            };
            var response = await _consumerTaskController.GetConsumerTaskById(findConsumerTasksByIdRequestMockDto);
            var result = response?.Result as OkObjectResult;
            Assert.True(result?.StatusCode == 200);
        }

        [Fact]
        public async void GetConsumerTaskById_Should_Return_Catch_Exception_In_Controller()
        {

            var loggerMock = new Mock<ILogger<ConsumerTaskController>>();
            var serviceMock = new Mock<IConsumerTaskService>();
            var controller = new ConsumerTaskController(loggerMock.Object, serviceMock.Object, null);
            var findConsumerTasksByIdRequestMockDto = new FindConsumerTasksByIdRequestMockDto();
            var expectedException = new Exception("Test exception");
            serviceMock.Setup(x => x.GetConsumerTask(findConsumerTasksByIdRequestMockDto))
           .ThrowsAsync(expectedException);
            var response = await controller.GetConsumerTaskById(findConsumerTasksByIdRequestMockDto);
            Assert.True(response?.Value?.TaskRewardDetail == null);

        }

        [Fact]
        public async void FindConsumerTaskAsync_When_TaskStatus_Is_NullOrEmpty_Should_Return_Task()
        {
            var consumerTaskRepoMock = new Mock<IConsumerTaskRepo>();
            var findConsumerTasksByIdRequestDto = new FindConsumerTasksByIdRequestDto
            {
                ConsumerCode = "cmr-c69905fc68ce4f36851f877bae38f22e",
                TaskId = 11,
                TaskStatus = null
            };
            consumerTaskRepoMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<ConsumerTaskModel, bool>>>(), false)).ReturnsAsync(ConsumerTaskMockModel.consumerData());
            var actualConsumerTask = await _consumerTaskService.GetConsumerTask(findConsumerTasksByIdRequestDto);
            Assert.NotNull(actualConsumerTask);
        }

        //#2
        [Fact]
        public async void Should_Get_Consumer_Task()
        {
            var findConsumerTaskRequestMockDto = new FindConsumerTaskRequestMockDto();
            var response = await _consumerTaskController.FindConsumerTasks(findConsumerTaskRequestMockDto);
            var result = response as OkObjectResult;
            Assert.True(result?.Value != null);
            Assert.True(result.StatusCode == 200);
        }

        [Fact]
        public async void FindConsumer_TaskData_null()
        {
            var findConsumerTasksByIdRequestMockDto = new FindConsumerTasksByIdRequestMockDto();
            _taskRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskModel, bool>>>(), false))
                .ReturnsAsync((TaskMockModel)null);
            var response = await _consumerTaskService.GetConsumerTask(findConsumerTasksByIdRequestMockDto);
            Assert.True(response.ConsumerTask == null);
            Assert.True(response.TaskRewardDetail == null);
        }

        [Fact]
        public async void FindConsumer_TaskCode_null()
        {
            var findConsumerTasksByIdRequestMockDto = new FindConsumerTasksByIdRequestMockDto()
            {
                ConsumerCode = "cmr-c69905fc68ce4f36851f877bae38f22e",
                TaskStatus = "completed",
                TaskId = 0,
                TaskCode = "tsk-210ddf7876234c11b64668d4246f0b44",
                TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4"
            };
            _taskRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskModel, bool>>>(), false))
                .ReturnsAsync((TaskMockModel)null);
            var response = await _consumerTaskService.GetConsumerTask(findConsumerTasksByIdRequestMockDto);
            Assert.True(response.ConsumerTask == null);
            Assert.True(response.TaskRewardDetail == null);
        }

        [Fact]
        public async void Should_FindConsumer_Get_TaskExternalCode()
        {
            var findConsumerTasksByIdExternalCodeRequestMockDto = new FindConsumerTasksByIdExternalCodeRequestMockDto();
            _taskRewardRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false)).ReturnsAsync(new TaskRewardExternalCodeMockModel());
            var response = await _consumerTaskService.GetConsumerTask(findConsumerTasksByIdExternalCodeRequestMockDto);
            Assert.True(response.ConsumerTask != null);
            Assert.True(response.TaskRewardDetail != null);
        }
        [Fact]
        public async void Should_FindConsumer_Get_TaskExternalCode_Returns_task_rewardNull()
        {
            var findConsumerTasksByIdExternalCodeRequestMockDto = new FindConsumerTasksByIdExternalCodeRequestMockDto();
            _taskRewardRepository
                    .Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false));

            // Act
            var response = await _consumerTaskService.GetConsumerTask(findConsumerTasksByIdExternalCodeRequestMockDto);

            // Assert
            Assert.NotNull(response);
            Assert.Null(response.ConsumerTask);
            Assert.Null(response.TaskRewardDetail);
        }

        [Fact]
        public async void Should_FindConsumer_Get_TaskExternalCode_Returns_Consumertask_Null()
        {
            var findConsumerTasksByIdExternalCodeRequestMockDto = new FindConsumerTasksByIdExternalCodeRequestMockDto();
            _taskRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskModel, bool>>>(), false))
         .ReturnsAsync(new TaskMockModel());
            _consumerTaskRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<ConsumerTaskModel, bool>>>(), false))
                .ReturnsAsync(new List<ConsumerTaskModel>()); // Simulate no consumer tasks found

            // Act
            var response = await _consumerTaskService.GetConsumerTask(findConsumerTasksByIdExternalCodeRequestMockDto);

            // Assert
            Assert.NotNull(response);
            Assert.Null(response.ConsumerTask);
        }

        [Fact]
        public async void Should_FindConsumer_Get_TaskExternalCode_NullCheck()
        {
            var findConsumerTasksByIdExternalCodeRequestMockDto = new FindConsumerTasksByIdExternalCodeRequestMockDto();
            _taskRewardRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false)).ReturnsAsync((TaskRewardExternalCodeMockModel)null);
            var response = await _consumerTaskService.GetConsumerTask(findConsumerTasksByIdExternalCodeRequestMockDto);
            Assert.Null(response.ConsumerTask);
            Assert.Null(response.TaskRewardDetail);
        }

        [Fact]
        public async void Should_FindConsumer_Get_taskRewardModel_Null()
        {
            var findConsumerTasksByIdExternalCodeRequestMockDto = new FindConsumerTasksByIdExternalCodeRequestMockDto();
            _taskRewardRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false)).ReturnsAsync(new TaskRewardExternalCodeMockModel());
            _taskRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskModel, bool>>>(), false))
                .ReturnsAsync((TaskMockModel)null);
            var response = await _consumerTaskService.GetConsumerTask(findConsumerTasksByIdExternalCodeRequestMockDto);
            Assert.True(response.ConsumerTask == null);
            Assert.True(response.TaskRewardDetail == null);
        }

        [Fact]
        public async void Should_consumerTask_Service_nullCheck()
        {
            var findConsumerTasksByIdRequestMockDto = new FindConsumerTasksByIdRequestMockDto();
            _taskRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskModel, bool>>>(), false))
                .ReturnsAsync((TaskMockModel)null);
            var response = await _consumerTaskService.GetConsumerTask(findConsumerTasksByIdRequestMockDto);
            Assert.True(response.ConsumerTask == null);
            Assert.True(response.TaskRewardDetail == null);
        }

        [Fact]
        public async void GetConsumerTask_ReturnsEmptyResponseDto_WhenTaskRepoReturnsNull()
        {
            var findConsumerTasksByIdRequestMockDto = new FindConsumerTasksByIdRequestMockDto();
            _taskRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskModel, bool>>>(), false))
                .ReturnsAsync((TaskMockModel)null);
            var result = await _consumerTaskService.GetConsumerTask(findConsumerTasksByIdRequestMockDto);
            Assert.NotNull(result);
        }

        [Fact]
        public async void FindConsumerTasks_Should_Return_Catch_Exception_In_Controller()
        {
            var loggerMock = new Mock<ILogger<ConsumerTaskController>>();
            var serviceMock = new Mock<IConsumerTaskService>();
            var controller = new ConsumerTaskController(loggerMock.Object, serviceMock.Object, _subTaskService);
            var findConsumerTaskRequestMockDto = new FindConsumerTaskRequestMockDto();
            var expectedException = new Exception("Test exception");
            serviceMock.Setup(x => x.GetConsumerTasks(findConsumerTaskRequestMockDto))
           .ThrowsAsync(expectedException);
            var response = await controller.FindConsumerTasks(findConsumerTaskRequestMockDto);
            var result = response as ObjectResult;
            Assert.False(result?.StatusCode == 500);
        }

        [Fact]
        public async void FindConsumerTasks_Should_TaskData_Null_Service()
        {
            var findConsumerTaskRequestMockDto = new FindConsumerTaskRequestMockDto();
            _taskRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskModel, bool>>>(), false))
                .ReturnsAsync((TaskMockModel)null);
            var response = await _consumerTaskService.GetConsumerTasks(findConsumerTaskRequestMockDto);
            Assert.True(response.ConsumerTask != null);
            Assert.True(response?.TaskRewardDetail?.Count == 0);
        }

        [Fact]
        public async void FindConsumerTasks_Should_TaskReward_Null_Service()
        {
            var findConsumerTaskRequestMockDto = new FindConsumerTaskRequestMockDto();
            _taskRewardRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false)).ReturnsAsync((TaskRewardExternalCodeMockModel)null);
            var response = await _consumerTaskService.GetConsumerTasks(findConsumerTaskRequestMockDto);
            Assert.True(response.ConsumerTask != null);
            Assert.True(response?.TaskRewardDetail?.Count == 0);
        }

        [Fact]
        public async void FindConsumerTasks_Should_ValidStart_Service()
        {
            var findConsumerTaskRequestMockDto = new FindConsumerTaskRequestMockDto();
            _taskRewardRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false)).ReturnsAsync(new TaskRewardMockModel());
            var response = await _consumerTaskService.GetConsumerTasks(findConsumerTaskRequestMockDto);
            Assert.True(response.TaskRewardDetail != null);
        }

        [Fact]
        public async void FindConsumerTasks_Should_ValidStart_Service_ValidStarts()
        {
            var findConsumerTaskRequestMockDto = new FindConsumerTaskRequestMockDto();
            DateTime tsNow = DateTime.UtcNow;
            _taskRewardRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false))
            .ReturnsAsync(new TaskRewardMockModel
            {
                ValidStartTs = tsNow.AddMinutes(10),
                Expiry = tsNow.AddMinutes(-10)
            });
            var response = await _consumerTaskService.GetConsumerTasks(findConsumerTaskRequestMockDto);
            Assert.NotNull(response.TaskRewardDetail);
        }
        [Fact]
        public async void FindConsumerTasks_Should_ValidStart_Service_Completed()
        {
            var findConsumerTaskRequestMockDto = new FindConsumerTaskRequestDto
            {
                ConsumerCode = "cmr-4e7b645aec2043608ebdeecc18b75602",
                TaskStatus = "completed"
            };
            _taskRewardRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false)).ReturnsAsync(new TaskRewardMockModel());
            var response = await _consumerTaskService.GetConsumerTasks(findConsumerTaskRequestMockDto);
            Assert.NotNull(response.TaskRewardDetail);
        }

        [Fact]
        public async void FindConsumerTasks_Should_TaskRewardDetailDto_Service()
        {
            var findConsumerTaskRequestMockDto = new FindConsumerTaskRequestMockDto();
            _taskRewardRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false)).ReturnsAsync(new TaskRewardDetailMockModel());
            var response = await _consumerTaskService.GetConsumerTasks(findConsumerTaskRequestMockDto);
            Assert.True(response.TaskRewardDetail != null);
        }

        [Fact]
        public async void FindConsumerTasks_Should_Return_Exception_Catch_In_Service()
        {
            var findConsumerTaskRequestMockDto = new FindConsumerTaskRequestMockDto();
            _consumerTaskRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<ConsumerTaskModel, bool>>>(), false))
              .ThrowsAsync(new Exception("Simulated exception"));
            var response = await _consumerTaskController.FindConsumerTasks(findConsumerTaskRequestMockDto);
            var result = response as NotFoundObjectResult;
            Assert.False(result?.StatusCode == 404);
        }

        [Fact]
        public async void Create_ConsumerTasks_Should_Ok_Resonse_In_Service()
        {
            var tenantCode = "ten-ecada21e57154928a2bb959e8365b8b4";
            var consumerCode = "cmr-c69905fc68ce4f36851f877bae38f22e";
            long taskId = 1;
            var consumerTaskMockDto = new ConsumerTaskMockDto();
            var consumerTaskMockModel = new ConsumerTaskMockModel();
            _consumerTaskRepo.Setup(x => x.GetConsumerTasksWithRewards(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>()))
                .ReturnsAsync((ConsumerTaskRewardModel)null);
            var mapper = new Mock<IMapper>();
            mapper.Setup(x => x.Map<ConsumerTaskModel>(It.IsAny<ConsumerTaskDto>()))
                .Returns(consumerTaskMockModel);
            _consumerTaskRepo.Setup(x => x.CreateAsync(It.IsAny<ConsumerTaskModel>()))
                .ReturnsAsync(consumerTaskMockModel);
            mapper.Setup(x => x.Map<ConsumerTaskDto>(It.IsAny<ConsumerTaskModel>()))
                .Returns(consumerTaskMockDto);
            var response = await _consumerTaskService.CreateConsumerTasks(consumerTaskMockDto);
            Assert.NotNull(response);
            Assert.NotNull(response.ConsumerTask);
        }

        [Fact]
        public void Create_ConsumerTasks_Should_Ok_Resonse_In_Service_IsValidRecurring()
        {
            var consumerTaskMockModel = new ConsumerTaskMockModel();
            var taskRewardMockModel = new TaskRewardMockModel();
            taskRewardMockModel.IsRecurring = true;
            bool response = TaskHelper.IsValidRecurring(taskRewardMockModel, consumerTaskMockModel);
            Assert.True(response == true);
        }
        [Fact]
        public void Create_ConsumerTasks_Should_Ok_Resonse_In_Service_IsValidRecurring_Quater()
        {
            var consumerTaskMockModel = new ConsumerTaskMockModel();
            var taskRewardMockModel = new TaskRewardMockModel { RecurrenceDefinitionJson = "{\r\n  \"recurrenceType\": \"PERIODIC\",\r\n  \"periodic\": {\r\n    \"period\": \"QUARTER\",\r\n    \"periodRestartDate\": 1\r\n  }\r\n}" };
            taskRewardMockModel.IsRecurring = true;
            bool response = TaskHelper.IsValidRecurring(taskRewardMockModel, consumerTaskMockModel);
            Assert.True(response);
        }

        [Fact]
        public void Create_ConsumerTasks_Should_Ok_Resonse_In_Service_IsValidRecurring_False()
        {
            var consumerTaskMockModel = new ConsumerTaskMockModel();
            var taskRewardMockModel = new TaskRewardMockModel { RecurrenceDefinitionJson = "{}" };
            taskRewardMockModel.IsRecurring = true;
            bool response = TaskHelper.IsValidRecurring(taskRewardMockModel, consumerTaskMockModel);
            Assert.False(response);
        }

        [Fact]
        public void Create_ConsumerTasks_Should_Ok_Resonse_In_Service_IsValidScheduleRecurring()
        {
            var taskRewardMockDto = new TaskRewardMockDto();
            taskRewardMockDto.IsRecurring = true;
            taskRewardMockDto.RecurrenceDefinitionJson = "{\"recurrenceType\":\"SCHEDULE\",\"schedules\":[{\"startDate\":\"01-01\",\"expiryDate\":\"12-31\"}]}";
            var consumerTaskMockModel = new ConsumerTaskMockModel();
            consumerTaskMockModel.TaskCompleteTs = DateTime.UtcNow.AddYears(-1);
            var recurrenceDetails = JsonConvert.DeserializeObject<RecurringDto>(taskRewardMockDto.RecurrenceDefinitionJson ?? string.Empty);
            bool response = TaskHelper.IsValidScheduleRecurring(recurrenceDetails, taskRewardMockDto.IsRecurring, consumerTaskMockModel);
            Assert.True(response);
        }

        [Fact]
        public void Create_ConsumerTasks_Should_Not_Ok_Resonse_In_Service_IsValidScheduleRecurring()
        {
            var taskRewardMockDto = new TaskRewardMockDto();
            taskRewardMockDto.IsRecurring = true;
            taskRewardMockDto.RecurrenceDefinitionJson = "{\"recurrenceType\":\"SCHEDULE\",\"schedules\":[{\"startDate\":\"01-01\",\"expiryDate\":\"12-31\"}]}";
            var consumerTaskMockModel = new ConsumerTaskMockModel();
            consumerTaskMockModel.TaskCompleteTs = DateTime.UtcNow;
            var recurrenceDetails = JsonConvert.DeserializeObject<RecurringDto>(taskRewardMockDto.RecurrenceDefinitionJson ?? string.Empty);
            bool response = TaskHelper.IsValidScheduleRecurring(recurrenceDetails, taskRewardMockDto.IsRecurring, consumerTaskMockModel);
            Assert.False(response);
        }

        [Theory]
        [InlineData("2025-06-20", 5, "MONTH", "2025-06-05", "2025-07-04 23:59:59.9999999")]
        [InlineData("2025-06-01", 15, "MONTH", "2025-05-15", "2025-06-14 23:59:59.9999999")]
        [InlineData("2025-01-10", 31, "MONTH", "2024-12-31", "2025-01-30 23:59:59.9999999")]
        [InlineData("2025-06-20", 5, "QUARTER", "2025-04-05", "2025-07-04 23:59:59.9999999")]
        [InlineData("2025-06-01", 15, "QUARTER", "2025-04-15", "2025-07-14 23:59:59.9999999")]
        [InlineData("2025-01-10", 31, "QUARTER", "2024-10-31", "2025-01-30 23:59:59.9999999")]
        public void GetPeriodStartAndEndDates_ShouldReturnCorrectPeriod(string nowStr, int restartDay, string period, string expectedStart, string expectedEnd)
        {
            // Arrange
            DateTime fixedNow = DateTime.Parse(nowStr);
            DateTime expectedStartDate = DateTime.Parse(expectedStart);
            DateTime expectedEndDate = DateTime.Parse(expectedEnd);

            // Simulate fixed current date by wrapping or injecting
            var (startDate, endDate) = TaskHelper.GetPeriodStartAndEndDates(fixedNow, restartDay, period); // Injected fixedNow

            // Assert
            Assert.Equal(expectedStartDate, startDate);
            Assert.Equal(expectedEndDate, endDate);
        }


        [Fact]
        public async Task1 CreateConsumerTasks_Should_Return_Conflict_When_Task_Incorrect_State()
        {
            // Arrange
            var consumerTaskDto = new ConsumerTaskDto
            {
                TaskId = 1,
                ConsumerCode = "consumer123",
                TenantCode = "tenant1",
                TaskStatus = Constants.Completed,
                TaskCompleteTs = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 2)
            };

            var existingTaskAndReward = new ConsumerTaskRewardModel
            {
                ConsumerTask = new ConsumerTaskModel
                {
                    TaskId = 1,
                    TaskStatus = Constants.Completed,
                    TaskCompleteTs = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 2),
                    TenantCode = "tenant1",
                    ConsumerTaskId = 101
                },
                TaskReward = new TaskRewardModel
                {
                    IsRecurring = true,
                    RecurrenceDefinitionJson = "{ \"recurrenceType\": \"PERIODIC\", \"periodic\": { \"period\": \"MONTH\", \"MaxOccurrences\": 1, \"periodRestartDate\": 1 } }"
                }
            };

            var recurrenceDetails = JsonConvert.DeserializeObject<RecurringDto>(existingTaskAndReward.TaskReward.RecurrenceDefinitionJson);

            // Mock repository and helper behavior
            _consumerTaskRepo.Setup(repo => repo.GetConsumerTasksWithRewards(consumerTaskDto.TenantCode, consumerTaskDto.ConsumerCode, consumerTaskDto.TaskId))
                .ReturnsAsync(existingTaskAndReward);

            _consumerTaskRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<ConsumerTaskModel, bool>>>(), false))
                .ReturnsAsync([existingTaskAndReward.ConsumerTask]);

            // Act
            var result = await _consumerTaskService.CreateConsumerTasks(consumerTaskDto);

            // Assert
            Assert.Equal(StatusCodes.Status409Conflict, result.ErrorCode);
        }

        [Fact]
        public async Task1 CreateConsumerTasks_Should_Return_Conflict_When_Task_Reaches_Max_Occurrences()
        {
            // Arrange
            var consumerTaskDto = new ConsumerTaskDto
            {
                TaskId = 1,
                ConsumerCode = "consumer123",
                TenantCode = "tenant1",
                TaskStatus = Constants.Completed,
                TaskCompleteTs = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 2)
            };

            var existingTaskAndReward = new ConsumerTaskRewardModel
            {
                ConsumerTask = new ConsumerTaskModel
                {
                    TaskId = 1,
                    TaskStatus = Constants.Completed,
                    TaskCompleteTs = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 2),
                    TenantCode = "tenant1",
                    ConsumerTaskId = 101
                },
                TaskReward = new TaskRewardModel
                {
                    IsRecurring = true,
                    RecurrenceDefinitionJson = "{ \"recurrenceType\": \"PERIODIC\", \"periodic\": { \"period\": \"MONTH\", \"MaxOccurrences\": 1, \"periodRestartDate\": 1 } }"
                }
            };

            var recurrenceDetails = JsonConvert.DeserializeObject<RecurringDto>(existingTaskAndReward.TaskReward.RecurrenceDefinitionJson);

            // Mock repository and helper behavior
            _consumerTaskRepo.Setup(repo => repo.GetConsumerTasksWithRewards(consumerTaskDto.TenantCode, consumerTaskDto.ConsumerCode, consumerTaskDto.TaskId))
                .ReturnsAsync(existingTaskAndReward);

            _consumerTaskRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<ConsumerTaskModel, bool>>>(), false))
                .ReturnsAsync([existingTaskAndReward.ConsumerTask]);

            // Act
            var result = await _consumerTaskService.CreateConsumerTasks(consumerTaskDto);

            // Assert
            Assert.Equal(StatusCodes.Status409Conflict, result.ErrorCode);
            Assert.Equal($"The maximum number of allowed occurrences ({recurrenceDetails?.periodic?.MaxOccurrences}) for this task within the MONTH recurrence period has been reached. Further completions will be available starting from the next period restart date.", result.ErrorMessage);
        }
        [Fact]
        public async Task1 CreateConsumerTasks_Should_Return_Conflict_When_ParentTaskEligibility_Is_Invalid()
        {
            // Arrange
            var consumerTaskDto = new ConsumerTaskDto
            {
                TaskId = 1,
                ConsumerCode = "consumer123",
                TenantCode = "tenant1",
                TaskStatus = Constants.Completed,
                ConsumerTaskId = 101,
                ParentConsumerTaskId = 1
            };

            var existingConsumerTask = new ConsumerTaskModel
            {
                TaskId = 1,
                TaskStatus = Constants.Completed,
                TaskCompleteTs = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 2),
                TenantCode = "tenant1",
                ConsumerTaskId = 101,
            };

            var parentTaskAndReward = new ConsumerTaskRewardModel
            {
                ConsumerTask = new ConsumerTaskModel
                {
                    TaskId = 1,
                    TaskStatus = Constants.Completed,
                    TaskCompleteTs = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 2),
                    TenantCode = "tenant1",
                    ParentConsumerTaskId = 1
                },
                TaskReward = new TaskRewardModel
                {
                    IsRecurring = true,
                    RecurrenceDefinitionJson = "{ \"recurrenceType\": \"PERIODIC\", \"periodic\": { \"period\": \"QUARTER\", \"MaxOccurrences\": 1, \"periodRestartDate\": 1 } }"
                }
            };

            var recurrenceDetails = JsonConvert.DeserializeObject<RecurringDto>(parentTaskAndReward.TaskReward.RecurrenceDefinitionJson);

            // Mock repository and helper behavior
            _consumerTaskRepo.Setup(repo => repo.GetConsumerTasksWithRewards(consumerTaskDto.TenantCode, consumerTaskDto.ConsumerCode, consumerTaskDto.TaskId))
                .ReturnsAsync(parentTaskAndReward);

            _consumerTaskRepo.Setup(repo => repo.GetConsumerTaskWithReward(consumerTaskDto.TenantCode, (long)consumerTaskDto.ParentConsumerTaskId, consumerTaskDto.TaskStatus))
                .ReturnsAsync(parentTaskAndReward);

            _consumerTaskRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<ConsumerTaskModel, bool>>>(), false))
                .ReturnsAsync([existingConsumerTask]);

            // Act
            var result = await _consumerTaskService.CreateConsumerTasks(consumerTaskDto);

            // Assert
            Assert.Equal(StatusCodes.Status409Conflict, result.ErrorCode);
            Assert.Equal($"The maximum number of allowed occurrences ({recurrenceDetails?.periodic?.MaxOccurrences}) for this task within the {recurrenceDetails?.periodic?.period} recurrence period has been reached. Further completions will be available starting from the next period restart date.", result.ErrorMessage);
        }
        [Fact]
        public async void Create_ConsumerTasks_Should_NotNull_In_Service()
        {
            _consumerTaskRepo.Setup(repo => repo.GetConsumerTasksWithRewards(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>()))
                                 .ReturnsAsync(new ConsumerTaskRewardMockModel());
            var consumerTaskMockDto = new ConsumerTaskMockDto();
            var response = await _consumerTaskController.PostConsumerTasks(consumerTaskMockDto);
            var result = response?.Result as ConflictObjectResult;
            Assert.True(result?.StatusCode == 409);
        }

        [Fact]
        public async void Create_ConsumerTasks_Should_Return_409_When_ValidStartTs_Is_Not_Valid()
        {
            _consumerTaskRepo.Setup(repo => repo.GetConsumerTasksWithRewards(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>()))
                                 .ReturnsAsync(new ConsumerTaskRewardMockModel());
            var mockRewardData = new TaskRewardIsMockModel().taskRewardData();
            mockRewardData[0].ValidStartTs = DateTime.UtcNow.AddMonths(1);
            mockRewardData[1].ValidStartTs = DateTime.UtcNow.AddDays(1);
            _taskRewardRepository.Setup(x => x.FindAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false)).ReturnsAsync(mockRewardData);
            var consumerTaskMockDto = new ConsumerTaskMockDto();
            var response = await _consumerTaskController.PostConsumerTasks(consumerTaskMockDto);
            var result = response?.Result as ConflictObjectResult;
            Assert.True(result?.StatusCode == 409);
        }

        [Fact]
        public async void Create_ConsumerTasks_Should_Return_409_When_TaskExpiryTS_Is_Not_Valid()
        {
            _consumerTaskRepo.Setup(repo => repo.GetConsumerTasksWithRewards(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>()))
                                 .ReturnsAsync(new ConsumerTaskRewardMockModel());
            var mockRewardData = new TaskRewardIsMockModel().taskRewardData();
            mockRewardData[0].Expiry = DateTime.UtcNow.AddYears(-1);
            mockRewardData[1].Expiry = DateTime.UtcNow.AddMonths(-1);
            _taskRewardRepository.Setup(x => x.FindAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false)).ReturnsAsync(mockRewardData);
            var consumerTaskMockDto = new ConsumerTaskMockDto();
            var response = await _consumerTaskController.PostConsumerTasks(consumerTaskMockDto);
            var result = response?.Result as ConflictObjectResult;
            Assert.True(result?.StatusCode == 409);
        }

        [Fact]
        public async void Create_ConsumerTasks_Should_Return_409_When_TaskReward_Is_Not_Valid()
        {
            _consumerTaskRepo.Setup(repo => repo.GetConsumerTasksWithRewards(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>()))
                                 .ReturnsAsync(new ConsumerTaskRewardMockModel());
            var mockRewardData = new TaskRewardIsMockModel().taskRewardData();
            mockRewardData[0].Expiry = DateTime.UtcNow.AddYears(-1);
            mockRewardData[1].Expiry = DateTime.UtcNow.AddMonths(-1);
            _taskRewardRepository.Setup(x => x.FindAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false));
            var consumerTaskMockDto = new ConsumerTaskMockDto();
            var response = await _consumerTaskController.PostConsumerTasks(consumerTaskMockDto);
            var result = response?.Result as ConflictObjectResult;
            Assert.True(result?.StatusCode == 409);
        }

        [Fact]
        public async void Create_ConsumerTasks_Should_Catch_Exception()
        {
            Mock<ISession> mockSession;
            Mock<IMapper> mockMapper;
            Mock<IConsumerTaskRepo> mockConsumerTaskRepo;
            ConsumerTaskService service;
            CreateConsumerTask(out mockSession, out mockMapper, out mockConsumerTaskRepo, out service);
            await Assert.ThrowsAsync<Exception>(async () => await service.CreateConsumerTasks(new ConsumerTaskDto()));
            mockSession.Verify(x => x.BeginTransaction(), Times.Once);
            mockMapper.Verify(x => x.Map<ConsumerTaskModel>(It.IsAny<ConsumerTaskDto>()), Times.Once);
            mockConsumerTaskRepo.Verify(x => x.CreateAsync(It.IsAny<ConsumerTaskModel>()), Times.Once);
        }

        //#3      
        [Fact]
        public async void Should_Post_Consumer_Tasks()
        {
            var consumerTaskMockDto = new ConsumerTaskMockDto();
            _consumerTaskRepo.Setup(x => x.GetConsumerTasksWithRewards(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>()))
                .ReturnsAsync((ConsumerTaskRewardModel)null);
            var response = await _consumerTaskController.PostConsumerTasks(consumerTaskMockDto);
            var result = response?.Result as OkObjectResult;
            Assert.True(result?.Value != null);
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public async void PostConsumerTasks_Should_Return_Catch_Exception_In_Controller()
        {
            var loggerMock = new Mock<ILogger<ConsumerTaskController>>();
            var serviceMock = new Mock<IConsumerTaskService>();
            var controller = new ConsumerTaskController(loggerMock.Object, serviceMock.Object, _subTaskService);
            var consumerTaskMockDto = new ConsumerTaskMockDto();
            var expectedException = new Exception("Test exception");
            serviceMock.Setup(x => x.CreateConsumerTasks(consumerTaskMockDto))
           .ThrowsAsync(expectedException);
            var response = await controller.PostConsumerTasks(consumerTaskMockDto);
            var result = response?.Result as NotFoundResult;
            Assert.False(result?.StatusCode == 404);
        }

        //#4
        [Fact]
        public async void Should_ConsumerTask_Update()
        {
            var consumerTaskMockDto = new UpdateConsumerTaskMockDto();
            var response = await _consumerTaskController.UpdateConsumerTask(consumerTaskMockDto);
            var result = response.Result as OkObjectResult;
            Assert.NotNull(result?.Value);
            Assert.Equal(200, result.StatusCode);
        }
        private static Microsoft.AspNetCore.Http.FormFile GetMockFormFile()
        {
            var fileName = "TestDocument.pdf";
            var content = "This is a test document.";
            var contentBytes = Encoding.UTF8.GetBytes(content);
            var stream = new MemoryStream(contentBytes);

            var formFile = new Microsoft.AspNetCore.Http.FormFile(stream, 0, contentBytes.Length, "TaskCompletionEvidenceDocument", fileName)
            {
                Headers = new Microsoft.AspNetCore.Http.HeaderDictionary(),
                ContentType = "application/pdf"
            };

            return formFile;
        }
        [Fact]
        public async void Should_ConsumerTask_Update_TaskCompleteTs()
        {
            var consumerTaskMockDto = new UpdateConsumerTaskMockDto();
            consumerTaskMockDto.TaskCompleteTs = null;
            consumerTaskMockDto.SpinWheelTaskEnabled = true;
            consumerTaskMockDto.TaskCompletionEvidenceDocument = GetMockFormFile();
            var response = await _consumerTaskController.UpdateConsumerTask(consumerTaskMockDto);
            var result = response.Result as OkObjectResult;
            Assert.True(result?.Value != null);
            Assert.True(result.StatusCode == 200);
        }

        [Fact]
        public async void Should_ConsumerTask_Update_Controller()
        {
            var updateConsumerTaskStatusMockDto = new UpdateConsumerTaskStatusMockDto();
            _consumerTaskRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ConsumerTaskModel, bool>>>(), false)).ReturnsAsync(new ConsumerTaskIdZeroMockModel());
            _heliosEventPublisher.Setup(x => x.PublishMessage(It.IsAny<EventHeaderDto>(), It.IsAny<ConsumerTaskEventDto>())).ReturnsAsync(It.IsAny<PublishResultDto>());
            var response = await _consumerTaskController.UpdateConsumerTask(updateConsumerTaskStatusMockDto);
            var result = response.Result as OkObjectResult;
            Assert.True(result?.Value != null);
            Assert.True(result.StatusCode == 200);
        }
        [Fact]
        public async void Should_ConsumerTask_UpdatePublishError_Controller()
        {
            var updateConsumerTaskStatusMockDto = new UpdateConsumerTaskStatusMockDto();
            _consumerTaskRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ConsumerTaskModel, bool>>>(), false)).ReturnsAsync(new ConsumerTaskIdMockModel());
            _heliosEventPublisher.Setup(x => x.PublishMessage(It.IsAny<EventHeaderDto>(), It.IsAny<ConsumerTaskEventDto>())).ReturnsAsync(new PublishResultDto { ErrorCode = StatusCodes.Status400BadRequest.ToString() });
            var response = await _consumerTaskController.UpdateConsumerTask(updateConsumerTaskStatusMockDto);
            var result = response.Result as OkObjectResult;
            Assert.True(result?.Value != null);
            Assert.True(result.StatusCode == 200);
        }
        [Fact]
        public async void Should_ConsumerTask_Exception_for_PublishMessage_Controller()
        {
            var updateConsumerTaskStatusMockDto = new UpdateConsumerTaskStatusMockDto();
            _consumerTaskRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ConsumerTaskModel, bool>>>(), false)).ReturnsAsync(new ConsumerTaskIdZeroMockModel());
            _heliosEventPublisher.Setup(x => x.PublishMessage(It.IsAny<EventHeaderDto>(), It.IsAny<ConsumerTaskEventDto>())).ThrowsAsync(new Exception("Simulated exception"));
            var response = await _consumerTaskController.UpdateConsumerTask(updateConsumerTaskStatusMockDto);
            var result = response.Result as OkObjectResult;
            Assert.True(result?.Value != null);
            Assert.True(result.StatusCode == 200);
        }

        [Fact]
        public async void Should_ConsumerTask_Update_NotFound_Controller()
        {
            var updateConsumerTaskStatusMockDto = new UpdateConsumerTaskStatusMockDto();
            _consumerTaskRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ConsumerTaskModel, bool>>>(), false)).ReturnsAsync(new ConsumerTaskIdZeroMockModel());
            var response = await _consumerTaskController.UpdateConsumerTask(updateConsumerTaskStatusMockDto);
            var result = response.Result as NotFoundResult;
            Assert.True(result == null);
        }

        [Fact]
        public async void Should_ConsumerTask_Update_Exception_Controller()
        {
            var updateConsumerTaskStatusMockDto = new UpdateConsumerTaskStatusMockDto();
            _consumerTaskRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ConsumerTaskModel, bool>>>(), false)).ThrowsAsync(new Exception("Simulated exception"));
            var response = await _consumerTaskController.UpdateConsumerTask(updateConsumerTaskStatusMockDto);
            var result = response.Result as NotFoundResult;
            Assert.True(result == null);
        }

        [Fact]
        public async void Should_ConsumerTask_Update_TaskCode_Controller()
        {
            var updateConsumerTaskStatusMockDto = new UpdateConsumerTaskStatusMockDto() { TaskCode = "" };
            _consumerTaskRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ConsumerTaskModel, bool>>>(), false)).ReturnsAsync(new ConsumerTaskIdZeroMockModel());
            var response = await _consumerTaskController.UpdateConsumerTask(updateConsumerTaskStatusMockDto);
            var result = response.Result as OkObjectResult;
            Assert.True(result.StatusCode == 200);
        }

        [Fact]
        public async void Should_ConsumerTask_Update_TaskReward_Null_Controller()
        {
            var updateConsumerTaskStatusMockDto = new UpdateConsumerTaskStatusMockDto() { TaskCode = "" };
            _consumerTaskRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ConsumerTaskModel, bool>>>(), false)).ReturnsAsync(new ConsumerTaskIdZeroMockModel() { ConsumerTaskId = 0 });
            _taskRewardRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false)).ReturnsAsync((TaskRewardMockModel)null);
            var response = await _consumerTaskController.UpdateConsumerTask(updateConsumerTaskStatusMockDto);
            var result = response.Result as BadRequestObjectResult;
            Assert.True(result?.StatusCode == 400);
        }

        [Fact]
        public async void Should_ConsumerTask_Update_TenantCode_Null_Controller()
        {
            var updateConsumerTaskStatusMockDto = new UpdateConsumerTaskStatusMockDto() { TenantCode = "", TaskId = 1 };
            _consumerTaskRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ConsumerTaskModel, bool>>>(), false)).ReturnsAsync(new ConsumerTaskIdZeroMockModel() { ConsumerTaskId = 0 });
            var response = await _consumerTaskController.UpdateConsumerTask(updateConsumerTaskStatusMockDto);
            var result = response.Result as BadRequestObjectResult;
            Assert.True(result?.StatusCode == 400);
        }

        [Fact]
        public async void Should_ConsumerTask_Update_Exception_Service()
        {
            var updateConsumerTaskStatusMockDto = new UpdateConsumerTaskStatusMockDto() { TaskCode = "" };
            _consumerTaskRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ConsumerTaskModel, bool>>>(), false)).ThrowsAsync(new Exception("Simulated exception"));
            await Assert.ThrowsAsync<Exception>(async () => await _consumerTaskService.UpdateConsumerTask(updateConsumerTaskStatusMockDto));
        }

        [Fact]
        public async void Should_ConsumerTask_Update_Try_Exception_Service()
        {
            var updateConsumerTaskStatusMockDto = new UpdateConsumerTaskStatusMockDto() { TaskCode = "" };
            _taskRewardRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false)).ThrowsAsync(new Exception("Simulated exception"));
            await _consumerTaskService.UpdateConsumerTask(updateConsumerTaskStatusMockDto);
        }

        [Fact]
        public async void Should_ConsumerTask_Update_Should_Throw_Exception_When_Task_Complete_Date_Is_Invalid()
        {
            var updateConsumerTaskStatusMockDto = new UpdateConsumerTaskStatusMockDto() { TaskCode = "", TaskCompleteTs = DateTime.UtcNow.AddMonths(-1), TaskStatus = "COMPLETED" };
            _taskRewardRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false)).ThrowsAsync(new Exception("Simulated exception"));
            await _consumerTaskService.UpdateConsumerTask(updateConsumerTaskStatusMockDto);
        }
        [Fact]
        public async void Should_ConsumerTask_Update_Should_Return_Error_Response_When_Task_Complete_Date_Is_Invalid()
        {
            var updateConsumerTaskStatusMockDto = new UpdateConsumerTaskStatusMockDto() { TaskCode = "", TaskCompleteTs = DateTime.UtcNow.AddMonths(-1), TaskStatus = "COMPLETED" };
            var result = await _consumerTaskService.UpdateConsumerTask(updateConsumerTaskStatusMockDto);
            Assert.Equal(0, result.ConsumerTaskId);
        }
        [Fact]
        public async void Should_ConsumerTask_Update_Should_Return_Error_When_Task_Complete_Date_Is_Invalid_Quarter()
        {
            var updateConsumerTaskStatusMockDto = new UpdateConsumerTaskStatusMockDto() { TaskCode = "", TaskCompleteTs = DateTime.UtcNow.AddMonths(-1), TaskStatus = "COMPLETED" };

            var taskRewardMockModel = new TaskRewardMockModel { RecurrenceDefinitionJson = "{\r\n  \"recurrenceType\": \"PERIODIC\",\r\n  \"periodic\": {\r\n    \"period\": \"QUARTER\",\r\n    \"periodRestartDate\": 1\r\n  }\r\n}" };
            _taskRewardRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false)).ReturnsAsync(taskRewardMockModel);
            var result = await _consumerTaskService.UpdateConsumerTask(updateConsumerTaskStatusMockDto);
            Assert.Equal(0, result.ConsumerTaskId);
        }
        [Fact]
        public async void Should_ConsumerTask_Update_Should_Return_Error_When_Task_Complete_Date_Is_Invalid_Schedule()
        {
            var updateConsumerTaskStatusMockDto = new UpdateConsumerTaskStatusMockDto() { TaskCode = "", TaskCompleteTs = DateTime.UtcNow.AddMonths(-1), TaskStatus = "COMPLETED" };

            var taskRewardMockModel = new TaskRewardMockModel { RecurrenceDefinitionJson = "{\"schedules\":[{\"startDate\":\"01-01\",\"expiryDate\":\"12-31\"}],\"recurrenceType\":\"SCHEDULE\"}" };
            _taskRewardRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false)).ReturnsAsync(taskRewardMockModel);
            var result = await _consumerTaskService.UpdateConsumerTask(updateConsumerTaskStatusMockDto);
            Assert.Equal(0, result.ConsumerTaskId);
        }
        [Fact]
        public async void Should_ConsumerTask_Update_Should_Return_Error_When_Task_Complete_Date_Is_Invalid_RecurrentDefNull()
        {
            var updateConsumerTaskStatusMockDto = new UpdateConsumerTaskStatusMockDto() { TaskCode = "", TaskCompleteTs = DateTime.UtcNow.AddMonths(-1), TaskStatus = "COMPLETED" };

            var taskRewardMockModel = new TaskRewardMockModel { RecurrenceDefinitionJson = string.Empty };
            _taskRewardRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false)).ReturnsAsync(taskRewardMockModel);
            var result = await _consumerTaskService.UpdateConsumerTask(updateConsumerTaskStatusMockDto);
            Assert.Equal(0, result.ConsumerTaskId);
        }

        [Fact]
        public async void Should_ConsumerTask_Update_Should_Return_Error_Response_When_Task_Complete_Date_Future_Date()
        {
            var updateConsumerTaskStatusMockDto = new UpdateConsumerTaskStatusMockDto() { TaskCode = "", TaskCompleteTs = DateTime.UtcNow.AddDays(2), TaskStatus = "COMPLETED" };
            var result = await _consumerTaskService.UpdateConsumerTask(updateConsumerTaskStatusMockDto);
            Assert.Equal(0, result.ConsumerTaskId);
        }

        [Fact]
        public async void Should_ConsumerTask_Update_Should_Return_Error_Response_When_Task_Complete_Date_Invalid_And_For_Recurring_Task()
        {
            var updateConsumerTaskStatusMockDto = new UpdateConsumerTaskStatusMockDto() { TaskCode = "", TaskCompleteTs = DateTime.UtcNow.AddMonths(-2), TaskStatus = "COMPLETED" };
            _taskRewardRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false)).ReturnsAsync(new TaskRewardMockModel()
            {
                IsRecurring = true
            });
            var result = await _consumerTaskService.UpdateConsumerTask(updateConsumerTaskStatusMockDto);
            Assert.Equal(0, result.ConsumerTaskId);
        }

        [Fact]
        public async System.Threading.Tasks.Task Should_CreateConsumerSubtask()
        {
            var consumerTaskMockDto = new UpdateConsumerTaskMockDto();
            await _subTaskService.CreateConsumerSubtask(consumerTaskMockDto);
        }

        [Fact]
        public async System.Threading.Tasks.Task Should_Create_ConsumerSubtask_Null()
        {
            var consumerTaskMockDto = new UpdateConsumerTaskMockDto();
            _subTaskRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<SubTaskModel, bool>>>(), false)).ReturnsAsync((SubTaskMockModel)null);
            await _subTaskService.CreateConsumerSubtask(consumerTaskMockDto);
        }

        [Fact]
        public async System.Threading.Tasks.Task Should_Create_ConsumerSubtask_Catch_Exception()
        {
            var consumerTaskMockDto = new UpdateConsumerTaskMockDto();
            _taskRewardRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false)).ThrowsAsync(new Exception("Simulated exception"));
            await Assert.ThrowsAsync<Exception>(async () => await _subTaskService.CreateConsumerSubtask(consumerTaskMockDto));
        }

        [Fact]
        public async void ConsumerTaskUpdate_Should_Return_Catch_Exception_In_Controller()
        {
            var loggerMock = new Mock<ILogger<ConsumerTaskController>>();
            var serviceMock = new Mock<IConsumerTaskService>();
            var controller = new ConsumerTaskController(loggerMock.Object, serviceMock.Object, _subTaskService);
            var consumerTaskMockDto = new UpdateConsumerTaskMockDto();
            var expectedException = new Exception("Test exception");
            serviceMock.Setup(x => x.UpdateConsumerTask(consumerTaskMockDto))
           .ThrowsAsync(expectedException);
            var response = await controller.UpdateConsumerTask(consumerTaskMockDto);
            Assert.True(response?.Value?.ConsumerTaskId <= 0);
        }

        [Fact]
        public async void ConsumerTaskUpdate_Should_Return_Exception_Catch_In_Service()
        {
            var consumerTaskMockDto = new UpdateConsumerTaskMockDto();
            _consumerTaskRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ConsumerTaskModel, bool>>>(), false))
              .ThrowsAsync(new Exception("Simulated exception"));
            var response = await _consumerTaskController.UpdateConsumerTask(consumerTaskMockDto);
            Assert.True(response?.Value?.ConsumerTaskId <= 0);
        }

        [Fact]
        public async void UpdateConsumerTask_Should_Handle_If_Condition()
        {
            var consumerTaskDto = new UpdateConsumerTaskMockDto();
            consumerTaskDto.TaskId = 0;
            ConsumerTaskService service = ConsumerTaskUpdateException();
            var result = await service.UpdateConsumerTask(consumerTaskDto);
            Assert.NotNull(result);
        }

        [Fact]
        public async void UpdateConsumerTask_Should_Handle_TaskId()
        {
            var consumerTaskDto = new UpdateConsumerTaskDto();
            consumerTaskDto.TaskId = 0;
            ConsumerTaskService service = TaskIdExceptionHandle();
            var result = await service.UpdateConsumerTask(consumerTaskDto);
            Assert.NotNull(result);
        }

        [Fact]
        public async void UpdateConsumerTask_Should_Handle_Exception()
        {
            var consumerTaskDto = new UpdateConsumerTaskMockDto();
            ConsumerTaskService service = ConsumerTaskUpdateServiceException();
            var result = await service.UpdateConsumerTask(consumerTaskDto);
            Assert.NotNull(result);
        }

        //#5
        [Fact]
        public async void Should_GetAll_Consumer_Task()
        {
            var consumerTaskRequestDto = new ConsumerTaskRequestMockDto();
            var response = await _consumerTaskController.GetAllConsumerTask(consumerTaskRequestDto);
            var result = response?.Result as OkObjectResult;
            Assert.NotNull(result?.Value);
            Assert.Equal(200, result.StatusCode);
        }
        [Fact]
        public async void Should_GetAll_Consumer_Task_Reurns_Success()
        {
            // Arrange
            var consumerTaskRepoMock = new Mock<IConsumerTaskRepo>();
            var consumerTaskRequestDto = new ConsumerTaskRequestMockDto();

            _taskRewardRepository
                .Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false))
                .ReturnsAsync(new TaskRewardExternalCodeMockModel());

            var consumerTasks = new List<ConsumerTaskModel>
            {
                new ConsumerTaskModel { TaskStatus = "IN_PROGRESS",  },
                new ConsumerTaskModel { TaskStatus = "COMPLETED", },

            };

            consumerTaskRepoMock
                .Setup(x => x.FindAsync(It.IsAny<Expression<Func<ConsumerTaskModel, bool>>>(), false))
                .ReturnsAsync(consumerTasks);
            var response = await _consumerTaskController.GetAllConsumerTask(consumerTaskRequestDto);
            var result = response?.Result as OkObjectResult;
            Assert.True(result?.Value != null);
            Assert.True(result.StatusCode == 200);
        }

        [Fact]
        public async void GetAllConsumerTask_Should_Return_Catch_Exception_In_Controller()
        {
            var loggerMock = new Mock<ILogger<ConsumerTaskController>>();
            var serviceMock = new Mock<IConsumerTaskService>();
            var controller = new ConsumerTaskController(loggerMock.Object, serviceMock.Object, _subTaskService);
            var consumerTaskRequestDto = new ConsumerTaskRequestMockDto();
            var expectedException = new Exception("Test exception");
            serviceMock.Setup(x => x.GetAllConsumerTask(consumerTaskRequestDto))
           .ThrowsAsync(expectedException);
            var response = await controller.GetAllConsumerTask(consumerTaskRequestDto);
            Assert.True(response?.Value?.AvailableTasks == null);

        }

        [Fact]
        public async void GetAllConsumerTask_Should_Return_Catch_Exception_In_Service()
        {
            var consumerTaskRequestDto = new ConsumerTaskRequestMockDto();
            _taskRewardRepository.Setup(x => x.FindAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false))
              .ThrowsAsync(new Exception("Simulated exception"));
            var response = await _consumerTaskController.GetAllConsumerTask(consumerTaskRequestDto);
            Assert.True(response?.Value?.AvailableTasks == null);
        }

        [Fact]
        public async void GetAllConsumerTask_Exception_In_Service()
        {
            ConsumerTaskService service = ConsumerTaskException();
            var consumerTaskRequestDto = new ConsumerTaskRequestDto
            {
                ConsumerCode = "cmr-c69905fc68ce4f36851f877bae38f22e",
                TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4"
            };
            var result = await service.GetAllConsumerTask(consumerTaskRequestDto);
            Assert.NotNull(result);
        }

        [Fact]
        public async void GetAllConsumerTask_Should_return_data()
        {
            var consumerTaskRequestDto = new ConsumerTaskRequestDto
            {
                ConsumerCode = "cmr-c69905fc68ce4f36851f877bae38f22e",
                TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4"
            };
            var result = await _consumerTaskService.GetAllConsumerTask(consumerTaskRequestDto);
            Assert.NotNull(result);
        }
        [Fact]
        public async void GetAllConsumerTask_Should_return_data_Periodic()
        {
            var consumerTaskRequestDto = new ConsumerTaskRequestDto
            {
                ConsumerCode = "cmr-c69905fc68ce4f36851f877bae38f22e",
                TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4"
            };
            var taskRewardMockModel = new TaskRewardMockModel { RecurrenceDefinitionJson = "{\r\n  \"recurrenceType\": \"PERIODIC\",\r\n  \"periodic\": {\r\n    \"period\": \"MONTH\",\r\n    \"periodRestartDate\": 0\r\n  }\r\n}" };
            var result = await _consumerTaskService.GetAllConsumerTask(consumerTaskRequestDto);
            Assert.NotNull(result);
        }
        [Fact]
        public async void GetAllConsumerTask_Should_Return_Empty_Tasks()
        {
            var consumerTaskRequestDto = new ConsumerTaskRequestDto
            {
                ConsumerCode = "cmr-c69905fc68ce4f36851f877bae38f22e",
                TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4"
            };

            _taskRewardRepository.Setup(x => x.FindAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false))
                .ReturnsAsync(new List<TaskRewardModel>());

            // Act
            var result = await _consumerTaskController.GetAllConsumerTask(consumerTaskRequestDto);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async void GetConsumerSubtask_okResponse()
        {
            var getConsumerSubtasksRequestMockDto = new GetConsumerSubtasksRequestMockDto();
            var subTaskService = new Mock<ISubtaskService>();
            subTaskService.Setup(x => x.GetConsumerSubtask(It.IsAny<GetConsumerSubtasksRequestDto>()))
                         .ReturnsAsync(new GetConsumerSubTaskResponseDto { ConsumerTaskDto = new ConsumerTaskMockDto[1] });
            var consumerTaskController = new ConsumerTaskController(_consumerTaskLogger.Object, _consumerTaskService, subTaskService.Object);
            var response = await consumerTaskController.GetConsumerSubtask(getConsumerSubtasksRequestMockDto);
            var result = response?.Result as OkObjectResult;
            Assert.NotNull(result);
        }

        [Fact]
        public async void CatchException_GetConsumerSubtask()
        {
            var getConsumerSubtasksRequestMockDto = new GetConsumerSubtasksRequestMockDto();
            var subTaskService = new Mock<ISubtaskService>();
            subTaskService.Setup(x => x.GetConsumerSubtask(It.IsAny<GetConsumerSubtasksRequestDto>()))
                         .ThrowsAsync(new Exception("inner Exception"));
            var consumerTaskController = new ConsumerTaskController(_consumerTaskLogger.Object, _consumerTaskService, subTaskService.Object);
            var response = await consumerTaskController.GetConsumerSubtask(getConsumerSubtasksRequestMockDto);
            Assert.NotNull(response);
        }

        [Fact]
        public async void GetConsumerSubtask_okResponse_Service()
        {
            var getConsumerSubtasksRequestMockDto = new GetConsumerSubtasksRequestMockDto();
            var response = await _subTaskService.GetConsumerSubtask(getConsumerSubtasksRequestMockDto);
            Assert.NotNull(response);
        }

        [Fact]
        public async void GetConsumerSubtask_CountCheck_Service()
        {
            var getConsumerSubtasksRequestMockDto = new GetConsumerSubtasksRequestMockDto();
            var consumerTaskRepo = new Mock<IConsumerTaskRepo>();
            consumerTaskRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<ConsumerTaskModel, bool>>>(), false))
                .ReturnsAsync(new List<ConsumerTaskModel>() { new ConsumerTaskMockModel { ConsumerCode = "" } });
            var response = await _subTaskService.GetConsumerSubtask(getConsumerSubtasksRequestMockDto);
            Assert.NotNull(response);
        }

        [Fact]
        public async void CatchException_GetConsumerSubtask_Service()
        {
            var getConsumerSubtasksRequestMockDto = new GetConsumerSubtasksRequestMockDto();
            var subTaskService = new Mock<ISubtaskService>();
            _consumerTaskRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<ConsumerTaskModel, bool>>>(), false))
                    .ThrowsAsync(new Exception("inner Exception"));
            await Assert.ThrowsAsync<Exception>(async () => await _subTaskService.GetConsumerSubtask(getConsumerSubtasksRequestMockDto));
        }

        [Fact]
        public async void CompleteSubtask_okResponse()
        {
            var updateSubtaskRequestMockDto = new UpdateSubtaskRequestMockDto();
            var subTaskService = new Mock<ISubtaskService>();
            subTaskService.Setup(x => x.UpdateConsumerSubtask(It.IsAny<UpdateSubtaskRequestDto>()))
                         .ReturnsAsync(new UpdateSubtaskResponseMockDto());
            var consumerTaskController = new ConsumerTaskController(_consumerTaskLogger.Object, _consumerTaskService, subTaskService.Object);
            var response = await consumerTaskController.CompleteSubtask(updateSubtaskRequestMockDto);
            var result = response?.Result as OkObjectResult;
            Assert.NotNull(result);
        }

        [Fact]
        public async void Catch_Exception_CompleteSubtask()
        {
            var updateSubtaskRequestMockDto = new UpdateSubtaskRequestMockDto();
            var subTaskService = new Mock<ISubtaskService>();
            subTaskService.Setup(x => x.UpdateConsumerSubtask(It.IsAny<UpdateSubtaskRequestDto>()))
                     .ThrowsAsync(new Exception("inner Exception"));
            var consumerTaskController = new ConsumerTaskController(_consumerTaskLogger.Object, _consumerTaskService, subTaskService.Object);
            var response = await consumerTaskController.CompleteSubtask(updateSubtaskRequestMockDto);
            Assert.NotNull(response);
        }

        [Fact]
        public async void UpdateConsumerSubtask_okResponse()
        {
            var updateSubtaskRequestMockDto = new UpdateSubtaskRequestMockDto();
            var response = await _subTaskService.UpdateConsumerSubtask(updateSubtaskRequestMockDto);
            Assert.NotNull(response);
        }

        [Fact]
        public async void UpdateConsumerSubtask_NullCheck_Service()
        {
            var updateSubtaskRequestMockDto = new UpdateSubtaskRequestMockDto();
            _consumerTaskRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ConsumerTaskModel, bool>>>(), false)).ReturnsAsync(new ConsumerTaskIdZeroMockModel());
            _taskRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskModel, bool>>>(), false)).ReturnsAsync(new TaskMockModel() { IsSubtask = false });
            var response = await _subTaskService.UpdateConsumerSubtask(updateSubtaskRequestMockDto);
            Assert.NotNull(response.AdditionalAmount == 0);
        }

        [Fact]
        public async void UpdateConsumerSubtask_parentTaskRewardNotNullCheck_Service()
        {
            var updateSubtaskRequestMockDto = new UpdateSubtaskRequestMockDto();
            _consumerTaskRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ConsumerTaskModel, bool>>>(), false)).ReturnsAsync(new ConsumerTaskIdZeroMockModel() { ProgressDetail = @"{""spinwheelProgress"": {""finalSlotIndex"": 2, ""spinwheelConfig"": {""probability"": 1,
""itemDefinition"": [{""itemText"": ""2"", ""lowProbability"": ""0.0"", ""highProbability"": ""0.25""}, {""itemText"": ""2"", ""lowProbability"": ""0.25"", ""highProbability"": ""0.50""}, 
{""itemText"": ""2"", ""lowProbability"": ""0.50"", ""highProbability"": ""0.72""}, {""itemText"": ""2"", ""lowProbability"": ""0.72"", ""highProbability"": ""0.8""},
{""itemText"": ""2"", ""lowProbability"": ""0.8"", ""highProbability"": ""0.9""}, {""itemText"": ""5"", ""lowProbability"": ""0.9"", ""highProbability"": ""1.0""}], ""itemTextSuffix"": ""x""}}}" });
            _taskRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskModel, bool>>>(), false)).ReturnsAsync(new TaskMockModel() { IsSubtask = true });
            _taskTypeRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskTypeModel, bool>>>(), false)).ReturnsAsync(new TaskTypeMockModel() { TaskTypeName = "SPINWHEEL_SUBTASK", });
            _taskRewardRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false)).ReturnsAsync(new TaskRewardMockModel());
            var response = await _subTaskService.UpdateConsumerSubtask(updateSubtaskRequestMockDto);
            Assert.NotNull(response.AdditionalAmount == 0);
        }

        [Fact]
        public async void UpdateConsumerSubtask_Exception_Service()
        {
            var updateSubtaskRequestMockDto = new UpdateSubtaskRequestMockDto();
            _consumerTaskRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ConsumerTaskModel, bool>>>(), false)).ReturnsAsync(new ConsumerTaskIdZeroMockModel() { TaskStatus = "IN_PROGRESS" });
            _taskRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskModel, bool>>>(), false)).ReturnsAsync(new TaskMockModel());
            _taskTypeRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskTypeModel, bool>>>(), false))
                   .ThrowsAsync(new Exception("inner Exception"));
            await Assert.ThrowsAsync<Exception>(async () => await _subTaskService.UpdateConsumerSubtask(updateSubtaskRequestMockDto));
        }

        [Fact]
        public async System.Threading.Tasks.Task RevertAllConsumerTasks_Should_Return_Bad_Request_Result()
        {
            var requestDto = new RevertAllConsumerTasksRequestMockDto
            {
                ConsumerCode = string.Empty
            };
            var result = await _consumerTaskController.RevertAllConsumerTasks(requestDto);
            Assert.IsType<BadRequestObjectResult>(result.Result);
            var badRequestObjectResult = result.Result as BadRequestObjectResult;
            Assert.Equal(Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, badRequestObjectResult?.StatusCode);
        }

        [Fact]
        public async System.Threading.Tasks.Task RevertAllConsumerTasks_Should_Return_Ok_Result()
        {
            var requestDto = new RevertAllConsumerTasksRequestMockDto();
            var transactionMock = new Mock<ITransaction>();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);
            var result = await _consumerTaskController.RevertAllConsumerTasks(requestDto);
            Assert.IsType<OkObjectResult>(result.Result);
            var okObjectResult = result.Result as OkObjectResult;
            Assert.Equal(Microsoft.AspNetCore.Http.StatusCodes.Status200OK, okObjectResult?.StatusCode);
        }

        [Fact]
        public async System.Threading.Tasks.Task RevertAllConsumerTasks_Should_Return_Internal_Server_Error_When_Exception_Occurred()
        {
            var requestDto = new RevertAllConsumerTasksRequestMockDto();
            var transactionMock = new Mock<ITransaction>();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);
            _consumerTaskRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<ConsumerTaskModel, bool>>>(), false)).ThrowsAsync(new Exception("Test Exception"));
            var result = await _consumerTaskController.RevertAllConsumerTasks(requestDto);
            Assert.IsType<ObjectResult>(result.Result);
            var objectResult = result.Result as ObjectResult;
            Assert.Equal(Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError, objectResult?.StatusCode);
        }

        #region PrivateMethod
        private static void CreateConsumerTask(out Mock<ISession> mockSession, out Mock<IMapper> mockMapper, out Mock<IConsumerTaskRepo> mockConsumerTaskRepo, out ConsumerTaskService service)
        {
            var consumerTaskDto = new ConsumerTaskDto();
            mockSession = new Mock<ISession>();
            mockMapper = new Mock<IMapper>();
            mockConsumerTaskRepo = new Mock<IConsumerTaskRepo>();
            var mockTaskRepo = new Mock<ITaskRepo>();
            var mockTaskRewardRepo = new Mock<ITaskRewardRepo>();
            var mockTaskDetailRepo = new Mock<ITaskDetailRepo>();
            var mockTermsOfServiceRepo = new Mock<ITermsOfServiceRepo>();
            var mockTaskRewardService = new Mock<ITaskRewardService>();
            var mockLogger = new Mock<ILogger<ConsumerTaskService>>();
            var mockTaskRewardLogger = new Mock<ILogger<TaskRewardService>>();
            var taskTypeRepo = new Mock<ITaskTypeRepo>();
            var subTaskService = new Mock<ISubtaskService>();
            var taskRewardTypeRepo = new Mock<ITaskRewardTypeRepo>();
            var mockTenantTaskCategory = new Mock<ITenantTaskCategoryRepo>();
            var taskRewardCollectionRepo = new Mock<ITaskRewardCollectionRepo>();
            var fileHelper = new Mock<IFileHelper>();
            var config = new Mock<IConfiguration>();
            var vault = new Mock<IVault>();
            var taskCommonHelper = new Mock<ITaskCommonHelper>();
            var heliosEventPublisher = new Mock<IHeliosEventPublisher<ConsumerTaskEventDto>>();
            var adventureRepo = new Mock<IAdventureRepo>();


            var commonTaskRewardService = new Mock<ICommonTaskRewardService>();
            mockSession.Setup(x => x.BeginTransaction()).Returns(Mock.Of<ITransaction>());
            mockMapper.Setup(x => x.Map<ConsumerTaskModel>(It.IsAny<ConsumerTaskDto>())).Returns(new ConsumerTaskModel());
            mockConsumerTaskRepo.Setup(x => x.CreateAsync(It.IsAny<ConsumerTaskModel>())).ThrowsAsync(new Exception("Simulated exception"));

            var taskRewardService = new TaskRewardService(mockTaskRewardLogger.Object, mockMapper.Object, mockTaskRewardRepo.Object,
                mockTaskRepo.Object,
                mockTaskDetailRepo.Object,
                mockTermsOfServiceRepo.Object,
                mockConsumerTaskRepo.Object,
                mockTenantTaskCategory.Object,
                taskTypeRepo.Object,
                taskRewardTypeRepo.Object,
                taskRewardCollectionRepo.Object,
                commonTaskRewardService.Object,
                adventureRepo.Object
                );
            service = new ConsumerTaskService(
                mockLogger.Object,
                mockMapper.Object,
                mockSession.Object,
                mockConsumerTaskRepo.Object,
                mockTaskRepo.Object,
                mockTaskDetailRepo.Object,
                mockTaskRewardRepo.Object,
                mockTermsOfServiceRepo.Object,
                taskRewardService,
                mockTenantTaskCategory.Object,
                taskTypeRepo.Object,
                subTaskService.Object, taskRewardTypeRepo.Object, fileHelper.Object,
                config.Object,
                vault.Object, taskCommonHelper.Object,
                commonTaskRewardService.Object, heliosEventPublisher.Object

            );
        }

        private static void UpdateConsumerTask(out Mock<ISession> mockSession, out Mock<IConsumerTaskRepo> mockConsumerTaskRepo, out ConsumerTaskService service)
        {
            mockSession = new Mock<ISession>();
            var mockMapper = new Mock<IMapper>();
            mockConsumerTaskRepo = new Mock<IConsumerTaskRepo>();
            var mockTaskRepo = new Mock<ITaskRepo>();
            var mockTaskRewardRepo = new Mock<ITaskRewardRepo>();
            var mockTaskDetailRepo = new Mock<ITaskDetailRepo>();
            var mockTermsOfServiceRepo = new Mock<ITermsOfServiceRepo>();
            var mockTaskRewardService = new Mock<ITaskRewardService>();
            var mockLogger = new Mock<ILogger<ConsumerTaskService>>();
            var taskRewardCollectionRepo = new Mock<ITaskRewardCollectionRepo>();
            var mockTaskRewardLogger = new Mock<ILogger<TaskRewardService>>();
            var mockTenantTaskCategory = new Mock<ITenantTaskCategoryRepo>();
            var taskTypeRepo = new Mock<ITaskTypeRepo>();
            var subTaskService = new Mock<ISubtaskService>();
            var taskRewardTypeRepo = new Mock<ITaskRewardTypeRepo>();
            var fileHelper = new Mock<IFileHelper>();
            var config = new Mock<IConfiguration>();
            var vault = new Mock<IVault>();
            var taskCommonHelper = new Mock<ITaskCommonHelper>();
            var commonTaskRewardService = new Mock<ICommonTaskRewardService>();
            var heliosEventPublisher = new Mock<IHeliosEventPublisher<ConsumerTaskEventDto>>();
            var adventureRepo = new Mock<IAdventureRepo>();

            var taskRewardService = new TaskRewardService(mockTaskRewardLogger.Object, mockMapper.Object, mockTaskRewardRepo.Object,
                    mockTaskRepo.Object,
                    mockTaskDetailRepo.Object,
                    mockTermsOfServiceRepo.Object,
                    mockConsumerTaskRepo.Object,
                    mockTenantTaskCategory.Object,
                    taskTypeRepo.Object,
                    taskRewardTypeRepo.Object,
                    taskRewardCollectionRepo.Object,
                    commonTaskRewardService.Object,
                    adventureRepo.Object
                );

            service = new ConsumerTaskService(
                mockLogger.Object,
                mockMapper.Object,
                mockSession.Object,
                mockConsumerTaskRepo.Object,
                mockTaskRepo.Object,
                mockTaskDetailRepo.Object,
                mockTaskRewardRepo.Object,
                mockTermsOfServiceRepo.Object,
                taskRewardService,
                mockTenantTaskCategory.Object,
                taskTypeRepo.Object,
                subTaskService.Object,
                taskRewardTypeRepo.Object,
                fileHelper.Object,
                config.Object,
                vault.Object, taskCommonHelper.Object,
                commonTaskRewardService.Object, heliosEventPublisher.Object

            );
            mockSession.Setup(x => x.BeginTransaction()).Returns(Mock.Of<ITransaction>());
            mockMapper.Setup(x => x.Map<ConsumerTaskModel>(It.IsAny<ConsumerTaskDto>())).Returns(new ConsumerTaskModel());

            mockConsumerTaskRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ConsumerTaskModel, bool>>>(), false)).ReturnsAsync(new ConsumerTaskModel());

            mockConsumerTaskRepo.Setup(x => x.UpdateAsync(It.IsAny<ConsumerTaskModel>())).ThrowsAsync(new Exception("Simulated exception"));
        }

        private static ConsumerTaskService ConsumerTaskException()
        {
            var mockSession = new Mock<ISession>();
            var mockMapper = new Mock<IMapper>();
            var mockConsumerTaskRepo = new Mock<IConsumerTaskRepo>();
            var mockTaskRepo = new Mock<ITaskRepo>();
            var mockTaskRewardRepo = new Mock<ITaskRewardRepo>();
            var mockTaskDetailRepo = new Mock<ITaskDetailRepo>();
            var mockTermsOfServiceRepo = new Mock<ITermsOfServiceRepo>();
            var mockTaskRewardService = new Mock<ITaskRewardService>();
            var mockLogger = new Mock<ILogger<ConsumerTaskService>>();
            var mockTaskRewardLogger = new Mock<ILogger<TaskRewardService>>();
            var mockTenantTaskCategory = new Mock<ITenantTaskCategoryRepo>();
            var taskRewardCollectionRepo = new Mock<ITaskRewardCollectionRepo>();
            var taskTypeRepo = new Mock<ITaskTypeRepo>();
            var subTaskService = new Mock<ISubtaskService>();
            var taskRewardTypeRepo = new Mock<ITaskRewardTypeRepo>();
            var fileHelper = new Mock<IFileHelper>();
            var config = new Mock<IConfiguration>();
            var vault = new Mock<IVault>();
            var taskCommonHelper = new Mock<ITaskCommonHelper>();
            var commonTaskRewardService = new Mock<ICommonTaskRewardService>();
            var heliosEventPublisher = new Mock<IHeliosEventPublisher<ConsumerTaskEventDto>>();
            var adventureRepo = new Mock<IAdventureRepo>();

            var taskRewardService = new TaskRewardService(mockTaskRewardLogger.Object, mockMapper.Object, mockTaskRewardRepo.Object,
                mockTaskRepo.Object,
                mockTaskDetailRepo.Object,
                mockTermsOfServiceRepo.Object,
                mockConsumerTaskRepo.Object,
                mockTenantTaskCategory.Object,
                taskTypeRepo.Object,
                taskRewardTypeRepo.Object,
                taskRewardCollectionRepo.Object,
                commonTaskRewardService.Object,
                adventureRepo.Object
            );

            var service = new ConsumerTaskService(
                mockLogger.Object,
                mockMapper.Object,
                mockSession.Object,
                mockConsumerTaskRepo.Object,
                mockTaskRepo.Object,
                mockTaskDetailRepo.Object,
                mockTaskRewardRepo.Object,
                mockTermsOfServiceRepo.Object,
                taskRewardService,
                mockTenantTaskCategory.Object,
                taskTypeRepo.Object,
                subTaskService.Object,
                taskRewardTypeRepo.Object,
                fileHelper.Object,
                config.Object,
                vault.Object, taskCommonHelper.Object,
                commonTaskRewardService.Object, heliosEventPublisher.Object


            );

            mockTaskRewardService.Setup(x => x.GetTaskRewardDetails(It.IsAny<FindTaskRewardRequestDto>()))
                                 .ThrowsAsync(new Exception("Simulated exception"));
            return service;
        }

        private static ConsumerTaskService ConsumerTaskUpdateException()
        {
            var mockSession = new Mock<ISession>();
            var mockMapper = new Mock<IMapper>();
            var mockConsumerTaskRepo = new Mock<IConsumerTaskRepo>();
            var mockTaskRepo = new Mock<ITaskRepo>();
            var mockTaskRewardRepo = new Mock<ITaskRewardRepo>();
            var mockTaskDetailRepo = new Mock<ITaskDetailRepo>();
            var mockTermsOfServiceRepo = new Mock<ITermsOfServiceRepo>();
            var mockTaskRewardService = new Mock<ITaskRewardService>();
            var mockLogger = new Mock<ILogger<ConsumerTaskService>>();
            var mockTaskRewardLogger = new Mock<ILogger<TaskRewardService>>();
            var mockTenantTaskCategory = new Mock<ITenantTaskCategoryRepo>();
            var taskRewardCollectionRepo = new Mock<ITaskRewardCollectionRepo>();
            var taskTypeRepo = new Mock<ITaskTypeRepo>();
            var subTaskService = new Mock<ISubtaskService>();
            var taskRewardTypeRepo = new Mock<ITaskRewardTypeRepo>();
            var fileHelper = new Mock<IFileHelper>();
            var config = new Mock<IConfiguration>();
            var vault = new Mock<IVault>();
            var taskCommonHelper = new Mock<ITaskCommonHelper>();
            var commonTaskRewardService = new Mock<ICommonTaskRewardService>();
            var heliosEventPublisher = new Mock<IHeliosEventPublisher<ConsumerTaskEventDto>>();
            var adventureRepo = new Mock<IAdventureRepo>();
            var taskRewardService = new TaskRewardService(mockTaskRewardLogger.Object, mockMapper.Object, mockTaskRewardRepo.Object,
                mockTaskRepo.Object,
                mockTaskDetailRepo.Object,
                mockTermsOfServiceRepo.Object,
                mockConsumerTaskRepo.Object,
                mockTenantTaskCategory.Object,
                taskTypeRepo.Object,
                taskRewardTypeRepo.Object,
                taskRewardCollectionRepo.Object,
                commonTaskRewardService.Object,
                adventureRepo.Object
            );

            var service = new ConsumerTaskService(
                mockLogger.Object,
                mockMapper.Object,
                mockSession.Object,
                mockConsumerTaskRepo.Object,
                mockTaskRepo.Object,
                mockTaskDetailRepo.Object,
                mockTaskRewardRepo.Object,
                mockTermsOfServiceRepo.Object,
                taskRewardService,
                mockTenantTaskCategory.Object,
                taskTypeRepo.Object,
                subTaskService.Object,
                taskRewardTypeRepo.Object,
                fileHelper.Object,
                config.Object,
                vault.Object, taskCommonHelper.Object,
                commonTaskRewardService.Object, heliosEventPublisher.Object

            );

            mockConsumerTaskRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ConsumerTaskModel, bool>>>(), false))
                .ReturnsAsync(new ConsumerTaskModel());
            return service;
        }

        private static ConsumerTaskService TaskIdExceptionHandle()
        {
            var mockSession = new Mock<ISession>();
            var mockMapper = new Mock<IMapper>();
            var mockConsumerTaskRepo = new Mock<IConsumerTaskRepo>();
            var mockTaskRepo = new Mock<ITaskRepo>();
            var mockTaskRewardRepo = new Mock<ITaskRewardRepo>();
            var mockTaskDetailRepo = new Mock<ITaskDetailRepo>();
            var mockTermsOfServiceRepo = new Mock<ITermsOfServiceRepo>();
            var mockTaskRewardService = new Mock<ITaskRewardService>();
            var mockLogger = new Mock<ILogger<ConsumerTaskService>>();
            var mockTaskRewardLogger = new Mock<ILogger<TaskRewardService>>();
            var mockTenantTaskCategory = new Mock<ITenantTaskCategoryRepo>();
            var taskRewardCollectionRepo = new Mock<ITaskRewardCollectionRepo>();
            var taskTypeRepo = new Mock<ITaskTypeRepo>();
            var subTaskService = new Mock<ISubtaskService>();
            var taskRewardTypeRepo = new Mock<ITaskRewardTypeRepo>();
            var fileHelper = new Mock<IFileHelper>();
            var config = new Mock<IConfiguration>();
            var vault = new Mock<IVault>();
            var taskCommonHelper = new Mock<ITaskCommonHelper>();
            var commonTaskRewardService = new Mock<ICommonTaskRewardService>();
            var heliosEventPublisher = new Mock<IHeliosEventPublisher<ConsumerTaskEventDto>>();
            var adventureRepo = new Mock<IAdventureRepo>();

            var taskRewardService = new TaskRewardService(mockTaskRewardLogger.Object, mockMapper.Object, mockTaskRewardRepo.Object,
                mockTaskRepo.Object,
                mockTaskDetailRepo.Object,
                mockTermsOfServiceRepo.Object,
                mockConsumerTaskRepo.Object,
                mockTenantTaskCategory.Object,
                taskTypeRepo.Object,
                taskRewardTypeRepo.Object,
                taskRewardCollectionRepo.Object,
                commonTaskRewardService.Object,
                adventureRepo.Object
            );

            var service = new ConsumerTaskService(
                mockLogger.Object,
                mockMapper.Object,
                mockSession.Object,
                mockConsumerTaskRepo.Object,
                mockTaskRepo.Object,
                mockTaskDetailRepo.Object,
                mockTaskRewardRepo.Object,
                mockTermsOfServiceRepo.Object,
                taskRewardService,
                mockTenantTaskCategory.Object,
               taskTypeRepo.Object,
                subTaskService.Object, taskRewardTypeRepo.Object, fileHelper.Object, config.Object, vault.Object, taskCommonHelper.Object, commonTaskRewardService.Object
                , heliosEventPublisher.Object);

            mockConsumerTaskRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ConsumerTaskModel, bool>>>(), false))
                .ReturnsAsync(new ConsumerTaskModel());
            return service;
        }

        private static ConsumerTaskService ConsumerTaskUpdateServiceException()
        {
            var mockSession = new Mock<ISession>();
            var mockMapper = new Mock<IMapper>();
            var mockConsumerTaskRepo = new Mock<IConsumerTaskRepo>();
            var mockTaskRepo = new Mock<ITaskRepo>();
            var mockTaskRewardRepo = new Mock<ITaskRewardRepo>();
            var mockTaskDetailRepo = new Mock<ITaskDetailRepo>();
            var mockTermsOfServiceRepo = new Mock<ITermsOfServiceRepo>();
            var mockTaskRewardService = new Mock<ITaskRewardService>();
            var mockLogger = new Mock<ILogger<ConsumerTaskService>>();
            var mockTaskRewardLogger = new Mock<ILogger<TaskRewardService>>();
            var mockTenantTaskCategory = new Mock<ITenantTaskCategoryRepo>();
            var taskTypeRepo = new Mock<ITaskTypeRepo>();
            var subTaskService = new Mock<ISubtaskService>();
            var taskRewardTypeRepo = new Mock<ITaskRewardTypeRepo>();
            var taskRewardCollectionRepo = new Mock<ITaskRewardCollectionRepo>();
            var fileHelper = new Mock<IFileHelper>();
            var config = new Mock<IConfiguration>();
            var vault = new Mock<IVault>();
            var taskCommonHelper = new Mock<ITaskCommonHelper>();
            var commonTaskRewardService = new Mock<ICommonTaskRewardService>();
            var heliosEventPublisher = new Mock<IHeliosEventPublisher<ConsumerTaskEventDto>>();
            var adventureRepo = new Mock<IAdventureRepo>();
            var taskRewardService = new TaskRewardService(mockTaskRewardLogger.Object, mockMapper.Object, mockTaskRewardRepo.Object,
                mockTaskRepo.Object,
                mockTaskDetailRepo.Object,
                mockTermsOfServiceRepo.Object,
                mockConsumerTaskRepo.Object,
                mockTenantTaskCategory.Object,
               taskTypeRepo.Object, taskRewardTypeRepo.Object,
               taskRewardCollectionRepo.Object,
               commonTaskRewardService.Object,
               adventureRepo.Object
            );

            var service = new ConsumerTaskService(
                mockLogger.Object,
                mockMapper.Object,
                mockSession.Object,
                mockConsumerTaskRepo.Object,
                mockTaskRepo.Object,
                mockTaskDetailRepo.Object,
                mockTaskRewardRepo.Object,
                mockTermsOfServiceRepo.Object,
                taskRewardService,
                mockTenantTaskCategory.Object,
                taskTypeRepo.Object,
                subTaskService.Object, taskRewardTypeRepo.Object, fileHelper.Object, config.Object, vault.Object, taskCommonHelper.Object,
                commonTaskRewardService.Object, heliosEventPublisher.Object

            );
            mockConsumerTaskRepo.Setup(x => x.CreateAsync(It.IsAny<ConsumerTaskModel>())).ThrowsAsync(new Exception("Simulated exception"));
            return service;
        }

        #endregion

        [Fact]
        public async void Should_Return_OkResponse_GetAvailableTaskRewardType_Controller()
        {
            var getRewardTypeConsumerTaskRequestMockDto = new GetRewardTypeConsumerTaskRequestMockDto();
            var response = await _consumerTaskController.GetAvailableTaskRewardType(getRewardTypeConsumerTaskRequestMockDto);
            var result = response.Result as OkObjectResult;
            Assert.True(result?.Value != null);
            Assert.True(result.StatusCode == 200);
        }

        [Fact]
        public async void Should_GetAvailableTaskRewardType_Controller_Exception()
        {
            var serviceMock = new Mock<IConsumerTaskService>();
            var controller = new ConsumerTaskController(_consumerTaskLogger.Object, serviceMock.Object, _subTaskService);
            var getRewardTypeConsumerTaskRequestMockDto = new GetRewardTypeConsumerTaskRequestMockDto();
            serviceMock.Setup(x => x.GetAvailableTaskRewardType(getRewardTypeConsumerTaskRequestMockDto))
           .ThrowsAsync(new Exception("Test exception"));
            await Assert.ThrowsAsync<Exception>(async () => await controller.GetAvailableTaskRewardType(getRewardTypeConsumerTaskRequestMockDto));
        }

        [Fact]
        public async void Should_Return_OkResponse_GetAvailableTaskRewardType_Service()
        {
            var getRewardTypeConsumerTaskRequestMockDto = new GetRewardTypeConsumerTaskRequestMockDto();
            _taskRewardTypeRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskRewardTypeModel, bool>>>(), false))
                .ReturnsAsync(new TaskRewardTypeMockModel());
            var response = await _consumerTaskService.GetAvailableTaskRewardType(getRewardTypeConsumerTaskRequestMockDto);
            Assert.True(response != null);
        }

        [Fact]
        public async void Should_Catch_Exception_GetAvailableTaskRewardType_Service()
        {
            var getRewardTypeConsumerTaskRequestMockDto = new GetRewardTypeConsumerTaskRequestMockDto();
            _taskRewardTypeRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskRewardTypeModel, bool>>>(), false))
                .ThrowsAsync(new Exception("Simulated exception"));
            await Assert.ThrowsAsync<Exception>(async () => await _consumerTaskService.
            GetAvailableTaskRewardType(getRewardTypeConsumerTaskRequestMockDto));
        }

        [Fact]
        public void Should_return_ConsumerTaskMap()
        {
            Configuration _configuration;
            _configuration = Fluently.Configure()
                    .Database(SQLiteConfiguration.Standard.InMemory().ShowSql())
                    .Mappings(m => m.FluentMappings.Add<ConsumerTaskMap>())
                    .BuildConfiguration();
        }

        [Fact]
        public void ConsumerTaskRepo_Constructor_ShouldInstantiate_WhenDependenciesAreProvided()
        {
            var mockLogger = new Mock<ILogger<BaseRepo<ConsumerTaskModel>>>();
            var mockSession = new Mock<ISession>();
            var repo = new ConsumerTaskRepo(mockLogger.Object, mockSession.Object);
            Assert.NotNull(repo);
        }

        [Fact]
        public void SubTaskRepo_Constructor_ShouldInstantiate_WhenDependenciesAreProvided()
        {
            var mockLogger = new Mock<ILogger<BaseRepo<SubTaskModel>>>();
            var mockSession = new Mock<ISession>();
            var repo = new SubTaskRepo(mockLogger.Object, mockSession.Object);
            Assert.NotNull(repo);
        }

        [Fact]
        public async void Should_Create_Subtask()
        {
            var SubtaskRequestDto = new SubtaskRequestDto
            {
                ParentTaskRewardCode = "xyz",
                ChildTaskRewardCode = "abc",
                Subtask = new PostSubTaskDto { CreateUser = "test" },
            };
            var mapper = new Mock<IMapper>();
            _taskRewardRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false)).ReturnsAsync(new TaskRewardMockModel());
            _subTaskRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<SubTaskModel, bool>>>(), false));

            _subTaskRepo.Setup(x => x.CreateAsync(It.IsAny<SubTaskModel>())).ReturnsAsync(new SubTaskModel { SubTaskId = 1 });
            var serviceResponse = await _subTaskService.CreateSubTask(SubtaskRequestDto);
            var response = await _consumerTaskController.CreateSubtask(SubtaskRequestDto);
            var result = response as OkObjectResult;
            Assert.True(result?.StatusCode == 200);
        }
        [Fact]
        public async void Should_Return_404_Create_Subtask()
        {
            var SubtaskRequestDto = new SubtaskRequestDto
            {
                ParentTaskRewardCode = "xyz",
                ChildTaskRewardCode = "abc",
                Subtask = new PostSubTaskDto { CreateUser = "test" },
            };
            var mapper = new Mock<IMapper>();
            _taskRewardRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false)).ReturnsAsync(new TaskRewardMockModel());
            _subTaskRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<SubTaskModel, bool>>>(), false));

            _subTaskRepo.Setup(x => x.CreateAsync(It.IsAny<SubTaskModel>())).ReturnsAsync(new SubTaskModel { SubTaskId = 0 }); ;
            var serviceResponse = await _subTaskService.CreateSubTask(SubtaskRequestDto);
            var response = await _consumerTaskController.CreateSubtask(SubtaskRequestDto);
            var result = response as ObjectResult;
            Assert.Equal(404, result?.StatusCode);
        }

        [Fact]
        public async void RemoveConsumerTask_ShouldReturnBadRequest_WhenTaskExternalCodeOrTenantCodeIsMissing()
        {
            // Arrange
            var requestDto = new DeleteConsumerTaskRequestDto
            {
                TaskExternalCode = string.Empty,
                TenantCode = string.Empty
            };

            // Act
            var result = await _consumerTaskController.RemoveConsumerTask(requestDto);

            // Assert
            var actionResult = Assert.IsType<ActionResult<BaseResponseDto>>(result);
            var statusCodeResult = Assert.IsType<ObjectResult>(actionResult.Result);
            Assert.Equal(StatusCodes.Status400BadRequest, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task1 RemoveConsumerTask_ShouldReturnNotFound_WhenTaskRewardIsNotFound()
        {
            // Arrange
            var requestDto = new DeleteConsumerTaskRequestDto
            {
                TaskExternalCode = "TASK001",
                TenantCode = "TENANT001",
                ConsumerCode = "CONSUMER001"
            };

            _taskRewardRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false));

            // Act
            var result = await _consumerTaskController.RemoveConsumerTask(requestDto);

            // Assert
            var actionResult = Assert.IsType<ActionResult<BaseResponseDto>>(result);
            var statusCodeResult = Assert.IsType<ObjectResult>(actionResult.Result);
            Assert.Equal(StatusCodes.Status404NotFound, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task1 RemoveConsumerTask_ShouldReturnNotFound_WhenConsumerTaskIsNotFound()
        {
            // Arrange
            var requestDto = new DeleteConsumerTaskRequestDto
            {
                TaskExternalCode = "TASK001",
                TenantCode = "TENANT001",
                ConsumerCode = "CONSUMER001"
            };

            var taskReward = new TaskRewardMockModel { };

            _taskRewardRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false))
                .ReturnsAsync(taskReward);

            _consumerTaskRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ConsumerTaskModel, bool>>>(), false));

            // Act
            var result = await _consumerTaskController.RemoveConsumerTask(requestDto);

            // Assert
            var actionResult = Assert.IsType<ActionResult<BaseResponseDto>>(result);
            var statusCodeResult = Assert.IsType<ObjectResult>(actionResult.Result);
            Assert.Equal(StatusCodes.Status404NotFound, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task1 RemoveConsumerTask_ShouldReturnSuccess_WhenTaskIsSuccessfullyDeleted()
        {
            // Arrange
            var requestDto = new DeleteConsumerTaskRequestDto
            {
                TaskExternalCode = "TASK001",
                TenantCode = "TENANT001",
                ConsumerCode = "CONSUMER001"
            };
            var expectedResponse = new BaseResponseDto();
            var taskReward = new TaskRewardMockModel() { TaskId = 1 };
            var consumerTask = new ConsumerTaskMockModel { ConsumerTaskId = 1 };

            _taskRewardRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false))
                .ReturnsAsync(taskReward);

            _consumerTaskRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ConsumerTaskModel, bool>>>(), false))
                .ReturnsAsync(consumerTask);


            _consumerTaskRepo
                .Setup(repo => repo.UpdateAsync(It.IsAny<ConsumerTaskModel>()))
                .ReturnsAsync(consumerTask);

            // Act
            var result = await _consumerTaskController.RemoveConsumerTask(requestDto);

            // Assert
            var actionResult = Assert.IsType<ActionResult<BaseResponseDto>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            Assert.IsType<BaseResponseDto>(okResult.Value);

        }

        [Fact]
        public async Task1 RemoveConsumerTask_ShouldReturnInternalServerError_WhenTaskDeletionFails()
        {
            // Arrange
            var requestDto = new DeleteConsumerTaskRequestDto
            {
                TaskExternalCode = "TASK001",
                TenantCode = "TENANT001",
                ConsumerCode = "CONSUMER001"
            };

            var taskReward = new TaskRewardMockModel() { TaskId = 1 };
            var consumerTask = new ConsumerTaskMockModel { ConsumerTaskId = 1 };

            _taskRewardRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false))
                .ReturnsAsync(taskReward);

            _consumerTaskRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ConsumerTaskModel, bool>>>(), false))
                .ReturnsAsync(consumerTask);


            _consumerTaskRepo
                .Setup(repo => repo.UpdateAsync(It.IsAny<ConsumerTaskModel>()));

            // Act
            var result = await _consumerTaskController.RemoveConsumerTask(requestDto);

            var actionResult = Assert.IsType<ActionResult<BaseResponseDto>>(result);
            var statusCodeResult = Assert.IsType<ObjectResult>(actionResult.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task1 RemoveConsumerTask_ShouldReturnInternalServerError_WhenException()
        {
            // Arrange
            var requestDto = new DeleteConsumerTaskRequestDto
            {
                TaskExternalCode = "TASK001",
                TenantCode = "TENANT001",
                ConsumerCode = "CONSUMER001"
            };

            var taskReward = new TaskRewardMockModel() { TaskId = 1 };
            var consumerTask = new ConsumerTaskMockModel { ConsumerTaskId = 1 };

            _taskRewardRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false))
                .ThrowsAsync(new Exception("test Exception"));

            _consumerTaskRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ConsumerTaskModel, bool>>>(), false))
                .ReturnsAsync(consumerTask);


            _consumerTaskRepo
                .Setup(repo => repo.UpdateAsync(It.IsAny<ConsumerTaskModel>()));

            // Act
            var result = await _consumerTaskController.RemoveConsumerTask(requestDto);

            var actionResult = Assert.IsType<ActionResult<BaseResponseDto>>(result);
            var statusCodeResult = Assert.IsType<ObjectResult>(actionResult.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
        }

        [Fact]
        public async void Should_Update_ConsumerTaskDetails_throw_error()
        {
            var consumerTaskMockDto = new ConsumerTaskMockDto();
            var response = await _consumerTaskController.UpdateConsumerTaskDetails(consumerTaskMockDto);

            var actionResult = Assert.IsType<ActionResult<ConsumerTaskDto>>(response);
            var statusCodeResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
            Assert.Equal(StatusCodes.Status404NotFound, statusCodeResult.StatusCode);
        }

        [Fact]
        public async void Should_Update_ConsumerTaskDetails()
        {
            var consumerTaskMockDto = new ConsumerTaskMockDto();
            var serviceMock = new Mock<IConsumerTaskService>();
            //It.IsAny<ConsumerTaskMockDto>()
            _consumerTaskRepo.Setup(x => x.FindOneAsync(It.IsAny<long>())).ReturnsAsync(new ConsumerTaskIdMockModel());
            var response = await _consumerTaskController.UpdateConsumerTaskDetails(consumerTaskMockDto);

            var result = response.Result as OkObjectResult;
            Assert.True(result?.Value != null);
            Assert.True(result.StatusCode == 200);
        }


        [Fact]
        public async void UpdateConsumerTask_Should_Handle()
        {
            var consumerTaskDto = new UpdateConsumerTaskMockDto();
            consumerTaskDto.TaskId = 0;
            ConsumerTaskService service = ConsumerTaskUpdateException();
            var result = await service.UpdateConsumerTaskDetails(consumerTaskDto);
            Assert.NotNull(result);
        }

        [Fact]
        public async void UpdateConsumerTask_Should_throw_exception()
        {
            var consumerTaskMockDto = new ConsumerTaskMockDto();
            var serviceMock = new Mock<IConsumerTaskService>();
            //It.IsAny<ConsumerTaskMockDto>()
            _consumerTaskRepo.Setup(x => x.FindOneAsync(It.IsAny<long>())).ThrowsAsync(new Exception("test exception"));
            var response = await _consumerTaskController.UpdateConsumerTaskDetails(consumerTaskMockDto);

            var statusCodeResult = Assert.IsType<ObjectResult>(response.Result);
            Assert.True(statusCodeResult?.StatusCode == 500);
        }


        [Fact]
        public async void GetConsumersByTaskId_ShouldReturnCompletedTasks_WhenDataIsValid()
        {
            // Arrange
            var request = new GetConsumerTaskByTaskId
            {
                TaskId = 123,
                TenantCode = "TENANT001",
                StartDate = new DateTime(2023, 1, 1),
                EndDate = new DateTime(2023, 1, 31)
            };

            var mockConsumerTasks = new List<ConsumerTaskModel>
    {
        new ConsumerTaskModel { ConsumerTaskId = 1, TaskId = 123, TenantCode = "TENANT001", TaskStatus = "COMPLETED", TaskCompleteTs = new DateTime(2023, 1, 10) },
        new ConsumerTaskModel { ConsumerTaskId = 2, TaskId = 123, TenantCode = "TENANT001", TaskStatus = "COMPLETED", TaskCompleteTs = new DateTime(2023, 1, 20) }
    };

            _consumerTaskRepo.Setup(x => x.GetPaginatedConsumerTask(It.IsAny<GetConsumerTaskByTaskId>()))
                    .ReturnsAsync(new PageinatedCompletedConsumerTaskDto() { CompletedTasks = mockConsumerTasks, TotalRecords = mockConsumerTasks.Count });


            // Act
            var response = await _consumerTaskController.GetConsumersByTaskId(request);

            // Assert
            var result = response.Result as OkObjectResult;
            Assert.NotNull(result?.Value);
            Assert.Equal(200, result.StatusCode);
            Assert.NotNull(result);
        }

        [Fact]
        public async void GetConsumersByTaskId_ShouldReturnInternalServerError_WhenExceptionOccurs()
        {
            // Arrange
            var request = new GetConsumerTaskByTaskId
            {
                TaskId = 123,
                TenantCode = "TENANT001",
                StartDate = new DateTime(2023, 1, 1),
                EndDate = new DateTime(2023, 1, 31)
            };


            _consumerTaskRepo.Setup(x => x.GetPaginatedConsumerTask(It.IsAny<GetConsumerTaskByTaskId>()))
                    .ThrowsAsync(new Exception("Database failure"));

            // Act
            var response = await _consumerTaskController.GetConsumersByTaskId(request);

            // Assert
            var result = response.Result as ObjectResult;
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, result.StatusCode);

            var errorResponse = Assert.IsType<BaseResponseDto>(result.Value);
            Assert.Equal(StatusCodes.Status500InternalServerError, errorResponse.ErrorCode);
            Assert.Contains("Database failure", errorResponse.ErrorMessage);
        }

        [Fact]
        public async Task1 UpdateHealthTaskProgress_TaskRewardNotFound_Returns404()
        {
            var request = CreateRequest(nameof(HealthTaskType.SLEEP));
            _taskRewardRepository.Setup(x => x.FindAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false));

            var response = await _consumerTaskController.UpdateHealthTaskProgress(request);

            var result = response as ObjectResult;
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status404NotFound, result.StatusCode);
        }

        [Fact]
        public async Task1 UpdateHealthTaskProgress_TaskRewardSelfReportFalse_Returns404()
        {
            var request = CreateRequest(nameof(HealthTaskType.SLEEP));
            _taskRewardRepository.Setup(x => x.FindAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false))
                 .ReturnsAsync(new List<TaskRewardModel>()
                {      new TaskRewardModel
                        {
                            TaskId = request.TaskId,
                            TenantCode = request.TenantCode,
                            SelfReport = false,
                            TaskCompletionCriteriaJson = "{\r\n  \"healthCriteria\": {\r\n    \"requiredSteps\": 107000,\r\n    \"healthTaskType\": \"InvalidType\"\r\n  },\r\n  \"completionPeriodType\": \"MONTH\",\r\n  \"completionCriteriaType\": \"HEALTH\"\r\n}\r\n"
                        }
                });

            var response = await _consumerTaskController.UpdateHealthTaskProgress(request);

            var result = response as ObjectResult;
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status404NotFound, result.StatusCode);
        }

        [Fact]
        public async Task1 UpdateHealthTaskProgress_InvalidCompletionCriteriaType_Returns400()
        {
            var request = CreateRequest("Test");
            _taskRewardRepository.Setup(x => x.FindAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false))
                .ReturnsAsync(new List<TaskRewardModel>()
                {      new TaskRewardModel
                        {
                            TaskId = request.TaskId,
                            TenantCode = request.TenantCode,
                            SelfReport = true,
                            TaskCompletionCriteriaJson = "{\r\n  \"healthCriteria\": {\r\n    \"requiredSteps\": 107000,\r\n    \"healthTaskType\": \"InvalidType\"\r\n  },\r\n  \"completionPeriodType\": \"MONTH\",\r\n  \"completionCriteriaType\": \"HEALTH\"\r\n}\r\n"
                        }
                });

            var response = await _consumerTaskController.UpdateHealthTaskProgress(request);

            var result = response as ObjectResult;
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task1 UpdateHealthTaskProgress_Consumer_Task_Not_Found_Returns404()
        {
            var request = CreateRequest(nameof(HealthTaskType.SLEEP));
            _taskRewardRepository.Setup(x => x.FindAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false))
                .ReturnsAsync(new List<TaskRewardModel>() { new TaskRewardModel
                {
                    TaskId = request.TaskId,
                    TenantCode = request.TenantCode,
                    SelfReport = true,
                    TaskCompletionCriteriaJson = "{\r\n  \"healthCriteria\": {\r\n    \"requiredSteps\": 107000,\r\n    \"healthTaskType\": \"HEALTH\"\r\n  },\r\n  \"completionPeriodType\": \"MONTH\",\r\n  \"completionCriteriaType\": \"HEALTH\"\r\n}\r\n"
                } });
            _consumerTaskRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<ConsumerTaskModel, bool>>>(), false));
            var response = await _consumerTaskController.UpdateHealthTaskProgress(request);

            var result = response as ObjectResult;
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status404NotFound, result.StatusCode);
        }


        [Fact]
        public async Task1 UpdateHealthTaskProgress_should_retunr200_when_helah_task_type_is_sleep()
        {
            var request = CreateRequest(nameof(HealthTaskType.SLEEP));
            request.NumberOfDays = 21;
            _taskRewardRepository.Setup(x => x.FindAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false))
                .ReturnsAsync(new List<TaskRewardModel>() {new TaskRewardModel
                {
                    TaskId = request.TaskId,
                    TenantCode = request.TenantCode,
                    SelfReport = true,
                    TaskCompletionCriteriaJson = "{\r\n  \"healthCriteria\": {\r\n    \"requiredSleep\": {\r\n      \"minSleepDuration\": 420,\r\n      \"numDaysAtOrAboveMinDuration\": 21\r\n    },\r\n    \"healthTaskType\": \"SLEEP\"\r\n  },\r\n  \"completionPeriodType\": \"MONTH\",\r\n  \"completionCriteriaType\": \"HEALTH\"\r\n}"
                } });

            var response = await _consumerTaskController.UpdateHealthTaskProgress(request);

            var result = response as OkObjectResult;
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);

        }

        [Fact]
        public async Task1 UpdateHealthTaskProgress_should_retunr200_when_helah_task_type_is_steps()
        {
            var request = CreateRequest(nameof(HealthTaskType.STEPS));
            request.Steps = 1000;
            _taskRewardRepository.Setup(x => x.FindAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false))
                .ReturnsAsync(new List<TaskRewardModel>() {new TaskRewardModel
                {
                    TaskId = request.TaskId,
                    TenantCode = request.TenantCode,
                    SelfReport = true,
                    TaskCompletionCriteriaJson = "{\r\n  \"healthCriteria\": {\r\n    \"requiredSteps\": 1000,\r\n    \"healthTaskType\": \"STEPS\"\r\n  },\r\n  \"completionPeriodType\": \"MONTH\",\r\n  \"completionCriteriaType\": \"HEALTH\"\r\n}\r\n"
                } });

            var response = await _consumerTaskController.UpdateHealthTaskProgress(request);

            var result = response as OkObjectResult;
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }

        [Fact]
        public async Task1 UpdateHealthTaskProgress_should_retunr200_when_helah_task_type_is_hydration()
        {
            var request = CreateRequest(nameof(HealthTaskType.HYDRATION));
            request.NumberOfDays = 21;
            _taskRewardRepository.Setup(x => x.FindAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false))
                .ReturnsAsync(new List<TaskRewardModel>() { new TaskRewardModel
                {
                    TaskId = request.TaskId,
                    TenantCode = request.TenantCode,
                    SelfReport = true,
                    TaskCompletionCriteriaJson = "{\r\n  \"healthCriteria\": {\r\n    \"requiredDays\": 21,\r\n    \"healthTaskType\": \"HYDRATION\"\r\n  },\r\n  \"completionPeriodType\": \"MONTH\",\r\n  \"completionCriteriaType\": \"HEALTH\"\r\n}\r\n"
                } });

            var response = await _consumerTaskController.UpdateHealthTaskProgress(request);

            var result = response as OkObjectResult;
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }

        [Fact]
        public async Task1 UpdateHealthTaskProgress_should_retunr200_when_helah_task_type_is_other()
        {
            var request = CreateRequest(nameof(HealthTaskType.OTHER));
            request.NumberOfDays = 21;
            _taskRewardRepository.Setup(x => x.FindAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false))
                .ReturnsAsync(new List<TaskRewardModel>() { new TaskRewardModel
                {
                    TaskId = request.TaskId,
                    TenantCode = request.TenantCode,
                    SelfReport = true,
                    TaskCompletionCriteriaJson = "{\r\n  \"healthCriteria\": {\r\n    \"requiredUnits\": 21,\r\n    \"healthTaskType\": \"OTHER\",\r\n\t\"unitType\": \"Days\"\r\n  },\r\n  \"completionPeriodType\": \"MONTH\",\r\n  \"completionCriteriaType\": \"HEALTH\"\r\n}"
                } });

            var response = await _consumerTaskController.UpdateHealthTaskProgress(request);

            var result = response as OkObjectResult;
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }
        [Fact]
        public async Task1 UpdateHealthTaskProgress_should_retunr200_when_helah_task_type_is_other_UI_Component()
        {
            var request = CreateRequest(nameof(HealthTaskType.OTHER));
            request.NumberOfDays = 21;
            _taskRewardRepository.Setup(x => x.FindAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false))
                .ReturnsAsync(new List<TaskRewardModel>() { new TaskRewardModel
                {
                    TaskId = request.TaskId,
                    TenantCode = request.TenantCode,
                    SelfReport = true,
TaskCompletionCriteriaJson = "{\r\n  \"selfReportType\": \"UI_COMPONENT\",\r\n    \"completionCriteriaType\": \"HEALTH\"\r\n,\r\n  \"healthCriteria\": {\r\n    \"requiredUnits\": 21,\r\n    \"healthTaskType\": \"OTHER\",\r\n    \"unitType\": \"Days\",\r\n    \"uiComponent\": [\r\n      {\r\n        \"isRequiredField\": true,\r\n        \"reportTypeLabel\": { \"en-US\": \"Blood Pressure\" }\r\n      }\r\n    ],\r\n    \"completionPeriodType\": \"MONTH\",\r\n    \"completionCriteriaType\": \"HEALTH\"\r\n  }\r\n}"
                } });

            var response = await _consumerTaskController.UpdateHealthTaskProgress(request);

            var result = response as OkObjectResult;
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }

        [Fact]
        public async Task1 UpdateHealthTaskProgress_should_retunr500_when_expectpion_occurred()
        {
            var request = CreateRequest(nameof(HealthTaskType.SLEEP));
            request.NumberOfDays = 21;
            _taskRewardRepository.Setup(x => x.FindAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false))
                .ThrowsAsync(new Exception("Testing"));

            var response = await _consumerTaskController.UpdateHealthTaskProgress(request);

            var result = response as ObjectResult;
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, result.StatusCode);

        }


        private UpdateHealthTaskProgressRequestDto CreateRequest(string healthTaskType, int? numberOfDays = null, int? steps = null)
        {
            return new UpdateHealthTaskProgressRequestDto
            {
                TenantCode = "TENANT1",
                ConsumerCode = "CONSUMER1",
                TaskId = 123,
                HealthTaskType = healthTaskType,
                NumberOfDays = numberOfDays,
                Steps = steps,
                HealthReport = new List<HealthTrackingDto>
                {
                    new HealthTrackingDto
                    {
                        HealthReportCompletionDate = DateTime.UtcNow.AddDays(-1),
                        HealthReportData = new List<HealthTrackingDetailDto>
                        {
                            new HealthTrackingDetailDto
                            {
                                HealthReportType = "Blood Pressure",
                                HealthReportValue = "120/80"
                            },
                            new HealthTrackingDetailDto
                            {
                                HealthReportType = "Heart Rate",
                                HealthReportValue = "70 bpm"
                            }

                        }
                    }
                }
            };

        }
    }
}


