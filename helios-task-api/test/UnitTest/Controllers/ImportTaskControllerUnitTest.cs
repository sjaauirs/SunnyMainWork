using Amazon.SecretsManager.Model;
using AutoMapper;
using Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NHibernate.Mapping.ByCode.Impl;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Api.Controller;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Models;
using SunnyRewards.Helios.Task.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Task.Infrastructure.Services;
using SunnyRewards.Helios.Task.Infrastructure.Services.Interface;
using SunnyRewards.Helios.Task.UnitTest.Fixtures.MockDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TaskAlias = System.Threading.Tasks.Task;
using Xunit;
using static SunnyRewards.Helios.Task.Core.Domain.Dtos.TaskRewardDto;
using SunnyRewards.Helios.Task.Infrastructure.Helpers.Interface;
using SunnyRewards.Helios.Task.Infrastructure.Helpers;

namespace SunnyRewards.Helios.Task.UnitTest.Controllers
{
    public class ImportTaskControllerUnitTest
    {
        private readonly Mock<ILogger<ImportTaskController>> _mockLogger;
        private readonly Mock<ILogger<ImportTaskService>> _mockServiceLogger;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ITaskRepo> _mockTaskRepo;
        private readonly Mock<ITaskRewardRepo> _mockTaskRewardRepo;
        private readonly Mock<ITaskDetailRepo> _mockTaskDetailRepo;
        private readonly Mock<ITaskTypeRepo> _mockTaskTypeRepo;
        private readonly Mock<ITaskCategoryRepo> _mockTaskCategoryRepo;
        private readonly Mock<ITaskRewardTypeRepo> _mockTaskRewardTypeRepo;
        private readonly Mock<ITermsOfServiceRepo> _mockTermsOfServiceRepo;
        private readonly Mock<ITaskService> _mockTaskService;
        private readonly Mock<ITaskRewardService> _mockTaskRewardService;
        private readonly Mock<ITaskDetailsService> _mockTaskDetailsService;

        private readonly ImportTaskService _importTaskService;
        private readonly Mock<IImportTriviaService> _importTriviaService;
        private readonly Mock<IImportQuestionnaireService> _importQuestionnaireService;
        private readonly Mock<ITaskSubtaskMappingImportService> _taskSubtaskMappingImportService;
        private readonly ImportTaskController _controller;

        //Questionnaire Mocks
        private Mock<IQuestionnaireQuestionRepo> _mockRepo;
        private Mock<IMapper> _mockQuesMapper;
        private Mock<ILogger<QuestionnaireQuestionService>> _mockQuesLogger;
        private Mock<IQuestionnaireService> _mockQuestionnaireService;
        private QuestionnaireQuestionService _service;
        private Mock<IQuestionnaireHelper> _questionnaireHelper;

        public ImportTaskControllerUnitTest()
        {
            _mockLogger = new Mock<ILogger<ImportTaskController>>();
            _mockServiceLogger = new Mock<ILogger<ImportTaskService>>();
            _mockMapper = new Mock<IMapper>();
            _mockTaskRepo = new Mock<ITaskRepo>();
            _mockTaskRewardRepo = new Mock<ITaskRewardRepo>();
            _mockTaskDetailRepo = new Mock<ITaskDetailRepo>();
            _mockTaskService = new Mock<ITaskService>();
            _mockTaskRewardService = new Mock<ITaskRewardService>();
            _mockTaskDetailsService = new Mock<ITaskDetailsService>();
            _mockTaskRewardTypeRepo = new Mock<ITaskRewardTypeRepo>();
            _mockTaskTypeRepo = new Mock<ITaskTypeRepo>();
            _importTriviaService = new Mock<IImportTriviaService>();
            _importQuestionnaireService = new Mock<IImportQuestionnaireService>();
            _mockTaskCategoryRepo = new Mock<ITaskCategoryRepo>();
            _taskSubtaskMappingImportService = new Mock<ITaskSubtaskMappingImportService>();
            _mockTermsOfServiceRepo = new Mock<ITermsOfServiceRepo>();

            _importTaskService = new ImportTaskService(
                _mockServiceLogger.Object,
                _mockMapper.Object,
                _mockTaskRepo.Object,
                _mockTaskRewardRepo.Object,
                _mockTaskDetailsService.Object,
                _mockTaskDetailRepo.Object,
                _mockTaskService.Object,
                _mockTaskRewardService.Object, _mockTaskTypeRepo.Object, _mockTaskRewardTypeRepo.Object,
                _mockTaskCategoryRepo.Object, _taskSubtaskMappingImportService.Object, _mockTermsOfServiceRepo.Object
            );
            _controller = new ImportTaskController(_mockLogger.Object, _importTaskService, _importTriviaService.Object, _importQuestionnaireService.Object);
        }

        private void SetupQuestionnaireQuestionMocks()
        {
            _mockRepo = new Mock<IQuestionnaireQuestionRepo>();
            _mockQuesMapper = new Mock<IMapper>();
            _mockQuesLogger = new Mock<ILogger<QuestionnaireQuestionService>>();
            _mockQuestionnaireService = new Mock<IQuestionnaireService>();
            _questionnaireHelper = new Mock<IQuestionnaireHelper>(); 

            _service = new QuestionnaireQuestionService(
                _mockRepo.Object,
                _mockQuesMapper.Object,
                _mockQuesLogger.Object,
                _mockQuestionnaireService.Object,
                _questionnaireHelper.Object
            );
        }

