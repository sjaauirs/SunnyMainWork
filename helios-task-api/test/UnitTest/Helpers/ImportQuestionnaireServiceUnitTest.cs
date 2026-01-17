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
using Microsoft.AspNetCore.Http;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Models;
using System.Linq.Expressions;
using Xunit;

namespace SunnyRewards.Helios.Task.UnitTest.Helpers
{
    public class ImportQuestionnaireServiceUnitTest
    {
        private readonly Mock<ILogger<ImportQuestionnaireService>> _mockLogger;
        private readonly IMapper _mockMapper;
        private readonly Mock<IQuestionnaireRepo> _mockQuestionnaireRepo;
        private readonly Mock<IQuestionnaireQuestionGroupRepo> _mockQuestionnaireQuestionGroupRepo;
        private readonly Mock<IQuestionnaireQuestionRepo> _mockQuestionnaireQuestionRepo;
        private readonly Mock<ITaskService> _mockTaskService;
        private readonly Mock<IQuestionnaireQuestionGroupService> _mockQuestionnaireQuesGroupService;
        private readonly Mock<IQuestionnaireQuestionService> _mockQuestionnaireQuesService;
        private readonly Mock<IQuestionnaireService> _mockQuestionnaireService;
        private readonly Mock<ITaskRewardRepo> _mockTaskRewardRepo;

        private readonly ImportQuestionnaireService _importQuestionnaireService;

        public ImportQuestionnaireServiceUnitTest()
        {
            // Initialize mocks
            _mockLogger = new Mock<ILogger<ImportQuestionnaireService>>();
            _mockMapper = new Mapper(new MapperConfiguration(
                          configure =>
                          {
                              configure.AddMaps(typeof(Infrastructure.Mappings.MappingProfile.QuestionnaireMapping).Assembly.FullName);
                              configure.AddMaps(typeof(Infrastructure.Mappings.MappingProfile.QuestionnaireQuestionGroupMapping).Assembly.FullName);
                              configure.AddMaps(typeof(Infrastructure.Mappings.MappingProfile.QuestionnaireQuestionMapping).Assembly.FullName);
                          }));
            _mockQuestionnaireRepo = new Mock<IQuestionnaireRepo>();
            _mockQuestionnaireQuestionGroupRepo = new Mock<IQuestionnaireQuestionGroupRepo>();
            _mockQuestionnaireQuestionRepo = new Mock<IQuestionnaireQuestionRepo>();
            _mockTaskService = new Mock<ITaskService>();
            _mockQuestionnaireQuesGroupService = new Mock<IQuestionnaireQuestionGroupService>();
            _mockQuestionnaireQuesService = new Mock<IQuestionnaireQuestionService>();
            _mockQuestionnaireService = new Mock<IQuestionnaireService>();
            _mockTaskRewardRepo = new Mock<ITaskRewardRepo>();

            // Create service instance with mocks
            _importQuestionnaireService = new ImportQuestionnaireService(
                _mockLogger.Object,
                _mockMapper,
                _mockQuestionnaireRepo.Object,
                _mockQuestionnaireQuestionGroupRepo.Object,
                _mockQuestionnaireQuestionRepo.Object,
                _mockTaskService.Object,
                _mockQuestionnaireQuesGroupService.Object,
                _mockQuestionnaireQuesService.Object,
                _mockQuestionnaireService.Object,
                _mockTaskRewardRepo.Object
            );
        }
        [Fact]
        public async void ImportQuestionnaire_ShouldReturnNoContent_WhenRequestIsInvalid()
        {
            // Arrange
            var requestDto = new ImportQuestionnaireRequestDto { QuestionnaireDetailDto = null, TenantCode = null };

            // Act
            var result = await _importQuestionnaireService.ImportQuestionnaire(requestDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status204NoContent, result.ErrorCode);
            Assert.Equal("Invalid Request", result.ErrorDescription);
        }

