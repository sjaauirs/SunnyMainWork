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
using Xunit;
using SunnyRewards.Helios.Task.Core.Domain.Models;
using System.Linq.Expressions;

namespace SunnyRewards.Helios.Task.UnitTest.Helpers
{
    public class ImportTriviaServiceUnitTest
    {
        private readonly Mock<ILogger<ImportTriviaService>> _mockLogger;
        private readonly IMapper _mockMapper;
        private readonly Mock<ITriviaRepo> _mockTriviaRepo;
        private readonly Mock<ITriviaQuestionGroupRepo> _mockTriviaQuestionGroupRepo;
        private readonly Mock<ITriviaQuestionRepo> _mockTriviaQuestionRepo;
        private readonly Mock<ITaskService> _mockTaskService;
        private readonly Mock<ITriviaQuestionGroupService> _mockTriviaQuesGroupService;
        private readonly Mock<ITriviaQuestionService> _mockTriviaQuesService;
        private readonly Mock<ITriviaService> _mockTriviaService;
        private readonly Mock<ITaskRewardRepo> _mockTaskRewardRepo;

        private readonly ImportTriviaService _importTriviaService;

        public ImportTriviaServiceUnitTest()
        {
            // Initialize mocks
            _mockLogger = new Mock<ILogger<ImportTriviaService>>();
            _mockMapper = new Mapper(new MapperConfiguration(
                          configure =>
                          {
                              configure.AddMaps(typeof(Infrastructure.Mappings.MappingProfile.TriviaMapping).Assembly.FullName);
                              configure.AddMaps(typeof(Infrastructure.Mappings.MappingProfile.TriviaQuestionGroupMapping).Assembly.FullName);
                              configure.AddMaps(typeof(Infrastructure.Mappings.MappingProfile.TriviaQuestionMapping).Assembly.FullName);
                          }));
            _mockTriviaRepo = new Mock<ITriviaRepo>();
            _mockTriviaQuestionGroupRepo = new Mock<ITriviaQuestionGroupRepo>();
            _mockTriviaQuestionRepo = new Mock<ITriviaQuestionRepo>();
            _mockTaskService = new Mock<ITaskService>();
            _mockTriviaQuesGroupService = new Mock<ITriviaQuestionGroupService>();
            _mockTriviaQuesService = new Mock<ITriviaQuestionService>();
            _mockTriviaService = new Mock<ITriviaService>();
            _mockTaskRewardRepo = new Mock<ITaskRewardRepo>();

            // Create service instance with mocks
            _importTriviaService = new ImportTriviaService(
                _mockLogger.Object,
                _mockMapper,
                _mockTriviaRepo.Object,
                _mockTriviaQuestionGroupRepo.Object,
                _mockTriviaQuestionRepo.Object,
                _mockTaskService.Object,
                _mockTriviaQuesGroupService.Object,
                _mockTriviaQuesService.Object,
                _mockTriviaService.Object,
                _mockTaskRewardRepo.Object
            );
        }
        [Fact]
        public async void ImportTrivia_ShouldReturnNoContent_WhenRequestIsInvalid()
        {
            // Arrange
            var requestDto = new ImportTriviaRequestDto { TriviaDetailDto = null, TenantCode = null };

            // Act
            var result = await _importTriviaService.ImportTrivia(requestDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status204NoContent, result.ErrorCode);
            Assert.Equal("Invalid Request", result.ErrorDescription);
        }