        [Fact]
        public async void ImportTask_ShouldReturn204WhenTaskRewardDetailsIsNull()
        {
            // Arrange
            var requestDto = new ImportTaskRewardDetailsRequestDto
            {
                TaskRewardDetails = null,
                TenantCode = "Test"

            };

            // Act
            var response = await _controller.ImportTask(requestDto);

            // Assert
            var result = response as ObjectResult;

            Assert.Equal(StatusCodes.Status204NoContent, result?.StatusCode);
        }
        [Fact]
        public async void ImportTask_ShouldlogWhenTaskRewardDetailsIsNull()
        {
            // Arrange
            var requestDto = new ImportTaskRewardDetailsRequestDto
            {
                TaskRewardDetails = new List<ImportTaskRewardDetailDto> { new ImportTaskRewardDetailDto { Task = null } },
                TenantCode = "Test"

            };

            // Act
            var response = await _controller.ImportTask(requestDto);

            // Assert
            var result = response as ObjectResult;

            Assert.Null(result?.StatusCode);
        }
        [Fact]
        public async void ImportTask_ShouldlogtasknameWhenTaskRewardDetailsIsNull()
        {
            // Arrange
            var requestDto = new ImportTaskRewardDetailsRequestDto
            {
                TaskRewardDetails = new List<ImportTaskRewardDetailDto> { new ImportTaskRewardDetailDto { Task = new ExportTaskDto { Task=new TaskDto { TaskName = null },TaskTypeCode="test" } } },
                TenantCode = "Test"
            };

            // Act
            var response = await _controller.ImportTask(requestDto);

            // Assert
            var result = response as ObjectResult;

            Assert.Null(result?.StatusCode);
        }
        [Fact]
        public async void ImportTask_ShouldReturnSuccessWhenTaskIsCreated()
        {
            // Arrange
            var taskRewardDetailDto = new ImportTaskRewardDetailDto
            {
                Task = new ExportTaskDto
                {
                    Task=new TaskDto
                    {
                        TaskName = "Test Task"
                    },
                    TaskTypeCode="test"
                }

            };
            var termsOfServiceDto = new GetTaskRewardByCodeResponseMockDto().TaskRewardDetail!.TermsOfService;
            var requestDto = new ImportTaskRewardDetailsRequestDto
            {
                TaskRewardDetails = new List<ImportTaskRewardDetailDto> { taskRewardDetailDto },
                TenantCode = "Test",
                TermsOfServices = new List<TermsOfServiceDto>() { termsOfServiceDto ?? new TermsOfServiceDto() }
            };
            var taskRequestDto = new CreateTaskRequestDto() {
                TaskTypeId = 101,
                TaskCode = "TASK-001",
                TaskName = "Daily Health Check",
                SelfReport = true,
                ConfirmReport = false,
                TaskCategoryId = 5,
            };

            var task = new TaskModel { TaskId = 1, TaskCode = "TASK_001" };
            _mockTaskRepo.Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<TaskModel, bool>>>(), false));

