using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using SunnyRewards.Helios.Task.Core.Domain.Constants;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Models;
using SunnyRewards.Helios.Task.Infrastructure.Helpers;
using SunnyRewards.Helios.Task.Infrastructure.Services.Interface;
using Xunit;

public class QuestionnaireHelperTests
{
    private readonly Mock<ITaskRewardService> _taskRewardServiceMock;
    private readonly Mock<ILogger<QuestionnaireHelper>> _loggerMock;
    private readonly QuestionnaireHelper _helper;

    public QuestionnaireHelperTests()
    {
        _taskRewardServiceMock = new Mock<ITaskRewardService>();
        _loggerMock = new Mock<ILogger<QuestionnaireHelper>>();
        _helper = new QuestionnaireHelper(_taskRewardServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public void FilterQuestionnaireJsonByLanguage_ReturnsNull_WhenInputIsNullOrWhitespace()
    {
        Assert.Null(_helper.FilterQuestionnaireJsonByLanguage(null, "en"));
        Assert.Equal("", _helper.FilterQuestionnaireJsonByLanguage("", "en"));
        Assert.Equal("   ", _helper.FilterQuestionnaireJsonByLanguage("   ", "en"));
    }

    [Fact]
    public void FilterQuestionnaireJsonByLanguage_ReturnsLocalizedContent_WhenLanguageExists()
    {
        var json = @"{ ""en"": { ""q"": ""English"" }, ""fr"": { ""q"": ""French"" } }";
        var result = _helper.FilterQuestionnaireJsonByLanguage(json, "fr");
        Assert.Equal(@"{""q"":""French""}", result);
    }

    [Fact]
    public void FilterQuestionnaireJsonByLanguage_ReturnsEnglish_WhenLanguageNotFound()
    {
        var json = @"{ ""en"": { ""q"": ""English"" }, ""fr"": { ""q"": ""French"" } }";
        var result = _helper.FilterQuestionnaireJsonByLanguage(json, "de");
        Assert.Equal(@"{""q"":""English""}", result);
    }

    [Fact]
    public void FilterQuestionnaireJsonByLanguage_ReturnsOriginal_WhenNoLanguageMatches()
    {
        var json = @"{ ""es"": { ""q"": ""Spanish"" } }";
        var result = _helper.FilterQuestionnaireJsonByLanguage(json, "de");
        Assert.Equal(json, result);
    }

    [Fact]
    public void FilterQuestionnaireJsonByLanguage_ReturnsOriginal_WhenJsonIsInvalid()
    {
        var invalidJson = @"{ ""en"": ""missing bracket"" ";
        var result = _helper.FilterQuestionnaireJsonByLanguage(invalidJson, "en");
        Assert.Equal(invalidJson, result);
    }

    [Fact]
    public async Task GetTaskRewardDetails_SetsTaskRewardDetail_WhenTaskExternalCodeIsNotNull()
    {
        var response = new GetQuestionnaireResponseDto
        {
            Questionnaire = new QuestionnaireDto()
        };
        var ctaTaskreward = new TaskRewardModel
        {
            TaskRewardCode = "TRC",
            TaskExternalCode = "EXT"
        };
        var languageCode = "en";
        var expectedDetail = new TaskRewardDetailDto();

        _taskRewardServiceMock
            .Setup(s => s.GetTaskRewardByCode(It.IsAny<GetTaskRewardByCodeRequestDto>()))
            .ReturnsAsync(new GetTaskRewardByCodeResponseDto { TaskRewardDetail = expectedDetail });

        await _helper.GetTaskRewardDetails(response, ctaTaskreward, languageCode);

        Assert.Equal(expectedDetail, response.Questionnaire.taskRewardDetail);
        _taskRewardServiceMock.Verify(s => s.GetTaskRewardByCode(It.Is<GetTaskRewardByCodeRequestDto>(
            dto => dto.TaskRewardCode == "TRC" && dto.LanguageCode == languageCode)), Times.Once);
    }

    [Fact]
    public async Task GetTaskRewardDetails_SetsTaskRewardDetailToNull_WhenTaskExternalCodeIsNull()
    {
        var response = new GetQuestionnaireResponseDto
        {
            Questionnaire = new QuestionnaireDto()
        };
        var ctaTaskreward = new TaskRewardModel
        {
            TaskRewardCode = "TRC",
            TaskExternalCode = null
        };
        var languageCode = "en";

        await _helper.GetTaskRewardDetails(response, ctaTaskreward, languageCode);

        Assert.Null(response.Questionnaire.taskRewardDetail);
        _taskRewardServiceMock.Verify(s => s.GetTaskRewardByCode(It.IsAny<GetTaskRewardByCodeRequestDto>()), Times.Never);
    }

    [Fact]
    public void NormalizeJsonInput_ShouldReturnNull_WhenInputIsNull()
    {
        var result = _helper.NormalizeJsonInput(null);
        Assert.Null(result);
    }

    [Fact]
    public void NormalizeJsonInput_ShouldReturnRawString_WhenInvalidJson()
    {
        var input = "not-a-json";

        var result = _helper.NormalizeJsonInput(input);

        Assert.Equal("not-a-json", result);
    }

    [Fact]
    public void NormalizeJsonInput_ShouldReturnNormalizeJsonInputdJson_WhenValidJsonString()
    {
        var input = "{\"name\":\"Alice\",\"age\":30}";

        var result = _helper.NormalizeJsonInput(input);

        Assert.Equal("{\"name\":\"Alice\",\"age\":30}", result);
    }

    [Fact]
    public void NormalizeJsonInput_ShouldUnwrapDoubleEncodedJson()
    {
        var input = "\"{\\\"name\\\":\\\"Alice\\\"}\""; // double-encoded

        var result = _helper.NormalizeJsonInput(input);

        Assert.Equal("{\"name\":\"Alice\"}", result);
    }

    [Fact]
    public void NormalizeJsonInput_ShouldUnwrapTripleEncodedJson()
    {
        var input = "\"\\\"{\\\\\\\"name\\\\\\\":\\\\\\\"Alice\\\\\\\"}\\\"\""; // triple encoded

        var result = _helper.NormalizeJsonInput(input);

        Assert.Equal("{\"name\":\"Alice\"}", result);
    }

    [Fact]
    public void NormalizeJsonInput_ShouldSerializeObject_WhenInputIsAnonymousObject()
    {
        var input = new { Name = "Bob", Age = 40 };

        var result = _helper.NormalizeJsonInput(input);

        Assert.Equal("{\"name\":\"Bob\",\"age\":40}", result);
    }
}
