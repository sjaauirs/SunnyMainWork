using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using SunnyRewards.Helios.Task.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Task.Infrastructure.Services.Interface;
using SunnyRewards.Helios.Task.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using Xunit;
using SunnyRewards.Helios.Task.Core.Domain.Models;
using System.Linq.Expressions;
using SunnyRewards.Helios.Task.Infrastructure.Mappings.MappingProfile;
using Amazon.CloudWatch;
using Microsoft.AspNetCore.Http;

namespace SunnyRewards.Helios.Task.UnitTest.Helpers
{
    public class TaskSubtaskMappingImportServiceTests
    {
        private readonly Mock<ILogger<TaskSubtaskMappingImportService>> _loggerMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ITaskRewardRepo> _taskRepoMock;
        private readonly Mock<ISubTaskRepo> _subTaskRepoMock;
        private readonly Mock<ISubtaskService> _subTaskServiceMock;
        private readonly Mock<ITaskExternalMappingRepo> _taskExternalMappingRepoMock;
        private readonly Mock<ITenantTaskCategoryRepo> _tenantTaskCategoryRepoMock;
        private readonly Mock<ITenantTaskCategoryService> _tenantTaskCategoryServiceMock;
        private readonly Mock<ITaskCategoryRepo> _taskCategoryRepoMock;
        private readonly Mock<ITaskService> _taskServiceMock;
        private readonly TaskSubtaskMappingImportService _service;

