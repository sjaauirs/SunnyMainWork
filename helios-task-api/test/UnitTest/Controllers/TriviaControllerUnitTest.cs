using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NHibernate;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Repositories;
using SunnyRewards.Helios.Task.Api.Controller;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Models;
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
    public class TriviaControllerUnitTest
    {
        private readonly Mock<ILogger<TriviaController>> _triviaLogger;
        private readonly Mock<ILogger<TriviaService>> _triviaServiceLogger;
        private readonly Mock<IMapper> _mapper;
        private readonly Mock<ITriviaQuestionRepo> _triviaQuestionRepo;
        private readonly Mock<ITriviaRepo> _triviaRepo;
        private readonly Mock<ITriviaQuestionGroupRepo> _triviaQuestionGroupRepo;
        private readonly Mock<ITaskRewardRepo> _taskRewardRepo;
        private readonly Mock<ITaskRewardService> _taskRewardService;
        private readonly Mock<IConsumerTaskRepo> _consumerTaskRepo;
        private readonly TriviaService _triviaService;
        private readonly TriviaController _triviaController;
        private readonly Mock<NHibernate.ISession> _session;
        public TriviaControllerUnitTest()
        {
            _triviaLogger = new Mock<ILogger<TriviaController>>();
            _triviaServiceLogger = new Mock<ILogger<TriviaService>>();
            _mapper = new Mock<IMapper>();
            _triviaQuestionRepo = new TriviaQuestionMockRepo();
            _triviaRepo = new TriviaMockRepo();
            _triviaQuestionGroupRepo = new TriviaQuestionGroupMockRepo();
            _taskRewardRepo = new TaskRewardMockRepo();
            _taskRewardService = new Mock<ITaskRewardService>();
            _consumerTaskRepo = new ConsumerTaskMockRepo();
            _session = new Mock<NHibernate.ISession>();

            _triviaService = new TriviaService(_triviaServiceLogger.Object,
                _mapper.Object, _triviaQuestionRepo.Object, _triviaRepo.Object, _triviaQuestionGroupRepo.Object, _taskRewardRepo.Object, _taskRewardService.Object, _consumerTaskRepo.Object, _session.Object);
            _triviaController = new TriviaController(_triviaLogger.Object, _triviaService);
        }

        [Fact]
        public async TaskAlias Should_GetTrivia()
        {
            long taskRewardId = 32;
            string? languageCode = "en-us";
            string consumerCode = "cmr-d72c3aac5e644d28ae8541a2041caefe";
            _mapper.Setup(x => x.Map<TriviaDto>(It.IsAny<TriviaModel>()))
           .Returns(new TriviaMockDto());
            _mapper.Setup(x => x.Map<List<TriviaQuestionDto>>(It.IsAny<List<TriviaQuestionModel>>()))
                .Returns(new List<TriviaQuestionDto> { new TriviaQuestionMockDto()
                });
            var response = await _triviaController.GetTrivia(taskRewardId, consumerCode, languageCode);
            var result = response?.Result as OkObjectResult;
            Assert.True(result?.Value != null);
            Assert.True(result.StatusCode == 200);
        }

        [Fact]
        public async TaskAlias Should_GetTrivia_nolanguage()
        {
            long taskRewardId = 32;
            string? languageCode = null;
            string consumerCode = "cmr-d72c3aac5e644d28ae8541a2041caefe";
            _mapper.Setup(x => x.Map<TriviaDto>(It.IsAny<TriviaModel>()))
           .Returns(new TriviaMockDto());
            _mapper.Setup(x => x.Map<List<TriviaQuestionDto>>(It.IsAny<List<TriviaQuestionModel>>()))
                .Returns(new List<TriviaQuestionDto> { new TriviaQuestionMockDto()
                });
            var response = await _triviaController.GetTrivia(taskRewardId, consumerCode, languageCode);
            var result = response?.Result as OkObjectResult;
            Assert.True(result?.Value != null);
            Assert.True(result.StatusCode == 200);
        }

        [Fact]
        public async TaskAlias Should_GetTrivia_es()
        {
            long taskRewardId = 32;
            string? languageCode = "es";
            string consumerCode = "cmr-d72c3aac5e644d28ae8541a2041caefe";
            _mapper.Setup(x => x.Map<TriviaDto>(It.IsAny<TriviaModel>()))
           .Returns(new TriviaMockDto());
            _mapper.Setup(x => x.Map<List<TriviaQuestionDto>>(It.IsAny<List<TriviaQuestionModel>>()))
                .Returns(new List<TriviaQuestionDto> { new TriviaQuestionMockDto()
                });
            var response = await _triviaController.GetTrivia(taskRewardId, consumerCode, languageCode);
            var result = response?.Result as OkObjectResult;
            Assert.True(result?.Value != null);
            Assert.True(result.StatusCode == 200);
        }

        [Fact]
        public async TaskAlias Should_NullCheck_GetTrivia()
        {
            long taskRewardId = 111;
            string? languageCode = "en-us";
            string consumerCode = "cmr-d72c3aac5e644d28ae8541a2041caefe";
            var triviaServiceMock = new Mock<ITriviaService>();
            var triviaService = triviaServiceMock.Setup(service => service.GetTrivia(taskRewardId, consumerCode, languageCode))
                            .ReturnsAsync((GetTriviaResponseMockDto)null);
            var triviaLoggerMock = new Mock<ILogger<TriviaController>>();
            var controller = new TriviaController(triviaLoggerMock.Object, triviaServiceMock.Object);
            var response = await controller.GetTrivia(taskRewardId, consumerCode, languageCode);
            var result = Assert.IsType<NotFoundResult>(response.Result);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async TaskAlias Should_Return_Exception_Catch_GetTrivia_Controller()
        {
            long taskRewardId = 4;
            string? languageCode = "en-us";
            string consumerCode = "cmr-d72c3aac5e644d28ae8541a2041caefe";
            var triviaLogger = new Mock<ILogger<TriviaController>>();
            var triviaService = new Mock<ITriviaService>();
            var controller = new TriviaController(triviaLogger.Object, triviaService.Object);
            var getTriviaRequestDto = new GetTriviaResponseMockDto();
            triviaService.Setup(x => x.GetTrivia(taskRewardId, consumerCode, languageCode)).ThrowsAsync(new Exception("An error occurred"));
            var result = await controller.GetTrivia(taskRewardId, consumerCode, languageCode);
            Assert.Equal("An error occurred", result?.Value?.ErrorMessage);
        }

        [Fact]
        public async TaskAlias Should_OkResponse_GetTrivia_Service()
        {
            var triviaService = new Mock<ITriviaService>();
            long taskRewardId = 456;
            string? languageCode = "en-us";
            string consumerCode = "cmr-d72c3aac5e644d28ae8541a2041caefe";
            triviaService.Setup(x => x.GetTrivia(taskRewardId, consumerCode,languageCode)).ReturnsAsync(new GetTriviaResponseMockDto());
            _mapper.Setup(x => x.Map<TriviaDto>(It.IsAny<TriviaModel>()))
           .Returns(new TriviaMockDto());
            _mapper.Setup(x => x.Map<List<TriviaQuestionDto>>(It.IsAny<List<TriviaQuestionModel>>()))
                .Returns(new List<TriviaQuestionDto> { new TriviaQuestionMockDto() });
            var response = await _triviaController.GetTrivia(taskRewardId, consumerCode, languageCode);
            var result = response?.Result as OkObjectResult;
            Assert.NotNull(result?.Value);
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public async TaskAlias Should_ReturnsNull_Response_GetTrivia()
        {
            string? languageCode = "en-us";
            long taskRewardId = 4;
            string consumerCode = "cmr-d72c3aac5e644d28ae8541a2041caefe";
            _triviaRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TriviaModel, bool>>>(), false))
                .ReturnsAsync((TriviaMockModel)null);
            var response = await _triviaService.GetTrivia(taskRewardId, consumerCode, languageCode);
            Assert.NotNull(response);
            Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
        }

        [Fact]
        public async TaskAlias Should_Return_Exception_Catch_GetTrivia_Service()
        {
            string? languageCode = "en-us";
            long taskRewardId = 5;
            string consumerCode = "cmr-d72c3aac5e644d28ae8541a2041caefe";
            _triviaRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TriviaModel, bool>>>(), false))
                 .ThrowsAsync(new Exception("An error occurred"));
            var response = await _triviaService.GetTrivia(taskRewardId, consumerCode, languageCode);
            Assert.NotNull(response);
            Assert.Equal(StatusCodes.Status500InternalServerError, response.ErrorCode);
        }

        [Fact]
        public async TaskAlias Should_OkResponse_GetTaskRewarddetails_Controller()
        {
            var getTaskRewardByCodeRequestMockDto = new GetTaskRewardByCodeRequestMockDto();
            var transactionMock = new Mock<ITransaction>();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);
            var postTaskProgressUpdateRequestMockDto = new PostTaskProgressUpdateRequestMockDto();
            var response = await _triviaController.TaskProgressUpdate(postTaskProgressUpdateRequestMockDto);
            var result = response.Result as OkObjectResult;
            Assert.NotNull(result?.Value);
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public async TaskAlias Should_OkResponse_TaskProgressUpdate_Controller()
        {
            var transactionMock = new Mock<ITransaction>();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);
            var postTaskProgressUpdateRequestMockDto = new PostTaskProgressUpdateRequestMockDto();
            var response = await _triviaController.TaskProgressUpdate(postTaskProgressUpdateRequestMockDto);
            var result = response.Result as OkObjectResult;
            Assert.NotNull(result?.Value);
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public async TaskAlias Should_Exception_TaskProgressUpdate_Controller()
        {
            var triviaServiceMock = new Mock<ITriviaService>();
            var controller = new TriviaController(_triviaLogger.Object, triviaServiceMock.Object);
            var postTaskProgressUpdateRequestMockDto = new PostTaskProgressUpdateRequestMockDto();
            triviaServiceMock.Setup(x => x.TaskProgressUpdate(postTaskProgressUpdateRequestMockDto)).ThrowsAsync(new Exception("inner Exception"));
            var response = await controller.TaskProgressUpdate(postTaskProgressUpdateRequestMockDto);
            Assert.NotNull(response);
        }

        [Fact]
        public async TaskAlias Should_OkResponse_TaskProgressUpdate_Service()
        {
            var transactionMock = new Mock<ITransaction>();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);
            var postTaskProgressUpdateRequestMockDto = new PostTaskProgressUpdateRequestMockDto();
            _mapper.Setup(x => x.Map<GetRewardTypeConsumerTaskRequestDto>(It.IsAny<ConsumerTaskModel>())).
                Returns(new GetRewardTypeConsumerTaskRequestMockDto());
            var response = await _triviaController.TaskProgressUpdate(postTaskProgressUpdateRequestMockDto);
            Assert.NotNull(response);
        }

        [Fact]
        public async TaskAlias Should_NullCheck_TaskProgressUpdate_Service()
        {
            var transactionMock = new Mock<ITransaction>();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);
            var postTaskProgressUpdateRequestMockDto = new PostTaskProgressUpdateRequestMockDto();
            _consumerTaskRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ConsumerTaskModel, bool>>>(), false))
               .ReturnsAsync((ConsumerTaskMockModel)null);
            var response = await _triviaService.TaskProgressUpdate(postTaskProgressUpdateRequestMockDto);
            Assert.NotNull(response);
        }

        [Fact]
        public async TaskAlias Should_Exception_TaskProgressUpdate_Service()
        {
            var transactionMock = new Mock<ITransaction>();
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);
            var postTaskProgressUpdateRequestMockDto = new PostTaskProgressUpdateRequestMockDto();
            _consumerTaskRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ConsumerTaskModel, bool>>>(), false))
                .ThrowsAsync(new Exception("Test exception"));
            var result = await _triviaService.TaskProgressUpdate(postTaskProgressUpdateRequestMockDto);
            Assert.NotNull(result);
        }

        [Fact]
        public void TriviaQuestionGroupRepo_Constructor_ShouldInstantiate_WhenDependenciesAreProvided()
        {
            var mockLogger = new Mock<ILogger<BaseRepo<TriviaQuestionGroupModel>>>();
            var mockSession = new Mock<NHibernate.ISession>();
            var repo = new TriviaQuestionGroupRepo(mockLogger.Object, mockSession.Object);
            Assert.NotNull(repo);
        }

        [Fact]
        public void TriviaQuestionRepo_Constructor_ShouldInstantiate_WhenDependenciesAreProvided()
        {
            var mockLogger = new Mock<ILogger<BaseRepo<TriviaQuestionModel>>>();
            var mockSession = new Mock<NHibernate.ISession>();
            var repo = new TriviaQuestionRepo(mockLogger.Object, mockSession.Object);
            Assert.NotNull(repo);
        }

        [Fact]
        public void TriviaRepo_Constructor_ShouldInstantiate_WhenDependenciesAreProvided()
        {
            var mockLogger = new Mock<ILogger<BaseRepo<TriviaModel>>>();
            var mockSession = new Mock<NHibernate.ISession>();
            var repo = new TriviaRepo(mockLogger.Object, mockSession.Object);
            Assert.NotNull(repo);
        }
        [Fact]
        public async TaskAlias Should_Create_Trivia()
        {
            var triviaRequest = new TriviaRequestDto
            {
                trivia = new Trivia
                {
                    CtaTaskExternalCode = "xyz",
                    CreateUser = "abc",
                    TriviaCode = "test",
                },
                TaskRewardCode = "gfs"
            };

            _triviaRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TriviaModel, bool>>>(), false));
            _taskRewardRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false)).ReturnsAsync(new TaskRewardMockModel());

            _mapper.Setup(x => x.Map<TriviaModel>(It.IsAny<Trivia>())).Returns(new TriviaModel
            {
                CtaTaskExternalCode = "xyz",
                CreateUser = "abc",
                TriviaCode = "test",
                TaskRewardId = 1,
                Id = 1
            });
            _triviaRepo.Setup(x => x.CreateAsync(It.IsAny<TriviaModel>())).ReturnsAsync(new TriviaModel { TriviaId = 1 });
            var serviceResponse = await _triviaService.CreateTrivia(triviaRequest);
            var response = await _triviaController.CreateTrivia(triviaRequest);
            var okResult = Assert.IsType<OkObjectResult>(response.Result);
            var returnValue = Assert.IsType<BaseResponseDto>(okResult.Value);
            Assert.Null(returnValue.ErrorCode);
        }
        [Fact]
        public async TaskAlias Should_Return_404_Create_Trivia()
        {
            var triviaRequest = new TriviaRequestDto
            {
                trivia = new Trivia
                {
                    CtaTaskExternalCode = "xyz",
                    CreateUser = "abc",
                    TriviaCode = "test",
                },
                TaskRewardCode = "gfs"
            };
            _mapper.Setup(x => x.Map<TriviaModel>(It.IsAny<Trivia>())).Returns(new TriviaModel
            {
                CtaTaskExternalCode = "xyz",
                CreateUser = "abc",
                TriviaCode = "test",
                TaskRewardId = 1,
                Id = 0
            });
            _triviaRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TriviaModel, bool>>>(), false));
            _taskRewardRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false));

            _triviaRepo.Setup(x => x.CreateAsync(It.IsAny<TriviaModel>())).ReturnsAsync(new TriviaModel { TriviaId = 0 });
            var serviceResponse = await _triviaService.CreateTrivia(triviaRequest);
            var response = await _triviaController.CreateTrivia(triviaRequest);
            var okResult = Assert.IsType<OkObjectResult>(response.Result);
            var returnValue = Assert.IsType<BaseResponseDto>(okResult.Value);
            Assert.Equal(404, returnValue.ErrorCode);
        }
        [Fact]
        public async TaskAlias Should_Return_500_Create_Trivia()
        {
            var triviaRequest = new TriviaRequestDto
            {
                trivia = new Trivia
                {
                    CtaTaskExternalCode = "xyz",
                    CreateUser = "abc",
                    TriviaCode = "test",
                },
                TaskRewardCode = "gfs"
            };
            _mapper.Setup(x => x.Map<TriviaModel>(It.IsAny<Trivia>())).Returns(new TriviaModel
            {
                CtaTaskExternalCode = "xyz",
                CreateUser = "abc",
                TriviaCode = "test",
                TaskRewardId = 1,
                Id = 0
            });
            _triviaRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TriviaModel, bool>>>(), false)).ThrowsAsync(new Exception("An error occurred"));

            _triviaRepo.Setup(x => x.CreateAsync(It.IsAny<TriviaModel>())).ReturnsAsync(new TriviaModel { TriviaId = 0 });
            var serviceResponse = await _triviaService.CreateTrivia(triviaRequest);
            var response = await _triviaController.CreateTrivia(triviaRequest);
            var okResult = Assert.IsType<OkObjectResult>(response.Result);
            var returnValue = Assert.IsType<BaseResponseDto>(okResult.Value);
            Assert.Equal(500, returnValue.ErrorCode);
        }
        [Fact]
        public async TaskAlias Should_Create_Trivia_Question()
        {
            var triviaRequest = new TriviaQuestionRequestDto
            {
                QuestionExternalCode = "xyz",
                CreateUser = "abc",
                TriviaQuestionCode = "test",
            };

            _triviaQuestionRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TriviaQuestionModel, bool>>>(), false));

            _mapper.Setup(x => x.Map<TriviaQuestionModel>(It.IsAny<TriviaQuestionRequestDto>())).Returns(new TriviaQuestionModel
            {
                QuestionExternalCode = "xyz",
                CreateUser = "abc",
                TriviaQuestionCode = "test",
                TriviaQuestionId = 1
            });
            _triviaQuestionRepo.Setup(x => x.CreateAsync(It.IsAny<TriviaQuestionModel>())).ReturnsAsync(new TriviaQuestionModel { TriviaQuestionId = 1 });
            var serviceResponse = await _triviaService.CreateTriviaQuestion(triviaRequest);
            var response = await _triviaController.CreateTriviaQuestion(triviaRequest);
            var okResult = Assert.IsType<OkObjectResult>(response.Result);
            var returnValue = Assert.IsType<BaseResponseDto>(okResult.Value);
            Assert.Null(returnValue.ErrorCode);
        }
        [Fact]
        public async TaskAlias Should_Return_404_Create_Trivia_Question()
        {
            var triviaRequest = new TriviaQuestionRequestDto
            {
                QuestionExternalCode = "xyz",
                CreateUser = "abc",
                TriviaQuestionCode = "test",
            };

            _triviaQuestionRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TriviaQuestionModel, bool>>>(), false));

            _mapper.Setup(x => x.Map<TriviaQuestionModel>(It.IsAny<TriviaQuestionRequestDto>())).Returns(new TriviaQuestionModel
            {
                QuestionExternalCode = "xyz",
                CreateUser = "abc",
                TriviaQuestionCode = "test",
                TriviaQuestionId = 0
            });
            _triviaQuestionRepo.Setup(x => x.CreateAsync(It.IsAny<TriviaQuestionModel>()));
            var serviceResponse = await _triviaService.CreateTriviaQuestion(triviaRequest);
            var response = await _triviaController.CreateTriviaQuestion(triviaRequest);
            var okResult = Assert.IsType<OkObjectResult>(response.Result);
            var returnValue = Assert.IsType<BaseResponseDto>(okResult.Value);
            Assert.Equal(500, returnValue.ErrorCode);
        }
        [Fact]
        public async TaskAlias Should_Return_500_Create_Trivia_Question()
        {
            var triviaRequest = new TriviaQuestionRequestDto
            {
                QuestionExternalCode = "xyz",
                CreateUser = "abc",
                TriviaQuestionCode = "test",
            };

            _triviaQuestionRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TriviaQuestionModel, bool>>>(), false)).ThrowsAsync(new Exception("An error occurred"));

            _mapper.Setup(x => x.Map<TriviaQuestionModel>(It.IsAny<TriviaQuestionRequestDto>())).Returns(new TriviaQuestionModel
            {
                QuestionExternalCode = "xyz",
                CreateUser = "abc",
                TriviaQuestionCode = "test",
                TriviaQuestionId = 0
            });
            _triviaQuestionRepo.Setup(x => x.CreateAsync(It.IsAny<TriviaQuestionModel>()));
            var serviceResponse = await _triviaService.CreateTriviaQuestion(triviaRequest);
            var response = await _triviaController.CreateTriviaQuestion(triviaRequest);
            var okResult = Assert.IsType<OkObjectResult>(response.Result);
            var returnValue = Assert.IsType<BaseResponseDto>(okResult.Value);
            Assert.Equal(500, returnValue.ErrorCode);
        }

        [Fact]
        public async TaskAlias Should_Create_Trivia_Question_Group()
        {
            var triviaRequest = new TriviaQuestionGroupRequestDto
            {
                TriviaCode = "xyz",
                TriviaQuestionCode = "test",
                TriviaQuestionGroup = new TriviaQuestionGroupPostRequestDto
                {
                    TriviaId = 1,
                    TriviaQuestionGroupId = 2,
                    TriviaQuestionId = 1,
                    SequenceNbr = 1,
                    ValidStartTs = DateTime.UtcNow,
                    ValidEndTs = DateTime.UtcNow.AddDays(3),
                    CreateUser = "abc",

                }
            };
            _triviaQuestionRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TriviaQuestionModel, bool>>>(), false)).ReturnsAsync(new TriviaQuestionMockModel());

            _triviaRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TriviaModel, bool>>>(), false)).ReturnsAsync(new TriviaMockModel());

            _triviaQuestionGroupRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TriviaQuestionGroupModel, bool>>>(), false));


            _mapper.Setup(x => x.Map<TriviaQuestionGroupModel>(It.IsAny<TriviaQuestionGroupPostRequestDto>())).Returns(new TriviaQuestionGroupMockModel());

            _triviaQuestionGroupRepo.Setup(x => x.CreateAsync(It.IsAny<TriviaQuestionGroupModel>())).ReturnsAsync(new TriviaQuestionGroupModel { TriviaQuestionGroupId = 1 });
            var serviceResponse = await _triviaService.CreateTriviaQuestionGroup(triviaRequest);
            var response = await _triviaController.CreateTriviaQuestionGroup(triviaRequest);
            var okResult = Assert.IsType<OkObjectResult>(response.Result);
            var returnValue = Assert.IsType<BaseResponseDto>(okResult.Value);
            Assert.Null(returnValue.ErrorCode);
        }
        [Fact]
        public async TaskAlias Should_Return_404_Create_Trivia_Question_Group()
        {
            var triviaRequest = new TriviaQuestionGroupRequestDto
            {
                TriviaCode = "xyz",
                TriviaQuestionCode = "test",
                TriviaQuestionGroup = new TriviaQuestionGroupPostRequestDto
                {
                    TriviaId = 1,
                    TriviaQuestionGroupId = 2,
                    TriviaQuestionId = 1,
                    SequenceNbr = 1,
                    ValidStartTs = DateTime.UtcNow,
                    ValidEndTs = DateTime.UtcNow.AddDays(3),
                    CreateUser = "abc",

                }
            };
            _triviaQuestionRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TriviaQuestionModel, bool>>>(), false)).ReturnsAsync(new TriviaQuestionMockModel());

            _triviaRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TriviaModel, bool>>>(), false)).ReturnsAsync(new TriviaMockModel());

            _triviaQuestionGroupRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TriviaQuestionGroupModel, bool>>>(), false));


            _mapper.Setup(x => x.Map<TriviaQuestionGroupModel>(It.IsAny<TriviaQuestionGroupPostRequestDto>())).Returns(new TriviaQuestionGroupMockModel());

            _triviaQuestionGroupRepo.Setup(x => x.CreateAsync(It.IsAny<TriviaQuestionGroupModel>())).ReturnsAsync(new TriviaQuestionGroupModel { TriviaQuestionGroupId = 0 });
            var serviceResponse = await _triviaService.CreateTriviaQuestionGroup(triviaRequest);
            var response = await _triviaController.CreateTriviaQuestionGroup(triviaRequest);

            var okResult = Assert.IsType<OkObjectResult>(response.Result);
            var returnValue = Assert.IsType<BaseResponseDto>(okResult.Value);
            Assert.Equal(404, returnValue.ErrorCode);
        }
        [Fact]
        public async TaskAlias Should_Return_500_Create_Trivia_Question_Group()
        {
            var triviaRequest = new TriviaQuestionGroupRequestDto
            {
                TriviaCode = "xyz",
                TriviaQuestionCode = "test",
                TriviaQuestionGroup = new TriviaQuestionGroupPostRequestDto
                {
                    TriviaId = 1,
                    TriviaQuestionGroupId = 2,
                    TriviaQuestionId = 1,
                    SequenceNbr = 1,
                    ValidStartTs = DateTime.UtcNow,
                    ValidEndTs = DateTime.UtcNow.AddDays(3),
                    CreateUser = "abc",
                }
            };
            _triviaQuestionRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TriviaQuestionModel, bool>>>(), false)).ThrowsAsync(new Exception("An error occurred"));

            _triviaRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TriviaModel, bool>>>(), false)).ReturnsAsync(new TriviaMockModel());

            _triviaQuestionGroupRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TriviaQuestionGroupModel, bool>>>(), false));


            _mapper.Setup(x => x.Map<TriviaQuestionGroupModel>(It.IsAny<TriviaQuestionGroupPostRequestDto>())).Returns(new TriviaQuestionGroupMockModel());

            _triviaQuestionGroupRepo.Setup(x => x.CreateAsync(It.IsAny<TriviaQuestionGroupModel>())).ReturnsAsync(new TriviaQuestionGroupModel { TriviaQuestionGroupId = 0 });
            var serviceResponse = await _triviaService.CreateTriviaQuestionGroup(triviaRequest);
            var response = await _triviaController.CreateTriviaQuestionGroup(triviaRequest);

            var okResult = Assert.IsType<OkObjectResult>(response.Result);
            var returnValue = Assert.IsType<BaseResponseDto>(okResult.Value);
            Assert.Equal(500, returnValue.ErrorCode);
        }
        [Fact]
        public async TaskAlias Should_Return_500_Create_Trivia_Question_Group_controller()
        {
            var triviaRequest = new TriviaQuestionGroupRequestDto
            {
                TriviaCode = "xyz",
                TriviaQuestionCode = "test",
                TriviaQuestionGroup = new TriviaQuestionGroupPostRequestDto
                {
                    TriviaId = 1,
                    TriviaQuestionGroupId = 2,
                    TriviaQuestionId = 1,
                    SequenceNbr = 1,
                    ValidStartTs = DateTime.UtcNow,
                    ValidEndTs = DateTime.UtcNow.AddDays(3),
                    CreateUser = "abc",

                }
            };
            var triviaService = new Mock<ITriviaService>();
            var triviaController = new TriviaController(_triviaLogger.Object, triviaService.Object);
            triviaService.Setup(x => x.CreateTriviaQuestionGroup(triviaRequest)).ThrowsAsync(new Exception("An error occurred"));
            var response = await triviaController.CreateTriviaQuestionGroup(triviaRequest);

            var okResult = Assert.IsType<ObjectResult>(response.Result);
            var returnValue = Assert.IsType<BaseResponseDto>(okResult.Value);
            Assert.Equal(500, returnValue.ErrorCode);
        }
        [Fact]
        public async TaskAlias Should_Return_500_Create_Trivia_controller()
        {
            var triviaRequest = new TriviaRequestDto
            {
                trivia = new Trivia
                {
                    CtaTaskExternalCode = "xyz",
                    CreateUser = "abc",
                    TriviaCode = "test",
                },
                TaskRewardCode = "gfs"
            };
            var triviaService = new Mock<ITriviaService>();
            var triviaController = new TriviaController(_triviaLogger.Object, triviaService.Object);
            triviaService.Setup(x => x.CreateTrivia(triviaRequest)).ThrowsAsync(new Exception("An error occurred"));
            var response = await triviaController.CreateTrivia(triviaRequest);

            var okResult = Assert.IsType<ObjectResult>(response.Result);
            var returnValue = Assert.IsType<BaseResponseDto>(okResult.Value);
            Assert.Equal(500, returnValue.ErrorCode);
        }
        [Fact]
        public async TaskAlias Should_Return_500_Create_Trivia_Question_controller()
        {
            var triviaRequest = new TriviaQuestionRequestDto
            {
                QuestionExternalCode = "xyz",
                CreateUser = "abc",
                TriviaQuestionCode = "test",
            };
            var triviaService = new Mock<ITriviaService>();
            var triviaController = new TriviaController(_triviaLogger.Object, triviaService.Object);
            triviaService.Setup(x => x.CreateTriviaQuestion(triviaRequest)).ThrowsAsync(new Exception("An error occurred"));
            var response = await triviaController.CreateTriviaQuestion(triviaRequest);

            var okResult = Assert.IsType<ObjectResult>(response.Result);
            var returnValue = Assert.IsType<BaseResponseDto>(okResult.Value);
            Assert.Equal(500, returnValue.ErrorCode);
        }

        [Fact]
        public async TaskAlias GetAllTrivia_ShouldReturnOk_WhenDataIsFetchedSuccessfully()
        {
            // Arrange
            var triviaList = new List<TriviaDataDto>
            {
                new TriviaDataDto { TriviaId = 1, TriviaCode = "TRIVIA01", TaskRewardId = 1001 }
            };
            var mockResponse = new TriviaResponseDto
            {
                TriviaList = triviaList
            };
           
            _triviaRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<TriviaModel, bool>>>(), false)).ReturnsAsync(new List<TriviaModel>
            {
                new TriviaModel() { TriviaId = 1, TriviaCode = "TRIVIA01", TaskRewardId = 1001 }
            });
            _mapper.Setup(x => x.Map<IList<TriviaDataDto>>(It.IsAny<IList<TriviaModel>>()))
                .Returns(triviaList);

            // Act
            var result = await _triviaController.GetAllTrivia();

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<TriviaResponseDto>(actionResult.Value);
            Assert.NotNull(response.TriviaList);
            Assert.Single(response.TriviaList);
            Assert.Equal("TRIVIA01", response.TriviaList.First().TriviaCode);
        }

        [Fact]
        public async TaskAlias GetAllTrivia_ShouldReturn500_WhenServiceThrowsException()
        {
            // Arrange
            var _mockTriviaService = new Mock<ITriviaService>();
            _mockTriviaService
                .Setup(s => s.GetAllTrivia())
                .ThrowsAsync(new Exception("Service Error"));
            var _mockLogger = new Mock<ILogger<TriviaController>>();
            var _controller = new TriviaController(_mockLogger.Object, _mockTriviaService.Object);

            // Act
            var result = await _controller.GetAllTrivia();

            // Assert
            var actionResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, actionResult.StatusCode);

            var response = Assert.IsType<TriviaResponseDto>(actionResult.Value);
            Assert.Equal(StatusCodes.Status500InternalServerError, response.ErrorCode);
            Assert.Equal("An unexpected error occurred while retrieving trivia data. Please try again later.", response.ErrorMessage);
        }
        [Fact]
        public async void UpdateTrivia_ShouldReturn404_WhenRequestDtoIsNull()
        {
            // Act
            var result = await _triviaService.UpdateTrivia(null);

            // Assert
            Assert.Equal(StatusCodes.Status404NotFound, result.ErrorCode);
            Assert.Equal("Trivia record Not Found", result.ErrorMessage);
        }

        [Fact]
        public async void UpdateTrivia_ShouldReturn404_WhenTaskRewardNotFound()
        {
            // Arrange
            var requestDto = new TriviaRequestDto { TaskRewardCode = "TR123", trivia = new Trivia { TriviaCode = "T123", CreateUser = "test" } };
            _taskRewardRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false))
                               ;

            // Act
            var result = await _triviaService.UpdateTrivia(requestDto);

            // Assert
            Assert.Equal(StatusCodes.Status404NotFound, result.ErrorCode);
            Assert.Equal("Task reward record not found", result.ErrorMessage);
        }

        [Fact]
        public async void UpdateTrivia_ShouldReturn409_WhenTriviaNotFound()
        {
            // Arrange
            var requestDto = new TriviaRequestDto { TaskRewardCode = "TR123", trivia = new Trivia {TriviaCode = "T123",CreateUser="test" } };
            _taskRewardRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false))
                               .ReturnsAsync(new TaskRewardModel { TaskRewardId = 1 });

            _triviaRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<TriviaModel, bool>>>(), false))
                          ;

            // Act
            var result = await _triviaService.UpdateTrivia(requestDto);

            // Assert
            Assert.Equal(StatusCodes.Status409Conflict, result.ErrorCode);
            Assert.Equal("Trivia does not exists", result.ErrorMessage);
        }

        [Fact]
        public async void UpdateTrivia_ShouldReturnSuccess_WhenTriviaUpdated()
        {
            // Arrange
            var requestDto = new TriviaRequestDto { TaskRewardCode = "TR123", trivia = new Trivia { TriviaCode = "T123", CreateUser = "test" } };
            var existingTrivia = new TriviaModel { TriviaId = 1, TriviaCode = "T123" };
            var taskRewardModel = new TaskRewardModel { TaskRewardId = 1 };

            _taskRewardRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false))
                               .ReturnsAsync(taskRewardModel);

            _triviaRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<TriviaModel, bool>>>(), false))
                           .ReturnsAsync(existingTrivia);

            _triviaRepo.Setup(repo => repo.UpdateAsync(It.IsAny<TriviaModel>()))
                           .ReturnsAsync(new TriviaModel { TriviaId = 1 });

            // Act
            var result = await _triviaService.UpdateTrivia(requestDto);

            // Assert
            Assert.Null(result.ErrorMessage);
        }

        [Fact]
        public async void UpdateTrivia_ShouldReturn404_WhenTriviaUpdateFails()
        {
            // Arrange
            var requestDto = new TriviaRequestDto { TaskRewardCode = "TR123", trivia = new Trivia { TriviaCode = "T123", CreateUser = "test" } };
            var existingTrivia = new TriviaModel { TriviaId = 1, TriviaCode = "T123" };
            var taskRewardModel = new TaskRewardModel { TaskRewardId = 1 };

            _taskRewardRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false))
                               .ReturnsAsync(taskRewardModel);

            _triviaRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<TriviaModel, bool>>>(), false))
                           .ReturnsAsync(existingTrivia);

            _triviaRepo.Setup(repo => repo.UpdateAsync(It.IsAny<TriviaModel>()))
                           .ReturnsAsync(new TriviaModel { TriviaId = 0 }); // Update failed

            // Act
            var result = await _triviaService.UpdateTrivia(requestDto);

            // Assert
            Assert.Equal(StatusCodes.Status404NotFound, result.ErrorCode);
            Assert.Equal("Trivia record Not Updated", result.ErrorMessage);
        }

        [Fact]
        public async void UpdateTrivia_ShouldReturn500_WhenExceptionOccurs()
        {
            // Arrange
            var requestDto = new TriviaRequestDto { TaskRewardCode = "TR123", trivia = new Trivia { TriviaCode = "T123", CreateUser = "test" } };
            _taskRewardRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false))
                               .ThrowsAsync(new Exception("Database connection failed"));

            // Act
            var result = await _triviaService.UpdateTrivia(requestDto);

            // Assert
            Assert.Equal(StatusCodes.Status500InternalServerError, result.ErrorCode);
            Assert.Equal("Trivia record Not Updated", result.ErrorMessage);
        }
    }
}
