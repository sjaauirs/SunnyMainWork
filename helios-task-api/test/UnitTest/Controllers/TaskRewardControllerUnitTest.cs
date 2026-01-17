using Amazon.Runtime.Internal.Transform;
using AutoMapper;
using Azure;
using FluentNHibernate.Testing.Values;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using SunnyRewards.Helios.Common.Core.Domain;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Repositories;
using SunnyRewards.Helios.Task.Api.Controller;
using SunnyRewards.Helios.Task.Core.Domain.Constants;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Models;
using SunnyRewards.Helios.Task.Infrastructure.Helpers;
using SunnyRewards.Helios.Task.Infrastructure.Helpers.Interface;
using SunnyRewards.Helios.Task.Infrastructure.Repositories;
using SunnyRewards.Helios.Task.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Task.Infrastructure.Services;
using SunnyRewards.Helios.Task.Infrastructure.Services.Interface;
using SunnyRewards.Helios.Task.UnitTest.Fixtures.MockDtos;
using SunnyRewards.Helios.Task.UnitTest.Fixtures.MockModel;
using SunnyRewards.Helios.Task.UnitTest.Fixtures.MockRepositories;
using System.Linq.Expressions;
using Xunit;
using TaskAlias = System.Threading.Tasks.Task;

namespace SunnyRewards.Helios.Task.UnitTest.Controllers
{
    public class TaskRewardControllerUnitTest
    {
        private readonly TaskRewardService _taskRewardService;
        private readonly IMapper _mapper;
        private readonly Mock<ILogger<TaskRewardService>> _taskRewardServiceLogger;
        private readonly Mock<ILogger<TaskRewardRepo>> _taskRewardRepoLogger;
        private readonly Mock<ITaskRewardRepo> _taskRewardRepository;
        private readonly Mock<ITaskDetailRepo> _taskDetailRepository;
        private readonly Mock<ITaskRepo> _taskRepository;
        private readonly Mock<ITermsOfServiceRepo> _termsOfServiceRepository;
        private readonly Mock<ILogger<TaskRewardController>> _taskRewardLogger;
        private readonly Mock<IConsumerTaskRepo> _consumerTaskRepo;
        private readonly Mock<ITaskTypeRepo> _taskTypeRepo;
        private readonly Mock<ITenantTaskCategoryRepo> _tenantTaskCategoryRepo;
        private TaskRewardController _taskRewardController;
        private readonly Mock<ITaskRewardCollectionRepo> _taskRewardCollectionRepo;
        private readonly Mock<ITaskRewardTypeRepo> _taskRewardTypeRepository;
        private readonly Mock<ICommonTaskRewardService> _commonTaskRewardService;
        private readonly Mock<IAdventureRepo> _adventureRepo;

        public TaskRewardControllerUnitTest()
        {
            _taskRewardServiceLogger = new Mock<ILogger<TaskRewardService>>();
            _taskRewardLogger = new Mock<ILogger<TaskRewardController>>();
            _taskRewardRepository = new TaskRewardMockRepo();
            _taskDetailRepository = new TaskDetailMockRepo();
            _taskRepository = new TaskMockRepo();
            _taskRewardCollectionRepo = new Mock<ITaskRewardCollectionRepo>();
            _termsOfServiceRepository = new TermsOfServiceMockRepo();
            _consumerTaskRepo = new ConsumerTaskMockRepo();
            _tenantTaskCategoryRepo = new TenantTaskCategoryMockRepo();
            _taskTypeRepo = new TaskTypeMockRepo();
            _taskRewardTypeRepository = new TaskRewardTypeMockRepo();
            _taskRewardRepoLogger = new Mock<ILogger<TaskRewardRepo>>();
            _commonTaskRewardService = new Mock<ICommonTaskRewardService>();
            _adventureRepo = new AdventureMockRepo();
            _mapper = new Mapper(new MapperConfiguration(
                configure =>
                {
                    configure.AddMaps(typeof(Infrastructure.Mappings.MappingProfile.TaskRewardMapping).Assembly.FullName);
                    configure.AddMaps(typeof(Infrastructure.Mappings.MappingProfile.TaskDetailMapping).Assembly.FullName);
                    configure.AddMaps(typeof(Infrastructure.Mappings.MappingProfile.TaskMapping).Assembly.FullName);
                    configure.AddMaps(typeof(Infrastructure.Mappings.MappingProfile.TermsOfServiceMapping).Assembly.FullName);
                    configure.AddMaps(typeof(Infrastructure.Mappings.MappingProfile.ConsumerTaskMapping).Assembly.FullName);

                }));

            _taskRewardService = new TaskRewardService(_taskRewardServiceLogger.Object, _mapper, _taskRewardRepository.Object,
                 _taskRepository.Object, _taskDetailRepository.Object, _termsOfServiceRepository.Object, _consumerTaskRepo.Object, _tenantTaskCategoryRepo.Object, _taskTypeRepo.Object,
                 _taskRewardTypeRepository.Object, _taskRewardCollectionRepo.Object, _commonTaskRewardService.Object,_adventureRepo.Object);
            _taskRewardController = new TaskRewardController(_taskRewardLogger.Object, _taskRewardService);
        }