        public TaskSubtaskMappingImportServiceTests()
        {
            _loggerMock = new Mock<ILogger<TaskSubtaskMappingImportService>>();
            _mapperMock = new Mock<IMapper>();
            _taskRepoMock = new Mock<ITaskRewardRepo>();
            _subTaskRepoMock = new Mock<ISubTaskRepo>();
            _subTaskServiceMock = new Mock<ISubtaskService>();
            _taskExternalMappingRepoMock = new Mock<ITaskExternalMappingRepo>();
            _tenantTaskCategoryRepoMock = new Mock<ITenantTaskCategoryRepo>();
            _tenantTaskCategoryServiceMock = new Mock<ITenantTaskCategoryService>();
            _taskCategoryRepoMock = new Mock<ITaskCategoryRepo>();
            _taskServiceMock = new Mock<ITaskService>();

            _service = new TaskSubtaskMappingImportService(
                _loggerMock.Object,
                _mapperMock.Object,
                _taskRepoMock.Object,
                _subTaskRepoMock.Object,
                _subTaskServiceMock.Object,
                _taskExternalMappingRepoMock.Object,
                _tenantTaskCategoryRepoMock.Object,
                _taskCategoryRepoMock.Object,
                _tenantTaskCategoryServiceMock.Object,
                _taskServiceMock.Object
            );
        }
        [Fact]
        public async void ImportSubtask_ShouldMapAndCallServiceMethods()
        {
            // Arrange
            var requestDto = new ImportTaskRewardDetailsRequestDto
            {
                SubTasks = new List<SubTaskDto>
                { new  SubTaskDto { ParentTaskRewardId=1, ChildTaskRewardId=2, ConfigJson = "{}" }
                    }

            };
            var taskIdCodePair = new Dictionary<long, string>();
            taskIdCodePair.Add(1, "123");
            taskIdCodePair.Add(2, "1245");

            _taskRepoMock.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false)).ReturnsAsync(new TaskRewardModel { TaskRewardId=1,TaskExternalCode = "Test Task",  TaskRewardCode = "123" });
            _subTaskRepoMock.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<SubTaskModel, bool>>>(), false)).ReturnsAsync(new SubTaskModel { ParentTaskRewardId = 1, ChildTaskRewardId = 2 });


            var subtaskUpdateRequestDto = new SubTaskUpdateRequestDto();
            _mapperMock.Setup(m => m.Map<SubTaskUpdateRequestDto>(It.IsAny<object>()))
                .Returns(subtaskUpdateRequestDto);

            _subTaskServiceMock.Setup(s => s.UpdateSubtask(It.IsAny<SubTaskUpdateRequestDto>()))
                .ReturnsAsync(new SubtaskResponseDto());

            // Act
            var response = await _service.ImportSubtask(requestDto, taskIdCodePair);

            // Assert
            _mapperMock.Verify(m => m.Map<SubTaskUpdateRequestDto>(It.IsAny<object>()), Times.AtLeastOnce);
        }
        [Fact]
        public async void ImportSubtask_ShouldMapAndCreateStutaskServiceMethods()
        {
            // Arrange
            var requestDto = new ImportTaskRewardDetailsRequestDto
            {
                SubTasks = new List<SubTaskDto>
                { new  SubTaskDto { ParentTaskRewardId=1, ChildTaskRewardId=2, ConfigJson = "{}" }
                    }

            };

            var taskIdCodePair = new Dictionary<long, string>();
            taskIdCodePair.Add(1, "123");
            taskIdCodePair.Add(2, "1245");

            _taskRepoMock.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false)).ReturnsAsync(new TaskRewardModel { TaskRewardId = 1, TaskExternalCode = "Test Task", TaskRewardCode = "123" });
            _subTaskRepoMock.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<SubTaskModel, bool>>>(), false));


            var subtaskUpdateRequestDto = new SubtaskRequestDto { ParentTaskRewardCode = "123", ChildTaskRewardCode = "1245", Subtask = new PostSubTaskDto { ParentTaskRewardId = 1, ChildTaskRewardId = 2, ConfigJson = "{}", CreateUser = "tre" } };
            _mapperMock.Setup(m => m.Map<SubtaskRequestDto>(It.IsAny<object>()))
                .Returns(subtaskUpdateRequestDto);

            _subTaskServiceMock.Setup(s => s.CreateSubTask(It.IsAny<SubtaskRequestDto>()))
                .ReturnsAsync(new SubtaskResponseDto());

            // Act
            var response = await _service.ImportSubtask(requestDto, taskIdCodePair);

            // Assert
            Assert.NotNull(response);
        }
        [Fact]
        public async void ImportSubtask_ShouldMapAndNotCreateStutaskServiceMethods()
        {
            // Arrange
            var requestDto = new ImportTaskRewardDetailsRequestDto
            {
                SubTasks = new List<SubTaskDto>
                { new  SubTaskDto { ParentTaskRewardId=1, ChildTaskRewardId=2, ConfigJson = "{}" }
                    }

            };

            var taskIdCodePair = new Dictionary<long, string>();
            taskIdCodePair.Add(1, "123");
            taskIdCodePair.Add(2, "1245");

            _taskRepoMock.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false)).ReturnsAsync(new TaskRewardModel { TaskRewardId = 1, TaskExternalCode = "Test Task", TaskRewardCode = "123" });
            _subTaskRepoMock.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<SubTaskModel, bool>>>(), false));


            var subtaskUpdateRequestDto = new SubtaskRequestDto { ParentTaskRewardCode = "123", ChildTaskRewardCode = "1245", Subtask = new PostSubTaskDto { ParentTaskRewardId = 1, ChildTaskRewardId = 2, ConfigJson = "{}", CreateUser = "tre" } };
            _mapperMock.Setup(m => m.Map<SubtaskRequestDto>(It.IsAny<object>()))
                .Returns(subtaskUpdateRequestDto);

            _subTaskServiceMock.Setup(s => s.CreateSubTask(It.IsAny<SubtaskRequestDto>()))
                .ReturnsAsync(new SubtaskResponseDto {ErrorCode= StatusCodes.Status500InternalServerError });

            // Act
            var response = await _service.ImportSubtask(requestDto, taskIdCodePair);

            // Assert
            Assert.NotNull(response);
        }
        [Fact]
        public async void ImportSubtask_ShouldMapAndcatchCreateStutaskServiceException()
        {
            // Arrange
            var requestDto = new ImportTaskRewardDetailsRequestDto
            {
                SubTasks = new List<SubTaskDto>
                { new  SubTaskDto { ParentTaskRewardId=1, ChildTaskRewardId=2, ConfigJson = "{}" }
                    }

            };

            var taskIdCodePair = new Dictionary<long, string>();
            taskIdCodePair.Add(1, "123");
            taskIdCodePair.Add(2, "1245");

            _taskRepoMock.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false)).ReturnsAsync(new TaskRewardModel { TaskRewardId = 1, TaskExternalCode = "Test Task", TaskRewardCode = "123" });
            _subTaskRepoMock.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<SubTaskModel, bool>>>(), false)).ThrowsAsync(new Exception("Simulated exception"));
            


            var subtaskUpdateRequestDto = new SubtaskRequestDto { ParentTaskRewardCode = "123", ChildTaskRewardCode = "1245", Subtask = new PostSubTaskDto { ParentTaskRewardId = 1, ChildTaskRewardId = 2, ConfigJson = "{}", CreateUser = "tre" } };
            _mapperMock.Setup(m => m.Map<SubtaskRequestDto>(It.IsAny<object>()))
                .Returns(subtaskUpdateRequestDto);

            _subTaskServiceMock.Setup(s => s.CreateSubTask(It.IsAny<SubtaskRequestDto>()))
                .ReturnsAsync(new SubtaskResponseDto {ErrorCode= StatusCodes.Status500InternalServerError });

            // Act
            var response = await _service.ImportSubtask(requestDto, taskIdCodePair);

            // Assert
            Assert.NotNull(response);
        }
        [Fact]
        public async void ImportTenantTaskCategoryMapping_ShouldCreateCategoryIfNotExists()
        {
            // Arrange
            var requestDto = new ImportTaskRewardDetailsRequestDto
            {
                TenantCode = "Tenant123",
                TenantTaskCategory = new List<ExportTenantTaskCategoryDto>
                {
                    new ExportTenantTaskCategoryDto { TaskCategoryCode="123", TenantTaskCategory= new TenantTaskCategoryDto{TaskCategoryId = 1 } }

                }
            };

            _taskCategoryRepoMock.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<TaskCategoryModel, bool>>>(), false))
              ;

            _tenantTaskCategoryServiceMock.Setup(service => service.CreateTenantTaskCategory(It.IsAny<TenantTaskCategoryRequestDto>()))
                .ReturnsAsync(new BaseResponseDto());

            // Act
            var response = await _service.ImportTenantTaskCategoryMapping(requestDto);

            // Assert
            Assert.NotNull(response);
        }
        [Fact]
        public async void ImportTenantTaskCategoryMapping_ShouldCreateCategory()
        {
            // Arrange
            var requestDto = new ImportTaskRewardDetailsRequestDto
            {
                TenantCode = "Tenant123",
                TenantTaskCategory = new List<ExportTenantTaskCategoryDto>
                {
                    new ExportTenantTaskCategoryDto { TaskCategoryCode="123", TenantTaskCategory= new TenantTaskCategoryDto{TaskCategoryId = 1 } }

                }
            };

            _taskCategoryRepoMock.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<TaskCategoryModel, bool>>>(), false)).ReturnsAsync(new TaskCategoryModel { TaskCategoryId = 1, TaskCategoryCode = "123" })
              ;
            _mapperMock.Setup(m => m.Map<TenantTaskCategoryRequestDto>(It.IsAny<object>()))
             .Returns(new TenantTaskCategoryRequestDto { TaskCategoryId = 1 });

            _tenantTaskCategoryServiceMock.Setup(service => service.CreateTenantTaskCategory(It.IsAny<TenantTaskCategoryRequestDto>()))
                .ReturnsAsync(new BaseResponseDto());

            // Act
            var response = await _service.ImportTenantTaskCategoryMapping(requestDto);

            // Assert
            Assert.NotNull(response);
        }
        [Fact]
        public async void ImportTenantTaskCategoryMapping_ShouldErrorCreateCategory()
        {
            // Arrange
            var requestDto = new ImportTaskRewardDetailsRequestDto
            {
                TenantCode = "Tenant123",
                TenantTaskCategory = new List<ExportTenantTaskCategoryDto>
                {
                    new ExportTenantTaskCategoryDto { TaskCategoryCode="123", TenantTaskCategory= new TenantTaskCategoryDto{TaskCategoryId = 1 } }

                }
            };

            _taskCategoryRepoMock.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<TaskCategoryModel, bool>>>(), false)).ReturnsAsync(new TaskCategoryModel { TaskCategoryId = 1, TaskCategoryCode = "123" })
              ;
            _mapperMock.Setup(m => m.Map<TenantTaskCategoryRequestDto>(It.IsAny<object>()))
             .Returns(new TenantTaskCategoryRequestDto { TaskCategoryId = 1 });

            _tenantTaskCategoryServiceMock.Setup(service => service.CreateTenantTaskCategory(It.IsAny<TenantTaskCategoryRequestDto>()))
                .ReturnsAsync(new BaseResponseDto { ErrorCode=StatusCodes.Status500InternalServerError});

            // Act
            var response = await _service.ImportTenantTaskCategoryMapping(requestDto);

            // Assert
            Assert.NotNull(response);
        }
        [Fact]
        public async void ImportTenantTaskCategoryMapping_ShouldUpdateCategory()
        {
            // Arrange
            var requestDto = new ImportTaskRewardDetailsRequestDto
            {
                TenantCode = "Tenant123",
                TenantTaskCategory = new List<ExportTenantTaskCategoryDto>
                {
                    new ExportTenantTaskCategoryDto { TaskCategoryCode="123", TenantTaskCategory= new TenantTaskCategoryDto{TaskCategoryId = 1 } }

                }
            };

            _taskCategoryRepoMock.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<TaskCategoryModel, bool>>>(), false)).ReturnsAsync(new TaskCategoryModel { TaskCategoryId = 1, TaskCategoryCode = "123" })
              ;
            _tenantTaskCategoryRepoMock.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<TenantTaskCategoryModel, bool>>>(), false)).ReturnsAsync(new TenantTaskCategoryModel { TaskCategoryId = 1, TenantCode = "123" })
              ;
            _mapperMock.Setup(m => m.Map<TenantTaskCategoryRequestDto>(It.IsAny<object>()))
             .Returns(new TenantTaskCategoryRequestDto { TaskCategoryId = 1 });

            _tenantTaskCategoryServiceMock.Setup(service => service.UpdateTenantTaskCategory(It.IsAny<TenantTaskCategoryDto>()))
                .ReturnsAsync(new TenantTaskCategoryResponseDto());

            // Act
            var response = await _service.ImportTenantTaskCategoryMapping(requestDto);

            // Assert
            Assert.NotNull(response);
        }
        [Fact]
        public async void ImportTenantTaskCategoryMapping_ShoulderrorUpdateCategory()
        {
            // Arrange
            var requestDto = new ImportTaskRewardDetailsRequestDto
            {
                TenantCode = "Tenant123",
                TenantTaskCategory = new List<ExportTenantTaskCategoryDto>
                {
                    new ExportTenantTaskCategoryDto { TaskCategoryCode="123", TenantTaskCategory= new TenantTaskCategoryDto{TaskCategoryId = 1 } }

                }
            };

            _taskCategoryRepoMock.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<TaskCategoryModel, bool>>>(), false)).ReturnsAsync(new TaskCategoryModel { TaskCategoryId = 1, TaskCategoryCode = "123" })
              ;
            _tenantTaskCategoryRepoMock.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<TenantTaskCategoryModel, bool>>>(), false)).ReturnsAsync(new TenantTaskCategoryModel { TaskCategoryId = 1, TenantCode = "123" })
              ;
            _mapperMock.Setup(m => m.Map<TenantTaskCategoryRequestDto>(It.IsAny<object>()))
             .Returns(new TenantTaskCategoryRequestDto { TaskCategoryId = 1 });

            _tenantTaskCategoryServiceMock.Setup(service => service.UpdateTenantTaskCategory(It.IsAny<TenantTaskCategoryDto>()))
                .ReturnsAsync(new TenantTaskCategoryResponseDto { ErrorCode = StatusCodes.Status500InternalServerError });

            // Act
            var response = await _service.ImportTenantTaskCategoryMapping(requestDto);

            // Assert
            Assert.NotNull(response);
        }
        [Fact]
        public async void ImportTenantTaskCategoryMapping_ShouldCatchexceptionUpdateCategory()
        {
            // Arrange
            var requestDto = new ImportTaskRewardDetailsRequestDto
            {
                TenantCode = "Tenant123",
                TenantTaskCategory = new List<ExportTenantTaskCategoryDto>
                {
                    new ExportTenantTaskCategoryDto { TaskCategoryCode="123", TenantTaskCategory= new TenantTaskCategoryDto{TaskCategoryId = 1 } }

                }
            };

            _taskCategoryRepoMock.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<TaskCategoryModel, bool>>>(), false)).ReturnsAsync(new TaskCategoryModel { TaskCategoryId = 1, TaskCategoryCode = "123" })
              ;
            _tenantTaskCategoryRepoMock.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<TenantTaskCategoryModel, bool>>>(), false)).ThrowsAsync(new Exception("Simulated exception"));

            ;
            _mapperMock.Setup(m => m.Map<TenantTaskCategoryRequestDto>(It.IsAny<object>()))
             .Returns(new TenantTaskCategoryRequestDto { TaskCategoryId = 1 });

            _tenantTaskCategoryServiceMock.Setup(service => service.UpdateTenantTaskCategory(It.IsAny<TenantTaskCategoryDto>()))
              .ThrowsAsync(new Exception("Simulated exception"));

            // Act
            var response = await _service.ImportTenantTaskCategoryMapping(requestDto);

            // Assert
            Assert.NotNull(response);
        }
        [Fact]
        public async void ImportTaskExternalMapping_ShouldCreateMappingExists()
        {
            // Arrange
            var requestDto = new ImportTaskRewardDetailsRequestDto
            {
                TenantCode = "Tenant123",
                TaskExternalMappings = new List<TaskExternalMappingDto>
                {

                        new TaskExternalMappingDto { TaskExternalCode = "Ext123", TaskThirdPartyCode = "TP123" }

                }
            };
            _taskExternalMappingRepoMock.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<TaskExternalMappingModel, bool>>>(), false))
              ;
            _mapperMock.Setup(m => m.Map<TaskExternalMappingRequestDto>(It.IsAny<object>()))
             .Returns(new TaskExternalMappingRequestDto { CreateUser = "test", TaskExternalCode = "123", TaskThirdPartyCode = "123", TenantCode = "123" });
            _taskServiceMock.Setup(service => service.CreateTaskExternalMapping(It.IsAny<TaskExternalMappingRequestDto>()))
              .ReturnsAsync(new BaseResponseDto());

            // Act
            var response = await _service.ImportTaskExternalMapping(requestDto);

            // Assert
            Assert.NotNull(response);

        }
        [Fact]
        public async void ImportTaskExternalMapping_ShouldNotCreateMappingExists()
        {
            // Arrange
            var requestDto = new ImportTaskRewardDetailsRequestDto
            {
                TenantCode = "Tenant123",
                TaskExternalMappings = new List<TaskExternalMappingDto>
                {

                        new TaskExternalMappingDto { TaskExternalCode = "Ext123", TaskThirdPartyCode = "TP123" }

                }
            };
            _taskExternalMappingRepoMock.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<TaskExternalMappingModel, bool>>>(), false))
              ;
            _mapperMock.Setup(m => m.Map<TaskExternalMappingRequestDto>(It.IsAny<object>()))
             .Returns(new TaskExternalMappingRequestDto { CreateUser = "test", TaskExternalCode = "123", TaskThirdPartyCode = "123", TenantCode = "123" });
            _taskServiceMock.Setup(service => service.CreateTaskExternalMapping(It.IsAny<TaskExternalMappingRequestDto>()))
              .ReturnsAsync(new BaseResponseDto { ErrorCode= StatusCodes.Status500InternalServerError });

            // Act
            var response = await _service.ImportTaskExternalMapping(requestDto);

            // Assert
            Assert.NotNull(response);

        }
        [Fact]
        public async void ImportTaskExternalMapping_ShouldCatchCreateMappingException()
        {
            // Arrange
            var requestDto = new ImportTaskRewardDetailsRequestDto
            {
                TenantCode = "Tenant123",
                TaskExternalMappings = new List<TaskExternalMappingDto>
                {

                        new TaskExternalMappingDto { TaskExternalCode = "Ext123", TaskThirdPartyCode = "TP123" }

                }
            };
            _taskExternalMappingRepoMock.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<TaskExternalMappingModel, bool>>>(), false)).ThrowsAsync(new Exception("Simulated exception"));

            
            _mapperMock.Setup(m => m.Map<TaskExternalMappingRequestDto>(It.IsAny<object>()))
             .Returns(new TaskExternalMappingRequestDto { CreateUser = "test", TaskExternalCode = "123", TaskThirdPartyCode = "123", TenantCode = "123" });
            _taskServiceMock.Setup(service => service.CreateTaskExternalMapping(It.IsAny<TaskExternalMappingRequestDto>()))
              .ThrowsAsync(new Exception("Simulated exception"));

            // Act
            var response = await _service.ImportTaskExternalMapping(requestDto);

            // Assert
            Assert.NotNull(response);

        }
    }
}