        [Fact]
        public async void ImportTrivia_ShouldReturnSuccess_WhenValidRequest()
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
                    TaskExternalCode = "Reward123"} }
                }
            };


            _mockTaskRewardRepo
                .Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false))
                .ReturnsAsync(new TaskRewardModel { TaskRewardId = 1 });

            _mockTriviaRepo
                .Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<TriviaModel, bool>>>(), false))
                .ReturnsAsync(new TriviaModel { TriviaId = 1, TriviaCode = "TRV123" });

            _mockTriviaService
                .Setup(service => service.UpdateTrivia(It.IsAny<TriviaRequestDto>()))
                .ReturnsAsync(new BaseResponseDto { ErrorCode = null });

            // Act
            var result = await _importTriviaService.ImportTriviaRecords(requestDto);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.ErrorCode);
            Assert.Null(result.ErrorMessage);
        }
        [Fact]
        public async void ImportTrivia_ShouldReturn_WhenValidRequest()
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
                    TaskExternalCode = "Reward123"} }
                }
            };


            _mockTaskRewardRepo
                .Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false))
                .ReturnsAsync(new TaskRewardModel { TaskRewardId = 1 });

            _mockTriviaRepo
                .Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<TriviaModel, bool>>>(), false))
                .ReturnsAsync(new TriviaModel { TriviaId = 1, TriviaCode = "TRV123" });

            _mockTriviaService
                .Setup(service => service.UpdateTrivia(It.IsAny<TriviaRequestDto>()))
                .ReturnsAsync(new BaseResponseDto { ErrorCode = StatusCodes.Status404NotFound });

            // Act
            var result = await _importTriviaService.ImportTrivia(requestDto);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.ErrorCode);
        }
        [Fact]
        public async void ImportTrivia_ShouldReturnerrorforquestion_WhenValidRequest()
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
                    TaskExternalCode = "Reward123"} },
                    TriviaQuestion = new List<TriviaQuestionDto> { new TriviaQuestionDto { QuestionExternalCode = "Q123", TriviaQuestionCode = "trq-test" } },
                  TriviaQuestionGroup=  new List<TriviaQuestionGroupDto> { new TriviaQuestionGroupDto{
                    TriviaId = 1,
                    TriviaQuestionId = 1,
                    SequenceNbr = 1 }
                }

                }
            };


            _mockTaskRewardRepo
                .Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false))
                .ReturnsAsync(new TaskRewardModel { TaskRewardId = 1 });

            _mockTriviaRepo
                .Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<TriviaModel, bool>>>(), false))
                .ReturnsAsync(new TriviaModel { TriviaId = 1, TriviaCode = "TRV123" });

            _mockTriviaService
                .Setup(service => service.UpdateTrivia(It.IsAny<TriviaRequestDto>()))
                .ReturnsAsync(new BaseResponseDto { ErrorCode = StatusCodes.Status404NotFound });

            // Act
            var result = await _importTriviaService.ImportTrivia(requestDto);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.ErrorCode);
        }
        [Fact]
        public async void ImportTrivia_ShouldReturnError_WhenValidRequest()
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
                    TaskExternalCode = "Reward123"} }
                }
            };


            _mockTaskRewardRepo
                .Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false))
                .ReturnsAsync(new TaskRewardModel { TaskRewardId = 1 });

            _mockTriviaRepo
                .Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<TriviaModel, bool>>>(), false))
                .ReturnsAsync(new TriviaModel { TriviaId = 1, TriviaCode = "TRV123" });

            _mockTriviaService
                .Setup(service => service.UpdateTrivia(It.IsAny<TriviaRequestDto>()))
                .ReturnsAsync(new BaseResponseDto { ErrorCode = StatusCodes.Status404NotFound });

            // Act
            var result = await _importTriviaService.ImportTriviaRecords(requestDto);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.ErrorCode);
            Assert.Null(result.ErrorMessage);
        }

        [Fact]
        public async void ImportTrivia_ShouldHandleException()
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
                    TaskExternalCode = "Reward123"} }
                }
            };

            _mockTriviaService
                .Setup(service => service.UpdateTrivia(It.IsAny<TriviaRequestDto>()))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _importTriviaService.ImportTriviaRecords(requestDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status406NotAcceptable, result.ErrorCode);
          
        }
        
      
        #region Trivia Question Tests

        [Fact]
        public async void ImportTriviaQuestionRecords_Should_Create_New_TriviaQuestion()
        {
            // Arrange
            var TriviaQuestionDto = new TriviaQuestionRequestDto { QuestionExternalCode = "Q123", CreateUser = "test", TriviaQuestionCode = "trq-test" };
            var requestDto = new ImportTriviaRequestDto
            {
                TriviaDetailDto = new ImportTriviaDetailDto { TriviaQuestion = new List<TriviaQuestionDto> { new TriviaQuestionDto { QuestionExternalCode = "Q123", TriviaQuestionCode = "trq-test" } } }
            };

            _mockTriviaQuestionRepo
                .Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<TriviaQuestionModel, bool>>>(), false)); // No existing question

            //_mockMapper
            //    .Setup(m => m.Map<TriviaQuestionRequestDto>(It.IsAny<TriviaQuestionDto>()))
            //    .Returns(TriviaQuestionDto);

            _mockTriviaService
                .Setup(svc => svc.CreateTriviaQuestion(It.IsAny<TriviaQuestionRequestDto>()))
                .ReturnsAsync(new BaseResponseDto { ErrorCode = null });

            // Act
            var result = await _importTriviaService.ImportTriviaQuestionRecords(requestDto);

            // Assert
            Assert.Null(result.ErrorCode);
            _mockTriviaService.Verify(svc => svc.CreateTriviaQuestion(It.IsAny<TriviaQuestionRequestDto>()), Times.Once);
        }
        [Fact]
        public async void ImportTriviaQuestionRecords_Should_ErrorCreate_New_TriviaQuestion()
        {
            // Arrange
            var TriviaQuestionDto = new TriviaQuestionRequestDto { QuestionExternalCode = "Q123", CreateUser = "test", TriviaQuestionCode = "trq-test" };
            var requestDto = new ImportTriviaRequestDto
            {
                TriviaDetailDto = new ImportTriviaDetailDto { TriviaQuestion = new List<TriviaQuestionDto> { new TriviaQuestionDto { QuestionExternalCode = "Q123", TriviaQuestionCode = "trq-test" } } }
            };

            _mockTriviaQuestionRepo
                .Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<TriviaQuestionModel, bool>>>(), false)); // No existing question

            //_mockMapper
            //    .Setup(m => m.Map<TriviaQuestionRequestDto>(It.IsAny<TriviaQuestionDto>()))
            //    .Returns(TriviaQuestionDto);

            _mockTriviaService
                .Setup(svc => svc.CreateTriviaQuestion(It.IsAny<TriviaQuestionRequestDto>()))
                .ReturnsAsync(new BaseResponseDto { ErrorCode = StatusCodes.Status404NotFound });

            // Act
            var result = await _importTriviaService.ImportTriviaQuestionRecords(requestDto);

            // Assert
            Assert.NotNull(result.ErrorCode);
        }

        [Fact]
        public async void ImportTriviaQuestionRecords_Should_Update_Existing_TriviaQuestion()
        {
            // Arrange
            var TriviaQuestionDto = new TriviaQuestionData { QuestionExternalCode = "Q123", TriviaQuestionCode = "trq-test" };
            var requestDto = new ImportTriviaRequestDto
            {
                TriviaDetailDto = new ImportTriviaDetailDto { TriviaQuestion = new List<TriviaQuestionDto> { new TriviaQuestionDto { QuestionExternalCode = "Q123", TriviaQuestionCode = "trq-test" } } }
            };

            var existingQuestion = new TriviaQuestionModel { TriviaQuestionCode = "trq-123" };

            _mockTriviaQuestionRepo
                .Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<TriviaQuestionModel, bool>>>(), false))
                .ReturnsAsync(existingQuestion); // Existing question found

            //_mockMapper
            //    .Setup(m => m.Map<TriviaQuestionData>(It.IsAny<TriviaQuestionRequestDto>()))
            //    .Returns(TriviaQuestionDto);

            _mockTriviaQuesService
                .Setup(svc => svc.UpdateTriviaQuestion(It.IsAny<string>(), It.IsAny<TriviaQuestionData>()))
                .ReturnsAsync(new TriviaQuestionUpdateResponseDto { ErrorCode = null });

            // Act
            var result = await _importTriviaService.ImportTriviaQuestionRecords(requestDto);

            // Assert
            Assert.Null(result.ErrorCode);
            _mockTriviaQuesService.Verify(svc => svc.UpdateTriviaQuestion(It.IsAny<string>(), It.IsAny<TriviaQuestionData>()), Times.Once);
        }
        [Fact]
        public async void ImportTriviaQuestionRecords_Should_Updateerror_Existing_TriviaQuestion()
        {
            // Arrange
            var TriviaQuestionDto = new TriviaQuestionData { QuestionExternalCode = "Q123", TriviaQuestionCode = "trq-test" };
            var requestDto = new ImportTriviaRequestDto
            {
                TriviaDetailDto = new ImportTriviaDetailDto { TriviaQuestion = new List<TriviaQuestionDto> { new TriviaQuestionDto { QuestionExternalCode = "Q123", TriviaQuestionCode = "trq-test" } } }
            };

            var existingQuestion = new TriviaQuestionModel { TriviaQuestionCode = "trq-123" };

            _mockTriviaQuestionRepo
                .Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<TriviaQuestionModel, bool>>>(), false))
                .ReturnsAsync(existingQuestion); // Existing question found

            //_mockMapper
            //    .Setup(m => m.Map<TriviaQuestionData>(It.IsAny<TriviaQuestionRequestDto>()))
            //    .Returns(TriviaQuestionDto);

            _mockTriviaQuesService
                .Setup(svc => svc.UpdateTriviaQuestion(It.IsAny<string>(), It.IsAny<TriviaQuestionData>()))
                .ReturnsAsync(new TriviaQuestionUpdateResponseDto { ErrorCode = StatusCodes.Status404NotFound });

            // Act
            var result = await _importTriviaService.ImportTriviaQuestionRecords(requestDto);

            // Assert
            Assert.Null(result.ErrorCode);
            _mockTriviaQuesService.Verify(svc => svc.UpdateTriviaQuestion(It.IsAny<string>(), It.IsAny<TriviaQuestionData>()), Times.Once);
        }

        #endregion

        #region Trivia Question Group Tests

        [Fact]
        public async void ImportTriviaQuestionGroupRecords_Should_Create_New_TriviaQuestionGroup()
        {
            // Arrange
            var TriviaQuestionGroupDto = new TriviaQuestionGroupRequestDto
            {
                TriviaCode = "test",
                TriviaQuestionCode = "test"
                ,
                TriviaQuestionGroup = new TriviaQuestionGroupPostRequestDto
                {
                    TriviaId = 1,
                    TriviaQuestionId = 1,
                    SequenceNbr = 1,
                    CreateUser = "test"
            ,
                    ValidEndTs = DateTime.Now.AddDays(1),
                    ValidStartTs = DateTime.Now
                }
            };
            var requestDto = new ImportTriviaRequestDto
            {
                TriviaDetailDto = new ImportTriviaDetailDto
                {
                    TriviaQuestionGroup = new List<TriviaQuestionGroupDto> { new TriviaQuestionGroupDto{
                    TriviaId = 1,
                    TriviaQuestionId = 1,
                    SequenceNbr = 1 }
                }
                }
            };
            var existingQuestion = new TriviaQuestionModel { TriviaQuestionCode = "trq-123" };

            _mockTriviaQuestionRepo
                .Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<TriviaQuestionModel, bool>>>(), false))
                .ReturnsAsync(existingQuestion);
            _mockTriviaRepo
                .Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<TriviaModel, bool>>>(), false))
                .ReturnsAsync(new TriviaModel { TriviaId = 1, TriviaCode = "TRV123" });
            _mockTriviaQuestionGroupRepo
                .Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<TriviaQuestionGroupModel, bool>>>(), false));

            //_mockMapper
            //    .Setup(m => m.Map<TriviaQuestionGroupRequestDto>(It.IsAny<TriviaQuestionGroupRequestDto>()))
            //    .Returns(TriviaQuestionGroupDto);

            _mockTriviaService
                .Setup(svc => svc.CreateTriviaQuestionGroup(It.IsAny<TriviaQuestionGroupRequestDto>()))
                .ReturnsAsync(new BaseResponseDto { ErrorCode = null });

            // Act
            var result = await _importTriviaService.ImportTriviaQuestionGroupRecords(requestDto);

            // Assert
            Assert.Null(result.ErrorCode);
            _mockTriviaService.Verify(svc => svc.CreateTriviaQuestionGroup(It.IsAny<TriviaQuestionGroupRequestDto>()), Times.Once);
        }

        [Fact]
        public async void ImportTriviaQuestionGroupRecords_Should_Update_Existing_TriviaQuestionGroup()
        {
            // Arrange
            var TriviaQuestionGroupDto = new TriviaQuestionGroupRequestDto
            {
                TriviaCode = "test",
                TriviaQuestionCode = "test"
                 ,
                TriviaQuestionGroup = new TriviaQuestionGroupPostRequestDto
                {
                    TriviaId = 1,
                    TriviaQuestionId = 1,
                    SequenceNbr = 1,
                    CreateUser = "test"
             ,
                    ValidEndTs = DateTime.Now.AddDays(1),
                    ValidStartTs = DateTime.Now
                }
            };
            var requestDto = new ImportTriviaRequestDto
            {
                TriviaDetailDto = new ImportTriviaDetailDto
                {
                    TriviaQuestionGroup = new List<TriviaQuestionGroupDto> { new TriviaQuestionGroupDto{
                    TriviaId = 1,
                    TriviaQuestionId = 1,
                    SequenceNbr = 1 }
                }
                }
            };
            var existingQuestion = new TriviaQuestionModel { TriviaQuestionCode = "trq-123" };

            _mockTriviaQuestionRepo
                .Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<TriviaQuestionModel, bool>>>(), false))
                .ReturnsAsync(existingQuestion);
            _mockTriviaRepo
                .Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<TriviaModel, bool>>>(), false))
                .ReturnsAsync(new TriviaModel { TriviaId = 1, TriviaCode = "TRV123" });

            var existingGroup = new TriviaQuestionGroupModel { TriviaQuestionGroupId = 10 };

            _mockTriviaQuestionGroupRepo
                .Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<TriviaQuestionGroupModel, bool>>>(), false))
                .ReturnsAsync(existingGroup); // Existing group found

            _mockTriviaQuesGroupService
                .Setup(svc => svc.UpdateTriviaQuestionGroup(It.IsAny<long>(), It.IsAny<TriviaQuestionGroupDto>()))
                .ReturnsAsync(new TriviaQuestionGroupUpdateResponseDto { ErrorCode = null });

            // Act
            var result = await _importTriviaService.ImportTriviaQuestionGroupRecords(requestDto);

            // Assert
            Assert.Null(result.ErrorCode);
        }
        [Fact]
        public async void ImportTriviaQuestionGroupRecords_Should_Return_Error_Update_Existing_TriviaQuestionGroup()
        {
            // Arrange
            var TriviaQuestionGroupDto = new TriviaQuestionGroupRequestDto
            {
                TriviaCode = "test",
                TriviaQuestionCode = "test"
                 ,
                TriviaQuestionGroup = new TriviaQuestionGroupPostRequestDto
                {
                    TriviaId = 1,
                    TriviaQuestionId = 1,
                    SequenceNbr = 1,
                    CreateUser = "test"
             ,
                    ValidEndTs = DateTime.Now.AddDays(1),
                    ValidStartTs = DateTime.Now
                }
            };
            var requestDto = new ImportTriviaRequestDto
            {
                TriviaDetailDto = new ImportTriviaDetailDto
                {
                    TriviaQuestionGroup = new List<TriviaQuestionGroupDto> { new TriviaQuestionGroupDto{
                    TriviaId = 1,
                    TriviaQuestionId = 1,
                    SequenceNbr = 1 }
                }
                }
            };
            var existingQuestion = new TriviaQuestionModel { TriviaQuestionCode = "trq-123" };

            _mockTriviaQuestionRepo
                .Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<TriviaQuestionModel, bool>>>(), false))
                .ReturnsAsync(existingQuestion);
            _mockTriviaRepo
                .Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<TriviaModel, bool>>>(), false))
                .ReturnsAsync(new TriviaModel { TriviaId = 1, TriviaCode = "TRV123" });

            var existingGroup = new TriviaQuestionGroupModel { TriviaQuestionGroupId = 10 };

            _mockTriviaQuestionGroupRepo
                .Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<TriviaQuestionGroupModel, bool>>>(), false))
                .ReturnsAsync(existingGroup); // Existing group found

            _mockTriviaQuesGroupService
                .Setup(svc => svc.UpdateTriviaQuestionGroup(It.IsAny<long>(), It.IsAny<TriviaQuestionGroupDto>()))
                .ReturnsAsync(new TriviaQuestionGroupUpdateResponseDto { ErrorCode = StatusCodes.Status404NotFound });

            // Act
            var result = await _importTriviaService.ImportTriviaQuestionGroupRecords(requestDto);

            // Assert
            Assert.NotNull(result.ErrorCode);
        }
      
        #endregion

    }
}
