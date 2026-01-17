using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Core.Domain.Models;
using SunnyRewards.Helios.Admin.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using System.Linq.Expressions;
using Xunit;

public class PickAPurseEventProcessorTests
{
    private readonly Mock<ILogger<PickAPurseEventProcessor>> _mockLogger;
    private readonly Mock<ILogger<EventProcessorHelper>> _mockeventProcessorLogger;
    private readonly Mock<IEventHandlerScriptRepo> _mockEventHandlerScriptRepo;
    private readonly Mock<IEventHandlerResultRepo> _mockEventHandlerResultRepo;
    private readonly Mock<IScriptRepo> _mockScriptRepo;
    private readonly Mock<IHeliosScriptEngine> _mockHeliosScriptEngine;
    private readonly Mock<IOnBoardingInitialFundingService> _mockOnBoardingInitialFundingService;

    private readonly PickAPurseEventProcessor _processor;
    private readonly IEventProcessorHelper _eventProcessorHelper;

    public PickAPurseEventProcessorTests()
    {
        _mockLogger = new Mock<ILogger<PickAPurseEventProcessor>>();
        _mockeventProcessorLogger = new Mock<ILogger<EventProcessorHelper>>();
        _mockEventHandlerScriptRepo = new Mock<IEventHandlerScriptRepo>();
        _mockEventHandlerResultRepo = new Mock<IEventHandlerResultRepo>();
        _mockScriptRepo = new Mock<IScriptRepo>();
        _mockHeliosScriptEngine = new Mock<IHeliosScriptEngine>();
        _mockOnBoardingInitialFundingService = new Mock<IOnBoardingInitialFundingService>();

        _eventProcessorHelper = new EventProcessorHelper(_mockeventProcessorLogger.Object, _mockEventHandlerScriptRepo.Object,
            _mockEventHandlerResultRepo.Object, _mockScriptRepo.Object, _mockHeliosScriptEngine.Object);

        _processor = new PickAPurseEventProcessor(
            _mockLogger.Object,
            _eventProcessorHelper,
            _mockOnBoardingInitialFundingService.Object
            
        );
    }