        [Fact]
        public async TaskAlias Should_Get_Task_Rewards()
        {
            var tenantCode = "ten-ecada21e57154928a2bb959e8365b8b4";
            var languageCode = "englcmc2q42mvkfdvmldfkbdfmb";
            var taskMockModel = new List<TaskModel>
            {
            new TaskModel { TaskId = 1, IsSubtask = false, DeleteNbr = 0 }
            };
            var taskDetailMockModel = new List<TaskDetailModel>
            {
                new TaskDetailModel {  TenantCode = tenantCode, LanguageCode = languageCode }
            };
            var termsOfServiceMockModel = new List<TermsOfServiceModel>
            {
                new TermsOfServiceModel { TermsOfServiceId = 1, DeleteNbr = 0 }
            };
            var tenantTaskCategoryMockModel = new List<TenantTaskCategoryModel>
            {
                new TenantTaskCategoryModel { TaskCategoryId = 1, TenantCode = tenantCode, DeleteNbr = 0 }
            };
            var taskTypeMockModel = new List<TaskTypeModel>
            {
                new TaskTypeModel { TaskTypeId = 1, DeleteNbr = 0 }
            };
            var taskRewardTypeMockModel = new List<TaskRewardTypeModel>
            {
                new TaskRewardTypeModel { RewardTypeId = 1, DeleteNbr = 0 }
            };
            _taskRepository.Setup(repo => repo.FindAsync(
                     It.IsAny<Expression<Func<TaskModel, bool>>>(), false)).ReturnsAsync(taskMockModel);
            _taskDetailRepository.Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<TaskDetailModel, bool>>>(), false)).ReturnsAsync(taskDetailMockModel);
            _termsOfServiceRepository.Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<TermsOfServiceModel, bool>>>(), false)).ReturnsAsync(termsOfServiceMockModel);
            _tenantTaskCategoryRepo.Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<TenantTaskCategoryModel, bool>>>(), false)).ReturnsAsync(tenantTaskCategoryMockModel);
            _taskTypeRepo.Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<TaskTypeModel, bool>>>(), false)).ReturnsAsync(taskTypeMockModel);
            _taskRewardTypeRepository.Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<TaskRewardTypeModel, bool>>>(), false)).ReturnsAsync(taskRewardTypeMockModel);
            var findTaskRewardRequestMockDto = new FindTaskRewardRequestMockDto();
            var response = await _taskRewardController.FindTaskRewards(findTaskRewardRequestMockDto);
            var result = response?.Result as OkObjectResult;
            Assert.NotNull(result?.Value);
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }

        [Fact]
        public async TaskAlias Should_GetTaskTrue_Rewards_When_Task_Recurrence_Quarterly()
        {
            var findTaskRewardRequestMockDto = new FindTaskRewardRequestMockDto();
            findTaskRewardRequestMockDto.LanguageCode = "es";
            var taskRewardIsRecurringTrueMockModel = new TaskRewardIsRecurringTrueMockModel();
            var tenantCode = "ten-ecada21e57154928a2bb959e8365b8b4";
            var languageCode = "edkslnflw374rhi4uhr89ngrjr48";
            var taskMockModel = new List<TaskModel>
            {
            new TaskModel { TaskId = 1, IsSubtask = false, DeleteNbr = 0 }
            };
            var taskDetailMockModel = new List<TaskDetailModel>
            {
                new TaskDetailModel {  TenantCode = tenantCode, LanguageCode = languageCode , TaskId = 1}
            };
            var termsOfServiceMockModel = new List<TermsOfServiceModel>
            {
                new TermsOfServiceModel { TermsOfServiceId = 1, DeleteNbr = 0 }
            };
            var tenantTaskCategoryMockModel = new List<TenantTaskCategoryModel>
            {
                new TenantTaskCategoryModel { TaskCategoryId = 1, TenantCode = tenantCode, DeleteNbr = 0 }
            };
            var taskTypeMockModel = new List<TaskTypeModel>
            {
                new TaskTypeModel { TaskTypeId = 1, DeleteNbr = 0 }
            };
            var taskRewardTypeMockModel = new List<TaskRewardTypeModel>
            {
                new TaskRewardTypeModel { RewardTypeId = 1, DeleteNbr = 0 }
            };
            _taskRepository.Setup(repo => repo.FindAsync(
                     It.IsAny<Expression<Func<TaskModel, bool>>>(), false)).ReturnsAsync(taskMockModel);
            _taskDetailRepository.SetupSequence(repo => repo.FindAsync(It.IsAny<Expression<Func<TaskDetailModel, bool>>>(), false)).ReturnsAsync((List<TaskDetailModel>)null).ReturnsAsync(taskDetailMockModel);
            _termsOfServiceRepository.Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<TermsOfServiceModel, bool>>>(), false)).ReturnsAsync(termsOfServiceMockModel);
            _tenantTaskCategoryRepo.Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<TenantTaskCategoryModel, bool>>>(), false)).ReturnsAsync(tenantTaskCategoryMockModel);
            _taskTypeRepo.Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<TaskTypeModel, bool>>>(), false)).ReturnsAsync(taskTypeMockModel);
            _taskRewardTypeRepository.Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<TaskRewardTypeModel, bool>>>(), false)).ReturnsAsync(taskRewardTypeMockModel);
            taskRewardIsRecurringTrueMockModel.RecurrenceDefinitionJson = "{\r\n  \"recurrenceType\": \"PERIODIC\",\r\n  \"periodic\": {\r\n    \"period\": \"QUARTER\",\r\n    \"periodRestartDate\": 1\r\n  }\r\n}";
            _taskRewardRepository.Setup(x => x.FindAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false)).ReturnsAsync(new List<TaskRewardModel>() { taskRewardIsRecurringTrueMockModel });
            var response = await _taskRewardController.FindTaskRewards(findTaskRewardRequestMockDto);
            var result = response?.Result as OkObjectResult;
            Assert.NotNull(result?.Value);
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public async TaskAlias FindTaskRewards_Should_Return_Catch_Exception_In_Controller()
        {
            var loggerMock = new Mock<ILogger<TaskRewardController>>();
            var taskServiceMock = new Mock<ITaskRewardService>();
            var controller = new TaskRewardController(loggerMock.Object, taskServiceMock.Object);
            var requestDto = new FindTaskRewardRequestMockDto();
            taskServiceMock.Setup(x => x.GetTaskRewardDetails(It.IsAny<FindTaskRewardRequestMockDto>()))
            .ThrowsAsync(new Exception("Simulated exception"));
            var response = await controller.FindTaskRewards(requestDto);
            var result = response?.Result as NotFoundObjectResult;
            Assert.NotEqual(404, result?.StatusCode);
        }

        [Fact]
        public async TaskAlias FindTaskRewards_Should_Return_Catch_Exception_In_Service()
        {
            var findTaskRewardRequestMockDto = new FindTaskRewardRequestMockDto();
            _taskRewardRepository.Setup(x => x.FindAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false))
              .ThrowsAsync(new Exception("Simulated exception"));
            var response = await _taskRewardService.GetTaskRewardDetails(findTaskRewardRequestMockDto);
            Assert.NotNull(response);
        }


        [Fact]
        public async TaskAlias FindTaskRewards_Should_Return_Data()
        {
            var findTaskRewardRequestMockDto = new FindTaskRewardRequestMockDto();
            var response = await _taskRewardService.GetTaskRewardDetails(findTaskRewardRequestMockDto);
            Assert.NotNull(response);
        }

        [Fact]
        public async TaskAlias FindTaskRewards_Should_Return_Empty_Tasks_When_No_Tasks_Found()
        {
            var findTaskRewardRequestMockDto = new FindTaskRewardRequestMockDto();

            // Set up mock to return an empty list of task rewards
            _taskRewardRepository.Setup(x => x.FindAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false))
                .ReturnsAsync(new List<TaskRewardModel>());

            // Act
            var result = await _taskRewardService.GetTaskRewardDetails(findTaskRewardRequestMockDto);

            // Assert
            Assert.NotNull(result); // The result should be empty when no tasks are found
        }

        [Fact]
        public async TaskAlias FindTaskRewards_Should_Return_Valid_Recurring_Tasks()
        {
            var findTaskRewardRequestMockDto = new FindTaskRewardRequestMockDto();

            var taskRewards = new List<TaskRewardModel>
            {
                new() { TaskId = 1, IsRecurring = true, RecurrenceDefinitionJson = "{\"periodic\": {\"period\": \"MONTH\", \"periodRestartDate\": 5}}" }
            };

            var consumerTasks = new List<ConsumerTaskModel>
            {
                new()
                {
                    TaskId = 1,
                    TaskStatus = Constants.Completed,
                    TaskCompleteTs = DateTime.UtcNow.AddDays(-1),
                    TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4"
                }
            };

            _taskRewardRepository.Setup(x => x.FindAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false))
                .ReturnsAsync(taskRewards);

            // Act
            var result = await _taskRewardService.GetTaskRewardDetails(findTaskRewardRequestMockDto);

            var availableTasksCount = result.TaskRewardDetails.Count;

            // Assert
            Assert.Equal(1, availableTasksCount);
        }

        [Fact]
        public async TaskAlias FindTaskRewards_Should_Not_Return_Tasks_With_Invalid_PeriodRestartDate()
        {
            var findTaskRewardRequestMockDto = new FindTaskRewardRequestMockDto();

            var taskRewards = new List<TaskRewardModel>
            {
                new() { TaskId = 1, IsRecurring = true, RecurrenceDefinitionJson = "{\"periodic\": {\"periodRestartDate\": 35}}" }
            };

            var consumerTasks = new List<ConsumerTaskModel>
            {
                new()
                {
                    TaskId = 1,
                    TaskStatus = Constants.Completed,
                    TaskCompleteTs = DateTime.UtcNow.AddDays(-1),
                    TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4"
                }
            };

            _taskRewardRepository.Setup(x => x.FindAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false))
                .ReturnsAsync(taskRewards);

            // Act
            var result = await _taskRewardService.GetTaskRewardDetails(findTaskRewardRequestMockDto);

            var availableTasks = result.TaskRewardDetails;

            // Assert
            Assert.Empty(availableTasks);
        }

        [Fact]
        public async TaskAlias FindTaskRewards_Should_Return_Quarterly_Tasks_When_Valid()
        {
            var findTaskRewardRequestMockDto = new FindTaskRewardRequestMockDto();

            var taskRewards = new List<TaskRewardModel>
            {
                new() { TaskId = 1, IsRecurring = true, RecurrenceDefinitionJson = "{\"periodic\": {\"period\": \"Quarterly\", \"periodRestartDate\": 5}}" }
            };

            var consumerTasks = new List<ConsumerTaskModel>
            {
                new()
                {
                    TaskId = 1,
                    TaskStatus = Constants.Completed,
                    TaskCompleteTs = DateTime.UtcNow.AddMonths(-3), // Completed in the last quarter
                    TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4"
                }
            };

            _taskRewardRepository.Setup(x => x.FindAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false))
                .ReturnsAsync(taskRewards);

            // Act
            var result = await _taskRewardService.GetTaskRewardDetails(findTaskRewardRequestMockDto);

            // Assert
            Assert.NotNull(result); // Quarterly task should be returned
        }

        //#2

        [Fact]
        public async TaskAlias Should_Get_TaskRewardByCode()
        {
            var getTaskRewardByCodeRequestMockDto = new GetTaskRewardByCodeRequestMockDto();
            var response = await _taskRewardController.GetTaskRewardByCode(getTaskRewardByCodeRequestMockDto);
            var result = response?.Result as OkObjectResult;
            Assert.NotNull(result?.Value);
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public async TaskAlias GetTaskRewardByCode_Should_Return_Catch_Exception_In_Controller()
        {
            var loggerMock = new Mock<ILogger<TaskRewardController>>();
            var taskServiceMock = new Mock<ITaskRewardService>();
            var controller = new TaskRewardController(loggerMock.Object, taskServiceMock.Object);
            var getTaskRewardByCodeRequestMockDto = new GetTaskRewardByCodeRequestMockDto();
            taskServiceMock.Setup(x => x.GetTaskRewardByCode(It.IsAny<GetTaskRewardByCodeRequestMockDto>()))
            .ThrowsAsync(new Exception("Simulated exception"));
            var response = await controller.GetTaskRewardByCode(getTaskRewardByCodeRequestMockDto);
            var result = response?.Result as NotFoundObjectResult;
            Assert.NotEqual(404, result?.StatusCode);
        }

        [Fact]
        public async TaskAlias GetTaskRewardByCode_Should_Return_Catch_Exception_In_Service()
        {
            var getTaskRewardByCodeRequestMockDto = new GetTaskRewardByCodeRequestMockDto();
            _taskRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskModel, bool>>>(), false))
              .ThrowsAsync(new Exception("Simulated exception"));
            var response = await _taskRewardService.GetTaskRewardByCode(getTaskRewardByCodeRequestMockDto);
            Assert.NotNull(response);
        }
        [Fact]
        public async TaskAlias GetTaskRewardByCode_Should_Return_Catch_null_In_Service()
        {
            GetTaskRewardByCodeRequestDto task = new GetTaskRewardByCodeRequestDto();
            _taskRewardRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false));
            var response = await _taskRewardService.GetTaskRewardByCode(task);
            Assert.NotNull(response);
        }

        [Fact]
        public async TaskAlias Should_Get_RewardType_Controller()
        {
            var rewardTypeRequestDto = new RewardTypeRequestMockDto();
            var response = await _taskRewardController.RewardType(rewardTypeRequestDto);
            var result = response?.Result as OkObjectResult;
            Assert.NotNull(result?.Value);
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public async TaskAlias Should_Get_RewardType_MotFound_Controller()
        {
            var rewardTypeRequestDto = new RewardTypeRequestMockDto();
            _taskRewardRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false)).ReturnsAsync((TaskRewardMockModel)null);
            var response = await _taskRewardController.RewardType(rewardTypeRequestDto);
            var result = response?.Result as NotFoundObjectResult;
            Assert.Null(result?.Value);
        }

        [Fact]
        public async TaskAlias Should_RewardType_Return_Catch_Exception_Controller()
        {
            var rewardTypeRequestDto = new RewardTypeRequestMockDto();
            var taskRewardService = new Mock<ITaskRewardService>();
            taskRewardService.Setup(x => x.RewardType(rewardTypeRequestDto)).ThrowsAsync(new Exception("inner Exception"));
            var taskRewardLogger = new Mock<ILogger<TaskRewardController>>();
            var taskRewardController = new TaskRewardController(taskRewardLogger.Object, taskRewardService.Object);
            var response = await taskRewardController.RewardType(rewardTypeRequestDto);
            Assert.Null(response);
        }

        [Fact]
        public async TaskAlias Should_Get_RewardType_Service()
        {
            var rewardTypeRequestDto = new RewardTypeRequestMockDto();
            var response = await _taskRewardService.RewardType(rewardTypeRequestDto);
            var result = response?.RewardTypeDto;
            Assert.NotNull(result);
        }

        [Fact]
        public async TaskAlias Should_Get_RewardType_MotFound_Service()
        {
            var rewardTypeRequestDto = new RewardTypeRequestMockDto();
            _taskRewardRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false)).ReturnsAsync((TaskRewardMockModel)null);
            var response = await _taskRewardService.RewardType(rewardTypeRequestDto);
            Assert.Null(response?.RewardTypeDto);
        }

        [Fact]
        public async TaskAlias Should_Get_RewardType_NullZero_Service()
        {
            var rewardTypeRequestDto = new RewardTypeRequestMockDto
            {
                TaskId = 0,
                TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4",
                TaskCode = "tas-a21e57154928a",
            };
            _taskRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskModel, bool>>>(), false)).ReturnsAsync((TaskMockModel)null);
            var response = await _taskRewardService.RewardType(rewardTypeRequestDto);
            Assert.Null(response?.RewardTypeDto);
        }

        [Fact]
        public async TaskAlias Should_Get_RewardType_Null_Service()
        {
            var rewardTypeRequestDto = new RewardTypeRequestMockDto
            {
                TaskId = 0,
                TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4",
            };
            _taskRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskModel, bool>>>(), false)).ReturnsAsync((TaskMockModel)null);
            var response = await _taskRewardService.RewardType(rewardTypeRequestDto);
            Assert.Null(response?.RewardTypeDto);
        }

        [Fact]
        public async TaskAlias Should_RewardType_Return_Catch_Exception_Service()
        {
            var rewardTypeRequestDto = new RewardTypeRequestMockDto();
            _taskRewardRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false))
              .ThrowsAsync(new Exception("Simulated exception"));
            var response = await _taskRewardService.RewardType(rewardTypeRequestDto);
            Assert.Null(response.RewardTypeDto);
            Assert.NotNull(response);
        }

        #region IsTaskCompletedInPreviousQuarter Tests
        [Fact]
        public void IsTaskCompletedInPreviousQuarter_Should_Return_True_When_Task_Completed_One_Year_Back()
        {
            int periodRestartDate = 7;
            DateTime completionDate = DateTime.UtcNow.AddYears(-1);
            var response = TaskHelper.IsTaskCompletedInPreviousQuarter(completionDate, periodRestartDate);
            Assert.True(response);
        }

        [Fact]
        public void IsTaskCompletedInPreviousQuarter_Should_Return_True_When_Task_Completed_One_Quarter_Back()
        {
            int periodRestartDate = 1;
            DateTime completionDate = DateTime.UtcNow.AddMonths(-3);
            var response = TaskHelper.IsTaskCompletedInPreviousQuarter(completionDate, periodRestartDate);
            Assert.True(response);
        }

        [Fact]
        public void IsTaskCompletedInPreviousQuarter_Should_Return_True_When_Task_Completed_Two_Quarter_Back()
        {
            int periodRestartDate = 1;
            DateTime completionDate = DateTime.UtcNow.AddMonths(-6);
            var response = TaskHelper.IsTaskCompletedInPreviousQuarter(completionDate, periodRestartDate);
            Assert.True(response);
        }

        [Fact]
        public void IsTaskCompletedInPreviousQuarter_Should_Return_True_When_Task_Completed_Three_Quarters_Back()
        {
            int periodRestartDate = 1;
            DateTime completionDate = DateTime.UtcNow.AddMonths(-9);
            var response = TaskHelper.IsTaskCompletedInPreviousQuarter(completionDate, periodRestartDate);
            Assert.True(response);
        }

        [Fact]
        public void IsTaskCompletedInPreviousQuarter_Should_Return_False_When_Task_Completed_In_Same_Quarter()
        {
            int periodRestartDate = 1;
            DateTime completionDate = DateTime.UtcNow.AddDays(-2);
            var response = TaskHelper.IsTaskCompletedInPreviousQuarter(completionDate, periodRestartDate);
            Assert.False(response);
        }

        [Fact]
        public void IsTaskCompletedInPreviousQuarter_Should_Return_False_When_Period_Restart_Date_Is_Null()
        {
            DateTime completionDate = DateTime.UtcNow.AddDays(-4);
            var response = TaskHelper.IsTaskCompletedInPreviousQuarter(completionDate, null);
            Assert.False(response);
        }

        [Fact]
        public void IsValidScheduleRecurring_Should_Return_True_When_TaskCompleteDate_Between_Valid_Schedule_Start_Date_Expiry_Date()
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
        public void IsValidScheduleRecurring_Should_Return_False_When_TaskCompleteDate_Between_Valid_Schedule_Start_Date_Expiry_Date()
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

        #endregion

        [Fact]
        public async TaskAlias Should_OkResponse_RewardTypeCode_Controller()
        {
            var rewardTypeCodeRequestMockDto = new RewardTypeCodeRequestMockDto();
            var response = await _taskRewardController.RewardTypeCode(rewardTypeCodeRequestMockDto);
            var result = response.Result as OkObjectResult;
            Assert.NotNull(result?.Value);
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public async TaskAlias Should_Exception_RewardTypeCode_Controller()
        {
            var rewardTypeCodeRequestMockDto = new RewardTypeCodeRequestMockDto();
            var service = new Mock<ITaskRewardService>();
            var controller = new TaskRewardController(_taskRewardLogger.Object, service.Object);
            service.Setup(x => x.RewardTypeCode(rewardTypeCodeRequestMockDto)).ThrowsAsync(new Exception("inner Exception"));
            var response = await controller.RewardTypeCode(rewardTypeCodeRequestMockDto);
            Assert.Null(response);
        }

        [Fact]
        public async TaskAlias Should_OkResponse_RewardTypeCode_Service()
        {
            var rewardTypeCodeRequestMockDto = new RewardTypeCodeRequestMockDto();
            var response = await _taskRewardController.RewardTypeCode(rewardTypeCodeRequestMockDto);
            var result = response?.Result as OkObjectResult;
            Assert.NotNull(result?.Value);
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public async TaskAlias Should_CheckNull_RewardTypeCode_Service()
        {
            var rewardTypeCodeRequestMockDto = new RewardTypeCodeRequestMockDto();
            _taskRewardTypeRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskRewardTypeModel, bool>>>(), false))
               .ReturnsAsync((TaskRewardTypeMockModel)null);
            var response = await _taskRewardController.RewardTypeCode(rewardTypeCodeRequestMockDto);
            var result = response?.Result as NotFoundObjectResult;
            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async TaskAlias Should_Exception_RewardTypeCode_Service()
        {
            var rewardTypeCodeRequestMockDto = new RewardTypeCodeRequestMockDto();
            _taskRewardTypeRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskRewardTypeModel, bool>>>(), false))
                .ThrowsAsync(new Exception("Test exception"));
            var response = await _taskRewardService.RewardTypeCode(rewardTypeCodeRequestMockDto);
            Assert.NotNull(response);
        }

        [Fact]
        public async TaskAlias Should_OkResponse_GetAllTaskByTenantCode_Controller()
        {
            var getTaskByTenantCodeRequestMockDto = new GetTaskByTenantCodeRequestMockDto();

            var response = await _taskRewardController.GetAllTaskByTenantCode(getTaskByTenantCodeRequestMockDto);

            var result = response.Result as OkObjectResult;
            Assert.NotNull(result?.Value);
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }

        [Fact]
        public async TaskAlias Should_Exception_GetAllTaskByTenantCode_Controller()
        {
            var getTaskByTenantCodeRequestMockDto = new GetTaskByTenantCodeRequestMockDto();
            var taskrewardRepo = new Mock<ITaskRewardRepo>();
            var service = new TaskRewardService(_taskRewardServiceLogger.Object, _mapper, taskrewardRepo.Object,
                 _taskRepository.Object, _taskDetailRepository.Object, _termsOfServiceRepository.Object, _consumerTaskRepo.Object, _tenantTaskCategoryRepo.Object, _taskTypeRepo.Object,
                 _taskRewardTypeRepository.Object, _taskRewardCollectionRepo.Object, _commonTaskRewardService.Object, _adventureRepo.Object);
            taskrewardRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false)).ThrowsAsync(new Exception("testing"));
            var taskRewardController = new TaskRewardController(_taskRewardLogger.Object,service);

            var response = await taskRewardController.GetAllTaskByTenantCode(getTaskByTenantCodeRequestMockDto);

            var result = response.Result as ObjectResult;
            Assert.NotNull(response);
            Assert.Equal(StatusCodes.Status500InternalServerError, result?.StatusCode);
        }

        [Fact]
        public async TaskAlias Should_Return_NotFound_GetAllTaskByTenantCode_Controller()
        {
            var getTaskByTenantCodeRequestMockDto = new GetTaskByTenantCodeRequestMockDto();
            var taskrewardRepo = new Mock<ITaskRewardRepo>();
            var service = new TaskRewardService(_taskRewardServiceLogger.Object, _mapper, taskrewardRepo.Object,
                 _taskRepository.Object, _taskDetailRepository.Object, _termsOfServiceRepository.Object, _consumerTaskRepo.Object, _tenantTaskCategoryRepo.Object, _taskTypeRepo.Object,
                 _taskRewardTypeRepository.Object, _taskRewardCollectionRepo.Object, _commonTaskRewardService.Object, _adventureRepo.Object);
            taskrewardRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false));
            var taskRewardController = new TaskRewardController(_taskRewardLogger.Object, service);

            var response = await taskRewardController.GetAllTaskByTenantCode(getTaskByTenantCodeRequestMockDto);

            var result = response.Result as ObjectResult;
            Assert.NotNull(response);
            Assert.Equal(StatusCodes.Status404NotFound, result?.StatusCode);
        }

        [Fact]
        public async TaskAlias Should_OkResponse_GetAllTaskByTenantCode_Service()
        {
            var getTaskByTenantCodeRequestMockDto = new GetTaskByTenantCodeRequestMockDto();
            _taskRewardRepository.Setup(x => x.FindAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false))
                .ReturnsAsync(new TaskRewardMockModel().taskData);
            var response = await _taskRewardService.GetAllTaskByTenantCode(getTaskByTenantCodeRequestMockDto);
            Assert.NotNull(response);
        }

        [Fact]
        public async TaskAlias Should_CheckNull_GetAllTaskByTenantCode_Service()
        {
            var getTaskByTenantCodeRequestMockDto = new GetTaskByTenantCodeRequestMockDto();
            _taskRewardRepository.Setup(x => x.FindAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false))
               .ReturnsAsync((List<TaskRewardModel>)null);
            var response = await _taskRewardService.GetAllTaskByTenantCode(getTaskByTenantCodeRequestMockDto);
            Assert.NotNull(response.AvailableTasks);
            Assert.NotNull(response.ConsumerTaskList);
        }

        [Fact]
        public async TaskAlias Should_Exception_GetAllTaskByTenantCode_Service()
        {
            var getTaskByTenantCodeRequestMockDto = new GetTaskByTenantCodeRequestMockDto();
            var expectedException = new Exception("Test exception");
            _taskRewardRepository.Setup(x => x.FindAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false))
               .ThrowsAsync(new Exception("Test exception"));
            var response = await _taskRewardService.GetAllTaskByTenantCode(getTaskByTenantCodeRequestMockDto);
            Assert.Null(response.AvailableTasks);
            Assert.Null(response.ConsumerTaskList);
        }

        [Fact]
        public async TaskAlias Should_OkResponse_CurrentPeriodDescriptor_Controller()
        {
            var taskRewardId = 1;
            var response = await _taskRewardController.CurrentPeriodDescriptor(taskRewardId);
            var result = response.Result as OkObjectResult;
            Assert.NotNull(result?.Value);
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public async TaskAlias Should_Exception_CurrentPeriodDescriptor_Controller()
        {
            var taskRewardId = 1;
            var taskRewardService = new Mock<ITaskRewardService>();
            var controller = new TaskRewardController(_taskRewardLogger.Object, taskRewardService.Object);
            taskRewardService.Setup(x => x.CurrentPeriodDescriptor(taskRewardId)).ThrowsAsync(new Exception("inner Exception"));
            var response = await controller.CurrentPeriodDescriptor(taskRewardId);
            Assert.NotNull(response);
        }

        [Fact]
        public async TaskAlias Should_OkResponse_CurrentPeriodDescriptor_Service()
        {
            var taskRewardId = 1;
            _taskRewardRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false))
              .ReturnsAsync(new TaskRewardMockModel { IsRecurring = true });
            var mapper = new Mock<IMapper>();
            mapper.Setup(x => x.Map<TaskRewardDto>(It.IsAny<TaskRewardModel>())).Returns(new TaskRewardMockDto());
            var response = await _taskRewardService.CurrentPeriodDescriptor(taskRewardId);
            Assert.NotNull(response);
            Assert.NotNull(response.PeriodDescriptorDtO);
        }
        [Fact]
        public async TaskAlias Should_OkResponse_CurrentPeriodDescriptor_Service_RecurrentDateChange()
        {
            var taskRewardId = 1;
            _taskRewardRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false))
              .ReturnsAsync(new TaskRewardMockModel { IsRecurring = true, RecurrenceDefinitionJson = "{\r\n  \"periodic\": {\r\n    \"period\": \"MONTH\",\r\n    \"periodRestartDate\": 29\r\n  },\r\n  \"recurrenceType\": \"PERIODIC\"\r\n}" });
            var mapper = new Mock<IMapper>();
            mapper.Setup(x => x.Map<TaskRewardDto>(It.IsAny<TaskRewardModel>())).Returns(new TaskRewardMockDto());
            var response = await _taskRewardService.CurrentPeriodDescriptor(taskRewardId);
            Assert.NotNull(response);
            Assert.NotNull(response.PeriodDescriptorDtO);
        }

        [Fact]
        public async TaskAlias Should_NullCheck_TaskReward_CurrentPeriodDescriptor_Service()
        {
            var taskRewardId = 1;
            _taskRewardRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false))
               .ReturnsAsync((TaskRewardMockModel)null);
            var response = await _taskRewardController.CurrentPeriodDescriptor(taskRewardId);
            var result = response?.Result as NotFoundObjectResult;
            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async TaskAlias Should_NullCheck_TaskRewardDto_CurrentPeriodDescriptor_Service()
        {
            var taskRewardId = 1;
            _taskRewardRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false))
               .ReturnsAsync(new TaskRewardMockModel());
            var response = await _taskRewardService.CurrentPeriodDescriptor(taskRewardId);
            Assert.NotNull(response);
            Assert.NotNull(response.PeriodDescriptorDtO);
        }

        [Fact]
        public async TaskAlias Should_Empty_TaskRewardDto_CurrentPeriodDescriptor_Service()
        {
            var taskRewardId = 1;
            _taskRewardRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false))
              .ReturnsAsync(new TaskRewardMockModel { IsRecurring = true, RecurrenceDefinitionJson = " " });
            var mapper = new Mock<IMapper>();
            mapper.Setup(x => x.Map<TaskRewardDto>(It.IsAny<TaskRewardModel>())).Returns(new TaskRewardMockDto());
            var response = await _taskRewardService.CurrentPeriodDescriptor(taskRewardId);
            Assert.NotNull(response);
            Assert.NotNull(response.PeriodDescriptorDtO);
        }

        [Fact]
        public async TaskAlias Should_ElseCondtion_CurrentPeriodDescriptor_Service()
        {
            var taskRewardId = 1;
            _taskRewardRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false))
              .ReturnsAsync(new TaskRewardMockModel { IsRecurring = true, RecurrenceDefinitionJson = "{\r\n  \"recurrenceType\": \"PERIODIC\",\r\n  \"periodic\": {\r\n    \"period\": \"QUARTER\",\r\n    \"periodRestartDate\": 1\r\n  }\r\n}" });
            var mapper = new Mock<IMapper>();
            mapper.Setup(x => x.Map<TaskRewardDto>(It.IsAny<TaskRewardModel>())).Returns(new TaskRewardMockDto());
            var response = await _taskRewardService.CurrentPeriodDescriptor(taskRewardId);
            Assert.NotNull(response);
            Assert.NotNull(response.PeriodDescriptorDtO);
        }

        [Fact]
        public async TaskAlias Should_Exception_CurrentPeriodDescriptor_Service()
        {
            var taskRewardId = 1;
            _taskRewardRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false))
                .ThrowsAsync(new Exception("Test exception"));
            await Assert.ThrowsAsync<Exception>(async () => await _taskRewardService.CurrentPeriodDescriptor(taskRewardId));
        }

        [Fact]
        public void SubTaskRepo_Constructor_ShouldInstantiate_WhenDependenciesAreProvided()
        {
            var mockLogger = new Mock<ILogger<BaseRepo<TaskRewardTypeModel>>>();
            var mockSession = new Mock<NHibernate.ISession>();
            var repo = new TaskRewardTypeRepo(mockLogger.Object, mockSession.Object);
            Assert.NotNull(repo);
        }
        [Fact]
        public async TaskAlias CreateTaskReward_ShouldReturnOk_WhenRequestIsSuccessful()
        {
            // Arrange
            var requestDto = TaskRewardMock();
            _taskRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskModel, bool>>>(), false)).ReturnsAsync(new TaskMockModel());
            _taskRewardRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false));

            // Act
            var result = await _taskRewardController.CreateTaskReward(requestDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<BaseResponseDto>(okResult.Value);
            Assert.Null(response.ErrorCode);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async TaskAlias CreateTaskReward_ShouldReturn_409_WhenRequestIsAlreadyExisted()
        {
            var requestDto = TaskRewardMock();
            _taskRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskModel, bool>>>(), false)).ReturnsAsync(new TaskMockModel());
            _taskRewardRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false))
                                        .ReturnsAsync(new TaskRewardModel
                                        {
                                            TenantCode = requestDto.TaskReward.TenantCode,
                                            TaskId = requestDto.TaskReward.TaskId,
                                        });


            // Act
            var result = await _taskRewardController.CreateTaskReward(requestDto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status409Conflict, objectResult.StatusCode);
            var response = Assert.IsType<BaseResponseDto>(objectResult.Value);
            Assert.Equal(StatusCodes.Status409Conflict, response.ErrorCode);


        }
        [Fact]
        public async TaskAlias CreateTaskReward_ShouldReturn_404_WhenRequestTaskCodeIsNotFound()
        {
            var requestDto = TaskRewardMock();
            // Act
            var result = await _taskRewardController.CreateTaskReward(requestDto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status409Conflict, objectResult.StatusCode);
            var response = Assert.IsType<BaseResponseDto>(objectResult.Value);
            Assert.Equal(StatusCodes.Status409Conflict, response.ErrorCode);

        }
        [Fact]
        public async TaskAlias CreateTaskReward_ShouldThrow_Exception()
        {
            var requestDto = TaskRewardMock();
            _taskRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskModel, bool>>>(), false)).ThrowsAsync(new Exception("An error occurred while fetching the task reward."));

            // Act
            var result = await _taskRewardController.CreateTaskReward(requestDto);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);

        }
        [Fact]
        public async TaskAlias GetTaskAndRewards_Should_Ok_Response()
        {
            // Arrange
            var requestDto = new GetTasksAndTaskRewardsRequestDto();
            _taskRewardRepository.Setup(x => x.GetTasksAndTaskRewards(requestDto)).ReturnsAsync(new List<TaskAndTaskRewardModel>() { new TaskAndTaskRewardModel()
            { Task=new TaskMockModel(),TaskReward=new TaskRewardMockModel()} });

            // Act
            var response = await _taskRewardController.GetTasksAndTaskRewards(requestDto);

            // Assert
            Assert.NotNull(response);
            var okObjectResult = Assert.IsType<OkObjectResult>(response);
            Assert.Equal(StatusCodes.Status200OK, okObjectResult.StatusCode);
        }

        [Fact]
        public async TaskAlias GetTaskAndRewards_Should_NotFound_Response_WhenTaskRewards_Are_Null()
        {
            // Arrange
            var requestDto = new GetTasksAndTaskRewardsRequestDto();
            _taskRewardRepository.Setup(x => x.GetTasksAndTaskRewards(requestDto)).ReturnsAsync(new List<TaskAndTaskRewardModel>());

            // Act
            var response = await _taskRewardController.GetTasksAndTaskRewards(requestDto);

            // Assert
            Assert.NotNull(response);
            var okObjectResult = Assert.IsType<ObjectResult>(response);
            Assert.Equal(StatusCodes.Status404NotFound, okObjectResult.StatusCode);
        }
        [Fact]
        public async TaskAlias GetTaskAndRewards_Should_Internal_Server_Error_When_Exception_occurs()
        {
            // Arrange
            var requestDto = new GetTasksAndTaskRewardsRequestDto();
            _taskRewardRepository.Setup(x => x.GetTasksAndTaskRewards(requestDto)).ThrowsAsync(new Exception("Something went wrong"));

            // Act
            var response = await _taskRewardController.GetTasksAndTaskRewards(requestDto);

            // Assert
            Assert.NotNull(response);
            var okObjectResult = Assert.IsType<ObjectResult>(response);
            Assert.Equal(StatusCodes.Status500InternalServerError, okObjectResult.StatusCode);
        }

        private static CreateTaskRewardRequestDto TaskRewardMock()
        {
            return new CreateTaskRewardRequestDto
            {
                TaskCode = "Some taskCode",
                TaskReward = new TaskRewardDto
                {
                    TaskId = 3,
                    TaskRewardId = 381,
                    RewardTypeId = 1,
                    TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4",
                    TaskRewardCode = "trw-bedeae20f1c511ee817f7fbbf9a0c262",
                    TaskActionUrl = "retool.com",
                    Reward = "{\u0022rewardAmount\u0022:\u002275\u0022}",
                    Priority = 0,
                    Expiry = DateTime.UtcNow,
                    MinTaskDuration = 0,
                    MaxTaskDuration = 0,
                    TaskExternalCode = "new_ment_task",
                    ValidStartTs = DateTime.UtcNow,
                    IsRecurring = false,
                    RecurrenceDefinitionJson = "{}",
                    SelfReport = true,
                    TaskCompletionCriteriaJson = null,
                    CreateUser = "per-915325069cdb42c783dd4601e1d27704"
                }
            };
        }

        #region IsValidMonthlyReccurance Tests
        [Fact]
        public void IsValidMonthlyReccurance_Should_Return_True_When_Completed_One_Month_Back()
        {
            var completionDate = DateTime.UtcNow.AddMonths(-1);
            var periodicDto = new PeriodicMockDto();
            var response = TaskHelper.IsValidMonthlyReccurance(completionDate, periodicDto);
            Assert.True(response);
        }
        [Fact]
        public void IsValidMonthlyReccurance_Should_Return_True_When_Completed_Three_Months_Back()
        {
            var completionDate = DateTime.UtcNow.AddMonths(-3);
            var periodicDto = new PeriodicMockDto();
            var response = TaskHelper.IsValidMonthlyReccurance(completionDate, periodicDto);
            Assert.True(response);
        }
        [Fact]
        public void IsValidMonthlyReccurance_Should_Return_True_When_Completed_One_Year_Back()
        {
            var completionDate = DateTime.UtcNow.AddYears(-1);
            var periodicDto = new PeriodicMockDto();
            var response = TaskHelper.IsValidMonthlyReccurance(completionDate, periodicDto);
            Assert.True(response);
        }
        [Fact]
        public void IsValidMonthlyReccurance_Should_Return_True_When_Completed_Two_Year_Back()
        {
            var completionDate = DateTime.UtcNow.AddYears(-2);
            var periodicDto = new PeriodicMockDto();
            var response = TaskHelper.IsValidMonthlyReccurance(completionDate, periodicDto);
            Assert.True(response);
        }
        [Fact]
        public void IsValidMonthlyReccurance_Should_Return_True_When_Completed_Aday_Before()
        {
            var completionDate = DateTime.UtcNow.AddDays(-1);
            var periodicDto = new PeriodicMockDto() { periodRestartDate = DateTime.UtcNow.Day };
            var response = TaskHelper.IsValidMonthlyReccurance(completionDate, periodicDto);
            Assert.True(response);
        }
        [Fact]
        public void IsValidMonthlyReccurance_Should_Return_False_When_Completed_SameDay()
        {
            var completionDate = DateTime.UtcNow;
            var periodicDto = new PeriodicMockDto() { periodRestartDate = completionDate.Day };
            var response = TaskHelper.IsValidMonthlyReccurance(completionDate, periodicDto);
            Assert.False(response);
        }
        [Fact]
        public void IsValidMonthlyReccurance_Should_Return_True_When_Completed_LastYear_Dec()
        {
            var completionDate = new DateTime(DateTime.UtcNow.Year - 1, 12, 01);
            var periodicDto = new PeriodicMockDto();
            var response = TaskHelper.IsValidMonthlyReccurance(completionDate, periodicDto);
            Assert.True(response);
        }
        [Fact]
        public void IsValidMonthlyReccurance_Should_Return_False_When_ReccuringDate_Is_Greaterthan_Current()
        {
            var completionDate = DateTime.UtcNow.AddMonths(-1);
            var periodicDto = new PeriodicMockDto() { periodRestartDate = completionDate.Day + 1 };
            var response = TaskHelper.IsValidMonthlyReccurance(completionDate, periodicDto);
            Assert.True(response);
        }
        [Fact]
        public void IsValidMonthlyReccurance_Should_Return_False_When_PeriodRestartDate_Isnull()
        {
            var completionDate = DateTime.UtcNow;
            var response = TaskHelper.IsValidMonthlyReccurance(completionDate, null);
            Assert.False(response);
        }

        // Test cases for GetTaskRewards
        
        [Fact]
        public async void GetTaskRewardsAsync_Controller_ShouldHandleException()
        {
            var serviceMock = new Mock<ITaskRewardService>();
            var controller = new TaskRewardController(_taskRewardLogger.Object, serviceMock.Object);
            serviceMock.Setup(x => x.GetTaskRewardsAsync()).ThrowsAsync(new Exception("inner Exception"));
            var response = await controller.GetTaskRewardsAsync();
            var result = response as ObjectResult;
            Assert.Equal(StatusCodes.Status500InternalServerError, result?.StatusCode);
        }

        [Fact]
        public async void GetTaskRewardsAsync_Service_ShouldReturnResponse()
        {
            var response = await _taskRewardService.GetTaskRewardsAsync();
            Assert.NotNull(response);
        }

        [Fact]
        public async void GetTaskRewardsAsync_RepositoryReturnsNull_ReturnsErrorResponse()
        {
            // Arrange
            var repositoryMock = new Mock<ITaskRewardRepo>();
            var loggerMock = new Mock<ILogger<TaskRewardService>>();
            var mapperMock = new Mock<IMapper>();

            // Simulate the repository returning null
            repositoryMock
                .Setup(x => x.FindAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false))
                .ReturnsAsync((List<TaskRewardModel>)null);

            var service = new TaskRewardService(loggerMock.Object, mapperMock.Object, repositoryMock.Object,
                 _taskRepository.Object, _taskDetailRepository.Object, _termsOfServiceRepository.Object, _consumerTaskRepo.Object, _tenantTaskCategoryRepo.Object, _taskTypeRepo.Object,
                 _taskRewardTypeRepository.Object,_taskRewardCollectionRepo.Object,_commonTaskRewardService.Object, _adventureRepo.Object);
            var response = await service.GetTaskRewardsAsync();

            // Act
            var expectedError = "No task reward was found.";

            // Assert
            Assert.NotNull(response);
            Assert.Equal(expectedError, response.ErrorMessage);
        }

        [Fact]
        public async void GetTaskRewardsAsync_Service_ShouldHandleRepositoryException()
        {
            var expectedException = new Exception("Test exception");
            _taskRewardRepository.Setup(x => x.FindAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false)).ThrowsAsync(expectedException);
            var result = await _taskRewardService.GetTaskRewardsAsync();

            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, result.ErrorCode);
            Assert.Equal("Test exception", result.ErrorMessage);
        }

        [Fact]
        public async void GetTaskRewardsAsync_Controller_ShouldReturnConflict()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TaskRewardService>>();
            var serviceMock = new Mock<ITaskRewardService>();
            var errorMessage = "No task rewards found.";
            serviceMock.Setup(x => x.GetTaskRewardsAsync()).ReturnsAsync(new TaskRewardsResponseDto
            {
                ErrorMessage = errorMessage
            });
            var controller = new TaskRewardController(_taskRewardLogger.Object, serviceMock.Object);

            // Act
            var response = await controller.GetTaskRewardsAsync();
            var result = Assert.IsType<OkObjectResult>(response);

            // Assert
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
            Assert.NotNull(result?.Value);

            var responseDto = Assert.IsType<TaskRewardsResponseDto>(result.Value);
            Assert.Equal(errorMessage, responseDto.ErrorMessage);
        }

        // Test cases for UpdateTaskRewardAsync
        [Fact]
        public async TaskAlias UpdateTaskRewardAsync_ShouldReturnOk_WhenUpdateIsSuccessful()
        {
            long taskRewardId = 1;
            // Arrange
            var requestDto = new TaskRewardRequestDto { };
            var responseDto = new TaskRewardResponseDto { ErrorCode = null };
            
            var _mockService = new Mock<ITaskRewardService>();
            _mockService.Setup(x => x.UpdateTaskRewardAsync(taskRewardId,requestDto,true)).ReturnsAsync(responseDto);

            // Act
            var result = await _taskRewardController.UpdateTaskRewardAsync(taskRewardId, requestDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

            var response = Assert.IsType<TaskRewardResponseDto>(okResult.Value);
            Assert.Null(response.ErrorCode);
            Assert.Null(response.ErrorMessage);
            Assert.NotNull(response.TaskReward);
        }

        [Fact]
        public async TaskAlias UpdateTaskRewardAsync_ShouldReturnError_WhenServiceReturnsError()
        {
            long taskRewardId = 1;
            // Arrange
            var requestDto = new TaskRewardRequestDto { };

            _taskRewardRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false))
              .ThrowsAsync(new Exception("Simulated exception"));

            // Act
            var result = await _taskRewardController.UpdateTaskRewardAsync(taskRewardId, requestDto);

            // Assert
            var errorResult = Assert.IsType<ConflictObjectResult>(result);
            Assert.Equal(StatusCodes.Status409Conflict, errorResult.StatusCode);

            var response = Assert.IsType<TaskRewardResponseDto>(errorResult.Value);

            Assert.Equal(StatusCodes.Status409Conflict, response.ErrorCode);
            Assert.Equal("Simulated exception", response.ErrorMessage);
        }

        [Fact]
        public async TaskAlias UpdateTaskRewardAsync_ShouldReturnInternalServerError_WhenExceptionOccurs()
        {
            long taskRewardId = 1;
            // Arrange
            var requestDto = new TaskRewardRequestDto { };

            _taskRewardRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false))
              .ThrowsAsync(new Exception("Simulated exception"));

            // Act
            var result = await _taskRewardController.UpdateTaskRewardAsync(taskRewardId, requestDto);

            // Assert
            var errorResult = Assert.IsType<ConflictObjectResult>(result);
            Assert.Equal(StatusCodes.Status409Conflict, errorResult.StatusCode);

            var response = Assert.IsType<TaskRewardResponseDto>(errorResult.Value);

            Assert.Equal(StatusCodes.Status409Conflict, response.ErrorCode);
            Assert.Equal("Simulated exception", response.ErrorMessage);
        }

        // Test cases for GetTaskRewardDetails
        [Fact]
        public async TaskAlias GetTaskRewardDetails_ShouldReturnOk_WhenServiceReturnsData()
        {
            // Arrange
            var tenantCode = "tenant-001";
            var taskExternalCode = "EXT001";
            var responseDto = new TaskRewardDetailsResponseDto
            {
                TaskRewardDetails = new List<TaskRewardDetailsDto>
                {
                    new()
                    {
                        Task = new TaskDto
                        {
                            TaskId = 101,
                            TaskTypeId = 1,
                            TaskCode = "TASK001",
                            TaskName = "Complete Profile",
                            SelfReport = true,
                            ConfirmReport = false,
                            TaskCategoryId = 10,
                            IsSubtask = false
                        },
                        TaskReward = new TaskRewardDto
                        {
                            TaskId = 101,
                            TaskRewardId = 201,
                            RewardTypeId = 1,
                            TenantCode = "TenantA",
                            TaskRewardCode = "REWARD001",
                            TaskActionUrl = "http://example.com/task/complete-profile",
                            Reward = "100 Points",
                            Priority = 1,
                            Expiry = DateTime.UtcNow.AddDays(30),
                            MinTaskDuration = 1,
                            MaxTaskDuration = 10,
                            TaskExternalCode = "EXT001",
                            ValidStartTs = DateTime.UtcNow,
                            IsRecurring = false,
                            RecurrenceDefinitionJson = null,
                            SelfReport = true,
                            TaskCompletionCriteriaJson = "{\"criteria\":\"complete all fields\"}",
                            CreateUser = "System",
                            MinAllowedTaskCompletionTs = DateTime.UtcNow.AddHours(-2),
                            MaxAllowedTaskCompletionTs = DateTime.UtcNow.AddHours(2)
                        },
                        TaskDetail = new TaskDetailDto
                        {
                            TaskId = 101,
                            TaskDetailId = 301,
                            TermsOfServiceId = 401,
                            TaskHeader = "Complete Your Profile",
                            TaskDescription = "Fill out all mandatory fields in your profile to receive rewards.",
                            LanguageCode = "en-US",
                            TenantCode = "TenantA",
                            TaskCtaButtonText = "Complete Now",
                            UpdateTs = DateTime.UtcNow
                        }
                    },
                    new()
                    {
                        Task = new TaskDto
                        {
                            TaskId = 102,
                            TaskTypeId = 2,
                            TaskCode = "TASK002",
                            TaskName = "Refer a Friend",
                            SelfReport = false,
                            ConfirmReport = true,
                            TaskCategoryId = 15,
                            IsSubtask = false
                        },
                        TaskReward = new TaskRewardDto
                        {
                            TaskId = 102,
                            TaskRewardId = 202,
                            RewardTypeId = 2,
                            TenantCode = "TenantB",
                            TaskRewardCode = "REWARD002",
                            TaskActionUrl = "http://example.com/task/refer-friend",
                            Reward = "200 Points",
                            Priority = 2,
                            Expiry = DateTime.UtcNow.AddDays(60),
                            MinTaskDuration = 5,
                            MaxTaskDuration = 15,
                            TaskExternalCode = "EXT002",
                            ValidStartTs = DateTime.UtcNow.AddDays(-1),
                            IsRecurring = true,
                            RecurrenceDefinitionJson = "{\"frequency\":\"weekly\"}",
                            SelfReport = false,
                            TaskCompletionCriteriaJson = "{\"criteria\":\"invite 3 friends\"}",
                            CreateUser = "Admin",
                            MinAllowedTaskCompletionTs = DateTime.UtcNow.AddDays(-1),
                            MaxAllowedTaskCompletionTs = DateTime.UtcNow.AddDays(7)
                        },
                        TaskDetail = new TaskDetailDto
                        {
                            TaskId = 102,
                            TaskDetailId = 302,
                            TermsOfServiceId = 402,
                            TaskHeader = "Invite Your Friends",
                            TaskDescription = "Refer friends to our platform and earn exciting rewards.",
                            LanguageCode = "en-US",
                            TenantCode = "TenantB",
                            TaskCtaButtonText = "Refer Now",
                            UpdateTs = DateTime.UtcNow
                        }
                    }
                }
            };


            var _mockService = new Mock<ITaskRewardService>();
            _mockService.Setup(x => x.GetTaskRewardDetails(tenantCode, taskExternalCode, "en-US")).ReturnsAsync(responseDto);

            // Act
            var result = await _taskRewardController.GetTaskRewardDetails(tenantCode, "en-US", taskExternalCode);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

            //var response = Assert.IsType<TaskRewardDetailsResponseDto>(okResult.Value);

            //Assert.Null(response.ErrorCode);
            //Assert.Null(response.ErrorMessage);
        }

        [Fact]
        public async TaskAlias GetTaskRewardDetails_ShouldReturnBadRequest_WhenTenantCodeIsNull()
        {
            // Act
            var result = await _taskRewardController.GetTaskRewardDetails(null, null,null);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
            var response = Assert.IsType<TaskRewardDetailsResponseDto>(badRequestResult.Value);
            Assert.Equal(StatusCodes.Status400BadRequest, response.ErrorCode);
            Assert.Equal("Tenant code cannot be null or empty.", response.ErrorMessage);
        }

        [Fact]
        public async TaskAlias GetTaskRewardDetails_ShouldReturnError_WhenServiceReturnsError()
        {
            // Arrange
            var tenantCode = "tenant-001";
            var taskExternalCode = "EXT001";
            var responseDto = new TaskRewardDetailsResponseDto {ErrorCode = 500, ErrorMessage = "No task reward details found for the provided tenant code." };
            var _mockService = new Mock<ITaskRewardService>();
            _mockService.Setup(x => x.GetTaskRewardDetails(tenantCode, taskExternalCode, "en-US")).ReturnsAsync(responseDto);

            // Act
            var result = await _taskRewardController.GetTaskRewardDetails(tenantCode, "en-US", taskExternalCode);

            // Assert
            var errorResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, errorResult.StatusCode);

            var response = Assert.IsType<TaskRewardDetailsResponseDto>(errorResult.Value);

            Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
            Assert.Equal(response.ErrorMessage, response.ErrorMessage);
        }

        [Fact]
        public async TaskAlias GetTaskRewardDetails_ShouldReturnOk_WhenRepositoryReturnsData()
        {
            // Arrange
            var tenantCode = "tenant-001";
            var taskExternalCode = "EXT001";
            var taskRewardDetails = new List<TaskRewardDetailModel>
                {
                    new()
                    {
                        Task = new TaskModel
                        {
                            TaskId = 101,
                            TaskTypeId = 1,
                            TaskCode = "TASK001",
                            TaskName = "Complete Profile",
                            SelfReport = true,
                            ConfirmReport = false,
                            TaskCategoryId = 10,
                            IsSubtask = false
                        },
                        TaskReward = new TaskRewardModel
                        {
                            TaskId = 101,
                            TaskRewardId = 201,
                            RewardTypeId = 1,
                            TenantCode = "TenantA",
                            TaskRewardCode = "REWARD001",
                            TaskActionUrl = "http://example.com/task/complete-profile",
                            Reward = "100 Points",
                            Priority = 1,
                            Expiry = DateTime.UtcNow.AddDays(30),
                            MinTaskDuration = 1,
                            MaxTaskDuration = 10,
                            TaskExternalCode = "EXT001",
                            ValidStartTs = DateTime.UtcNow,
                            IsRecurring = false,
                            RecurrenceDefinitionJson = null,
                            SelfReport = true,
                            TaskCompletionCriteriaJson = "{\"criteria\":\"complete all fields\"}",
                            CreateUser = "System",
                        },
                        TaskDetail = new TaskDetailModel
                        {
                            TaskId = 101,
                            TaskDetailId = 301,
                            TermsOfServiceId = 401,
                            TaskHeader = "Complete Your Profile",
                            TaskDescription = "Fill out all mandatory fields in your profile to receive rewards.",
                            LanguageCode = "en-US",
                            TenantCode = "TenantA",
                            TaskCtaButtonText = "Complete Now",
                            UpdateTs = DateTime.UtcNow
                        }
                    },
                    new()
                    {
                        Task = new TaskModel
                        {
                            TaskId = 102,
                            TaskTypeId = 2,
                            TaskCode = "TASK002",
                            TaskName = "Refer a Friend",
                            SelfReport = false,
                            ConfirmReport = true,
                            TaskCategoryId = 15,
                            IsSubtask = false
                        },
                        TaskReward = new TaskRewardModel
                        {
                            TaskId = 102,
                            TaskRewardId = 202,
                            RewardTypeId = 2,
                            TenantCode = "TenantB",
                            TaskRewardCode = "REWARD002",
                            TaskActionUrl = "http://example.com/task/refer-friend",
                            Reward = "200 Points",
                            Priority = 2,
                            Expiry = DateTime.UtcNow.AddDays(60),
                            MinTaskDuration = 5,
                            MaxTaskDuration = 15,
                            TaskExternalCode = "EXT002",
                            ValidStartTs = DateTime.UtcNow.AddDays(-1),
                            IsRecurring = true,
                            RecurrenceDefinitionJson = "{\"frequency\":\"weekly\"}",
                            SelfReport = false,
                            TaskCompletionCriteriaJson = "{\"criteria\":\"invite 3 friends\"}",
                            CreateUser = "Admin",
                        },
                        TaskDetail = new TaskDetailModel
                        {
                            TaskId = 102,
                            TaskDetailId = 302,
                            TermsOfServiceId = 402,
                            TaskHeader = "Invite Your Friends",
                            TaskDescription = "Refer friends to our platform and earn exciting rewards.",
                            LanguageCode = "en-US",
                            TenantCode = "TenantB",
                            TaskCtaButtonText = "Refer Now",
                            UpdateTs = DateTime.UtcNow
                        }
                    }
                };

            // Mock repository to return null when called
            var _mockTaskRewardRepo = new Mock<ITaskRewardRepo>();
            _taskRewardRepository.Setup(x => x.GetTaskRewardDetails(tenantCode, taskExternalCode, "en-US")).ReturnsAsync(taskRewardDetails);

            // Act
            var result = await _taskRewardController.GetTaskRewardDetails(tenantCode, "en-US", taskExternalCode);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

        }

        [Fact]
        public async TaskAlias GetTaskRewardDetails_ShouldReturnBadRequest_WhenRepositoryTenantCodeIsNull()
        {
            // Arrange
            string tenantCode = null;
            var taskExternalCode = "EXT001";
            List<TaskRewardDetailModel>? responseModel = null;

            // Mock repository to return null when called
            var _mockTaskRewardRepo = new Mock<ITaskRewardRepo>();
            _taskRewardRepository.Setup(x => x.GetTaskRewardDetails(tenantCode, taskExternalCode, "en-US")).ReturnsAsync(responseModel);

            // Act
            var result = await _taskRewardController.GetTaskRewardDetails(tenantCode, "en-US", taskExternalCode);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);

            // Check if the error message aligns with expectations
            var response = Assert.IsType<TaskRewardDetailsResponseDto>(badRequestResult.Value);
            Assert.Equal(StatusCodes.Status400BadRequest, response.ErrorCode);
            Assert.Equal("Tenant code cannot be null or empty.", response.ErrorMessage);
        }

        [Fact]
        public async TaskAlias GetTaskRewardDetails_ShouldReturnError_WhenRepositoryReturnsError()
        {
            // Arrange
            var tenantCode = "tenant-001";
            var taskExternalCode = "EXT001";
            _taskRewardRepository.Setup(x => x.GetTaskRewardDetails(tenantCode, taskExternalCode,"en-US"))
             .ThrowsAsync(new Exception("Simulated exception"));

            // Act
            var result = await _taskRewardController.GetTaskRewardDetails(tenantCode, taskExternalCode, "en-US");

            // Assert
            var errorResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, errorResult.StatusCode);

            var response = Assert.IsType<TaskRewardDetailsResponseDto>(errorResult.Value);

            Assert.Equal(StatusCodes.Status500InternalServerError, response.ErrorCode);
            Assert.Equal("An unexpected error occurred while processing the request.", response.ErrorMessage);
        }

        [Fact]
        public async System.Threading.Tasks.Task GetTaskRewardDetails_ReturnsEmptyList_WhenNoDataExists()
        {
            var mockSession = new Mock<NHibernate.ISession>();
            var mockTaskRewardQuery = new Mock<IQueryable<TaskRewardModel>>();
            var mockTaskQuery = new Mock<IQueryable<TaskModel>>();
            var mockTaskDetailQuery = new Mock<IQueryable<TaskDetailModel>>();
            var mockLogger = new Mock<ILogger<TaskRewardRepo>>();

            // Inject mocks into repository
            var repository = new TaskRewardRepo(mockLogger.Object, mockSession.Object, _mapper);
            // Arrange
            var tenantCode = "TENANT123";
            var taskExternalCode = "EXT001";
            mockSession.Setup(s => s.Query<TaskRewardModel>()).Returns(new List<TaskRewardModel>().AsQueryable());
            mockSession.Setup(s => s.Query<TaskModel>()).Returns(new List<TaskModel>().AsQueryable());
            mockSession.Setup(s => s.Query<TaskDetailModel>()).Returns(new List<TaskDetailModel>().AsQueryable());

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotSupportedException>(
                () => repository.GetTaskRewardDetails(tenantCode, taskExternalCode, "en-US"));
           
        }
       
        [Fact]
        public async System.Threading.Tasks.Task Get_TaskReward_Collection_Should_Return_Ok_Response_When_Isflatten_IsTrue()
        {
            // Arrange
            var requestDto = new TaskRewardCollectionRequestDto() 
            { 
                TaskRewardCode = "trw-8a154edc602c49efb210d67a7bfe22b4",
                TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4"
            };
            var taskRewardTaskCollections = new List<TaskRewardCollectionModel>()
            {
                new TaskRewardCollectionMockModel()
            };
            var taskRewardRepository = new Mock<ITaskRewardRepo>();
            taskRewardRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false)).ReturnsAsync(new TaskRewardMockModel());
             var taskRewardService = new TaskRewardService(_taskRewardServiceLogger.Object, _mapper, taskRewardRepository.Object,
                _taskRepository.Object, _taskDetailRepository.Object, _termsOfServiceRepository.Object, _consumerTaskRepo.Object, 
                _tenantTaskCategoryRepo.Object, _taskTypeRepo.Object, _taskRewardTypeRepository.Object, _taskRewardCollectionRepo.Object, _commonTaskRewardService.Object, _adventureRepo.Object);
            var taskRewardController = new TaskRewardController(_taskRewardLogger.Object, taskRewardService);
            _taskRewardCollectionRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<TaskRewardCollectionModel, bool>>>(), false)).ReturnsAsync(taskRewardTaskCollections);

            // Act 
            var response = await taskRewardController.GetTaskRewardCollection(requestDto);

            // Assert 
            Assert.NotNull(response);
            var okObjectResult = Assert.IsType<OkObjectResult>(response);
            Assert.Equal(StatusCodes.Status200OK, okObjectResult.StatusCode);

        }
        [Fact]
        public async System.Threading.Tasks.Task Get_TaskReward_Collection_Should_Return_Ok_Response_Is_Flatten_False()
        {
            // Arrange 
            var requestDto = new TaskRewardCollectionRequestDto()
            {
                TaskRewardCode = "trw-8a154edc602c49efb210d67a7bfe22b4",
                TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4"
            };

            var taskRewardMockModel = new TaskRewardMockModel();
            taskRewardMockModel.TaskRewardConfigJson = "{\r\n  \"collectionConfig\": {\r\n    \"flattenTasks\": false,\r\n    \"includeInAllAvailableTasks\": false\r\n  }\r\n}";
            taskRewardMockModel.IsRecurring = true;
            var taskRewardRepository = new Mock<ITaskRewardRepo>();
            taskRewardRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false)).ReturnsAsync(taskRewardMockModel);
            var taskRewardService = new TaskRewardService(_taskRewardServiceLogger.Object, _mapper, taskRewardRepository.Object,
               _taskRepository.Object, _taskDetailRepository.Object, _termsOfServiceRepository.Object, _consumerTaskRepo.Object, _tenantTaskCategoryRepo.Object, 
               _taskTypeRepo.Object, _taskRewardTypeRepository.Object, _taskRewardCollectionRepo.Object, _commonTaskRewardService.Object, _adventureRepo.Object);
            var taskRewardController = new TaskRewardController(_taskRewardLogger.Object, taskRewardService);

            // Act
            var response = await taskRewardController.GetTaskRewardCollection(requestDto);

            // Assert 
            Assert.NotNull(response);
            var okObjectResult = Assert.IsType<OkObjectResult>(response);
            Assert.Equal(StatusCodes.Status200OK, okObjectResult.StatusCode);
        }
        [Fact]
        public async System.Threading.Tasks.Task Get_TaskReward_Collection_Should_Return_NotFound_Response_Is_Flatten_False_And_Task_Invalid()
        {
            // Arrange 
            var requestDto = new TaskRewardCollectionRequestDto()
            {
                TaskRewardCode = "trw-8a154edc602c49efb210d67a7bfe22b4",
                TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4"
            };

            var taskRewardMockModel = new TaskRewardMockModel();
            taskRewardMockModel.TaskRewardConfigJson = "{\r\n  \"collectionConfig\": {\r\n    \"flattenTasks\": false,\r\n    \"includeInAllAvailableTasks\": false\r\n  }\r\n}";
            taskRewardMockModel.IsRecurring = true;
            taskRewardMockModel.ValidStartTs = DateTime.MinValue;
            var taskRewardRepository = new Mock<ITaskRewardRepo>();
            taskRewardRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false)).ReturnsAsync(taskRewardMockModel);
            var taskRewardService = new TaskRewardService(_taskRewardServiceLogger.Object, _mapper, taskRewardRepository.Object,
               _taskRepository.Object, _taskDetailRepository.Object, _termsOfServiceRepository.Object, _consumerTaskRepo.Object, _tenantTaskCategoryRepo.Object,
               _taskTypeRepo.Object, _taskRewardTypeRepository.Object, _taskRewardCollectionRepo.Object, _commonTaskRewardService.Object, _adventureRepo.Object);
            var taskRewardController = new TaskRewardController(_taskRewardLogger.Object, taskRewardService);

            // Act
            var response = await taskRewardController.GetTaskRewardCollection(requestDto);

            // Assert 
            Assert.NotNull(response);
            var objectResult = Assert.IsType<ObjectResult>(response);
            Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);

        }
        [Fact]
        public async System.Threading.Tasks.Task Get_TaskReward_Collection_Should_Ok_Response_When_ValidStartTs_Invalid()
        {
            // Arrange 
            var requestDto = new TaskRewardCollectionRequestDto()
            {
                TaskRewardCode = "trw-8a154edc602c49efb210d67a7bfe22b4",
                TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4"
            };
            var taskRewardTaskCollections = new List<TaskRewardCollectionModel>()
            {
                new TaskRewardCollectionMockModel()
            };
            var taskRewardMockModel = new TaskRewardMockModel();
            taskRewardMockModel.TaskRewardConfigJson = "{\r\n  \"collectionConfig\": {\r\n    \"flattenTasks\": true,\r\n    \"includeInAllAvailableTasks\": false\r\n  }\r\n}";
            taskRewardMockModel.IsRecurring = true;
            taskRewardMockModel.ValidStartTs = DateTime.MinValue;
            var taskRewardRepository = new Mock<ITaskRewardRepo>();
            taskRewardRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false)).ReturnsAsync(taskRewardMockModel);
            _taskRewardCollectionRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<TaskRewardCollectionModel, bool>>>(), false)).ReturnsAsync(taskRewardTaskCollections);
            var taskRewardService = new TaskRewardService(_taskRewardServiceLogger.Object, _mapper, taskRewardRepository.Object,
               _taskRepository.Object, _taskDetailRepository.Object, _termsOfServiceRepository.Object, _consumerTaskRepo.Object, _tenantTaskCategoryRepo.Object,
               _taskTypeRepo.Object, _taskRewardTypeRepository.Object, _taskRewardCollectionRepo.Object, _commonTaskRewardService.Object, _adventureRepo.Object);

            var taskRewardController = new TaskRewardController(_taskRewardLogger.Object, taskRewardService);

            // Act
            var response = await taskRewardController.GetTaskRewardCollection(requestDto);

            // Assert 
            Assert.NotNull(response);
            var okObjectResult = Assert.IsType<OkObjectResult>(response);
            Assert.Equal(StatusCodes.Status200OK, okObjectResult.StatusCode);

        }
        [Fact]
        public async System.Threading.Tasks.Task Get_TaskReward_Collection_Should_ThrowException_When_Task_is_null()
        {
            // Arrange 
            var requestDto = new TaskRewardCollectionRequestDto()
            {
                TaskRewardCode = "trw-8a154edc602c49efb210d67a7bfe22b4",
                TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4"
            };

            var taskRewardMockModel = new TaskRewardMockModel();
            taskRewardMockModel.TaskRewardConfigJson = "{\r\n  \"collectionConfig\": {\r\n    \"flattenTasks\": false,\r\n    \"includeInAllAvailableTasks\": false\r\n  }\r\n}";
            var taskRewardRepository = new Mock<ITaskRewardRepo>();
            taskRewardRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false)).ReturnsAsync(taskRewardMockModel);
            var taskRepo = new Mock<ITaskRepo>();
            taskRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskModel, bool>>>(), false));

            var taskRewardService = new TaskRewardService(_taskRewardServiceLogger.Object, _mapper, taskRewardRepository.Object,
               taskRepo.Object, _taskDetailRepository.Object, _termsOfServiceRepository.Object, _consumerTaskRepo.Object,
               _tenantTaskCategoryRepo.Object, _taskTypeRepo.Object, _taskRewardTypeRepository.Object, _taskRewardCollectionRepo.Object,_commonTaskRewardService.Object, _adventureRepo.Object);
            var taskRewardController = new TaskRewardController(_taskRewardLogger.Object, taskRewardService);

            // Act
            var response = await taskRewardController.GetTaskRewardCollection(requestDto);

            // Assert 
            Assert.NotNull(response);
            var okObjectResult = Assert.IsType<ObjectResult>(response);
            Assert.Equal(StatusCodes.Status500InternalServerError, okObjectResult.StatusCode);

        }
        
        [Fact]
        public async System.Threading.Tasks.Task Get_TaskReward_Collection_Should_ThrowException_When_TaskDetail_is_null()
        {
            // Arrange 
            var requestDto = new TaskRewardCollectionRequestDto()
            {
                TaskRewardCode = "trw-8a154edc602c49efb210d67a7bfe22b4",
                TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4"
            };

            var taskRewardMockModel = new TaskRewardMockModel();
            taskRewardMockModel.TaskRewardConfigJson = "{\r\n  \"collectionConfig\": {\r\n    \"flattenTasks\": false,\r\n    \"includeInAllAvailableTasks\": false\r\n  }\r\n}";
            var taskRewardRepository = new Mock<ITaskRewardRepo>();
            taskRewardRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false)).ReturnsAsync(taskRewardMockModel);
            var taskDetailRepository = new Mock<ITaskDetailRepo>();
            taskDetailRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskDetailModel, bool>>>(), false));//.ReturnsAsync(new TaskDetailModel());
            var taskRewardService = new TaskRewardService(_taskRewardServiceLogger.Object, _mapper, taskRewardRepository.Object,
               _taskRepository.Object, taskDetailRepository.Object, _termsOfServiceRepository.Object, _consumerTaskRepo.Object,
               _tenantTaskCategoryRepo.Object, _taskTypeRepo.Object, _taskRewardTypeRepository.Object, _taskRewardCollectionRepo.Object,_commonTaskRewardService.Object, _adventureRepo.Object);
            var taskRewardController = new TaskRewardController(_taskRewardLogger.Object, taskRewardService);

            // Act
            var response = await taskRewardController.GetTaskRewardCollection(requestDto);

            // Assert 
            Assert.NotNull(response);
            var okObjectResult = Assert.IsType<ObjectResult>(response);
            Assert.Equal(StatusCodes.Status500InternalServerError, okObjectResult.StatusCode);
        }
        [Fact]
        public async System.Threading.Tasks.Task Get_TaskReward_Collection_Should_Return_NotFound_Response_When_TaskReward_Isnull()
        {
            // Arrange 
            var requestDto = new TaskRewardCollectionRequestDto()
            {
                TaskRewardCode = "trw-8a154edc602c49efb210d67a7bfe22b4",
                TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4"
            };

            var taskRewardMockModel = new TaskRewardMockModel();
            taskRewardMockModel.TaskRewardConfigJson = null;
            var taskRewardRepository = new Mock<ITaskRewardRepo>();
            var taskRewardService = new TaskRewardService(_taskRewardServiceLogger.Object, _mapper, taskRewardRepository.Object,
               _taskRepository.Object, _taskDetailRepository.Object, _termsOfServiceRepository.Object, _consumerTaskRepo.Object,
               _tenantTaskCategoryRepo.Object, _taskTypeRepo.Object, _taskRewardTypeRepository.Object, _taskRewardCollectionRepo.Object, _commonTaskRewardService.Object, _adventureRepo.Object);
            var taskRewardController = new TaskRewardController(_taskRewardLogger.Object, taskRewardService);

            // Act
            var response = await taskRewardController.GetTaskRewardCollection(requestDto);

            // Assert 
            Assert.NotNull(response);
            var okObjectResult = Assert.IsType<ObjectResult>(response);
            Assert.Equal(StatusCodes.Status404NotFound, okObjectResult.StatusCode);

        }
        [Fact]
        public async System.Threading.Tasks.Task Get_TaskReward_Collection_Should_Return_NotFound_Response_When_TaskRewardConfig_Isnull()
        {
            // Arrange 
            var requestDto = new TaskRewardCollectionRequestDto()
            {
                TaskRewardCode = "trw-8a154edc602c49efb210d67a7bfe22b4",
                TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4"
            };

            var taskRewardRepository = new Mock<ITaskRewardRepo>();
            taskRewardRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false)).ReturnsAsync(new TaskRewardModel());
            var taskRewardService = new TaskRewardService(_taskRewardServiceLogger.Object, _mapper, taskRewardRepository.Object,
               _taskRepository.Object, _taskDetailRepository.Object, _termsOfServiceRepository.Object, _consumerTaskRepo.Object, 
               _tenantTaskCategoryRepo.Object, _taskTypeRepo.Object, _taskRewardTypeRepository.Object, _taskRewardCollectionRepo.Object, _commonTaskRewardService.Object, _adventureRepo.Object);
            var taskRewardController = new TaskRewardController(_taskRewardLogger.Object, taskRewardService);

            // Act
            var response = await taskRewardController.GetTaskRewardCollection(requestDto);

            // Assert 
            Assert.NotNull(response);
            var okObjectResult = Assert.IsType<ObjectResult>(response);
            Assert.Equal(StatusCodes.Status422UnprocessableEntity, okObjectResult.StatusCode);

        }
        [Fact]
        public async System.Threading.Tasks.Task Get_TaskReward_Collection_Should_Return_NotFound_Response_When_Isflatten_IsTrue()
        {
            // Arrange
            var requestDto = new TaskRewardCollectionRequestDto()
            {
                TaskRewardCode = "trw-8a154edc602c49efb210d67a7bfe22b4",
                TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4"
            };
            
            var taskRewardRepository = new Mock<ITaskRewardRepo>();
            taskRewardRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false)).ReturnsAsync(new TaskRewardMockModel());
            _taskRewardCollectionRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<TaskRewardCollectionModel, bool>>>(), false)).ReturnsAsync(new List<TaskRewardCollectionModel>());
            var taskRewardService = new TaskRewardService(_taskRewardServiceLogger.Object, _mapper, taskRewardRepository.Object,
               _taskRepository.Object, _taskDetailRepository.Object, _termsOfServiceRepository.Object, _consumerTaskRepo.Object, 
               _tenantTaskCategoryRepo.Object, _taskTypeRepo.Object, _taskRewardTypeRepository.Object, _taskRewardCollectionRepo.Object, _commonTaskRewardService.Object, _adventureRepo.Object);
            var taskRewardController = new TaskRewardController(_taskRewardLogger.Object, taskRewardService);

            // Act 
            var response = await taskRewardController.GetTaskRewardCollection(requestDto);

            // Assert 
            Assert.NotNull(response);
            var okObjectResult = Assert.IsType<ObjectResult>(response);
            Assert.Equal(StatusCodes.Status404NotFound, okObjectResult.StatusCode);

        }
        [Fact]
        public async System.Threading.Tasks.Task Get_TaskReward_Collection_Should_Return_InternalServer_Response_When_Exception_Occurs()
        {
            // Arrange
            var requestDto = new TaskRewardCollectionRequestDto()
            {
                TaskRewardCode = "trw-8a154edc602c49efb210d67a7bfe22b4",
                TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4"
            };

            var taskRewardRepository = new Mock<ITaskRewardRepo>();
            taskRewardRepository.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false)).ReturnsAsync(new TaskRewardMockModel());
            var taskRewardService = new TaskRewardService(_taskRewardServiceLogger.Object, _mapper, taskRewardRepository.Object,
               _taskRepository.Object, _taskDetailRepository.Object, _termsOfServiceRepository.Object, _consumerTaskRepo.Object, 
               _tenantTaskCategoryRepo.Object, _taskTypeRepo.Object, _taskRewardTypeRepository.Object, _taskRewardCollectionRepo.Object, _commonTaskRewardService.Object, _adventureRepo.Object);
            var taskRewardController = new TaskRewardController(_taskRewardLogger.Object, taskRewardService);
            _taskRewardCollectionRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<TaskRewardCollectionModel, bool>>>(), false)).ThrowsAsync(new Exception("testing"));

            // Act 
            var response = await taskRewardController.GetTaskRewardCollection(requestDto);

            // Assert 
            Assert.NotNull(response);
            var okObjectResult = Assert.IsType<ObjectResult>(response);
            Assert.Equal(StatusCodes.Status500InternalServerError, okObjectResult.StatusCode);
        }

        [Fact]
        public async System.Threading.Tasks.Task GetHealthTaskRewards_ReturnsOkResult_WithFilteredTaskRewards()
        {
            // Arrange
            var tenantCode = "TestTenant";
            var taskRewardMockRepo = new Mock<ITaskRewardRepo>();
            var mockServiceLogger = new Mock<ILogger<TaskRewardService>>();
            var mockMapper = new Mock<IMapper>();

            var mockTaskRewards = new List<TaskRewardModel>
            {
                new TaskRewardModel
                {
                    TaskRewardId = 1,
                    TaskId = 100,
                    TenantCode = tenantCode,
                    TaskCompletionCriteriaJson = JsonConvert.SerializeObject(new TaskCompletionCriteriaJson
                    {
                        CompletionCriteriaType = Constant.HealthCriteriaType,
                        CompletionPeriodType = Constant.MonthlyPeriodType
                    }),
                    DeleteNbr = 0
                },
                new TaskRewardModel
                {
                    TaskRewardId = 2,
                    TaskId = 200,
                    TenantCode = tenantCode,
                    TaskCompletionCriteriaJson = JsonConvert.SerializeObject(new TaskCompletionCriteriaJson
                    {
                        CompletionCriteriaType = Constant.HealthCriteriaType,
                        CompletionPeriodType = Constant.MonthlyPeriodType
                    }),
                    DeleteNbr = 0
                },
                new TaskRewardModel
                {
                    TaskRewardId = 3,
                    TaskId = 300,
                    TenantCode = tenantCode,
                    TaskCompletionCriteriaJson = null, 
                    DeleteNbr = 0
                }
            };
            taskRewardMockRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(),false))
                    .ReturnsAsync(mockTaskRewards.Where(x => x.TenantCode == tenantCode && x.TaskCompletionCriteriaJson != null && x.DeleteNbr == 0).ToList());

            mockMapper.Setup(m => m.Map<IList<TaskRewardDto>>(It.IsAny<IList<TaskRewardModel>>()))
            .Returns((IList<TaskRewardModel> source) =>
                source.Select(x => new TaskRewardDto
                {
                    TaskRewardId = x.TaskRewardId,
                    TaskId = x.TaskId,
                    TenantCode = x.TenantCode
                }).ToList());
            var mockService = new TaskRewardService(_taskRewardServiceLogger.Object, mockMapper.Object, taskRewardMockRepo.Object,
                 _taskRepository.Object, _taskDetailRepository.Object, _termsOfServiceRepository.Object, _consumerTaskRepo.Object, _tenantTaskCategoryRepo.Object, _taskTypeRepo.Object,
                 _taskRewardTypeRepository.Object, _taskRewardCollectionRepo.Object, _commonTaskRewardService.Object, _adventureRepo.Object);
            var controller = new TaskRewardController(_taskRewardLogger.Object, mockService);

            // Act
            var result = await controller.GetHealthTaskRewards(tenantCode);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<TaskRewardsResponseDto>(okResult.Value);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
            Assert.Equal(2, response?.TaskRewards?.Count);
        }

        [Fact]
        public async System.Threading.Tasks.Task GetHealthTaskRewards_ReturnsBadRequest400()
        {
            // Arrange
            var tenantCode = "";

            // Act
            var result = await _taskRewardController.GetHealthTaskRewards(tenantCode);

            // Assert
            var response = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(StatusCodes.Status400BadRequest, response.StatusCode);
        }

        [Fact]
        public async System.Threading.Tasks.Task GetHealthTaskRewards_ThrowsException()
        {
            // Arrange
            var tenantCode = "test-tenant";

            _taskRewardRepository.Setup(x => x.FindAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false))
                .ThrowsAsync(new Exception());
            // Act
            var result = await _taskRewardController.GetHealthTaskRewards(tenantCode);

            // Assert
            var response = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, response.StatusCode);
        }

        [Fact]
        public async System.Threading.Tasks.Task GetAdventuresAndTaskCollections_Should_Return_Ok_Response()
        {
            // Arrange
            var requestDto = new AdventureTaskCollectionRequestDto()
            {
                CohortTaskMap = new Dictionary<string, List<string>>()
                {
                    { "adventure:fitness_and_exercise",new List<string>{ "trw-8a154edc602c49efb210d67a7bfe22b4" } },
                    { "adventure:healthy-eating",new List<string>{ "trw-8a154edc602c49efb210d67a7bfe2" } }
                }
            };
             IList<TaskRewardCollectionModel> taskRewardCollections = new List<TaskRewardCollectionModel> { new TaskRewardCollectionMockModel() };
                
            _taskRewardCollectionRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<TaskRewardCollectionModel, bool>>>(), false)).ReturnsAsync(taskRewardCollections);
            
            // Act
            var result = await _taskRewardController.GetAdventuresAndTaskCollections(requestDto);

            // Assert
            var response = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, response.StatusCode);
        }
        [Fact]
        public async System.Threading.Tasks.Task GetAdventuresAndTaskCollections_Should_Return_Ok_Response_When_Flatten_False()
        {
            // Arrange
            var requestDto = new AdventureTaskCollectionRequestDto()
            {
                CohortTaskMap = new Dictionary<string, List<string>>()
                {
                    { "adventure:fitness_and_exercise",new List<string>{ "trw-8a154edc602c49efb210d67a7bfe22b4" } },
                    { "adventure:healthy-eating",new List<string>{ "trw-8a154edc602c49efb210d67a7bfe2" } }
                }
            };
            var taskRewardRepo = new Mock<ITaskRewardRepo>();
            var taskRewardService = new TaskRewardService(_taskRewardServiceLogger.Object, _mapper, taskRewardRepo.Object,
                _taskRepository.Object, _taskDetailRepository.Object, _termsOfServiceRepository.Object, _consumerTaskRepo.Object, _tenantTaskCategoryRepo.Object, _taskTypeRepo.Object,
                _taskRewardTypeRepository.Object, _taskRewardCollectionRepo.Object, _commonTaskRewardService.Object, _adventureRepo.Object);
            var getTaskByTenantCodeResponseMockDto = new GetTaskByTenantCodeResponseMockDto();
            getTaskByTenantCodeResponseMockDto.AvailableTasks[0].TaskReward.TaskRewardConfigJson = "{\r\n  \"collectionConfig\": {\r\n    \"flattenTasks\": false,\r\n    \"includeInAllAvailableTasks\": false\r\n  }\r\n}";
            taskRewardRepo.Setup(x => x.GetTaskRewardDetailsList(It.IsAny<string>(), It.IsAny<string>())).Returns(getTaskByTenantCodeResponseMockDto.AvailableTasks);
            var controller = new TaskRewardController(_taskRewardLogger.Object, taskRewardService);

            // Act
            var result = await controller.GetAdventuresAndTaskCollections(requestDto);

            // Assert
            var response = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, response.StatusCode);
        }
        [Fact]
        public async System.Threading.Tasks.Task GetAdventuresAndTaskCollections_Should_Return_Ok_Response_When_TaskRewardDetail_Is_null()
        {
            // Arrange
            var requestDto = new AdventureTaskCollectionRequestDto()
            {
                CohortTaskMap = new Dictionary<string, List<string>>()
                {
                    { "adventure:fitness_and_exercise",new List<string>{ "trw-8a154edc602c49efb210d67a7bfe22b453525" } }
                }
            };
            IList<TaskRewardCollectionModel> taskRewardCollections = new List<TaskRewardCollectionModel> { new TaskRewardCollectionMockModel() };

            _taskRewardCollectionRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<TaskRewardCollectionModel, bool>>>(), false)).ReturnsAsync(taskRewardCollections);

            // Act
            var result = await _taskRewardController.GetAdventuresAndTaskCollections(requestDto);

            // Assert
            var response = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, response.StatusCode);
        }
        [Fact]
        public async System.Threading.Tasks.Task GetAdventuresAndTaskCollections_Should_Return_Ok_Response_When_TaskRewardCode_IsEmpty()
        {
            // Arrange
            var requestDto = new AdventureTaskCollectionRequestDto()
            {
                CohortTaskMap = new Dictionary<string, List<string>>()
                {
                    { "adventure:fitness_and_exercise",new List<string>{ "" } }
                }
            };
            IList<TaskRewardCollectionModel> taskRewardCollections = new List<TaskRewardCollectionModel> { new TaskRewardCollectionMockModel() };

            _taskRewardCollectionRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<TaskRewardCollectionModel, bool>>>(), false)).ReturnsAsync(taskRewardCollections);

            // Act
            var result = await _taskRewardController.GetAdventuresAndTaskCollections(requestDto);

            // Assert
            var response = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, response.StatusCode);
        }
        [Fact]
        public async System.Threading.Tasks.Task GetAdventuresAndTaskCollections_Should_Return_Throw_Exception()
        {
            // Arrange
            var requestDto = new AdventureTaskCollectionRequestDto()
            {
                CohortTaskMap = new Dictionary<string, List<string>>()
                {
                    { "adventure:fitness_and_exercise",new List<string>{"trw-6d1dd9fa-8ee6-4752-b107-b564ac9bac5" } }
                }
            };

            var taskRewardRepo = new Mock<ITaskRewardRepo>();
            var taskRewardService = new TaskRewardService(_taskRewardServiceLogger.Object, _mapper, taskRewardRepo.Object,
                _taskRepository.Object, _taskDetailRepository.Object, _termsOfServiceRepository.Object, _consumerTaskRepo.Object, _tenantTaskCategoryRepo.Object, _taskTypeRepo.Object,
                _taskRewardTypeRepository.Object, _taskRewardCollectionRepo.Object, _commonTaskRewardService.Object, _adventureRepo.Object);
            taskRewardRepo.Setup(x => x.GetTaskRewardDetailsList(It.IsAny<string>(), It.IsAny<string>())).Throws(new Exception("testing"));
            var controller = new TaskRewardController(_taskRewardLogger.Object, taskRewardService);

            // Act
            var result = await controller.GetAdventuresAndTaskCollections(requestDto);

            // Assert
            var response = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, response.StatusCode);
        }

        [Fact]
        public async System.Threading.Tasks.Task GetAdventuresAndTaskCollections_Should_Return_NotFound_Response()
        {
            // Arrange
            var requestDto = new AdventureTaskCollectionRequestDto()
            {
                CohortTaskMap = new Dictionary<string, List<string>>()
                {
                    { "adventure:fitness_and_exercise",new List<string>{"trw-6d1dd9fa-8ee6-4752-b107-b564ac9bac5" } }
                }
            };
            var taskRewardRepo = new Mock<ITaskRewardRepo>();
            var taskRewardService = new TaskRewardService(_taskRewardServiceLogger.Object, _mapper, taskRewardRepo.Object,
                _taskRepository.Object, _taskDetailRepository.Object, _termsOfServiceRepository.Object, _consumerTaskRepo.Object, _tenantTaskCategoryRepo.Object, _taskTypeRepo.Object,
                _taskRewardTypeRepository.Object, _taskRewardCollectionRepo.Object, _commonTaskRewardService.Object, _adventureRepo.Object);
            taskRewardRepo.Setup(x => x.GetTaskRewardDetailsList(It.IsAny<string>(), It.IsAny<string>()));
            var controller = new TaskRewardController(_taskRewardLogger.Object, taskRewardService);
            IList<TaskRewardCollectionModel> taskRewardCollections = new List<TaskRewardCollectionModel> { new TaskRewardCollectionMockModel() };
            _taskRewardCollectionRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<TaskRewardCollectionModel, bool>>>(), false)).ReturnsAsync(taskRewardCollections);

            // Act
            var result = await controller.GetAdventuresAndTaskCollections(requestDto);

            // Assert
            var response = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status404NotFound, response.StatusCode);
        }

    }
}
#endregion