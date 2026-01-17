using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using SunnyRewards.Helios.Admin.Infrastructure.Services;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using Newtonsoft.Json;
using SunnyBenefits.Fis.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using Newtonsoft.Json.Serialization;
using SunnyBenefits.Fis.Core.Domain.Enums;

public class ConsumerTaskEventServiceTests
{
    private readonly Mock<ILogger<ConsumerTaskEventService>> _loggerMock;
    private readonly Mock<ITaskClient> _taskClientMock;
    private readonly Mock<IUserClient> _userClientMock;
    private readonly Mock<ITenantClient> _tenantClientMock;
    private readonly Mock<IFisClient> _fisClientMock;
    private readonly Mock<ICohortClient> _cohortClientMock;
    private readonly ConsumerTaskEventService _service;

    public ConsumerTaskEventServiceTests()
    {
        _loggerMock = new Mock<ILogger<ConsumerTaskEventService>>();
        _taskClientMock = new Mock<ITaskClient>();
        _userClientMock = new Mock<IUserClient>();
        _tenantClientMock = new Mock<ITenantClient>();
        _fisClientMock = new Mock<IFisClient>();
        _cohortClientMock = new Mock<ICohortClient>();

        _service = new ConsumerTaskEventService(
            _loggerMock.Object,
            _taskClientMock.Object,
            _userClientMock.Object,
            _tenantClientMock.Object,
            _fisClientMock.Object,
            _cohortClientMock.Object
        );
    }

    [Fact]
    public void ConsumerTaskEventProcess_ShouldReturnBadRequest_WhenMissingRequiredFields()
    {
        var request = new ConsumerTaskEventRequestDto { TenantCode = "", ConsumerCode = "" };

        var result = _service.ConsumerTaskEventProcess(request);

        Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
        Assert.Equal("One or more required parameters are missing.", result.ErrorMessage);
    }

    [Fact]
    public void ConsumerTaskEventProcess_ShouldReturnNotFound_WhenConsumerNotFound()
    {
        var request = new ConsumerTaskEventRequestDto { TenantCode = "Tenant1", ConsumerCode = "Consumer1" };
        _userClientMock.Setup(x => x.Post<GetConsumerResponseDto>(It.IsAny<string>(), It.IsAny<GetConsumerRequestDto>()));
        

        var result = _service.ConsumerTaskEventProcess(request);

        Assert.Equal(StatusCodes.Status404NotFound, result.ErrorCode);
        Assert.Equal("Consumer not found or invalid consumer code.", result.ErrorMessage);
    }

    [Fact]
    public void ConsumerTaskEventProcess_ShouldReturnNotFound_WhenTenantMismatch()
    {
        var request = new ConsumerTaskEventRequestDto { TenantCode = "Tenant1", ConsumerCode = "Consumer1" };
        var consumerResponse = new GetConsumerResponseDto { Consumer = new ConsumerDto { ConsumerCode = "Consumer1", TenantCode = "DifferentTenant" } };
        _userClientMock.Setup(x => x.Post<GetConsumerResponseDto>(It.IsAny<string>(), It.IsAny<GetConsumerRequestDto>()))
            .ReturnsAsync(consumerResponse);

        var result = _service.ConsumerTaskEventProcess(request);

        Assert.Equal(StatusCodes.Status404NotFound, result.ErrorCode);
        Assert.Equal("Tenant code does not match the consumer's tenant.", result.ErrorMessage);
    }