            _mockTaskRewardRepo.Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false))
              ;

            _mockTaskService.Setup(service => service.CreateTask(It.IsAny<CreateTaskRequestDto>()))
                .ReturnsAsync(new BaseResponseDto { ErrorCode = null });

            _mockTaskTypeRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskTypeModel, bool>>>(), false)).ReturnsAsync(new TaskTypeModel() { Id = 1, TaskTypeCode = "tsc-6763067260" });

            _mockTaskRewardService.Setup(service => service.CreateTaskReward(It.IsAny<CreateTaskRewardRequestDto>()))
                .ReturnsAsync(new BaseResponseDto());

            _mockTaskDetailsService.Setup(service => service.CreateTaskDetails(It.IsAny<CreateTaskDetailsRequestDto>()))
                .ReturnsAsync(new BaseResponseDto());
            _mockMapper.Setup(m => m.Map<CreateTaskRequestDto>(It.IsAny<object>())).Returns((object source) => taskRequestDto);
            _mockMapper.Setup(m => m.Map<TermsOfServiceModel>(It.IsAny<TermsOfServiceDto>())).Returns((object source) => new TermsOfServiceModel());
            // Act
            var result = await _controller.ImportTask(requestDto);

            // Assert
            _mockTaskService.Verify(service => service.CreateTask(It.IsAny<CreateTaskRequestDto>()), Times.Once);
        }
        [Fact]
        public async void InsertTaskRewardAndDetails_ShouldReturnSuccess_WhenBothCreateTaskRewardAndCreateTaskDetailsSucceed()
        {
            // Arrange
            var taskCode = "TASK001";
            var taskRewardDetailDto = new ImportTaskRewardDetailDto
            {
               
                  TaskReward = new ExportTaskRewardDto { TaskReward=new TaskRewardDto { TaskRewardCode = "RWD001" }, TaskRewardTypeCode="test" },
                TaskDetail =new List<TaskDetailDto> { new TaskDetailDto { TaskDescription = "Sample Task Detail" } }

            };
            _mockTaskRewardTypeRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<TaskRewardTypeModel, bool>>>(), false)).ReturnsAsync(new TaskRewardTypeModel { RewardTypeCode = "test" });

            // Mock CreateTaskReward to return success
            _mockTaskRewardService
                .Setup(service => service.CreateTaskReward(It.IsAny<CreateTaskRewardRequestDto>()))
                .ReturnsAsync(new BaseResponseDto { ErrorCode = null }); // Success response

            _mockMapper.Setup(x => x.Map<PostTaskDetailsDto>(It.IsAny<TaskDetailDto>())).Returns(new PostTaskDetailsDto { TaskDetailId=1});
            // Mock CreateTaskDetails to return success
            _mockTaskDetailsService
                .Setup(service => service.CreateTaskDetails(It.IsAny<CreateTaskDetailsRequestDto>()))
                .ReturnsAsync(new BaseResponseDto { ErrorCode = null }); // Success response
            var taskRewardList = new List<ImportTaskRewardDto>();
            var termsOfServices = new List<TermsOfServiceDto>();
            // Act
            var result = await _importTaskService.InsertTaskRewardAndDetails(taskCode,1, taskRewardDetailDto, It.IsAny<string>(), 
                taskRewardList, termsOfServices);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.ErrorCode); // Success, no error
        }
        [Fact]
        public async void InsertTaskRewardAndDetails_ShouldReturnSuccess_WhenBothNotTaskRewardAndCreateTaskDetailsSucceed()
        {
            // Arrange
            var taskCode = "TASK001";
            var taskRewardDetailDto = new ImportTaskRewardDetailDto();

            // Mock CreateTaskReward to return success
            _mockTaskRewardService
                .Setup(service => service.CreateTaskReward(It.IsAny<CreateTaskRewardRequestDto>()))
                .ReturnsAsync(new BaseResponseDto { ErrorCode = null }); // Success response

            // Mock CreateTaskDetails to return success
            _mockTaskDetailsService
                .Setup(service => service.CreateTaskDetails(It.IsAny<CreateTaskDetailsRequestDto>()))
                .ReturnsAsync(new BaseResponseDto { ErrorCode = null }); // Success response
            var taskRewardList = new List<ImportTaskRewardDto>();
            var termsOfServices = new List<TermsOfServiceDto>();
            // Act
            var result = await _importTaskService.InsertTaskRewardAndDetails(taskCode, 1,taskRewardDetailDto, It.IsAny<string>(),
                taskRewardList, termsOfServices);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.ErrorCode); // Success, no error
        }
        [Fact]
        public async void ImportTask_ShouldUpdateTaskWhenTaskExists()
        {
            // Arrange
            var taskRewardDetailDto = new ImportTaskRewardDetailDto
            {
                Task = new ExportTaskDto
                {
                    Task = new TaskDto
                    {
                        TaskName = "Test Task"
                    },
                    TaskTypeCode = "test",
                    TaskCategoryCode=null,
                },
                TaskReward = new ExportTaskRewardDto { TaskReward = new TaskRewardDto { TaskRewardCode = "RWD001" }, TaskRewardTypeCode = "test" },
                TaskDetail = new List<TaskDetailDto> { new TaskDetailDto { TaskDescription = "Sample Task Detail" } }

            };
            var termsOfServiceDto = new GetTaskRewardByCodeResponseMockDto().TaskRewardDetail!.TermsOfService;

            var requestDto = new ImportTaskRewardDetailsRequestDto
            {
                TaskRewardDetails = new List<ImportTaskRewardDetailDto> { taskRewardDetailDto },
                TenantCode = "Test",
                TermsOfServices = new List<TermsOfServiceDto>() { termsOfServiceDto ?? new TermsOfServiceDto() }
            };
            requestDto.TenantCode = "Test";
            var task = new TaskModel { TaskId = 1, TaskCode = "test" };


            TaskRequestDto taskRequestDto = new TaskRequestDto { TaskCode = "test" };
            _mockMapper.Setup(x => x.Map<TaskRequestDto>(It.IsAny<TaskModel>())).Returns(taskRequestDto);
            _mockTaskRepo.Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<TaskModel, bool>>>(), false))
                .ReturnsAsync(new List<TaskModel> { task });
            _mockTaskTypeRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<TaskTypeModel, bool>>>(), false))
               .ReturnsAsync(new TaskTypeModel { TaskTypeCode = "test", TaskTypeId = 1 });
            _mockTaskCategoryRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<TaskCategoryModel, bool>>>(), false))
                 .ReturnsAsync(new TaskCategoryModel { TaskCategoryCode = "test", TaskCategoryId = 1 });
            _mockTaskRewardRepo.Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false))
                .ReturnsAsync(new List<TaskRewardModel> { new TaskRewardModel { TaskRewardId = 1, TaskExternalCode = "test" } });
            _mockTaskService.Setup(service => service.UpdateImportTaskAsync(It.IsAny<long>(), taskRequestDto))
                .ReturnsAsync(new TaskResponseDto { Task = new TaskDto { TaskId = 1, TaskCode = "test" } });
            _mockMapper.Setup(x => x.Map<TaskRequestDto>(It.IsAny<TaskDto>())).Returns(taskRequestDto);
            _mockMapper.Setup(m => m.Map<TermsOfServiceModel>(It.IsAny<TermsOfServiceDto>())).Returns((object source) => new TermsOfServiceModel());
            _mockTermsOfServiceRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TermsOfServiceModel, bool>>>(), false)).ReturnsAsync(new TermsOfServiceModel { TermsOfServiceId = 1 });


            TaskRewardRequestDto UpdateTaskRewardRequestDto = new TaskRewardRequestDto
            {
                TaskExternalCode = "test"
            };
            _mockMapper.Setup(x => x.Map<TaskRewardRequestDto>(It.IsAny<TaskRewardDto>())).Returns(UpdateTaskRewardRequestDto);
            TaskDetailRequestDto UpdateTasdetRequestDto = new TaskDetailRequestDto
            {
                TaskDescription = "test"
            };
            _mockMapper.Setup(x => x.Map<TaskDetailRequestDto>(It.IsAny<TaskDetailDto>())).Returns(UpdateTasdetRequestDto);

            _mockTaskRewardService.Setup(service => service.UpdateTaskRewardAsync(It.IsAny<long>(), It.IsAny<TaskRewardRequestDto>(), false))
                .ReturnsAsync(new TaskRewardResponseDto());

            _mockTaskDetailsService.Setup(service => service.UpdateTaskDetailAsync(It.IsAny<long>(), It.IsAny<TaskDetailRequestDto>()))
                .ReturnsAsync(new TaskDetailResponseDto());

            // Act
            var result = await _importTaskService.ImportTask(requestDto);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.ErrorCode);
        }
        [Fact]
        public async void ImportTask_ShouldUpdateTaskandSkipTaskExists()
        {
            // Arrange
            var taskRewardDetailDto = new ImportTaskRewardDetailDto
            {
                Task = new ExportTaskDto
                {
                    Task = new TaskDto
                    {
                        TaskName = "Test Task"
                    },
                    TaskTypeCode = "test",
                    TaskCategoryCode= "test",
                },
                TaskReward = new ExportTaskRewardDto { TaskReward = new TaskRewardDto { TaskRewardCode = "RWD001" }, TaskRewardTypeCode = "test" },
                TaskDetail =new List<TaskDetailDto> { new TaskDetailDto { TaskDescription = "Sample Task Detail" } }

            };
            var requestDto = new ImportTaskRewardDetailsRequestDto
            {
                TaskRewardDetails = new List<ImportTaskRewardDetailDto> { taskRewardDetailDto },
                TenantCode = "Test"

            };
            requestDto.TenantCode = "Test";
            var task = new TaskModel { TaskId = 1, TaskCode = "Test" };

            TaskRequestDto taskRequestDto = new TaskRequestDto { TaskCode = "Test" };
            _mockMapper.Setup(x => x.Map<TaskRequestDto>(It.IsAny<TaskModel>())).Returns(taskRequestDto);

            _mockTaskRepo.Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<TaskModel, bool>>>(), false))
                 .ReturnsAsync(new List<TaskModel> { task });
            _mockTaskTypeRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<TaskTypeModel, bool>>>(), false))
                 .ReturnsAsync(new TaskTypeModel  { TaskTypeCode="test" ,TaskTypeId=1});
            _mockTaskCategoryRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<TaskCategoryModel, bool>>>(), false))
                 .ReturnsAsync(new TaskCategoryModel { TaskCategoryCode="test",TaskCategoryId=1 });
            _mockTaskRewardRepo.Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false))
                .ReturnsAsync(new List<TaskRewardModel> { new TaskRewardModel { TaskRewardId = 1, TaskExternalCode = "test" } });
            _mockTaskService.Setup(service => service.UpdateImportTaskAsync(It.IsAny<long>(), taskRequestDto))
                .ReturnsAsync(new TaskResponseDto { Task = new TaskDto { TaskId = 1, TaskCode = "Test" } });
            TaskRewardRequestDto UpdateTaskRewardRequestDto = new TaskRewardRequestDto
            {
                TaskExternalCode = "test"
            };
            _mockMapper.Setup(x => x.Map<TaskRewardRequestDto>(It.IsAny<TaskRewardDto>())).Returns(UpdateTaskRewardRequestDto);
            TaskDetailRequestDto UpdateTasdetRequestDto = new TaskDetailRequestDto
            {
                TaskDescription = "test"
            };
            _mockMapper.Setup(x => x.Map<TaskDetailRequestDto>(It.IsAny<TaskDetailDto>())).Returns(UpdateTasdetRequestDto);
            _mockMapper.Setup(x => x.Map<TaskRequestDto>(It.IsAny<TaskDto>())).Returns(taskRequestDto);

            _mockTaskRewardService.Setup(service => service.UpdateTaskRewardAsync(It.IsAny<long>(), It.IsAny<TaskRewardRequestDto>(), false))
                .ReturnsAsync(new TaskRewardResponseDto());

            _mockTaskDetailsService.Setup(service => service.UpdateTaskDetailAsync(It.IsAny<long>(), It.IsAny<TaskDetailRequestDto>()))
                .ReturnsAsync(new TaskDetailResponseDto());

            // Act
            var result = await _importTaskService.ImportTask(requestDto);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.ErrorCode);
        }
        [Fact]
        public async void ImportTask_ShouldSkipcategoryTaskandSkipTaskExists()
        {
            // Arrange
            var taskRewardDetailDto = new ImportTaskRewardDetailDto
            {
                Task = new ExportTaskDto
                {
                    Task = new TaskDto
                    {
                        TaskName = "Test Task"
                    },
                    TaskTypeCode = "test",
                    TaskCategoryCode= "test",
                },
                TaskReward = new ExportTaskRewardDto { TaskReward = new TaskRewardDto { TaskRewardCode = "RWD001" }, TaskRewardTypeCode = "test" },
                TaskDetail =new List<TaskDetailDto> { new TaskDetailDto { TaskDescription = "Sample Task Detail" } }

            };
            var requestDto = new ImportTaskRewardDetailsRequestDto
            {
                TaskRewardDetails = new List<ImportTaskRewardDetailDto> { taskRewardDetailDto },
                TenantCode = "Test"

            };
            requestDto.TenantCode = "Test";
            var task = new TaskModel { TaskId = 1, TaskCode = "Test" };

            TaskRequestDto taskRequestDto = new TaskRequestDto { TaskCode = "Test" };
            _mockMapper.Setup(x => x.Map<TaskRequestDto>(It.IsAny<TaskModel>())).Returns(taskRequestDto);

            _mockTaskRepo.Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<TaskModel, bool>>>(), false))
                 .ReturnsAsync(new List<TaskModel> { task });
            _mockTaskTypeRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<TaskTypeModel, bool>>>(), false))
                 .ReturnsAsync(new TaskTypeModel  { TaskTypeCode="test" ,TaskTypeId=1});
            _mockTaskCategoryRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<TaskCategoryModel, bool>>>(), false))
               ;
            _mockTaskRewardRepo.Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false))
                .ReturnsAsync(new List<TaskRewardModel> { new TaskRewardModel { TaskRewardId = 1, TaskExternalCode = "test" } });
            _mockTaskService.Setup(service => service.UpdateImportTaskAsync(It.IsAny<long>(), taskRequestDto))
                .ReturnsAsync(new TaskResponseDto { Task = new TaskDto { TaskId = 1, TaskCode = "Test" } });
            TaskRewardRequestDto UpdateTaskRewardRequestDto = new TaskRewardRequestDto
            {
                TaskExternalCode = "test"
            };
            _mockMapper.Setup(x => x.Map<TaskRewardRequestDto>(It.IsAny<TaskRewardDto>())).Returns(UpdateTaskRewardRequestDto);
            TaskDetailRequestDto UpdateTasdetRequestDto = new TaskDetailRequestDto
            {
                TaskDescription = "test"
            };
            _mockMapper.Setup(x => x.Map<TaskDetailRequestDto>(It.IsAny<TaskDetailDto>())).Returns(UpdateTasdetRequestDto);
            _mockMapper.Setup(x => x.Map<TaskRequestDto>(It.IsAny<TaskDto>())).Returns(taskRequestDto);

            _mockTaskRewardService.Setup(service => service.UpdateTaskRewardAsync(It.IsAny<long>(), It.IsAny<TaskRewardRequestDto>(), false))
                .ReturnsAsync(new TaskRewardResponseDto());

            _mockTaskDetailsService.Setup(service => service.UpdateTaskDetailAsync(It.IsAny<long>(), It.IsAny<TaskDetailRequestDto>()))
                .ReturnsAsync(new TaskDetailResponseDto());

            // Act
            var result = await _importTaskService.ImportTask(requestDto);

            // Assert
            Assert.Null(result.ErrorCode);
        }
        [Fact]
        public async void ImportTask_ShouldInsertRewardTaskWhenTaskExists()
        {
            // Arrange
            // Arrange
            var taskRewardDetailDto = new ImportTaskRewardDetailDto
            {
                Task = new ExportTaskDto
                {
                    Task = new TaskDto
                    {
                        TaskName = "Test Task",
                        TaskTypeId = 1
                    },
                    TaskTypeCode = "test"
                },
                TaskReward = new ExportTaskRewardDto { TaskReward = new TaskRewardDto { TaskRewardCode = "RWD001" }, TaskRewardTypeCode = "test" },
                TaskDetail =new List<TaskDetailDto> { new TaskDetailDto { TaskDescription = "Sample Task Detail" } }

            };
            var requestDto = new ImportTaskRewardDetailsRequestDto
            {
                TaskRewardDetails = new List<ImportTaskRewardDetailDto> { taskRewardDetailDto },
                TenantCode = "Test"

            };
            TaskRequestDto taskRequestDto = GetTaskRequestDto();
            requestDto.TenantCode = "Test";
            var task = new TaskModel { TaskId = 1, TaskCode = "TASK_001", TaskTypeId = 1 };
            _mockTaskRepo.Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<TaskModel, bool>>>(), false))
                  .ReturnsAsync(new List<TaskModel> { task });

            _mockTaskTypeRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskTypeModel, bool>>>(), false)).ReturnsAsync(new TaskTypeModel() { Id = 1, TaskTypeCode = "tsc-6763067260" });

            _mockMapper.Setup(x => x.Map<TaskRequestDto>(It.IsAny<TaskModel>())).Returns(taskRequestDto);

            _mockTaskRewardRepo.Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false))
               ;
            _mockTaskService.Setup(service => service.UpdateImportTaskAsync(It.IsAny<long>(), taskRequestDto))
                .ReturnsAsync(new TaskResponseDto { Task = new TaskDto { TaskId = 1, TaskCode = "TASK_001" } });
            TaskRewardRequestDto UpdateTaskRewardRequestDto = new TaskRewardRequestDto
            {
                TaskExternalCode = "test"
            };
            _mockMapper.Setup(x => x.Map<TaskRewardRequestDto>(It.IsAny<TaskRewardDto>())).Returns(UpdateTaskRewardRequestDto);
            _mockMapper.Setup(m => m.Map<TaskRequestDto>(It.IsAny<object>()))
           .Returns((object source) => taskRequestDto);
            CreateTaskDetailsRequestDto UpdateTasdetRequestDto = new CreateTaskDetailsRequestDto
            {
                TaskDetail = new PostTaskDetailsDto { TaskDescription = "test" }
            };
            _mockMapper.Setup(x => x.Map<CreateTaskDetailsRequestDto>(It.IsAny<TaskDetailDto>())).Returns(UpdateTasdetRequestDto);

            _mockTaskRewardService.Setup(service => service.UpdateTaskRewardAsync(It.IsAny<long>(), It.IsAny<TaskRewardRequestDto>(), false))
                .ReturnsAsync(new TaskRewardResponseDto());

            _mockTaskDetailsService.Setup(service => service.UpdateTaskDetailAsync(It.IsAny<long>(), It.IsAny<TaskDetailRequestDto>()))
                .ReturnsAsync(new TaskDetailResponseDto());
            _mockTaskRewardTypeRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskRewardTypeModel, bool>>>(), false))
                .ReturnsAsync(new TaskRewardTypeModel() { RewardTypeCode = "test" });


            // Act
            var result = await _importTaskService.ImportTask(requestDto);

            // Assert
            Assert.Null(result.ErrorCode);
        }

        private static TaskRequestDto GetTaskRequestDto()
        {
            return new TaskRequestDto
            {
                TaskTypeId = 101,
                TaskCode = "TASK-001",
                TaskName = "Daily Health Check",
                SelfReport = true,
                ConfirmReport = false,
                TaskCategoryId = 5,
                IsSubtask = false,
                UpdateUser = "admin_user"
            };
        }

        [Fact]
        public async void ImportTask_ShouldUpdateTaskWhenexception()
        {
            // Arrange
            var taskRewardDetailDto = new ImportTaskRewardDetailDto
            {
                Task = new ExportTaskDto
                {
                    Task = new TaskDto
                    {
                        TaskName = "Test Task"
                    },
                    TaskTypeCode = "test"
                },
                TaskReward = new ExportTaskRewardDto { TaskReward = new TaskRewardDto { TaskRewardCode = "RWD001" }, TaskRewardTypeCode = "test" },
                TaskDetail =new List<TaskDetailDto> { new TaskDetailDto { TaskDescription = "Sample Task Detail" } }

            };
            var requestDto = new ImportTaskRewardDetailsRequestDto
            {
                TaskRewardDetails = new List<ImportTaskRewardDetailDto> { taskRewardDetailDto },
                TenantCode = "Test"

            };

            var task = new TaskModel { TaskId = 1, TaskCode = "TASK_001" };
            _mockTaskRepo.Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<TaskModel, bool>>>(), false))
