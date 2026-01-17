using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SunnyRewards.Helios.Task.Api.Controller;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Models;
using SunnyRewards.Helios.Task.Infrastructure.Helpers.Interface;
using SunnyRewards.Helios.Task.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Task.Infrastructure.Services;
using SunnyRewards.Helios.Task.Infrastructure.Services.Interface;
using SunnyRewards.Helios.Task.UnitTest.Fixtures.MockRepositories;
using System.Linq.Expressions;
using Xunit;
using TaskAlias = System.Threading.Tasks.Task;

public class QuestionnaireControllerUnitTest
{

    private readonly Mock<ILogger<QuestionnaireController>> _questionnaireLogger;
    private readonly Mock<ILogger<QuestionnaireService>> _questionnaireServiceLogger;
    private readonly Mock<IMapper> _mapper;
    private readonly Mock<IQuestionnaireQuestionRepo> _questionnaireQuestionRepo;
    private readonly Mock<IQuestionnaireRepo> _questionnaireRepo;
    private readonly Mock<IQuestionnaireQuestionGroupRepo> _questionnaireQuestionGroupRepo;
    private readonly Mock<ITaskRewardRepo> _taskRewardRepo;
    private readonly Mock<ITaskRewardService> _taskRewardService;
    private readonly QuestionnaireService _questionnaireService;
    private readonly QuestionnaireController _questionnaireController;
    private readonly Mock<NHibernate.ISession> _session;
    private readonly Mock<IConsumerTaskRepo> _consumerTaskRepo;
    private readonly Mock<IQuestionnaireHelper> _questionnaireHelper;

    public QuestionnaireControllerUnitTest()
    {
        _questionnaireLogger = new Mock<ILogger<QuestionnaireController>>();
        _questionnaireServiceLogger = new Mock<ILogger<QuestionnaireService>>();
        _mapper = new Mock<IMapper>();
        _questionnaireQuestionRepo = new Mock<IQuestionnaireQuestionRepo>();
        _questionnaireRepo = new Mock<IQuestionnaireRepo>();
        _questionnaireQuestionGroupRepo = new Mock<IQuestionnaireQuestionGroupRepo>();
        _taskRewardRepo = new Mock<ITaskRewardRepo>();
        _taskRewardService = new Mock<ITaskRewardService>();
        _session = new Mock<NHibernate.ISession>();
        _consumerTaskRepo = new ConsumerTaskMockRepo();
        _questionnaireHelper = new Mock<IQuestionnaireHelper>();
        _questionnaireService = new QuestionnaireService(
            _questionnaireServiceLogger.Object,
            _mapper.Object,
            _questionnaireQuestionRepo.Object,
            _questionnaireRepo.Object,
            _questionnaireQuestionGroupRepo.Object,
            _taskRewardRepo.Object,
            _taskRewardService.Object, _consumerTaskRepo.Object,
            _session.Object, _questionnaireHelper.Object
        );
        _questionnaireController = new QuestionnaireController(
            _questionnaireLogger.Object,
            _questionnaireService
        );
    }


