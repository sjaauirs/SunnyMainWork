using AutoMapper;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NHibernate.Cfg;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Repositories;
using SunnyRewards.Helios.Task.Api.Controller;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Models;
using SunnyRewards.Helios.Task.Infrastructure.Mappings;
using SunnyRewards.Helios.Task.Infrastructure.Repositories;
using SunnyRewards.Helios.Task.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Task.Infrastructure.Services;
using SunnyRewards.Helios.Task.Infrastructure.Services.Interface;
using SunnyRewards.Helios.Task.UnitTest.Fixtures.MockDtos;
using SunnyRewards.Helios.Task.UnitTest.Fixtures.MockModel;
using SunnyRewards.Helios.Task.UnitTest.Fixtures.MockRepositories;
using System.Linq.Expressions;
using Xunit;

namespace SunnyRewards.Helios.Task.UnitTest.Controllers
{
    public class TaskControllerUnitTest
    {
        private readonly ITaskService _taskService;
        private readonly IMapper _mapper;
        private readonly Mock<ILogger<TaskService>> _taskServiceLogger;
        private readonly Mock<ITaskRepo> _taskRepository;
        private readonly Mock<ITaskRewardRepo> _taskRewardRepository;
        private readonly Mock<ITaskDetailRepo> _taskDetailRepository;
        private readonly Mock<ITaskTypeRepo> _taskTypeRepository;
        private readonly Mock<ITermsOfServiceRepo> _termsOfServiceRepository;
        private readonly Mock<ILogger<TaskController>> _taskLogger;
        private readonly Mock<ITenantTaskCategoryRepo> _tenantTaskCategoryRepo;
        private readonly Mock<ITaskRewardTypeRepo> _taskRewardTypeRepo;
        private readonly Mock<IConsumerTaskRepo> _consumerTaskRepo;
        private readonly Mock<ISubTaskRepo> _subTaskRepo;
        private readonly Mock<ITaskExternalMappingRepo> _taskExternalMappingRepo;
        private readonly Mock<ITriviaRepo> _triviaRepo;
        private readonly Mock<ITriviaQuestionGroupRepo> _triviaQuestionGroupRepo;
        private readonly Mock<ITriviaQuestionRepo> _triviaQuestionRepo;
        private readonly Mock<ITaskCategoryRepo> _mockTaskCategoryRepo;
        private readonly Mock<IQuestionnaireRepo> _questionnaireRepo;
        private readonly Mock<IQuestionnaireQuestionGroupRepo> _questionnaireQuestionGroupRepo;
        private readonly Mock<IQuestionnaireQuestionRepo> _questionnaireQuestionRepo;

        private TaskController _taskController;
        public TaskControllerUnitTest()
        {
            _taskServiceLogger = new Mock<ILogger<TaskService>>();
            _taskLogger = new Mock<ILogger<TaskController>>();
            _taskRepository = new TaskMockRepo();
            _taskDetailRepository = new TaskDetailMockRepo();
            _taskRewardRepository = new TaskRewardMockRepo();
            _termsOfServiceRepository = new TermsOfServiceMockRepo();
            _taskTypeRepository = new TaskTypeMockRepo();
            _tenantTaskCategoryRepo = new TenantTaskCategoryMockRepo();
            _taskRewardTypeRepo = new TaskRewardTypeMockRepo();
            _consumerTaskRepo = new ConsumerTaskMockRepo();
            _subTaskRepo = new SubTaskMockRepo();
            _taskExternalMappingRepo = new TaskExternalMappingMockRepo();
            _triviaRepo = new TriviaMockRepo();
            _triviaQuestionGroupRepo = new TriviaQuestionGroupMockRepo();
            _triviaQuestionRepo = new Mock<ITriviaQuestionRepo>();
            _mockTaskCategoryRepo = new Mock<ITaskCategoryRepo>();
            _questionnaireRepo = new Mock<IQuestionnaireRepo>();
            _questionnaireQuestionGroupRepo = new Mock<IQuestionnaireQuestionGroupRepo>();
            _questionnaireQuestionRepo = new Mock<IQuestionnaireQuestionRepo>();

            _mapper = new Mapper(new MapperConfiguration(
                configure =>
                {
                    configure.AddMaps(typeof(Infrastructure.Mappings.MappingProfile.TaskMapping).Assembly.FullName);
                    configure.AddMaps(typeof(Infrastructure.Mappings.MappingProfile.TaskDetailMapping).Assembly.FullName);
                }));
            _taskService = new TaskService(_taskServiceLogger.Object, _mapper, _taskRepository.Object, _taskRewardRepository.Object, _taskDetailRepository.Object, _termsOfServiceRepository.Object,
                _tenantTaskCategoryRepo.Object, _taskTypeRepository.Object, _taskRewardTypeRepo.Object, _consumerTaskRepo.Object,
                _subTaskRepo.Object, _taskExternalMappingRepo.Object, _triviaRepo.Object, _triviaQuestionGroupRepo.Object, _triviaQuestionRepo.Object, _mockTaskCategoryRepo.Object,
                _questionnaireRepo.Object,_questionnaireQuestionGroupRepo.Object, _questionnaireQuestionRepo.Object);
            _taskController = new TaskController(_taskLogger.Object, _taskService);
        }