    [Fact]
    public async Task ProcessEvent_ScriptExists_ExecutesAndSavesResult()
    {
        // Arrange
        var eventRequest = new PostEventRequestModel
        {
            EventType = "TestEvent",
            EventSubtype = "SubType",
            ConsumerCode = "Consumer123",
            TenantCode = "Tenant123",
            EventCode = "EventCode123",
            EventData = "{}"
        };

        var eventHandlerScript = new EventHandlerScriptModel
        {
            EventType = "TestEvent",
            EventSubType = "SubType",
            ScriptId = 1,
            DeleteNbr = 0
        };

        var scriptModel = new ScriptModel
        {
            ScriptId = 1,
            ScriptSource = "return true;",
            ScriptJson = MockScriptJsonDto(),
            DeleteNbr = 0
        };

        var scriptExecutionResult = new ScriptExecutionResultDto
        {
            ResultCode = 0,
            ResultMap = new Dictionary<string, object> { { "Result", "Success" } }
        };

        _mockEventHandlerScriptRepo
            .Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<EventHandlerScriptModel, bool>>>(), false))
            .ReturnsAsync(eventHandlerScript); 

        _mockScriptRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ScriptModel, bool>>>(), false)).ReturnsAsync(new ScriptModel
        {
            ScriptId = 1,
            ScriptJson = MockScriptJsonDto(),
            ScriptSource = "xyz",
            DeleteNbr = 0
        });

        _mockHeliosScriptEngine
            .Setup(engine => engine.ExecuteScript(
                It.IsAny<ScriptContext>(),
                It.IsAny<ScriptArgumentContext>(),
                It.IsAny<string>()
            ))
            .Returns(scriptExecutionResult);

        // Act
        var result = await _processor.ProcessEvent(eventRequest);

        // Assert
        Assert.True(result);

        _mockEventHandlerResultRepo.Verify(repo => repo.CreateAsync(It.IsAny<EventHandlerResultModel>()), Times.Once);

    }

    [Fact]
    public async Task ProcessEvent_NoScript_ThrowsError()
    {
        // Arrange
        var eventRequest = new PostEventRequestModel
        {
            EventType = "NonExistentEvent",
            EventSubtype = "SubType",
            ConsumerCode = "Consumer123",
            TenantCode = "Tenant123",
            EventData = "{\"pickedPurseLabels\":[\"OTC\",\"FOD\",\"REW\"]}"
        };

        _mockEventHandlerScriptRepo
           .Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<EventHandlerScriptModel, bool>>>(), false));

        // Act
        var exception = await Assert.ThrowsAsync<InvalidDataException>(() => _processor.ProcessEvent(eventRequest));

        _mockEventHandlerResultRepo.Verify(repo => repo.CreateAsync(It.IsAny<EventHandlerResultModel>()), Times.Never);
    }

    [Fact]
    public async Task ProcessEvent_ArgumentContextCreationFails_ThrowsExceptionAndLogsError()
    {
        // Arrange
        var eventRequest = new PostEventRequestModel
        {
            EventType = "TestEvent",
            EventSubtype = "SubType",
            EventCode = "EventCode123",
            EventData = "{\"pickedPurseLabels\":[\"OTC\",\"FOD\",\"REW\"]}"
        };

        var eventHandlerScript = new EventHandlerScriptModel
        {
            EventType = "TestEvent",
            EventSubType = "SubType",
            ScriptId = 1,
            DeleteNbr = 0
        };

        var scriptModel = new ScriptModel
        {
            ScriptId = 1,
            ScriptJson = "InvalidJson",
            ScriptSource = "SomeSource",
            DeleteNbr = 0
        };

        _mockEventHandlerScriptRepo
            .Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<EventHandlerScriptModel, bool>>>(), false))
            .ReturnsAsync(eventHandlerScript);

        _mockScriptRepo
            .Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<ScriptModel, bool>>>(), false))
            .ReturnsAsync(scriptModel);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<JsonReaderException>(() => _processor.ProcessEvent(eventRequest));

       
    }

    [Fact]
    public async Task ProcessEvent_ScriptExecutionReturnsNonZeroResultCode_ThrowsInvalidDataException()
    {
        // Arrange
        var eventRequest = new PostEventRequestModel
        {
            EventType = "TestEvent",
            EventSubtype = "SubType",
            EventCode = "EventCode123",
            EventData = "{}"
        };

        var eventHandlerScript = new EventHandlerScriptModel
        {
            EventType = "TestEvent",
            EventSubType = "SubType",
            ScriptId = 1,
            DeleteNbr = 0
        };

        var scriptModel = new ScriptModel
        {
            ScriptId = 1,
            ScriptSource = "return false;",
            ScriptJson = MockScriptJsonDto(),
            DeleteNbr = 0
        };

        var scriptExecutionResult = new ScriptExecutionResultDto
        {
            ResultCode = 500,
            ResultMap = new Dictionary<string, object> { { "Error", "Failure" } }
        };

        _mockEventHandlerScriptRepo
            .Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<EventHandlerScriptModel, bool>>>(), false))
            .ReturnsAsync(eventHandlerScript);

        _mockScriptRepo
            .Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<ScriptModel, bool>>>(), false))
            .ReturnsAsync(scriptModel);

        _mockHeliosScriptEngine
            .Setup(engine => engine.ExecuteScript(
                It.IsAny<ScriptContext>(),
                It.IsAny<ScriptArgumentContext>(),
                It.IsAny<string>()
            ))
            .Returns(scriptExecutionResult);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidDataException>(() => _processor.ProcessEvent(eventRequest));
   }

    public string MockScriptJsonDto()
    {
        // Arrange
        var scriptDto = new ScriptJsonDto
        {
            Args = new List<Argument>
    {
         new Argument { ArgName = "initialFundingRequestDto", ArgType = "object" },
          new Argument { ArgName = "onBoardingInitialFundingService", ArgType = "object" }
    },
            Result = new Result
            {
                ResultMap = "Map1",
                ResultCode = "200",
                ErrorMessage = string.Empty
            }
        };

        return JsonConvert.SerializeObject(scriptDto);

    }


    

}