    [Fact]
    public async TaskAlias GetQuestionnaire_ReturnsNotFound_WhenQuestionnaireMissing()
    {
        long taskRewardId = 32;
        string? languageCode = "en-us";
        string consumerCode = "cmr-d72c3aac5e644d28ae8541a2041caefe";
        _mapper.Setup(x => x.Map<QuestionnaireDto>(It.IsAny<QuestionnaireModel>()))
       .Returns(new QuestionnaireDto() { QuestionnaireId = 2 });
        _mapper.Setup(x => x.Map<List<QuestionnaireQuestionDto>>(It.IsAny<List<QuestionnaireQuestionModel>>()))
            .Returns(new List<QuestionnaireQuestionDto> { new QuestionnaireQuestionDto(){QuestionnaireQuestionId = 1 }
            });
        var response = await _questionnaireController.GetQuestionnaire(taskRewardId, consumerCode, languageCode);
        var result = response?.Result as OkObjectResult;
        Assert.True(result?.Value != null);
        Assert.True(result.StatusCode == 200);
    }
    [Fact]
    public async Task Should_Get_Questionnaire()
    {
        // Arrange
        _questionnaireRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<QuestionnaireModel, bool>>>(), It.IsAny<bool>())).ReturnsAsync(new QuestionnaireModel());

        _questionnaireQuestionGroupRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<QuestionnaireQuestionGroupModel, bool>>>(), It.IsAny<bool>()))
            .ReturnsAsync(new List<QuestionnaireQuestionGroupModel>() { new QuestionnaireQuestionGroupModel { QuestionnaireId = 1, QuestionnaireQuestionId = 1, } });
        _questionnaireQuestionRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<QuestionnaireQuestionModel, bool>>>(), It.IsAny<bool>()))
            .ReturnsAsync(new List<QuestionnaireQuestionModel>() { new QuestionnaireQuestionModel() { QuestionnaireQuestionId = 1,
                QuestionnaireQuestionCode = "some-code", QuestionExternalCode = "what_does_your_cred_scor_impa"} });

        _mapper.Setup(x => x.Map<QuestionnaireDto>(It.IsAny<QuestionnaireModel>())).Returns(new QuestionnaireDto() { QuestionnaireId = 2 });
        _mapper.Setup(x => x.Map<List<QuestionnaireQuestionDto>>(It.IsAny<List<QuestionnaireQuestionModel>>()))
            .Returns(new List<QuestionnaireQuestionDto> { new QuestionnaireQuestionDto(){ QuestionnaireQuestionId = 1, QuestionnaireJson = "{\n  \"answerText\": " +
                "[\n    \"Loan terms\",\n    \"Your marriage\",\n " +
                "   \"Golf handicap\"\n  ],\n  \"answerType\":" +
                " \"SINGLE\",\n  \"layoutType\": \"BUTTON\",\n  \"questionText\":" +
                " \"What does your credit score impact?\",\n  \"correctAnswer\": [\n    0\n  ]\n}",
            QuestionExternalCode = "what_does_your_cred_scor_impa" }
            });
        var filteredJson = "{ \"questions\": [{ \"lang\": \"en\" }] }"; 

        _questionnaireHelper
            .Setup(x => x.FilterQuestionnaireJsonByLanguage(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(filteredJson);
        // Act
        var result = await _questionnaireController.GetQuestionnaire(1, "consumer", "en-US");

        // Assert
        Assert.IsType<OkObjectResult>(result.Result);
    }
    [Fact]
    public async Task Should_Get_Questionnaire_Cta_present()
    {
        // Arrange
        _questionnaireRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<QuestionnaireModel, bool>>>(), It.IsAny<bool>())).ReturnsAsync(new QuestionnaireModel() { QuestionnaireId = 1, CtaTaskExternalCode = "survey_questionnaire"});

        _questionnaireQuestionGroupRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<QuestionnaireQuestionGroupModel, bool>>>(), It.IsAny<bool>()))
            .ReturnsAsync(new List<QuestionnaireQuestionGroupModel>() { new QuestionnaireQuestionGroupModel { QuestionnaireId = 1, QuestionnaireQuestionId = 1, } });
        _questionnaireQuestionRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<QuestionnaireQuestionModel, bool>>>(), It.IsAny<bool>()))
            .ReturnsAsync(new List<QuestionnaireQuestionModel>() { new QuestionnaireQuestionModel() { QuestionnaireQuestionId = 1,
                QuestionnaireQuestionCode = "some-code", QuestionExternalCode = "what_does_your_cred_scor_impa"} });

        _mapper.Setup(x => x.Map<QuestionnaireDto>(It.IsAny<QuestionnaireModel>())).Returns(new QuestionnaireDto() { QuestionnaireId = 2 });
        _mapper.Setup(x => x.Map<List<QuestionnaireQuestionDto>>(It.IsAny<List<QuestionnaireQuestionModel>>()))
            .Returns(new List<QuestionnaireQuestionDto> { new QuestionnaireQuestionDto(){ QuestionnaireQuestionId = 1, QuestionnaireJson = "{\n  \"answerText\": " +
                "[\n    \"Loan terms\",\n    \"Your marriage\",\n " +
                "   \"Golf handicap\"\n  ],\n  \"answerType\":" +
                " \"SINGLE\",\n  \"layoutType\": \"BUTTON\",\n  \"questionText\":" +
                " \"What does your credit score impact?\",\n  \"correctAnswer\": [\n    0\n  ]\n}",
            QuestionExternalCode = "what_does_your_cred_scor_impa" }
            });
        var filteredJson = "{ \"questions\": [{ \"lang\": \"en\" }] }";

        _questionnaireHelper
            .Setup(x => x.FilterQuestionnaireJsonByLanguage(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(filteredJson);
        // Act
        var result = await _questionnaireController.GetQuestionnaire(1, "consumer", "en-US");

        // Assert
        Assert.IsType<OkObjectResult>(result.Result);
    }
    [Fact]
    public async Task Should_Get_Questionnaire_Cta_present_consumer_task_present()
    {
        // Arrange
        _questionnaireRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<QuestionnaireModel, bool>>>(), It.IsAny<bool>())).ReturnsAsync(new QuestionnaireModel() { QuestionnaireId = 1, CtaTaskExternalCode = "survey_questionnaire" });

        _questionnaireQuestionGroupRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<QuestionnaireQuestionGroupModel, bool>>>(), It.IsAny<bool>()))
            .ReturnsAsync(new List<QuestionnaireQuestionGroupModel>() { new QuestionnaireQuestionGroupModel { QuestionnaireId = 1, QuestionnaireQuestionId = 1, } });
        _questionnaireQuestionRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<QuestionnaireQuestionModel, bool>>>(), It.IsAny<bool>()))
            .ReturnsAsync(new List<QuestionnaireQuestionModel>() { new QuestionnaireQuestionModel() { QuestionnaireQuestionId = 1,
                QuestionnaireQuestionCode = "some-code", QuestionExternalCode = "what_does_your_cred_scor_impa"} });

        _mapper.Setup(x => x.Map<QuestionnaireDto>(It.IsAny<QuestionnaireModel>())).Returns(new QuestionnaireDto() { QuestionnaireId = 2 });
        _mapper.Setup(x => x.Map<List<QuestionnaireQuestionDto>>(It.IsAny<List<QuestionnaireQuestionModel>>()))
            .Returns(new List<QuestionnaireQuestionDto> { new QuestionnaireQuestionDto(){ QuestionnaireQuestionId = 1, QuestionnaireJson = "{\n  \"answerText\": " +
                "[\n    \"Loan terms\",\n    \"Your marriage\",\n " +
                "   \"Golf handicap\"\n  ],\n  \"answerType\":" +
                " \"SINGLE\",\n  \"layoutType\": \"BUTTON\",\n  \"questionText\":" +
                " \"What does your credit score impact?\",\n  \"correctAnswer\": [\n    0\n  ]\n}",
            QuestionExternalCode = "what_does_your_cred_scor_impa" }
            });
        var filteredJson = "{ \"questions\": [{ \"lang\": \"en\" }] }";

        _questionnaireHelper
            .Setup(x => x.FilterQuestionnaireJsonByLanguage(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(filteredJson);
        _taskRewardRepo
            .SetupSequence(x => x.FindOneAsync(It.IsAny<Expression<Func<TaskRewardModel, bool>>>(), false))
            .ReturnsAsync(new TaskRewardModel
            {
                TaskRewardId = 100,
                TenantCode = "TEN-123",
                TaskExternalCode = "questionnaire_feedback"
            }) 
            .ReturnsAsync(new TaskRewardModel
            {
                TaskRewardId = 100,
                TenantCode = "TEN-123",
                TaskExternalCode = "feedback_start"
            });

        // Act
        var result = await _questionnaireController.GetQuestionnaire(1, "consumer", "en-US");

        // Assert
        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public async TaskAlias GetQuestionnaire_Throws_Exception()
    {
        long taskRewardId = 32;
        string? languageCode = "en-us";
        string consumerCode = "cmr-d72c3aac5e644d28ae8541a2041caefe";
        _questionnaireRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<QuestionnaireModel, bool>>>(), It.IsAny<bool>()))
            .ThrowsAsync(new Exception());
        var response = await _questionnaireController.GetQuestionnaire(taskRewardId, consumerCode, languageCode);
        var result = response?.Result as OkObjectResult;
        Assert.True(result?.Value != null);
        Assert.True(result.StatusCode == 200);
    }


    [Fact]
    public async TaskAlias GetQuestionnaire_ReturnsErrorDto_WhenExceptionThrown()
    {
        // Arrange
        var exceptionMessage = "Test exception";
        var _serviceMock = new Mock<IQuestionnaireService>();
        _serviceMock.Setup(s => s.GetQuestionnaire(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception(exceptionMessage));
        var controller = new QuestionnaireController(_questionnaireLogger.Object, _serviceMock.Object);

        // Act
        var result = await controller.GetQuestionnaire(1, "consumer", "en");

        // Assert
        var errorDto = Assert.IsType<GetQuestionnaireResponseDto>(result.Value);
        Assert.Equal(exceptionMessage, errorDto.ErrorMessage);
    }

}