        [Fact]
        public async void Should_Get_Task_Reward()
        {
            var getTaskRewardRequestMockDto = new GetTaskRewardRequestMockDto();
            getTaskRewardRequestMockDto.LanguageCode = "es";
            var response = await _taskController.GetTasksByTaskRewardCode(getTaskRewardRequestMockDto);
            var result = response?.Result as OkObjectResult;
            Assert.True(result?.Value != null);
            Assert.True(result.StatusCode == 200);
        }

        [Fact]
        public async void Should_Return_Exception_Catch_Task_Reward()
        {
            var taskLogger = new Mock<ILogger<TaskController>>();
            var taskService = new Mock<ITaskService>();
            var controller = new TaskController(taskLogger.Object, taskService.Object);
            var getTaskRewardRequestMockDto = new GetTaskRewardRequestMockDto();
            taskService.Setup(x => x.GetTasksByTaskRewardCode(It.IsAny<GetTaskRewardRequestMockDto>())).ThrowsAsync(new Exception("Simulated exception"));
            var result = await controller.GetTasksByTaskRewardCode(getTaskRewardRequestMockDto);
            Assert.True(result == null);
        }

        [Fact]
        public async void Should_Catch_Get_TaskData_Service_Level_Ok_Response()
        {
            var taskId = 1;
            TaskService taskService = TaskDataException();
            _taskRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskModel, bool>>>(), false)).ReturnsAsync(new TaskMockModel());
            var response = await _taskService.GetTaskData(taskId);
            Assert.True(response != null);
        }

        [Fact]
        public async void Should_Catch_Get_TaskData_Service_Level_Exception()
        {
            var taskId = 0;
            TaskService taskService = TaskDataException();
            var result = await taskService.GetTaskData(taskId);
            Assert.NotNull(result);
            Assert.IsType<TaskDto>(result);
            Assert.True(result.TaskId == 0);
        }

        [Fact]
        public async void GetTasksByTaskRewardCode_ValidInput_ReturnsValidResponse()
        {
            _taskRewardRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false)).ReturnsAsync(new TaskRewardMockModel());
            _taskRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskModel, bool>>>(), false)).ReturnsAsync(new TaskMockModel());
            _taskDetailRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskDetailModel, bool>>>(), false)).ReturnsAsync(new TaskDetailMockModel());
            _termsOfServiceRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TermsOfServiceModel, bool>>>(), false)).ReturnsAsync(new TermsOfServiceMockModel());
            _tenantTaskCategoryRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TenantTaskCategoryModel, bool>>>(), false)).ReturnsAsync(new TenantTaskCategoryMockModel());
            _taskTypeRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskTypeModel, bool>>>(), false)).ReturnsAsync(new TaskTypeMockModel());
            _taskRewardTypeRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskRewardTypeModel, bool>>>(), false)).ReturnsAsync(new TaskRewardTypeMockModel());
            var requestDto = new GetTaskRewardRequestMockDto
            {
                TaskRewardCodes = new List<string> { "code1", "code2" }
            };
            var response = await _taskService.GetTasksByTaskRewardCode(requestDto);
            Assert.NotNull(response);
            Assert.NotNull(response.TaskRewardDetails);
            Assert.Equal(2, response.TaskRewardDetails.Count);
        }

        [Fact]
        public async void GetTasksByTaskRewardCode_ValidInput_Returns_NullCheck()
        {
            _taskRewardRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false)).ReturnsAsync(new TaskRewardMockModel());
            _taskRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskModel, bool>>>(), false)).ReturnsAsync((TaskMockModel)null);
            _taskDetailRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskDetailModel, bool>>>(), false)).ReturnsAsync(new TaskDetailMockModel());
            _termsOfServiceRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TermsOfServiceModel, bool>>>(), false)).ReturnsAsync(new TermsOfServiceMockModel());
            _tenantTaskCategoryRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TenantTaskCategoryModel, bool>>>(), false)).ReturnsAsync(new TenantTaskCategoryMockModel());
            _taskTypeRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskTypeModel, bool>>>(), false)).ReturnsAsync(new TaskTypeMockModel());
            _taskRewardTypeRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskRewardTypeModel, bool>>>(), false)).ReturnsAsync(new TaskRewardTypeMockModel());
            var requestDto = new GetTaskRewardRequestMockDto
            {
                TaskRewardCodes = new List<string> { "code1", "code2" }
            };
            var response = await _taskService.GetTasksByTaskRewardCode(requestDto);
            Assert.NotNull(response);
            Assert.NotNull(response.TaskRewardDetails);
            Assert.NotEqual(2, response.TaskRewardDetails.Count);
        }

        [Fact]
        public async void Should_Catch_Task_Reward_Service_Level_Exception()
        {
            TaskService taskService = TaskRewardException();
            var dto = new GetTaskRewardRequestMockDto();
            var result = await taskService.GetTasksByTaskRewardCode(dto);
            Assert.True(result?.TaskRewardDetails?.Count <= 0);
        }

        #region PrivateMethod
        private static TaskService TaskDataException()
        {
            var mockTaskRepo = new Mock<ITaskRepo>();
            var mockTaskRewardRepo = new Mock<ITaskRewardRepo>();
            var mockTaskDetailRepo = new Mock<ITaskDetailRepo>();
            var mockTermsOfServiceRepo = new Mock<ITermsOfServiceRepo>();
            var tenantTaskCategoryRepo = new Mock<ITenantTaskCategoryRepo>();
            var taskTypeRepo = new Mock<ITaskTypeRepo>();
            var taskCategoryRepo = new Mock<ITaskCategoryRepo>();
            var taskRewardTypeRepo = new Mock<ITaskRewardTypeRepo>();
            var consumerTaskRepo = new Mock<IConsumerTaskRepo>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger<TaskService>>();
            var subTaskRepo = new Mock<ISubTaskRepo>();
            var taskExternalMappingRepo = new Mock<ITaskExternalMappingRepo>();
            var triviaRepo = new Mock<ITriviaRepo>();
            var triviaQuestionGroupRepo = new Mock<ITriviaQuestionGroupRepo>();
            var triviaQuestionRepo = new Mock<ITriviaQuestionRepo>();
            var questionnaireRepo = new Mock<IQuestionnaireRepo>();
            var questionnaireQuestionGroupRepo = new Mock<IQuestionnaireQuestionGroupRepo>();
            var questionnaireQuestionRepo = new Mock<IQuestionnaireQuestionRepo>();
            mockTaskRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskModel, bool>>>(), false))
                .ThrowsAsync(new Exception("Simulated repository exception"));
            var taskService = new TaskService(
                mockLogger.Object,
                mockMapper.Object,
                mockTaskRepo.Object,
                mockTaskRewardRepo.Object,
                mockTaskDetailRepo.Object,
                mockTermsOfServiceRepo.Object,
                tenantTaskCategoryRepo.Object,
                taskTypeRepo.Object, taskRewardTypeRepo.Object, consumerTaskRepo.Object,
                subTaskRepo.Object, taskExternalMappingRepo.Object, triviaRepo.Object, triviaQuestionGroupRepo.Object, triviaQuestionRepo.Object, taskCategoryRepo.Object,
                questionnaireRepo.Object, questionnaireQuestionGroupRepo.Object, questionnaireQuestionRepo.Object
               );
            return taskService;
        }

        private static TaskService TaskRewardException()
        {
            var _taskRepo = new Mock<ITaskRepo>();
            var taskReward = new Mock<ITaskRewardRepo>();
            var _taskDetailRepo = new Mock<ITaskDetailRepo>();
            var _termsOfServiceRepo = new Mock<ITermsOfServiceRepo>();
            var _mapper = new Mock<IMapper>();
            var _taskLogger = new Mock<ILogger<TaskService>>();
            var _tenantCategoryRepo = new Mock<ITenantTaskCategoryRepo>();
            var taskTypeRepo = new Mock<ITaskTypeRepo>();
            var taskRewardTypeRepo = new Mock<ITaskRewardTypeRepo>();
            var consumerTaskRepo = new Mock<IConsumerTaskRepo>();
            var subTaskRepo = new Mock<ISubTaskRepo>();
            var taskExternalMappingRepo = new Mock<ITaskExternalMappingRepo>();
            var triviaRepo = new Mock<ITriviaRepo>();
            var triviaQuestionGroupRepo = new Mock<ITriviaQuestionGroupRepo>();
            var triviaQuestionRepo = new Mock<ITriviaQuestionRepo>();
            var taskCategoryRepo = new Mock<ITaskCategoryRepo>();
            var questionnaireRepo = new Mock<IQuestionnaireRepo>();
            var questionnaireQuestionGroupRepo = new Mock<IQuestionnaireQuestionGroupRepo>();
            var questionnaireQuestionRepo = new Mock<IQuestionnaireQuestionRepo>();
            var taskService = new TaskService(_taskLogger.Object, _mapper.Object, _taskRepo.Object, taskReward.Object, _taskDetailRepo.Object, _termsOfServiceRepo.Object,
                _tenantCategoryRepo.Object, taskTypeRepo.Object, taskRewardTypeRepo.Object, consumerTaskRepo.Object, subTaskRepo.Object, taskExternalMappingRepo.Object,
                triviaRepo.Object, triviaQuestionGroupRepo.Object, triviaQuestionRepo.Object, taskCategoryRepo.Object,questionnaireRepo.Object, questionnaireQuestionGroupRepo.Object, questionnaireQuestionRepo.Object);

            _taskRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskModel, bool>>>(), false)).ReturnsAsync(new TaskMockModel());

            _taskDetailRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskDetailModel, bool>>>(), false)).ReturnsAsync(new TaskDetailMockModel());

            _termsOfServiceRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TermsOfServiceModel, bool>>>(), false)).ReturnsAsync(new TermsOfServiceMockModel());

            _mapper.Setup(m => m.Map<TaskRewardDto>(It.IsAny<TaskRewardDto>()))
            .Returns(new TaskRewardDto());

            _mapper.Setup(m => m.Map<TaskDto>(It.IsAny<TaskDto>()))
                .Returns(new TaskDto());

            _mapper.Setup(m => m.Map<TaskDetailDto>(It.IsAny<TaskDetailDto>()))
                .Returns(new TaskDetailDto());

            _mapper.Setup(m => m.Map<TermsOfServiceDto>(It.IsAny<TermsOfServiceDto>()))
                .Returns(new TermsOfServiceDto());
            return taskService;
        }
        #endregion

        [Fact]
        public void Should_return_SubTaskMap()
        {
            Configuration _configuration;
            _configuration = Fluently.Configure()
                    .Database(SQLiteConfiguration.Standard.InMemory().ShowSql())
                    .Mappings(m => m.FluentMappings.Add<SubTaskMap>())
                    .BuildConfiguration();
        }

        [Fact]
        public void Should_return_TaskCategoryMap()
        {
            Configuration _configuration;
            _configuration = Fluently.Configure()
                    .Database(SQLiteConfiguration.Standard.InMemory().ShowSql())
                    .Mappings(m => m.FluentMappings.Add<TaskCategoryMap>())
                    .BuildConfiguration();
        }

        [Fact]
        public void Should_return_TaskDetailMap()
        {
            Configuration _configuration;
            _configuration = Fluently.Configure()
                    .Database(SQLiteConfiguration.Standard.InMemory().ShowSql())
                    .Mappings(m => m.FluentMappings.Add<TaskDetailMap>())
                    .BuildConfiguration();
        }

        [Fact]
        public void Should_return_TaskMap()
        {
            Configuration _configuration;
            _configuration = Fluently.Configure()
                    .Database(SQLiteConfiguration.Standard.InMemory().ShowSql())
                    .Mappings(m => m.FluentMappings.Add<TaskMap>())
                    .BuildConfiguration();
        }

        [Fact]
        public void Should_return_TenantTaskCategoryMap()
        {
            Configuration _configuration;
            _configuration = Fluently.Configure()
                    .Database(SQLiteConfiguration.Standard.InMemory().ShowSql())
                    .Mappings(m => m.FluentMappings.Add<TenantTaskCategoryMap>())
                    .BuildConfiguration();
        }

        [Fact]
        public void TaskCategoryRepo_Constructor_ShouldInstantiate_WhenDependenciesAreProvided()
        {
            var mockLogger = new Mock<ILogger<BaseRepo<TaskCategoryModel>>>();
            var mockSession = new Mock<NHibernate.ISession>();
            var repo = new TaskCategoryRepo(mockLogger.Object, mockSession.Object);
            Assert.NotNull(repo);
        }

        [Fact]
        public void TenantTaskCategoryRepo_Constructor_ShouldInstantiate_WhenDependenciesAreProvided()
        {
            var mockLogger = new Mock<ILogger<BaseRepo<TenantTaskCategoryModel>>>();
            var mockSession = new Mock<NHibernate.ISession>();
            var repo = new Infrastructure.Repositories.TenantTaskCategoryRepo(mockLogger.Object, mockSession.Object);
            Assert.NotNull(repo);
        }

        [Fact]
        public async System.Threading.Tasks.Task GetTaskExport_ReturnsOkResult_WhenExportIsSuccessful()
        {
            // Arrange
            var requestDto = new ExportTaskRequestDto { TenantCode = "tenant1" };
            var taskExternalMappingMap = new TaskExternalMappingMap();
            var taskExternalMappingRepo = new TaskExternalMappingMockRepo();

            // Act
            var result = await _taskController.GetTaskExport(requestDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async System.Threading.Tasks.Task GetTaskExport_ReturnsInternalServerError_WhenExceptionIsThrown()
        {
            // Arrange
            var requestDto = new ExportTaskRequestDto { TenantCode = "tenant1" };

            _subTaskRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<SubTaskModel, bool>>>(), false)).ThrowsAsync(new Exception("Testing"));
            // Act
            var result = await _taskController.GetTaskExport(requestDto);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            var responseDto = Assert.IsType<ExportTaskResponseDto>(statusCodeResult.Value);
            Assert.Equal(500, responseDto.ErrorCode);
        }

        [Fact]
        public async System.Threading.Tasks.Task GetTaskExport_ReturnsErrorResult_WhenExportFails()
        {
            // Arrange
            var requestDto = new ExportTaskRequestDto { TenantCode = "tenant1" };

            var mockTaskService = new Mock<ITaskService>();
            mockTaskService.Setup(x => x.GetTaskExport(It.IsAny<ExportTaskRequestDto>()))
                .ReturnsAsync(new ExportTaskResponseDto() { ErrorCode = 400 });
            var taskLoggerMock = new Mock<ILogger<TaskController>>();
            var controller = new TaskController(taskLoggerMock.Object, mockTaskService.Object);
            // Act
            var result = await controller.GetTaskExport(requestDto);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(400, statusCodeResult.StatusCode);
        }
        [Fact]
        public async void Should_Create_TaskExternalMapping()
        {
            var taskExternalMapping = new TaskExternalMappingRequestDto
            {
                TaskExternalCode = "xyz",
                CreateUser = "abc",
                TaskThirdPartyCode = "test",
                TenantCode = "ten-123"
            };
            var mapper = new Mock<IMapper>();
            mapper.Setup(x => x.Map<TaskExternalMappingModel>(It.IsAny<TaskExternalMappingRequestDto>()))
               .Returns(new TaskExternalMappingModel
               {
                   TaskExternalCode = "xyz",
                   CreateUser = "abc",
                   TaskThirdPartyCode = "test",
                   TenantCode = "ten-123",
                   TaskExternalMappingId = 1,
               });
            _taskExternalMappingRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskExternalMappingModel, bool>>>(), false));

            _taskExternalMappingRepo.Setup(x => x.CreateAsync(It.IsAny<TaskExternalMappingModel>())).ReturnsAsync(new TaskExternalMappingModel { TaskExternalMappingId = 1 });
            var serviceResponse = await _taskService.CreateTaskExternalMapping(taskExternalMapping);
            var response = await _taskController.CreateTaskExternalMapping(taskExternalMapping);
            var result = response as OkObjectResult;
            Assert.True(result?.StatusCode == 200);
        }
        [Fact]
        public async void Should_Return_404_Create_TaskExternalMapping()
        {
            var taskExternalMapping = new TaskExternalMappingRequestDto
            {
                TaskExternalCode = "xyz",
                CreateUser = "abc",
                TaskThirdPartyCode = "test",
                TenantCode = "ten-123"
            };
            var mapper = new Mock<IMapper>();
            _taskExternalMappingRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskExternalMappingModel, bool>>>(), false));
            mapper.Setup(x => x.Map<TaskExternalMappingModel>(It.IsAny<TaskExternalMappingRequestDto>()));

            _taskExternalMappingRepo.Setup(x => x.CreateAsync(It.IsAny<TaskExternalMappingModel>())).ReturnsAsync(new TaskExternalMappingModel { TaskExternalMappingId = 0 });
            var serviceResponse = await _taskService.CreateTaskExternalMapping(taskExternalMapping);
            var response = await _taskController.CreateTaskExternalMapping(taskExternalMapping);
            var result = response as ObjectResult;
            Assert.Equal(404, result?.StatusCode);
        }
        [Fact]
        public async void Should_Return_500_Create_Subtask()
        {
            var taskExternalMapping = new TaskExternalMappingRequestDto
            {
                TaskExternalCode = "xyz",
                CreateUser = "abc",
                TaskThirdPartyCode = "test",
                TenantCode = "ten-123"
            };
            var mapper = new Mock<IMapper>();
            _taskExternalMappingRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskExternalMappingModel, bool>>>(), false)).ThrowsAsync(new Exception("An error occurred"));
            mapper.Setup(x => x.Map<TaskExternalMappingModel>(It.IsAny<TaskExternalMappingRequestDto>()));

            _taskExternalMappingRepo.Setup(x => x.CreateAsync(It.IsAny<TaskExternalMappingModel>())).ReturnsAsync(new TaskExternalMappingModel { TaskExternalMappingId = 0 });
            var serviceResponse = await _taskService.CreateTaskExternalMapping(taskExternalMapping);
            var response = await _taskController.CreateTaskExternalMapping(taskExternalMapping);
            var okResult = Assert.IsType<ObjectResult>(response);
            var returnValue = Assert.IsType<BaseResponseDto>(okResult.Value);
            Assert.Equal(returnValue.ErrorCode, 500);
        }
        [Fact]
        public async System.Threading.Tasks.Task Create_Task_Returns422Result_WhenTaskCodeNotEqual()
        {
            // Arrange
            var requestDto = new CreateTaskRequestDto
            {
                TaskId = 47,
                TaskTypeId = 1,
                TaskCode = "tsk-90399d4b7682458cbc9a93206967",
                TaskName = "Enroll in pre diabetes program",
                SelfReport = false,
                ConfirmReport = false,
                TaskCategoryId = 0,
                IsSubtask = false,
                CreateUser = "per-915325069cdb42c783dd4601e1d27704"
            };
            // Act
            var result = await _taskController.CreateTask(requestDto);

            // Assert
            var okResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(422, okResult.StatusCode);
        }
        [Fact]
        public async System.Threading.Tasks.Task Create_Task_Returns409_When_TaskAlready_Exists()
        {
            //Arrange
            var requestDto = new CreateTaskRequestDto
            {
                TaskId = 47,
                TaskTypeId = 1,
                TaskCode = "tsk-210ddf7876234c11b64668d4246f0b44",
                TaskName = "Enroll in pre diabetes program",
                SelfReport = false,
                ConfirmReport = false,
                TaskCategoryId = 0,
                IsSubtask = false,
                CreateUser = "per-915325069cdb42c783dd4601e1d27704"
            };
            // Act
            var result = await _taskController.CreateTask(requestDto);

            // Assert
            var res = Assert.IsType<ObjectResult>(result);
            Assert.Equal(409, res.StatusCode);
        }
        [Fact]
        public async System.Threading.Tasks.Task Create_Task_return_Successful()
        {
            //Arrange
            var requestDto = new CreateTaskRequestDto
            {
                TaskId = 47,
                TaskTypeId = 1,
                TaskCode = "tsk-210ddf7876234c11b64668d4246f0b44",
                TaskName = "Enroll in pre diabetes program",
                SelfReport = false,
                ConfirmReport = false,
                TaskCategoryId = 0,
                IsSubtask = false,
                CreateUser = "per-915325069cdb42c783dd4601e1d27704"
            };

            _taskRepository
                .Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskModel, bool>>>(), false))
                 .ReturnsAsync((TaskModel)null);

            // Act
            var result = await _taskController.CreateTask(requestDto);

            var res = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, res.StatusCode);

        }

        [Fact]
        public async void Should_Return_500_Create_TaskExternalMapping_controller()
        {
            var taskExternalMapping = new TaskExternalMappingRequestDto
            {
                TaskExternalCode = "xyz",
                CreateUser = "abc",
                TaskThirdPartyCode = "test",
                TenantCode = "ten-123"
            };
            var taskService = new Mock<ITaskService>();
            var taskController = new TaskController(_taskLogger.Object, taskService.Object);
            taskService.Setup(x => x.CreateTaskExternalMapping(taskExternalMapping)).ThrowsAsync(new Exception("An error occurred"));
            var response = await taskController.CreateTaskExternalMapping(taskExternalMapping);

            var okResult = Assert.IsType<ObjectResult>(response);
            var returnValue = Assert.IsType<BaseResponseDto>(okResult.Value);
            Assert.Equal(returnValue.ErrorCode, 500);
        }
        [Fact]
        public async System.Threading.Tasks.Task GetTaskByTaskName_Should_Return_Ok_Response()
        {
            // Arrange
            var requestDto = new GetTaskByTaskNameRequestDto();
            var tasks = new List<TaskModel>()
            {
                new TaskMockModel()
            };
            _taskRepository
              .Setup(x => x.FindAsync(It.IsAny<Expression<Func<TaskModel, bool>>>(), false))
               .ReturnsAsync(tasks);
            //Act
            var response = await _taskController.GetTaskByTaskName(requestDto);

            // Assert
            Assert.NotNull(response);
            var okObjectResult = Assert.IsType<OkObjectResult>(response);
            Assert.Equal(StatusCodes.Status200OK, okObjectResult.StatusCode);
        }
        [Fact]
        public async System.Threading.Tasks.Task GetTaskByTaskName_Should_Return_NotFound_Response()
        {
            // Arrange
            var requestDto = new GetTaskByTaskNameRequestDto();
            _taskRepository
              .Setup(x => x.FindAsync(It.IsAny<Expression<Func<TaskModel, bool>>>(), false));

            //Act
            var response = await _taskController.GetTaskByTaskName(requestDto);

            // Assert
            Assert.NotNull(response);
            var objectResult = Assert.IsType<ObjectResult>(response);
            Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);
        }
        [Fact]
        public async System.Threading.Tasks.Task GetTaskByTaskName_Should_Return_InternalServerError_Response()
        {
            // Arrange
            var requestDto = new GetTaskByTaskNameRequestDto();
            _taskRepository
              .Setup(x => x.FindAsync(It.IsAny<Expression<Func<TaskModel, bool>>>(), false)).ThrowsAsync(new Exception("testing"));

            //Act
            var response = await _taskController.GetTaskByTaskName(requestDto);

            // Assert
            Assert.NotNull(response);
            var objectResult = Assert.IsType<ObjectResult>(response);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        [Fact]
        public async System.Threading.Tasks.Task GetTasksAsync_ShouldReturnOk_WhenServiceReturnsValidResponse()
        {
            // Arrange
            var expectedResponse = new TasksResponseDto
            {
                ErrorCode = null,
                ErrorMessage = null,
                Tasks =
                [
                    new() { TaskId = 1, TaskName = "Task1" },
                    new() { TaskId = 2, TaskName = "Task2" }
                ]
            };

            var _mockTaskService = new Mock<ITaskService>();
            var _controller = new TaskController(_taskLogger.Object, _mockTaskService.Object);

            _mockTaskService
                .Setup(service => service.GetTasksAsync())
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.GetTasksAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
            Assert.Equal(expectedResponse, okResult.Value);
        }

        [Fact]
        public async System.Threading.Tasks.Task GetTasksAsync_ShouldReturnInternalServerError_WhenExceptionIsThrown()
        {
            // Arrange
            var exceptionMessage = "Something went wrong.";

            var _mockTaskService = new Mock<ITaskService>();
            var _controller = new TaskController(_taskLogger.Object, _mockTaskService.Object);

            _mockTaskService
                .Setup(service => service.GetTasksAsync())
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await _controller.GetTasksAsync();

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        [Fact]
        public async void GetTasksAsync_Controller_ShouldHandleException()
        {
            var serviceMock = new Mock<ITaskService>();
            var controller = new TaskController(_taskLogger.Object, serviceMock.Object);
            serviceMock.Setup(x => x.GetTasksAsync()).ThrowsAsync(new Exception("inner Exception"));
            var response = await controller.GetTasksAsync();
            var result = response as ObjectResult;
            Assert.Equal(StatusCodes.Status500InternalServerError, result?.StatusCode);
        }

        [Fact]
        public async void GetTasksAsync_Service_ShouldReturnResponse()
        {
            var response = await _taskService.GetTasksAsync();
            Assert.NotNull(response);
        }

        [Fact]
        public async void GetTasksAsync_RepositoryReturnsNull_ReturnsErrorResponse()
        {
            // Arrange
            var repositoryMock = new Mock<ITaskRepo>();
            var loggerMock = new Mock<ILogger<TaskService>>();
            var mapperMock = new Mock<IMapper>();

            // Simulate the repository returning null
            repositoryMock
                .Setup(x => x.FindAsync(It.IsAny<Expression<Func<TaskModel, bool>>>(), false))
                .ReturnsAsync((List<TaskModel>)null);

            var service = new TaskService(loggerMock.Object, mapperMock.Object, repositoryMock.Object, _taskRewardRepository.Object, _taskDetailRepository.Object, _termsOfServiceRepository.Object,
                _tenantTaskCategoryRepo.Object, _taskTypeRepository.Object, _taskRewardTypeRepo.Object, _consumerTaskRepo.Object,
                _subTaskRepo.Object, _taskExternalMappingRepo.Object, _triviaRepo.Object, _triviaQuestionGroupRepo.Object, _triviaQuestionRepo.Object,_mockTaskCategoryRepo.Object,
                _questionnaireRepo.Object, _questionnaireQuestionGroupRepo.Object, _questionnaireQuestionRepo.Object);
            var response = await service.GetTasksAsync();

            // Act
            var expectedError = "No task was found.";

            // Assert
            Assert.NotNull(response);
            Assert.Equal(expectedError, response.ErrorMessage);
        }

        [Fact]
        public async void GetTasksAsync_Service_ShouldHandleRepositoryException()
        {
            var expectedException = new Exception("Test exception");
            _taskRepository.Setup(x => x.FindAsync(It.IsAny<Expression<Func<TaskModel, bool>>>(), false)).ThrowsAsync(expectedException);
            var result = await _taskService.GetTasksAsync();

            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, result.ErrorCode);
            Assert.Equal("Test exception", result.ErrorMessage);
        }

        [Fact]
        public async void GetTasksAsync_Controller_ShouldReturnNotFound()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TaskCategoryService>>();
            var serviceMock = new Mock<ITaskService>();
            var errorMessage = "No task was found.";
            serviceMock.Setup(x => x.GetTasksAsync()).ReturnsAsync(new TasksResponseDto
            {
                ErrorCode = StatusCodes.Status404NotFound,
                ErrorMessage = errorMessage
            });
            var controller = new TaskController(_taskLogger.Object, serviceMock.Object);

            // Act
            var response = await controller.GetTasksAsync();
            var result = Assert.IsType<OkObjectResult>(response);

            // Assert
            Assert.Equal(StatusCodes.Status200OK, result?.StatusCode);
            Assert.NotNull(result?.Value);

            var responseDto = Assert.IsType<TasksResponseDto>(result.Value);
            Assert.Equal(errorMessage, responseDto.ErrorMessage);
        }

        [Fact]
        public async System.Threading.Tasks.Task UpdateTaskAsync_ShouldReturnOk_WhenServiceReturnsValidResponse()
        {
            long taskId = 47;
            //Arrange
            var requestDto = new TaskRequestDto
            {
                TaskTypeId = 1,
                TaskCode = "tsk-210ddf7876234c11b64668d4246f0b44",
                TaskName = "Enroll in pre diabetes program",
                SelfReport = false,
                ConfirmReport = false,
                TaskCategoryId = 0,
                IsSubtask = false,
            };

            var taskModel = new TaskModel
            {
                TaskId = 47,
                TaskTypeId = 1,
                TaskCode = "tsk-210ddf7876234c11b64668d4246f0b44",
                TaskName = "Enroll in pre diabetes program",
                SelfReport = false,
                ConfirmReport = false,
                TaskCategoryId = 0,
                IsSubtask = false,
            };

            _taskRepository.Setup(x => x.UpdateAsync(taskModel)).ReturnsAsync(taskModel);

            // Act
            var result = await _taskController.UpdateTaskAsync(taskId, requestDto);

            var res = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, res.StatusCode);
        }
        [Fact]
        public async System.Threading.Tasks.Task ServiceUpdateTaskAsync_ShouldReturnOk_WhenServiceReturnsValidResponse()
        {
            long taskId = 47;
            //Arrange
            var requestDto = new TaskRequestDto
            {
                TaskTypeId = 1,
                TaskCode = "tsk-210ddf7876234c11b64668d4246f0b44",
                TaskName = "Enroll in pre diabetes program",
                SelfReport = false,
                ConfirmReport = false,
                TaskCategoryId = 0,
                IsSubtask = false,
            };

            var taskModel = new TaskModel
            {
                TaskId = 47,
                TaskTypeId = 1,
                TaskCode = "tsk-210ddf7876234c11b64668d4246f0b44",
                TaskName = "Enroll in pre diabetes program",
                SelfReport = false,
                ConfirmReport = false,
                TaskCategoryId = 0,
                IsSubtask = false,
            };
            _taskRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskModel, bool>>>(), false)).ReturnsAsync(new TaskMockModel());

            _taskRepository.Setup(x => x.UpdateAsync(taskModel)).ReturnsAsync(taskModel);

            // Act
            var result = await _taskService.UpdateImportTaskAsync(taskId, requestDto);

            var res = Assert.IsType<TaskResponseDto>(result);
            Assert.NotNull(res);
        }  
        [Fact]
        public async System.Threading.Tasks.Task ServiceUpdateTaskAsyncnull_ShouldReturnOk_WhenServiceReturnsValidResponse()
        {
            long taskId = 47;
            //Arrange
            var requestDto = new TaskRequestDto
            {
                TaskTypeId = 1,
                TaskCode = "tsk-210ddf7876234c11b64668d4246f0b44",
                TaskName = "Enroll in pre diabetes program",
                SelfReport = false,
                ConfirmReport = false,
                TaskCategoryId = 0,
                IsSubtask = false,
            };

            var taskModel = new TaskModel
            {
                TaskId = 47,
                TaskTypeId = 1,
                TaskCode = "tsk-210ddf7876234c11b64668d4246f0b44",
                TaskName = "Enroll in pre diabetes program",
                SelfReport = false,
                ConfirmReport = false,
                TaskCategoryId = 0,
                IsSubtask = false,
            };
            _taskRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskModel, bool>>>(), false));

            _taskRepository.Setup(x => x.UpdateAsync(taskModel));

            // Act
            var result = await _taskService.UpdateImportTaskAsync(taskId, requestDto);

            var res = Assert.IsType<TaskResponseDto>(result);
            Assert.NotNull(res);
        }
         [Fact]
        public async System.Threading.Tasks.Task ServiceUpdateTaskAsyncexce_ShouldReturnOk_WhenServiceReturnsValidResponse()
        {
            long taskId = 47;
            //Arrange
            var requestDto = new TaskRequestDto
            {
                TaskTypeId = 1,
                TaskCode = "tsk-210ddf7876234c11b64668d4246f0b44",
                TaskName = "Enroll in pre diabetes program",
                SelfReport = false,
                ConfirmReport = false,
                TaskCategoryId = 0,
                IsSubtask = false,
            };

            var taskModel = new TaskModel
            {
                TaskId = 47,
                TaskTypeId = 1,
                TaskCode = "tsk-210ddf7876234c11b64668d4246f0b44",
                TaskName = "Enroll in pre diabetes program",
                SelfReport = false,
                ConfirmReport = false,
                TaskCategoryId = 0,
                IsSubtask = false,
            };
            _taskRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskModel, bool>>>(), false)).ThrowsAsync(new Exception("Simulated exception"));

            _taskRepository.Setup(x => x.UpdateAsync(taskModel)); 

            // Act
            var result = await _taskService.UpdateImportTaskAsync(taskId, requestDto);

            var res = Assert.IsType<TaskResponseDto>(result);
            Assert.NotNull(res);
        }

        [Fact]
        public async System.Threading.Tasks.Task UpdateTaskAsync_ShouldReturnError_WhenServiceReturnsConflict()
        {
            long taskId = 0;

            // Arrange
            var updateTaskRequestDto = new TaskRequestDto
            {
                TaskTypeId = 1,
                TaskCode = "tsk-210ddf7876234c11b64668d4246f0b44",
                TaskName = "Enroll in pre diabetes program",
                SelfReport = false,
                ConfirmReport = false,
                TaskCategoryId = null,
                IsSubtask = false,
            };

            var taskResponseDto = new TaskResponseDto
            {
                Task = null,
                ErrorCode = StatusCodes.Status409Conflict // Simulating conflict
            };

            var _mockTaskService = new Mock<ITaskService>();

            // Setup the service to return a response with a 409 error code
            _mockTaskService
                .Setup(x => x.UpdateTaskAsync(taskId, It.IsAny<TaskRequestDto>()))
                .ReturnsAsync(taskResponseDto);

            // Create the controller with the mocked service
            _taskController = new TaskController(_taskLogger.Object, _mockTaskService.Object);

            // Act
            var result = await _taskController.UpdateTaskAsync(taskId, updateTaskRequestDto);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            Assert.Equal(StatusCodes.Status409Conflict, conflictResult.StatusCode); // Asserting the status code is 409
            Assert.Null(((TaskResponseDto)conflictResult.Value)?.Task); // Ensure no task is returned
            Assert.Equal(StatusCodes.Status409Conflict, ((TaskResponseDto)conflictResult.Value)?.ErrorCode); // Check error code in response
        }
    }
}