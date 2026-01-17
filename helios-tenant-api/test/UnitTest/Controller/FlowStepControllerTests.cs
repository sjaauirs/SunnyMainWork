using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SunnyRewards.Helios.Common.Core.Repositories;
using SunnyRewards.Helios.Tenant.Api.Controllers;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;
using SunnyRewards.Helios.Tenant.Core.Domain.Models;
using SunnyRewards.Helios.Tenant.Infrastructure.Repositories;
using SunnyRewards.Helios.Tenant.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Tenant.Infrastructure.Services;
using Xunit;

public class FlowStepControllerTests
{
    private readonly Mock<IFlowStepRepo> _flowStepRepoMock;
    private readonly FlowStepService _flowStepService;
    private readonly FlowStepController _controller;

    public FlowStepControllerTests()
    {
        _flowStepRepoMock = new Mock<IFlowStepRepo>();

        var loggerService = new Mock<ILogger<FlowStepService>>();
        var loggerController = new Mock<ILogger<FlowStepController>>();
        var mapper = new Mock<IMapper>();

        _flowStepService = new FlowStepService(loggerService.Object, _flowStepRepoMock.Object, mapper.Object);
        _controller = new FlowStepController(loggerController.Object, _flowStepService);
    }

    private FlowRequestDto GetRequest() => new()
    {
        TenantCode = "TEN123",
        FlowId = 1,
        CohortCodes = new List<string> { "COHORT1" },
        EffectiveDate = DateTime.UtcNow
    };

    [Fact]
    public async Task GetFlowSteps_ShouldReturnNotFound_WhenRepoReturnsNull()
    {
        // Arrange
        var request = GetRequest();

        _flowStepRepoMock
            .Setup(r => r.GetFlowSteps(It.IsAny<FlowRequestDto>()))
            .Returns((FlowResponseDto?)null);

        // Act
        var result = await _controller.GetFlowSteps(request);

        // Assert
        var OkResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<FlowResponseDto>(OkResult.Value);
        Assert.Equal("No flow found", response.ErrorMessage);

        _flowStepRepoMock.Verify(r => r.GetFlowSteps(It.IsAny<FlowRequestDto>()), Times.Once);
    }


    [Fact]
    public async Task GetFlowSteps_ShouldReturnOk_WhenFlowFound()
    {
        // Arrange
        var request = GetRequest();
        var repoResponse = new FlowResponseDto
        {
            TenantCode = "TEN123",
            CohortCode = "COHORT1",
            FlowId = 1,
            VersionNumber = 2,
            Steps = new List<FlowStepDto>
        {
            new FlowStepDto
            {
                StepId = 101,
                StepIdx = 1,
                ComponentType = "TRIVIA-CARD",
                ComponentName = "Trivia Question",
                OnSuccessStepId = 201,
                OnFailureStepId = 301
            }
        }
        };

        _flowStepRepoMock
            .Setup(r => r.GetFlowSteps(It.IsAny<FlowRequestDto>()))
            .Returns(repoResponse);

        // Act
        var result = await _controller.GetFlowSteps(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<FlowResponseDto>(okResult.Value);
        Assert.Equal("TEN123", response.TenantCode);
        Assert.Single(response.Steps);

        _flowStepRepoMock.Verify(r => r.GetFlowSteps(It.Is<FlowRequestDto>(x => x.TenantCode == "TEN123")), Times.Once);
    }
    [Fact]
    public async Task GetFlowSteps_ShouldReturn500_WhenRepoThrowsException()
    {
        // Arrange
        var request = GetRequest();

        _flowStepRepoMock
            .Setup(r => r.GetFlowSteps(It.IsAny<FlowRequestDto>()))
            .Throws(new Exception("DB Failure"));

        // Act
        var ex = await Assert.ThrowsAsync<Exception>(() => _controller.GetFlowSteps(request));

        // Assert
        Assert.Equal("DB Failure", ex.Message);
        _flowStepRepoMock.Verify(r => r.GetFlowSteps(It.IsAny<FlowRequestDto>()), Times.Once);
    }
    [Fact]
    public void GetFlowSteps_ShouldReturnResponseWithSteps_WhenFlowAndStepsExist()
    {
        // Arrange
        var tenantCode = "TEN123";
        var effectiveDate = DateTime.UtcNow;

        var flowModels = new List<FlowModel>
    {
        new FlowModel
        {
            Pk = 1,
            TenantCode = tenantCode,
            CohortCode = "COHORT1",
            DeleteNbr = 0,
            EffectiveStartTs = effectiveDate.AddDays(-1),
            EffectiveEndTs = null,
            VersionNbr = 2
        }
    }.AsQueryable();

        var stepModels = new List<FlowStepModel>
    {
        new FlowStepModel
        {
            Pk = 101,
            StepIdx = 1,
            FlowFk = 1,
            DeleteNbr = 0,
            CurrentComponentCatalogueFk = 1001,
            OnSuccessComponentCatalogueFk = 201,
            OnFailureComponentCatalogueFk = 301
        }
    }.AsQueryable();

        var componentCatalogueModels = new List<ComponentCatalogueModel>
    {
        new ComponentCatalogueModel
        {
            Pk = 1001,
            ComponentName = "Trivia Question",
            ComponentTypeFk = 5001,
            DeleteNbr = 0
        }
    }.AsQueryable();

        var componentTypeModels = new List<ComponentTypeModel>
    {
        new ComponentTypeModel
        {
            Pk = 5001,
            ComponentType = "TRIVIA-CARD",
            DeleteNbr = 0,
            IsActive = true
        }
    }.AsQueryable();

        var sessionMock = new Mock<NHibernate.ISession>();
        sessionMock.Setup(s => s.Query<FlowModel>()).Returns(flowModels);
        sessionMock.Setup(s => s.Query<FlowStepModel>()).Returns(stepModels);
        sessionMock.Setup(s => s.Query<ComponentCatalogueModel>()).Returns(componentCatalogueModels);
        sessionMock.Setup(s => s.Query<ComponentTypeModel>()).Returns(componentTypeModels);

        var loggerMock = new Mock<ILogger<BaseRepo<FlowStepModel>>>();
        var repo = new FlowStepRepo(loggerMock.Object, sessionMock.Object);

        var request = new FlowRequestDto
        {
            TenantCode = tenantCode,
            FlowId = 1,
            CohortCodes = new List<string> { "COHORT1" },
            EffectiveDate = effectiveDate
        };

        // Act
        var result = repo.GetFlowSteps(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(tenantCode, result.TenantCode);
        Assert.Equal(1, result.FlowId);
        Assert.Equal("COHORT1", result.CohortCode);
        Assert.Single(result.Steps);
        Assert.Equal("Trivia Question", result.Steps[0].ComponentName);
        Assert.Equal("TRIVIA-CARD", result.Steps[0].ComponentType);
    }


}