    [Fact]
    public void ConsumerTaskEventProcess_ShouldReturnServerError_WhenTenantOptionsNotDefined()
    {
        var request = new ConsumerTaskEventRequestDto { TenantCode = "Tenant1", ConsumerCode = "Consumer1" };
        var consumerResponse = new GetConsumerResponseDto { Consumer = new ConsumerDto { ConsumerCode = "Consumer1", TenantCode = "Tenant1" } };
        _userClientMock.Setup(x => x.Post<GetConsumerResponseDto>(It.IsAny<string>(), It.IsAny<GetConsumerRequestDto>()))
            .ReturnsAsync(consumerResponse);
        _tenantClientMock.Setup(x => x.Post<TenantDto>(It.IsAny<string>(), It.IsAny<GetTenantCodeRequestDto>()))
            .ReturnsAsync(new TenantDto { TenantOption = null });

        var result = _service.ConsumerTaskEventProcess(request);

        Assert.Equal(StatusCodes.Status500InternalServerError, result.ErrorCode);
        Assert.Equal("Tenant Options not defined for TenantCode :Tenant1", result.ErrorMessage);
    }

    [Fact]
    public void ConsumerTaskEventProcess_SuccessfulCardIssuance_ANY_ReturnsSuccess()
    {
        // Arrange
        var request = new ConsumerTaskEventRequestDto { TenantCode = "T123", ConsumerCode = "C123" };

        var consumerResp = new GetConsumerResponseDto { Consumer = new ConsumerDto { TenantCode = "T123", ConsumerCode = "C123" } };
        _userClientMock.Setup(x => x.Post<GetConsumerResponseDto>(It.IsAny<string>(), It.IsAny<GetConsumerRequestDto>()))
            .Returns(Task.FromResult(consumerResp));

        var tenantOptions = new TenantOptions
        {
            BenefitsOptions = new BenefitsOptions
            {
                DisableOnboardingFlow = true,
                CardIssueFlowType = new List<CardIssueFlowType>
                {
                    new CardIssueFlowType
                    {
                        FlowType = "TASK_COMPLETION_CHECK",
                        CohortCode = new List<string> ()
                    }
                },
                TaskCompletionCheckCode = new List<string> { "ANY" }
            }
        };

        var settings = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            },
            Formatting = Formatting.Indented // Optional: Makes it more readable
        };

        _tenantClientMock.Setup(x => x.Post<TenantDto>(It.IsAny<string>(), It.IsAny<GetTenantCodeRequestDto>()))
            .Returns(Task.FromResult(new TenantDto { TenantOption = JsonConvert.SerializeObject(tenantOptions , settings) }));

        var completedTasks = new List<ConsumerTaskDto>
            {
                new ConsumerTaskDto {ConsumerCode = "C123" , TaskId = 1 }
            };

        _taskClientMock.Setup(x => x.Post<ConsumerTaskResponseDto>(It.IsAny<string>(), It.IsAny<ConsumerTaskRequestDto>()))
            .Returns(Task.FromResult(new ConsumerTaskResponseDto { CompletedTasks = new List<TaskRewardDetailDto>() { new TaskRewardDetailDto() { } } }));

        _fisClientMock.Setup(x => x.Post<GetConsumerAccountResponseDto>(It.IsAny<string>(), It.IsAny<GetConsumerAccountRequestDto>()))
            .Returns(Task.FromResult(new GetConsumerAccountResponseDto() { ConsumerAccount = new ConsumerAccountDto() { CardIssueStatus = nameof(CardIssueStatus.NOT_ELIGIBLE) } }));

        _fisClientMock.Setup(x => x.Put<ConsumerAccountResponseDto>(It.IsAny<string>(), It.IsAny<UpdateCardIssueRequestDto>()))
            .Returns(Task.FromResult(new ConsumerAccountResponseDto()));

        // Act
        var response = _service.ConsumerTaskEventProcess(request);

        // Assert
        Assert.Null(response.ErrorCode);
        Assert.Null(response.ErrorMessage);
    }

    [Fact]
    public void ConsumerTaskEventProcess_SuccessfulCardIssuance_ANY_NoConsumerAccount()
    {
        // Arrange
        var request = new ConsumerTaskEventRequestDto { TenantCode = "T123", ConsumerCode = "C123" };

        var consumerResp = new GetConsumerResponseDto { Consumer = new ConsumerDto { TenantCode = "T123", ConsumerCode = "C123" } };
        _userClientMock.Setup(x => x.Post<GetConsumerResponseDto>(It.IsAny<string>(), It.IsAny<GetConsumerRequestDto>()))
            .Returns(Task.FromResult(consumerResp));

        var tenantOptions = new TenantOptions
        {
            BenefitsOptions = new BenefitsOptions
            {
                DisableOnboardingFlow = true,
                CardIssueFlowType = new List<CardIssueFlowType>
                {
                    new CardIssueFlowType
                    {
                        FlowType = "TASK_COMPLETION_CHECK",
                        CohortCode = new List<string> ()
                    }
                },
                TaskCompletionCheckCode = new List<string> { "ANY" }
            }
        };

        var settings = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            },
            Formatting = Formatting.Indented // Optional: Makes it more readable
        };

        _tenantClientMock.Setup(x => x.Post<TenantDto>(It.IsAny<string>(), It.IsAny<GetTenantCodeRequestDto>()))
            .Returns(Task.FromResult(new TenantDto { TenantOption = JsonConvert.SerializeObject(tenantOptions, settings) }));

        var completedTasks = new List<ConsumerTaskDto>
            {
                new ConsumerTaskDto {ConsumerCode = "C123" , TaskId = 1 }
            };

        _taskClientMock.Setup(x => x.Post<ConsumerTaskResponseDto>(It.IsAny<string>(), It.IsAny<ConsumerTaskRequestDto>()))
            .Returns(Task.FromResult(new ConsumerTaskResponseDto { CompletedTasks = new List<TaskRewardDetailDto>() { new TaskRewardDetailDto() { } } }));

        _fisClientMock.Setup(x => x.Post<GetConsumerAccountResponseDto>(It.IsAny<string>(), It.IsAny<GetConsumerAccountRequestDto>()))
            .Returns(Task.FromResult(new GetConsumerAccountResponseDto() {  }));

        _fisClientMock.Setup(x => x.Put<ConsumerAccountResponseDto>(It.IsAny<string>(), It.IsAny<UpdateCardIssueRequestDto>()))
            .Returns(Task.FromResult(new ConsumerAccountResponseDto()));

        // Act
        var response = _service.ConsumerTaskEventProcess(request);

        // Assert
        Assert.Equal(400, response.ErrorCode);
        Assert.Contains("Consumer Account not found for ConsumerCode", response.ErrorMessage);
    }

    [Fact]
    public void ConsumerTaskEventProcess_SuccessfulCardIssuance_ANY_ConsumerStateNotValid()
    {
        // Arrange
        var request = new ConsumerTaskEventRequestDto { TenantCode = "T123", ConsumerCode = "C123" };

        var consumerResp = new GetConsumerResponseDto { Consumer = new ConsumerDto { TenantCode = "T123", ConsumerCode = "C123" } };
        _userClientMock.Setup(x => x.Post<GetConsumerResponseDto>(It.IsAny<string>(), It.IsAny<GetConsumerRequestDto>()))
            .Returns(Task.FromResult(consumerResp));

        var tenantOptions = new TenantOptions
        {
            BenefitsOptions = new BenefitsOptions
            {
                DisableOnboardingFlow = true,
                CardIssueFlowType = new List<CardIssueFlowType>
                {
                    new CardIssueFlowType
                    {
                        FlowType = "TASK_COMPLETION_CHECK",
                        CohortCode = new List<string> ()
                    }
                },
                TaskCompletionCheckCode = new List<string> { "ANY" }
            }
        };

        var settings = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            },
            Formatting = Formatting.Indented // Optional: Makes it more readable
        };

        _tenantClientMock.Setup(x => x.Post<TenantDto>(It.IsAny<string>(), It.IsAny<GetTenantCodeRequestDto>()))
            .Returns(Task.FromResult(new TenantDto { TenantOption = JsonConvert.SerializeObject(tenantOptions, settings) }));

        var completedTasks = new List<ConsumerTaskDto>
            {
                new ConsumerTaskDto {ConsumerCode = "C123" , TaskId = 1 }
            };

        _taskClientMock.Setup(x => x.Post<ConsumerTaskResponseDto>(It.IsAny<string>(), It.IsAny<ConsumerTaskRequestDto>()))
            .Returns(Task.FromResult(new ConsumerTaskResponseDto { CompletedTasks = new List<TaskRewardDetailDto>() { new TaskRewardDetailDto() { } } }));

        _fisClientMock.Setup(x => x.Post<GetConsumerAccountResponseDto>(It.IsAny<string>(), It.IsAny<GetConsumerAccountRequestDto>()))
            .Returns(Task.FromResult(new GetConsumerAccountResponseDto() { ConsumerAccount = new ConsumerAccountDto() { CardIssueStatus = "ABC" } }));

        _fisClientMock.Setup(x => x.Put<ConsumerAccountResponseDto>(It.IsAny<string>(), It.IsAny<UpdateCardIssueRequestDto>()))
            .Returns(Task.FromResult(new ConsumerAccountResponseDto()));

        // Act
        var response = _service.ConsumerTaskEventProcess(request);

        // Assert
        Assert.Equal(409, response.ErrorCode);
        Assert.Contains("Consumer Account status is already", response.ErrorMessage);
    }

    [Fact]
    public void ConsumerTaskEventProcess_SuccessfulCardIssuance_Specific_ReturnsSuccess()
    {
        // Arrange
        var request = new ConsumerTaskEventRequestDto { TenantCode = "T123", ConsumerCode = "C123" };

        var consumerResp = new GetConsumerResponseDto { Consumer = new ConsumerDto { TenantCode = "T123", ConsumerCode = "C123" } };
        _userClientMock.Setup(x => x.Post<GetConsumerResponseDto>(It.IsAny<string>(), It.IsAny<GetConsumerRequestDto>()))
            .Returns(Task.FromResult(consumerResp));

        var tenantOptions = new TenantOptions
        {
            BenefitsOptions = new BenefitsOptions
            {
                DisableOnboardingFlow = true,
                CardIssueFlowType = new List<CardIssueFlowType>
                {
                    new CardIssueFlowType
                    {
                        FlowType = "TASK_COMPLETION_CHECK",
                        CohortCode = new List<string> ()
                    }
                },
                TaskCompletionCheckCode = new List<string> { "trw-0101" }
            }
        };

        var settings = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            },
            Formatting = Formatting.Indented // Optional: Makes it more readable
        };

        _tenantClientMock.Setup(x => x.Post<TenantDto>(It.IsAny<string>(), It.IsAny<GetTenantCodeRequestDto>()))
            .Returns(Task.FromResult(new TenantDto { TenantOption = JsonConvert.SerializeObject(tenantOptions, settings) }));

        var completedTasks = new List<TaskRewardDetailDto>
            {
                new TaskRewardDetailDto {TaskReward = new TaskRewardDto() { TaskRewardCode = "trw-0101" } }
            };

        _taskClientMock.Setup(x => x.Post<ConsumerTaskResponseDto>(It.IsAny<string>(), It.IsAny<ConsumerTaskRequestDto>()))
            .Returns(Task.FromResult(new ConsumerTaskResponseDto { CompletedTasks = completedTasks }));

        _fisClientMock.Setup(x => x.Post<GetConsumerAccountResponseDto>(It.IsAny<string>(), It.IsAny<GetConsumerAccountRequestDto>()))
            .Returns(Task.FromResult(new GetConsumerAccountResponseDto() { ConsumerAccount = new ConsumerAccountDto() { CardIssueStatus = nameof(CardIssueStatus.NOT_ELIGIBLE) } }));

        _fisClientMock.Setup(x => x.Put<ConsumerAccountResponseDto>(It.IsAny<string>(), It.IsAny<UpdateCardIssueRequestDto>()))
            .Returns(Task.FromResult(new ConsumerAccountResponseDto()));

        // Act
        var response = _service.ConsumerTaskEventProcess(request);

        // Assert
        Assert.Null(response.ErrorCode);
        Assert.Null(response.ErrorMessage);
    }

    [Fact]
    public void ConsumerTaskEventProcess_SuccessfulCardIssuance_Specific_TaskRewardcodeNotMatched_ReturnsSuccess()
    {
        // Arrange
        var request = new ConsumerTaskEventRequestDto { TenantCode = "T123", ConsumerCode = "C123" };

        var consumerResp = new GetConsumerResponseDto { Consumer = new ConsumerDto { TenantCode = "T123", ConsumerCode = "C123" } };
        _userClientMock.Setup(x => x.Post<GetConsumerResponseDto>(It.IsAny<string>(), It.IsAny<GetConsumerRequestDto>()))
            .Returns(Task.FromResult(consumerResp));

        var tenantOptions = new TenantOptions
        {
            BenefitsOptions = new BenefitsOptions
            {
                DisableOnboardingFlow = true,
                CardIssueFlowType = new List<CardIssueFlowType>
                {
                    new CardIssueFlowType
                    {
                        FlowType = "TASK_COMPLETION_CHECK",
                        CohortCode = new List<string> ()
                    }
                },
                TaskCompletionCheckCode = new List<string> { "trw-0102" }
            }
        };

        var settings = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            },
            Formatting = Formatting.Indented // Optional: Makes it more readable
        };

        _tenantClientMock.Setup(x => x.Post<TenantDto>(It.IsAny<string>(), It.IsAny<GetTenantCodeRequestDto>()))
            .Returns(Task.FromResult(new TenantDto { TenantOption = JsonConvert.SerializeObject(tenantOptions, settings) }));

        var completedTasks = new List<TaskRewardDetailDto>
            {
                new TaskRewardDetailDto {TaskReward = new TaskRewardDto() { TaskRewardCode = "trw-0101" } }
            };

        _taskClientMock.Setup(x => x.Post<ConsumerTaskResponseDto>(It.IsAny<string>(), It.IsAny<ConsumerTaskRequestDto>()))
            .Returns(Task.FromResult(new ConsumerTaskResponseDto { CompletedTasks = completedTasks }));

        _fisClientMock.Setup(x => x.Post<GetConsumerAccountResponseDto>(It.IsAny<string>(), It.IsAny<GetConsumerAccountRequestDto>()))
            .Returns(Task.FromResult(new GetConsumerAccountResponseDto() { ConsumerAccount = new ConsumerAccountDto() { CardIssueStatus = nameof(CardIssueStatus.NOT_ELIGIBLE) } }));


        _fisClientMock.Setup(x => x.Put<ConsumerAccountResponseDto>(It.IsAny<string>(), It.IsAny<UpdateCardIssueRequestDto>()))
            .Returns(Task.FromResult(new ConsumerAccountResponseDto()));

        // Act
        var response = _service.ConsumerTaskEventProcess(request);

        // Assert
        Assert.Equal(400 , response.ErrorCode);
    }

    [Fact]
    public void ConsumerTaskEventProcess_ShouldReturnInternalServerError_WhenExceptionOccurs()
    {
        // Arrange
        var request = new ConsumerTaskEventRequestDto { TenantCode = "T123", ConsumerCode = "C123" };
        _userClientMock.Setup(u => u.Post<GetConsumerResponseDto>(It.IsAny<string>(), It.IsAny<GetConsumerRequestDto>()))
            .Throws(new Exception("Unexpected error"));

        // Act
        var result = _service.ConsumerTaskEventProcess(request);

        // Assert
        Assert.Equal(StatusCodes.Status500InternalServerError, result.ErrorCode);
        Assert.Contains("An unexpected error occurred", result.ErrorMessage);
    }
}