.ThrowsAsync(new Exception("Simulated exception"));
            TaskRequestDto taskRequestDto = new TaskRequestDto { TaskCode = "TASK_001" };
            _mockMapper.Setup(x => x.Map<TaskRequestDto>(It.IsAny<TaskModel>())).Returns(taskRequestDto); ;


            _mockTaskRewardRepo.Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false))
                .ReturnsAsync(new List<TaskRewardModel> { new TaskRewardModel { TaskRewardId = 1, TaskExternalCode = "test" } });
            _mockTaskService.Setup(service => service.UpdateImportTaskAsync(It.IsAny<long>(), taskRequestDto))
                .ReturnsAsync(new TaskResponseDto { Task = new TaskDto { TaskId = 1, TaskCode = "TASK_001" } });

            _mockTaskRewardService.Setup(service => service.UpdateTaskRewardAsync(It.IsAny<long>(), It.IsAny<TaskRewardRequestDto>(), false))
                .ReturnsAsync(new TaskRewardResponseDto());

            _mockTaskDetailsService.Setup(service => service.UpdateTaskDetailAsync(It.IsAny<long>(), It.IsAny<TaskDetailRequestDto>()))
                .ReturnsAsync(new TaskDetailResponseDto());

            // Act
            var result = await _controller.ImportTask(requestDto);
            var response = result as ObjectResult;
            // Assert
            Assert.True(response?.StatusCode == 500);
        }
        [Fact]
        public async void ImportTask_ShouldincrementCountWhenTaskIsNotCreated()
        {
            // Arrange
            var taskRewardDetailDto = new ImportTaskRewardDetailDto
            {
                Task = new ExportTaskDto
                {
                    Task = new TaskDto
                    {
                        TaskName = "Test Task"
                    },
                    TaskTypeCode = "test"
                },
                TaskReward = new ExportTaskRewardDto { TaskReward = new TaskRewardDto { TaskRewardCode = "RWD001" }, TaskRewardTypeCode = "test" },
                TaskDetail =new List<TaskDetailDto> { new TaskDetailDto { TaskDescription = "Sample Task Detail" } }

            };
            var requestDto = new ImportTaskRewardDetailsRequestDto
            {
                TaskRewardDetails = new List<ImportTaskRewardDetailDto> { taskRewardDetailDto },
                TenantCode = "Test"

            };
            TaskRequestDto taskRequestDto = new TaskRequestDto { TaskCode = "TASK_001" };
            _mockMapper.Setup(x => x.Map<TaskRequestDto>(It.IsAny<TaskModel>())).Returns(taskRequestDto);

            var task = new TaskModel { TaskId = 1, TaskCode = "TASK_001" };
            _mockTaskRepo.Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<TaskModel, bool>>>(), false))
                .ReturnsAsync(new List<TaskModel> { new TaskModel { TaskName = "Test Task" } });


            _mockTaskRewardRepo.Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false))
              ;

            _mockTaskService.Setup(service => service.UpdateImportTaskAsync(It.IsAny<long>(), It.IsAny<TaskRequestDto>())).ReturnsAsync(new TaskResponseDto { Task = new TaskDto { TaskCode = "TASK_001" } })
               ;



            _mockTaskRewardService.Setup(service => service.CreateTaskReward(It.IsAny<CreateTaskRewardRequestDto>()))
               .ReturnsAsync(new BaseResponseDto { ErrorCode = 3 });

            _mockTaskDetailsService.Setup(service => service.CreateTaskDetails(It.IsAny<CreateTaskDetailsRequestDto>()))
                .ReturnsAsync(new BaseResponseDto());

            // Act
            var result = await _importTaskService.ImportTask(requestDto);

            // Assert
            _mockTaskService.Verify(service => service.UpdateImportTaskAsync(It.IsAny<long>(), It.IsAny<TaskRequestDto>()), Times.Once);
        }
        [Fact]
        public async void ImportTask_ShouldincrementCountWhenTaskIsNotUpdated()
        {
            // Arrange
            // Arrange
            var taskRewardDetailDto = new ImportTaskRewardDetailDto
            {
                Task = new ExportTaskDto
                {
                    Task = new TaskDto
                    {
                        TaskName = "Test Task"
                    },
                    TaskTypeCode = "test"
                },
                TaskReward = new ExportTaskRewardDto { TaskReward = new TaskRewardDto { TaskRewardCode = "RWD001" }, TaskRewardTypeCode = "test" },
                TaskDetail =new List<TaskDetailDto> { new TaskDetailDto { TaskDescription = "Sample Task Detail" } }

            };
            var requestDto = new ImportTaskRewardDetailsRequestDto
            {
                TaskRewardDetails = new List<ImportTaskRewardDetailDto> { taskRewardDetailDto },
                TenantCode = "Test"

            };
            TaskRequestDto taskRequestDto = new TaskRequestDto { TaskCode = "TASK_001" };
            _mockMapper.Setup(x => x.Map<TaskRequestDto>(It.IsAny<TaskModel>())).Returns(taskRequestDto);

            var task = new TaskModel { TaskId = 1, TaskCode = "TASK_001" };
            _mockTaskRepo.Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<TaskModel, bool>>>(), false)).ReturnsAsync(new List<TaskModel> { new TaskModel { TaskName = "Test Task" } });

            _mockTaskRewardRepo.Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false))
                .ReturnsAsync(new List<TaskRewardModel> { new TaskRewardModel { TaskRewardId = 1, TaskExternalCode = "test" } });

            _mockTaskService.Setup(service => service.UpdateImportTaskAsync(It.IsAny<long>(), It.IsAny<TaskRequestDto>())).ReturnsAsync(new TaskResponseDto { Task = new TaskDto { TaskCode = "TASK_001" } })
               ;

            TaskRewardRequestDto UpdateTaskRewardRequestDto = new TaskRewardRequestDto
            {
                TaskExternalCode = "test"
            };

            _mockMapper.Setup(x => x.Map<TaskRewardRequestDto>(It.IsAny<TaskRewardDto>())).Returns(UpdateTaskRewardRequestDto);
            TaskDetailRequestDto UpdateTasdetRequestDto = new TaskDetailRequestDto
            {
                TaskDescription = "test"
            };
            _mockMapper.Setup(x => x.Map<TaskDetailRequestDto>(It.IsAny<TaskDetailDto>())).Returns(UpdateTasdetRequestDto);

            _mockTaskRewardService.Setup(service => service.UpdateTaskRewardAsync(It.IsAny<long>(), It.IsAny<TaskRewardRequestDto>(), false))
               .ReturnsAsync(new TaskRewardResponseDto { ErrorCode = 3 });

            _mockTaskDetailsService.Setup(service => service.CreateTaskDetails(It.IsAny<CreateTaskDetailsRequestDto>()))
                .ReturnsAsync(new BaseResponseDto());

            // Act
            var result = await _importTaskService.ImportTask(requestDto);

            // Assert
            _mockTaskService.Verify(service => service.UpdateImportTaskAsync(It.IsAny<long>(), It.IsAny<TaskRequestDto>()), Times.Once);
        }
        [Fact]
        public async void ImportTask_ShouldincrementCountWhenTaskIscreatedUpdated()
        {
            // Arrange
            // Arrange
            var taskRewardDetailDto = new ImportTaskRewardDetailDto
            {
                Task = new ExportTaskDto
                {
                    Task = new TaskDto
                    {
                        TaskName = "Test Task"
                    },
                    TaskTypeCode = "test"
                },
                TaskReward = new ExportTaskRewardDto { TaskReward = new TaskRewardDto { TaskRewardCode = "RWD001" }, TaskRewardTypeCode = "test" },
                TaskDetail =new List<TaskDetailDto> { new TaskDetailDto { TaskDescription = "Sample Task Detail" } }

            };
            var requestDto = new ImportTaskRewardDetailsRequestDto
            {
                TaskRewardDetails = new List<ImportTaskRewardDetailDto> { taskRewardDetailDto },
                TenantCode = "Test"

            };


            var task = new TaskModel { TaskId = 1, TaskCode = "TASK_001" };
            CreateTaskRequestDto taskRequestDto = new CreateTaskRequestDto { TaskCode = "TASK_001" };
            _mockMapper.Setup(x => x.Map<CreateTaskRequestDto>(It.IsAny<TaskDto>())).Returns(taskRequestDto);
            _mockTaskRepo.SetupSequence(repo => repo.FindAsync(It.IsAny<Expression<Func<TaskModel, bool>>>(), false))
    .ReturnsAsync((List<TaskModel>)null)   // First call returns null
    .ReturnsAsync(new List<TaskModel> { task });
            _mockTaskRewardRepo.Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false))
                .ReturnsAsync(new List<TaskRewardModel> { new TaskRewardModel { TaskRewardId = 1, TaskExternalCode = "test" } });

            _mockTaskService.Setup(service => service.CreateTask(It.IsAny<CreateTaskRequestDto>()))
               .ReturnsAsync(new BaseResponseDto { ErrorCode = null });



            _mockTaskRewardService.Setup(service => service.CreateTaskReward(It.IsAny<CreateTaskRewardRequestDto>()))
                .ReturnsAsync(new BaseResponseDto());
            CreateTaskRewardRequestDto UpdateTaskRewardRequestDto = new CreateTaskRewardRequestDto
            {
                TaskReward = new TaskRewardDto { TaskExternalCode = "test" }
            };

            TaskDetailRequestDto UpdateTasdetRequestDto = new TaskDetailRequestDto
            {
                TaskDescription = "test"
            };
            _mockTaskRewardService.Setup(service => service.CreateTaskReward(It.IsAny<CreateTaskRewardRequestDto>()))
               .ReturnsAsync(new TaskRewardResponseDto { ErrorCode = 3 });

            _mockTaskDetailsService.Setup(service => service.CreateTaskDetails(It.IsAny<CreateTaskDetailsRequestDto>()))
                .ReturnsAsync(new BaseResponseDto());

            // Act
            var result = await _importTaskService.ImportTask(requestDto);

            // Assert
            _mockTaskService.Verify(service => service.CreateTask(It.IsAny<CreateTaskRequestDto>()), Times.Once);
        }
        [Fact]
        public async void ImportTask_ShouldincrementCountWhenTaskCreatedButnotreward()
        {
            // Arrange
            var taskRewardDetailDto = new ImportTaskRewardDetailDto
            {
                Task = new ExportTaskDto
                {
                    Task = new TaskDto
                    {
                        TaskName = "Test Task"
                    },
                    TaskTypeCode = "test"
                },
                TaskReward = new ExportTaskRewardDto { TaskReward = new TaskRewardDto { TaskRewardCode = "RWD001" }, TaskRewardTypeCode = "test" },
                TaskDetail =new List<TaskDetailDto> { new TaskDetailDto { TaskDescription = "Sample Task Detail" } }

            };
            var requestDto = new ImportTaskRewardDetailsRequestDto
            {
                TaskRewardDetails = new List<ImportTaskRewardDetailDto> { taskRewardDetailDto },
                TenantCode = "Test"

            };

            var task = new TaskModel { TaskId = 1, TaskCode = "TASK_001" };
            _mockTaskRepo.Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<TaskModel, bool>>>(), false));
            CreateTaskRequestDto taskRequestDto = new CreateTaskRequestDto { TaskCode = "TASK_001" };
            _mockMapper.Setup(x => x.Map<CreateTaskRequestDto>(It.IsAny<TaskDto>())).Returns(taskRequestDto);
            _mockTaskRewardRepo.Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false))
              ;

            _mockTaskService.Setup(service => service.CreateTask(It.IsAny<CreateTaskRequestDto>()))
                .ReturnsAsync(new BaseResponseDto { ErrorCode = null });



            _mockTaskRewardService.Setup(service => service.CreateTaskReward(It.IsAny<CreateTaskRewardRequestDto>()))
                ;

            _mockTaskDetailsService.Setup(service => service.CreateTaskDetails(It.IsAny<CreateTaskDetailsRequestDto>()))
                .ReturnsAsync(new BaseResponseDto());

            // Act
            var result = await _importTaskService.ImportTask(requestDto);

            // Assert
            _mockTaskService.Verify(service => service.CreateTask(It.IsAny<CreateTaskRequestDto>()), Times.Once);
        }
        [Fact]
        public async void ImportTrivia_ShouldReturn200_WhenImportIsSuccessful()
        {
            // Arrange
            var requestDto = new ImportTriviaRequestDto
            {
                TenantCode = "Tenant123",
                TriviaDetailDto = new ImportTriviaDetailDto
                {
                    Trivia = new List<ExportTriviaDto>
                { new ExportTriviaDto{
                   Trivia= new TriviaDto {  TriviaId = 1 },
                    TaskExternalCode = "Reward123" } }
                }
            }; var successResponse = new BaseResponseDto { ErrorCode = null };

            _importTriviaService.Setup(service => service.ImportTrivia(requestDto))
                                    .ReturnsAsync(successResponse);

            // Act
            var result = await _controller.ImportTrivia(requestDto) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }

        [Fact]
        public async void ImportTrivia_ShouldReturnErrorStatusCode_WhenServiceReturnsError()
        {
            // Arrange
            var requestDto = new ImportTriviaRequestDto
            {
                TenantCode = "Tenant123",
                TriviaDetailDto = new ImportTriviaDetailDto
                {
                    Trivia = new List<ExportTriviaDto>
                { new ExportTriviaDto{
                   Trivia= new TriviaDto {  TriviaId = 1 },
                     TaskExternalCode = "Reward123" } }
                }
            }; var errorResponse = new BaseResponseDto { ErrorCode = StatusCodes.Status400BadRequest };

            _importTriviaService.Setup(service => service.ImportTrivia(requestDto))
                                    .ReturnsAsync(errorResponse);

            // Act
            var result = await _controller.ImportTrivia(requestDto) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        }

        [Fact]
        public async void ImportTrivia_ShouldReturn500_WhenExceptionOccurs()
        {
            // Arrange
            var requestDto = new ImportTriviaRequestDto
            {
                TenantCode = "Tenant123",
                TriviaDetailDto = new ImportTriviaDetailDto
                {
                    Trivia = new List<ExportTriviaDto>
                { new ExportTriviaDto{
                   Trivia= new TriviaDto {  TriviaId = 1 },
                    TaskExternalCode = "Reward123" } }
                }
            };
            _importTriviaService.Setup(service => service.ImportTrivia(requestDto))
                                    .ThrowsAsync(new Exception("Unexpected error"));

            // Act
            var result = await _controller.ImportTrivia(requestDto) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, result.StatusCode);
           
        }

        [Fact]
        public async void ImportQuestionnaire_ShouldReturn200_WhenImportIsSuccessful()
        {
            // Arrange
            var requestDto = new ImportQuestionnaireRequestDto
            {
                TenantCode = "Tenant123",
                QuestionnaireDetailDto = new ImportQuestionnaireDetailDto
                {
                    Questionnaire = new List<ExportQuestionnaireDto>
                { new ExportQuestionnaireDto{
                   Questionnaire= new QuestionnaireDto { QuestionnaireId = 1 },
                    TaskExternalCode = "Reward123" } }
                }
            }; var successResponse = new BaseResponseDto { ErrorCode = null };

            _importQuestionnaireService.Setup(service => service.ImportQuestionnaire(requestDto))
                                    .ReturnsAsync(successResponse);

            // Act
            var result = await _controller.ImportQuestionnaire(requestDto) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }

        [Fact]
        public async void ImportQuestionnaire_ShouldReturnErrorStatusCode_WhenServiceReturnsError()
        {
            // Arrange
            var requestDto = new ImportQuestionnaireRequestDto
            {
                TenantCode = "Tenant123",
                QuestionnaireDetailDto = new ImportQuestionnaireDetailDto
                {
                    Questionnaire = new List<ExportQuestionnaireDto>
                { new ExportQuestionnaireDto{
                   Questionnaire= new QuestionnaireDto {  QuestionnaireId = 1 },
                     TaskExternalCode = "Reward123" } }
                }
            }; var errorResponse = new BaseResponseDto { ErrorCode = StatusCodes.Status400BadRequest };

            _importQuestionnaireService.Setup(service => service.ImportQuestionnaire(requestDto))
                                    .ReturnsAsync(errorResponse);

            // Act
            var result = await _controller.ImportQuestionnaire(requestDto) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        }

        [Fact]
        public async void ImportQuestionnaire_ShouldReturn500_WhenExceptionOccurs()
        {
            // Arrange
            var requestDto = new ImportQuestionnaireRequestDto
            {
                TenantCode = "Tenant123",
                QuestionnaireDetailDto = new ImportQuestionnaireDetailDto
                {
                    Questionnaire = new List<ExportQuestionnaireDto>
                { new ExportQuestionnaireDto{
                   Questionnaire= new QuestionnaireDto {  QuestionnaireId = 1 },
                    TaskExternalCode = "Reward123" } }
                }
            };
            _importQuestionnaireService.Setup(service => service.ImportQuestionnaire(requestDto))
                                    .ThrowsAsync(new Exception("Unexpected error"));

            // Act
            var result = await _controller.ImportQuestionnaire(requestDto) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, result.StatusCode);

        }

        [Fact]
        public async TaskAlias GetAllQuestionnaireQuestions_ReturnsMappedQuestions()
        {
            // Arrange
            SetupQuestionnaireQuestionMocks();
            var questions = new List<QuestionnaireQuestionModel>
            {
                new QuestionnaireQuestionModel { QuestionnaireJson = "{\"en\":\"test\"}", DeleteNbr = 0 }
            };
            _mockRepo.Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<QuestionnaireQuestionModel, bool>>>(), false))
                .ReturnsAsync(questions);
            _questionnaireHelper.Setup(s => s.FilterQuestionnaireJsonByLanguage(It.IsAny<string>(), It.IsAny<string>()))
                .Returns("{\"en\":\"test\"}");
            _mockQuesMapper.Setup(m => m.Map<List<QuestionnaireQuestionData>>(questions))
                .Returns(new List<QuestionnaireQuestionData>
                {
                    new QuestionnaireQuestionData
                    {
                        QuestionnaireQuestionCode = "SampleCode", 
                        QuestionExternalCode = "SampleExternalCode" 
                    }
                });

            // Act
            var result = await _service.GetAllQuestionnaireQuestions("en");

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.QuestionnaireQuestions);
            Assert.Single(result.QuestionnaireQuestions);
        }
        [Fact]
        public async TaskAlias UpdateQuestionnaireQuestion_ReturnsBadRequest_WhenCodeMismatch()
        {

            // Arrange
            SetupQuestionnaireQuestionMocks();
            var dto = new QuestionnaireQuestionData { QuestionnaireQuestionCode = "code1", QuestionExternalCode = "SampleExternalCode" };
            // Act
            var result = await _service.UpdateQuestionnaireQuestion("code2", dto);

            // Assert
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
            Assert.False(result.IsSuccess);
        }

        [Fact]
        public async TaskAlias UpdateQuestionnaireQuestion_ReturnsNotFound_WhenNotExists()
        {
            // Arrange
            SetupQuestionnaireQuestionMocks();
            var dto = new QuestionnaireQuestionData { QuestionnaireQuestionCode = "code1", QuestionExternalCode = "SampleExternalCode" };
            _mockRepo.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<QuestionnaireQuestionModel, bool>>>(), false))
                .ReturnsAsync((QuestionnaireQuestionModel)null);

            // Act
            var result = await _service.UpdateQuestionnaireQuestion("code1", dto);

            // Assert
            Assert.Equal(StatusCodes.Status404NotFound, result.ErrorCode);
            Assert.False(result.IsSuccess);
        }

        [Fact]
        public async TaskAlias UpdateQuestionnaireQuestion_UpdatesAndReturnsSuccess()
        {
            // Arrange
            SetupQuestionnaireQuestionMocks();
            var model = new QuestionnaireQuestionModel { QuestionnaireQuestionCode = "code1", DeleteNbr = 0 };
            var dto = new QuestionnaireQuestionData
            {
                QuestionnaireQuestionCode = "code1",
                QuestionnaireJson = "{\"en\":\"test\"}",
                QuestionExternalCode = "ext",
                UpdateUser = "user"
            };

            _mockRepo.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<QuestionnaireQuestionModel, bool>>>(), false))
                .ReturnsAsync(model);

            _mockRepo.Setup(r => r.UpdateAsync(model))
                .ReturnsAsync(model); 

            // Act
            var result = await _service.UpdateQuestionnaireQuestion("code1", dto);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Null(result.ErrorCode);
        }


        [Fact]
        public async TaskAlias UpdateQuestionnaireQuestion_ReturnsError_OnException()
        {
            // Arrange
            SetupQuestionnaireQuestionMocks();
            var dto = new QuestionnaireQuestionData { QuestionnaireQuestionCode = "code1", QuestionExternalCode = "sampleExternalCode" };
            _mockRepo.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<QuestionnaireQuestionModel, bool>>>(), false))
                .ThrowsAsync(new Exception("fail"));

            // Act
            var result = await _service.UpdateQuestionnaireQuestion("code1", dto);

            // Assert
            Assert.Equal(StatusCodes.Status500InternalServerError, result.ErrorCode);
            Assert.False(result.IsSuccess);
        }

        [Fact]
        public async TaskAlias CreateQuestionnaireQuestion_ReturnsNotFound_WhenDtoNull()
        {
            // Act
            SetupQuestionnaireQuestionMocks();
            var result = await _service.CreateQuestionnaireQuestion(null);

            // Assert
            Assert.Equal(StatusCodes.Status404NotFound, result.ErrorCode);
        }

        [Fact]
        public async TaskAlias CreateQuestionnaireQuestion_ReturnsConflict_WhenExists()
        {
            // Arrange
            SetupQuestionnaireQuestionMocks();
            var dto = new QuestionnaireQuestionRequestDto { QuestionExternalCode = "ext", QuestionnaireQuestionCode = "sampleExternalCode",CreateUser = "System" };
            var model = new QuestionnaireQuestionModel { QuestionExternalCode = "ext", DeleteNbr = 0 };
            _mockRepo.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<QuestionnaireQuestionModel, bool>>>(), false))
                .ReturnsAsync(model);

            // Act
            var result = await _service.CreateQuestionnaireQuestion(dto);

            // Assert
            Assert.Equal(StatusCodes.Status409Conflict, result.ErrorCode);
        }

        [Fact]
        public async TaskAlias CreateQuestionnaireQuestion_ReturnsSuccess_WhenCreated()
        {
            // Arrange
            SetupQuestionnaireQuestionMocks();
            var dto = new QuestionnaireQuestionRequestDto { QuestionExternalCode = "ext", QuestionnaireJson = "{}", CreateUser = "System", QuestionnaireQuestionCode = "SampleCode" };
            _mockRepo.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<QuestionnaireQuestionModel, bool>>>(), false));
            _mockQuesMapper.Setup(m => m.Map<QuestionnaireQuestionModel>(dto))
                .Returns(new QuestionnaireQuestionModel());
            _mockRepo.Setup(r => r.CreateAsync(It.IsAny<QuestionnaireQuestionModel>()))
                .ReturnsAsync(new QuestionnaireQuestionModel { QuestionnaireQuestionId = 1 });

            // Act
            var result = await _service.CreateQuestionnaireQuestion(dto);

            // Assert
            Assert.Null(result.ErrorCode);
        }

        [Fact]
        public async TaskAlias CreateQuestionnaireQuestion_ReturnsError_OnException()
        {
            // Arrange
            SetupQuestionnaireQuestionMocks();
            var dto = new QuestionnaireQuestionRequestDto { QuestionExternalCode = "ext", QuestionnaireJson = "{}", CreateUser = "System", QuestionnaireQuestionCode = "SampleCode" };
            _mockRepo.Setup(r => r.FindOneAsync(It.IsAny<Expression<Func<QuestionnaireQuestionModel, bool>>>(), false))
                .ThrowsAsync(new Exception("fail"));

            // Act
            var result = await _service.CreateQuestionnaireQuestion(dto);

            // Assert
            Assert.Equal(StatusCodes.Status500InternalServerError, result.ErrorCode);
        }
    }
}