        [Fact]
        public async void ImportQuestionnaire_ShouldReturnSuccess_WhenValidRequest()
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
                    TaskExternalCode = "Reward123"} }
                }
            };


            _mockTaskRewardRepo
                .Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false))
                .ReturnsAsync(new TaskRewardModel { TaskRewardId = 1 });

            _mockQuestionnaireRepo
                .Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<QuestionnaireModel, bool>>>(), false))
                .ReturnsAsync(new QuestionnaireModel { QuestionnaireId = 1, QuestionnaireCode = "QNC123" });

            _mockQuestionnaireService
                .Setup(service => service.UpdateQuestionnaire(It.IsAny<QuestionnaireRequestDto>()))
                .ReturnsAsync(new BaseResponseDto { ErrorCode = null });

            // Act
            var result = await _importQuestionnaireService.ImportQuestionnaireRecords(requestDto);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.ErrorCode);
            Assert.Null(result.ErrorMessage);
        }
        [Fact]
        public async void ImportQuestionnaire_ShouldReturn_WhenValidRequest()
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
                    TaskExternalCode = "Reward123"} }
                }
            };


            _mockTaskRewardRepo
                .Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false))
                .ReturnsAsync(new TaskRewardModel { TaskRewardId = 1 });

            _mockQuestionnaireRepo
                .Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<QuestionnaireModel, bool>>>(), false))
                .ReturnsAsync(new QuestionnaireModel { QuestionnaireId = 1, QuestionnaireCode = "QNC123" });

            _mockQuestionnaireService
                .Setup(service => service.UpdateQuestionnaire(It.IsAny<QuestionnaireRequestDto>()))
                .ReturnsAsync(new BaseResponseDto { ErrorCode = StatusCodes.Status404NotFound });

            // Act
            var result = await _importQuestionnaireService.ImportQuestionnaire(requestDto);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.ErrorCode);
        }
        [Fact]
        public async void ImportQuestionnaire_ShouldReturnerrorforquestion_WhenValidRequest()
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
                    TaskExternalCode = "Reward123"} },
                    QuestionnaireQuestion = new List<QuestionnaireQuestionDto> { new QuestionnaireQuestionDto { QuestionExternalCode = "Q123", QuestionnaireQuestionCode = "qqc-test" } },
                    QuestionnaireQuestionGroup = new List<QuestionnaireQuestionGroupDto> { new QuestionnaireQuestionGroupDto{
                    QuestionnaireId = 1,
                    QuestionnaireQuestionId = 1,
                    SequenceNbr = 1 }
                }

                }
            };


            _mockTaskRewardRepo
                .Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false))
                .ReturnsAsync(new TaskRewardModel { TaskRewardId = 1 });

            _mockQuestionnaireRepo
                .Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<QuestionnaireModel, bool>>>(), false))
                .ReturnsAsync(new QuestionnaireModel { QuestionnaireId = 1, QuestionnaireCode = "QNC123" });

            _mockQuestionnaireService
                .Setup(service => service.UpdateQuestionnaire(It.IsAny<QuestionnaireRequestDto>()))
                .ReturnsAsync(new BaseResponseDto { ErrorCode = StatusCodes.Status404NotFound });

            // Act
            var result = await _importQuestionnaireService.ImportQuestionnaire(requestDto);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.ErrorCode);
        }
        [Fact]
        public async void ImportQuestionnaire_ShouldReturnError_WhenValidRequest()
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
                    TaskExternalCode = "Reward123"} }
                }
            };


            _mockTaskRewardRepo
                .Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false))
                .ReturnsAsync(new TaskRewardModel { TaskRewardId = 1 });

            _mockQuestionnaireRepo
                .Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<QuestionnaireModel, bool>>>(), false))
                .ReturnsAsync(new QuestionnaireModel { QuestionnaireId = 1, QuestionnaireCode = "QNC123" });

            _mockQuestionnaireService
                .Setup(service => service.UpdateQuestionnaire(It.IsAny<QuestionnaireRequestDto>()))
                .ReturnsAsync(new BaseResponseDto { ErrorCode = StatusCodes.Status404NotFound });

            // Act
            var result = await _importQuestionnaireService.ImportQuestionnaireRecords(requestDto);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.ErrorCode);
            Assert.Null(result.ErrorMessage);
        }

        [Fact]
        public async void ImportQuestionnaire_ShouldHandleException()
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
                    TaskExternalCode = "Reward123"} }
                }
            };

            _mockQuestionnaireService
                .Setup(service => service.UpdateQuestionnaire(It.IsAny<QuestionnaireRequestDto>()))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _importQuestionnaireService.ImportQuestionnaireRecords(requestDto);

            // Assert
            Assert.NotNull(result);

        }


        #region Questionnaire Question Tests

        [Fact]
        public async void ImportQuestionnaireQuestionRecords_Should_Create_New_QuestionnaireQuestion()
        {
            // Arrange
            var QuestionnaireQuestionDto = new QuestionnaireQuestionRequestDto { QuestionExternalCode = "Q123", CreateUser = "test", QuestionnaireQuestionCode = "qqc-test" };
            var requestDto = new ImportQuestionnaireRequestDto
            {
                QuestionnaireDetailDto = new ImportQuestionnaireDetailDto { QuestionnaireQuestion = new List<QuestionnaireQuestionDto> { new QuestionnaireQuestionDto { QuestionExternalCode = "Q123", QuestionnaireQuestionCode = "qqc-test" } } }
            };

            _mockQuestionnaireQuestionRepo
                .Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<QuestionnaireQuestionModel, bool>>>(), false)); // No existing question

            _mockQuestionnaireQuesService
                .Setup(svc => svc.CreateQuestionnaireQuestion(It.IsAny<QuestionnaireQuestionRequestDto>()))
                .ReturnsAsync(new BaseResponseDto { ErrorCode = null });

            // Act
            var result = await _importQuestionnaireService.ImportQuestionnaireQuestionRecords(requestDto);

            // Assert
            Assert.Null(result.ErrorCode);
            _mockQuestionnaireQuesService.Verify(svc => svc.CreateQuestionnaireQuestion(It.IsAny<QuestionnaireQuestionRequestDto>()), Times.Once);
        }
        [Fact]
        public async void ImportQuestionnaireQuestionRecords_Should_ErrorCreate_New_QuestionnaireQuestion()
        {
            // Arrange
            var QuestionnaireQuestionDto = new QuestionnaireQuestionRequestDto { QuestionExternalCode = "Q123", CreateUser = "test", QuestionnaireQuestionCode = "qqc-test" };
            var requestDto = new ImportQuestionnaireRequestDto
            {
                QuestionnaireDetailDto = new ImportQuestionnaireDetailDto { QuestionnaireQuestion = new List<QuestionnaireQuestionDto> { new QuestionnaireQuestionDto { QuestionExternalCode = "Q123", QuestionnaireQuestionCode = "qqc-test" } } }
            };

            _mockQuestionnaireQuestionRepo
                .Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<QuestionnaireQuestionModel, bool>>>(), false)); // No existing question

            _mockQuestionnaireQuesService
                .Setup(svc => svc.CreateQuestionnaireQuestion(It.IsAny<QuestionnaireQuestionRequestDto>()))
                .ReturnsAsync(new BaseResponseDto { ErrorCode = StatusCodes.Status404NotFound });

            // Act
            var result = await _importQuestionnaireService.ImportQuestionnaireQuestionRecords(requestDto);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async void ImportQuestionnaireQuestionRecords_Should_Update_Existing_QuestionnaireQuestion()
        {
            // Arrange
            var QuestionnaireQuestionDto = new QuestionnaireQuestionData { QuestionExternalCode = "Q123", QuestionnaireQuestionCode = "qqc-test" };
            var requestDto = new ImportQuestionnaireRequestDto
            {
                QuestionnaireDetailDto = new ImportQuestionnaireDetailDto { QuestionnaireQuestion = new List<QuestionnaireQuestionDto> { new QuestionnaireQuestionDto { QuestionExternalCode = "Q123", QuestionnaireQuestionCode = "qqc-test" } } }
            };

            var existingQuestion = new QuestionnaireQuestionModel { QuestionnaireQuestionCode = "qqc-123" };

            _mockQuestionnaireQuestionRepo
                .Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<QuestionnaireQuestionModel, bool>>>(), false))
                .ReturnsAsync(existingQuestion); 

            _mockQuestionnaireQuesService
                .Setup(svc => svc.UpdateQuestionnaireQuestion(It.IsAny<string>(), It.IsAny<QuestionnaireQuestionData>()))
                .ReturnsAsync(new QuestionnaireQuestionUpdateResponseDto { ErrorCode = null });

            // Act
            var result = await _importQuestionnaireService.ImportQuestionnaireQuestionRecords(requestDto);

            // Assert
            Assert.Null(result.ErrorCode);
            _mockQuestionnaireQuesService.Verify(svc => svc.UpdateQuestionnaireQuestion(It.IsAny<string>(), It.IsAny<QuestionnaireQuestionData>()), Times.Once);
        }
        [Fact]
        public async void ImportQuestionnaireQuestionRecords_Should_Updateerror_Existing_QuestionnaireQuestion()
        {
            // Arrange
            var QuestionnaireQuestionDto = new QuestionnaireQuestionData { QuestionExternalCode = "Q123", QuestionnaireQuestionCode = "qqc-test" };
            var requestDto = new ImportQuestionnaireRequestDto
            {
                QuestionnaireDetailDto = new ImportQuestionnaireDetailDto { QuestionnaireQuestion = new List<QuestionnaireQuestionDto> { new QuestionnaireQuestionDto { QuestionExternalCode = "Q123", QuestionnaireQuestionCode = "qqc-test" } } }
            };

            var existingQuestion = new QuestionnaireQuestionModel { QuestionnaireQuestionCode = "qqc-123" };

            _mockQuestionnaireQuestionRepo
                .Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<QuestionnaireQuestionModel, bool>>>(), false))
                .ReturnsAsync(existingQuestion); 

            _mockQuestionnaireQuesService
                .Setup(svc => svc.UpdateQuestionnaireQuestion(It.IsAny<string>(), It.IsAny<QuestionnaireQuestionData>()))
                .ReturnsAsync(new QuestionnaireQuestionUpdateResponseDto { ErrorCode = StatusCodes.Status404NotFound });

            // Act
            var result = await _importQuestionnaireService.ImportQuestionnaireQuestionRecords(requestDto);

            // Assert
            Assert.Null(result.ErrorCode);
            _mockQuestionnaireQuesService.Verify(svc => svc.UpdateQuestionnaireQuestion(It.IsAny<string>(), It.IsAny<QuestionnaireQuestionData>()), Times.Once);
        }

        #endregion

        #region Questionnaire Question Group Tests

        [Fact]
        public async void ImportQuestionnaireQuestionGroupRecords_Should_Create_New_QuestionnaireQuestionGroup()
        {
            // Arrange
            var QuestionnaireQuestionGroupDto = new QuestionnaireQuestionGroupRequestDto
            {
                QuestionnaireCode = "test",
                QuestionnaireQuestionCode = "test"
                ,
                QuestionnaireQuestionGroup = new QuestionnaireQuestionGroupPostRequestDto
                {
                    QuestionnaireId = 1,
                    QuestionnaireQuestionId = 1,
                    SequenceNbr = 1,
                    CreateUser = "test"
            ,
                    ValidEndTs = DateTime.Now.AddDays(1),
                    ValidStartTs = DateTime.Now
                }
            };
            var requestDto = new ImportQuestionnaireRequestDto
            {
                QuestionnaireDetailDto = new ImportQuestionnaireDetailDto
                {
                    QuestionnaireQuestionGroup = new List<QuestionnaireQuestionGroupDto> { new QuestionnaireQuestionGroupDto{
                    QuestionnaireId = 1,
                    QuestionnaireQuestionId = 1,
                    SequenceNbr = 1 }
                }
                }
            };
            var existingQuestion = new QuestionnaireQuestionModel { QuestionnaireQuestionCode = "qqc-123" };

            _mockQuestionnaireQuestionRepo
                .Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<QuestionnaireQuestionModel, bool>>>(), false))
                .ReturnsAsync(existingQuestion);
            _mockQuestionnaireRepo
                .Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<QuestionnaireModel, bool>>>(), false))
                .ReturnsAsync(new QuestionnaireModel { QuestionnaireId = 1, QuestionnaireCode = "QNC123" });
            _mockQuestionnaireQuestionGroupRepo
                .Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<QuestionnaireQuestionGroupModel, bool>>>(), false));

            _mockQuestionnaireService
                .Setup(svc => svc.CreateQuestionnaireQuestionGroup(It.IsAny<QuestionnaireQuestionGroupRequestDto>()))
                .ReturnsAsync(new BaseResponseDto { ErrorCode = null });

            // Act
            var result = await _importQuestionnaireService.ImportQuestionnaireQuestionGroupRecords(requestDto);

            // Assert
            Assert.Null(result.ErrorCode);
            _mockQuestionnaireService.Verify(svc => svc.CreateQuestionnaireQuestionGroup(It.IsAny<QuestionnaireQuestionGroupRequestDto>()), Times.Once);
        }

        [Fact]
        public async void ImportQuestionnaireQuestionGroupRecords_Should_Update_Existing_QuestionnaireQuestionGroup()
        {
            // Arrange
            var QuestionnaireQuestionGroupDto = new QuestionnaireQuestionGroupRequestDto
            {
                QuestionnaireCode = "test",
                QuestionnaireQuestionCode = "test"
                 ,
                QuestionnaireQuestionGroup = new QuestionnaireQuestionGroupPostRequestDto
                {
                    QuestionnaireId = 1,
                    QuestionnaireQuestionId = 1,
                    SequenceNbr = 1,
                    CreateUser = "test"
             ,
                    ValidEndTs = DateTime.Now.AddDays(1),
                    ValidStartTs = DateTime.Now
                }
            };
            var requestDto = new ImportQuestionnaireRequestDto
            {
                QuestionnaireDetailDto = new ImportQuestionnaireDetailDto
                {
                    QuestionnaireQuestionGroup = new List<QuestionnaireQuestionGroupDto> { new QuestionnaireQuestionGroupDto{
                    QuestionnaireId = 1,
                    QuestionnaireQuestionId = 1,
                    SequenceNbr = 1 }
                }
                }
            };
            var existingQuestion = new QuestionnaireQuestionModel { QuestionnaireQuestionCode = "qqc-123" };

            _mockQuestionnaireQuestionRepo
                .Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<QuestionnaireQuestionModel, bool>>>(), false))
                .ReturnsAsync(existingQuestion);
            _mockQuestionnaireRepo
                .Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<QuestionnaireModel, bool>>>(), false))
                .ReturnsAsync(new QuestionnaireModel { QuestionnaireId = 1, QuestionnaireCode = "QNC123" });

            var existingGroup = new QuestionnaireQuestionGroupModel { QuestionnaireQuestionGroupId = 10 };

            _mockQuestionnaireQuestionGroupRepo
                .Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<QuestionnaireQuestionGroupModel, bool>>>(), false))
                .ReturnsAsync(existingGroup); // Existing group found

            _mockQuestionnaireQuesGroupService
                .Setup(svc => svc.UpdateQuestionnaireQuestionGroup(It.IsAny<long>(), It.IsAny<QuestionnaireQuestionGroupDto>()))
                .ReturnsAsync(new QuestionnaireQuestionGroupUpdateResponseDto { ErrorCode = null });

            // Act
            var result = await _importQuestionnaireService.ImportQuestionnaireQuestionGroupRecords(requestDto);

            // Assert
            Assert.Null(result.ErrorCode);
        }
        [Fact]
        public async void ImportQuestionnaireQuestionGroupRecords_Should_Return_Error_Update_Existing_QuestionnaireQuestionGroup()
        {
            // Arrange
            var QuestionnaireQuestionGroupDto = new QuestionnaireQuestionGroupRequestDto
            {
                QuestionnaireCode = "test",
                QuestionnaireQuestionCode = "test"
                 ,
                QuestionnaireQuestionGroup = new QuestionnaireQuestionGroupPostRequestDto
                {
                    QuestionnaireId = 1,
                    QuestionnaireQuestionId = 1,
                    SequenceNbr = 1,
                    CreateUser = "test"
             ,
                    ValidEndTs = DateTime.Now.AddDays(1),
                    ValidStartTs = DateTime.Now
                }
            };
            var requestDto = new ImportQuestionnaireRequestDto
            {
                QuestionnaireDetailDto = new ImportQuestionnaireDetailDto
                {
                    QuestionnaireQuestionGroup = new List<QuestionnaireQuestionGroupDto> { new QuestionnaireQuestionGroupDto{
                    QuestionnaireId = 1,
                    QuestionnaireQuestionId = 1,
                    SequenceNbr = 1 }
                }
                }
            };
            var existingQuestion = new QuestionnaireQuestionModel { QuestionnaireQuestionCode = "qqc-123" };

            _mockQuestionnaireQuestionRepo
                .Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<QuestionnaireQuestionModel, bool>>>(), false))
                .ReturnsAsync(existingQuestion);
            _mockQuestionnaireRepo
                .Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<QuestionnaireModel, bool>>>(), false))
                .ReturnsAsync(new QuestionnaireModel { QuestionnaireId = 1, QuestionnaireCode = "QNC123" });

            var existingGroup = new QuestionnaireQuestionGroupModel { QuestionnaireQuestionGroupId = 10 };

            _mockQuestionnaireQuestionGroupRepo
                .Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<QuestionnaireQuestionGroupModel, bool>>>(), false))
                .ReturnsAsync(existingGroup); // Existing group found

            _mockQuestionnaireQuesGroupService
                .Setup(svc => svc.UpdateQuestionnaireQuestionGroup(It.IsAny<long>(), It.IsAny<QuestionnaireQuestionGroupDto>()))
                .ReturnsAsync(new QuestionnaireQuestionGroupUpdateResponseDto { ErrorCode = StatusCodes.Status404NotFound });

            // Act
            var result = await _importQuestionnaireService.ImportQuestionnaireQuestionGroupRecords(requestDto);

            // Assert
            Assert.NotNull(result);
        }

        #endregion

    }
}
