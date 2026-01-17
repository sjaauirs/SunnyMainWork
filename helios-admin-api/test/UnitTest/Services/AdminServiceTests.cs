using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using FirebaseAdmin.Auth.Hash;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Core.Domain.Models;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Admin.UnitTest.Fixtures.MockModels;
using Xunit;

public class AdminServiceTests
{
    private readonly Mock<ILogger<AdminService>> _mockLogger;
    private readonly Mock<IEventHandlerScriptRepo> _mockEventHandlerScriptRepo;
    private readonly Mock<ITenantTaskRewardScriptRepo> _mockTenantTaskRewardScriptRepo;
    private readonly Mock<IMapper> _mockMapper;
    private readonly AdminService _adminService;
    private readonly Mock<IScriptRepo> _mockScriptRepo;
    private readonly Mock<IUserContextService> _contextservice;

    public AdminServiceTests()
    {
        _mockLogger = new Mock<ILogger<AdminService>>();
        _mockEventHandlerScriptRepo = new Mock<IEventHandlerScriptRepo>();
        _mockTenantTaskRewardScriptRepo = new Mock<ITenantTaskRewardScriptRepo>();
        _mockMapper = new Mock<IMapper>();
        _mockScriptRepo = new Mock<IScriptRepo>();
        _contextservice = new Mock<IUserContextService>();
        _adminService = new AdminService(
            _mockLogger.Object,
            _mockEventHandlerScriptRepo.Object,
            _mockTenantTaskRewardScriptRepo.Object,
            _mockMapper.Object,
            _mockScriptRepo.Object, _contextservice.Object
            );
    }

    [Fact]
    public void GetAdminScriptsAsync_ShouldReturnEmptyResponse_WhenNoScriptsFound()
    {
        // Arrange
        var tenantCode = "ten-ecada21e57154928a2bb959e8365b8b4";
        _mockEventHandlerScriptRepo.Setup(repo => repo.GetEventHandlerScripts(It.IsAny<string>())).Returns(new List<ExportEventHandlerScriptDto>());
        _mockTenantTaskRewardScriptRepo.Setup(r => r.GetTenantTaskRewardScripts(It.IsAny<string>())).Returns(new List<ExportTenantTaskRewardScriptDto>());

        // Act
        var result = _adminService.GetAdminScripts(tenantCode);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(StatusCodes.Status404NotFound, result.ErrorCode);
        Assert.Contains("No event handler or task-reward scripts", result.ErrorMessage);
    }

    [Fact]
    public void GetAdminScriptsAsync_ShouldReturnValidData_WhenScriptsExist()
    {
        // Arrange
        var tenantCode = "ten-ecada21e57154928a2bb959e8365b8b4";
        var eventHandlerScripts = new List<ExportEventHandlerScriptDto>()
        {
            new ExportEventHandlerScriptDto()
            {
                EventHandlerScript = new EventHandlerScriptMockModel(),
                Script = new ScriptMockModel()
            }
        };
        var taskRewardScripts = new List<ExportTenantTaskRewardScriptDto>()
        {
            new ExportTenantTaskRewardScriptDto()
            {
                TenantTaskRewardScript = new TenantTaskRewardScriptModelMock(),
                Script = new ScriptMockModel()
            }
        };
        _mockEventHandlerScriptRepo.Setup(repo => repo.GetEventHandlerScripts(It.IsAny<string>())).Returns(eventHandlerScripts);
        _mockTenantTaskRewardScriptRepo.Setup(r => r.GetTenantTaskRewardScripts(It.IsAny<string>())).Returns(taskRewardScripts);

        _mockMapper.Setup(m => m.Map<List<EventHandlerScriptDto>>(It.IsAny<List<EventHandlerResultModel>>()))
            .Returns(new List<EventHandlerScriptDto> { new EventHandlerScriptDto { ScriptId = 1 } });

        _mockMapper.Setup(m => m.Map<List<TenantTaskRewardScriptDto>>(It.IsAny<List<TenantTaskRewardScriptModel>>()))
            .Returns(new List<TenantTaskRewardScriptDto> { new TenantTaskRewardScriptDto { ScriptId = 2 } });

        _mockMapper.Setup(m => m.Map<List<ScriptDto>>(It.IsAny<List<ScriptModel>>()))
            .Returns(new List<ScriptDto> { new ScriptDto { ScriptId = 1 }, new ScriptDto { ScriptId = 2 } });


        // Act
        var result = _adminService.GetAdminScripts(tenantCode);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Scripts.Count);
    }

    [Fact]
    public void GetAdminScriptsAsync_ShouldHandleExceptionGracefully()
    {
        // Arrange
        var tenantCode = "ten-ecada21e57154928a2bb959e8365b8b4";
        _mockEventHandlerScriptRepo.Setup(repo => repo.GetEventHandlerScripts(It.IsAny<string>()))
            .Throws(new Exception("Database error"));

        // Act
        var result = _adminService.GetAdminScripts(tenantCode);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, result.ErrorCode);
        Assert.Equal("Database error", result.ErrorMessage);
    }

    [Fact]
    public async Task CreateAdminScripts_ShouldReturnSuccess_WhenNoErrors()
    {
        // Arrange
        var request = new ImportAdminRequestDto
        {
            TenantCode = "TEST",
            Scripts = new List<ScriptDto>
            {
                new ScriptDto { ScriptName = "AdminScript", ScriptCode = "src-67161077189781" }
            },
            TenantTaskRewardScripts = new List<TenantTaskRewardScriptDto>
            {
                new TenantTaskRewardScriptDto
                {
                    TenantTaskRewardScriptCode = "trs-1079177178787187847",
                    TaskRewardCode = "trw-7871877846817871807",
                    ScriptId = 1
                }
            },
            EventHandlerScripts = new List<EventHandlerScriptDto>
            {
                new EventHandlerScriptDto { ScriptId = 1 }
            }
        };

        var rewardCodeMap = new Dictionary<string, string>
        {
            { "trw-7871877846817871807", "trw-787100190919812807717871" }
        };

        _mockScriptRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<ScriptModel, bool>>>(), false));
        _mockScriptRepo.Setup(repo => repo.CreateAsync(It.IsAny<ScriptModel>()))
                   .ReturnsAsync(new ScriptMockModel());

        _mockMapper.Setup(m => m.Map<ScriptModel>(It.IsAny<ScriptDto>()))
                   .Returns(new ScriptModel { ScriptId = 1 });

        _mockTenantTaskRewardScriptRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<TenantTaskRewardScriptModel, bool>>>(), false));
        _mockTenantTaskRewardScriptRepo.Setup(repo => repo.CreateAsync(It.IsAny<TenantTaskRewardScriptModel>()))
                                       .ReturnsAsync(new TenantTaskRewardScriptModelMock());
        _mockMapper.Setup(m => m.Map<TenantTaskRewardScriptModel>(It.IsAny<TenantTaskRewardScriptDto>()))
                   .Returns(new TenantTaskRewardScriptModel());

        _mockEventHandlerScriptRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<EventHandlerScriptModel, bool>>>(), false));
        _mockEventHandlerScriptRepo.Setup(repo => repo.CreateAsync(It.IsAny<EventHandlerScriptModel>()))
                                   .ReturnsAsync(new EventHandlerScriptMockModel());
        _mockMapper.Setup(m => m.Map<EventHandlerScriptModel>(It.IsAny<EventHandlerScriptDto>()))
                   .Returns(new EventHandlerScriptModel());

        // Act
        var result = await _adminService.CreateAdminScripts(request, rewardCodeMap);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(StatusCodes.Status200OK, result.ErrorCode ?? StatusCodes.Status200OK);
        Assert.True(string.IsNullOrEmpty(result.ErrorMessage));
    }
    [Fact]
    public async Task CreateAdminScripts_InvalidTaskRewardCode_ReturnsPartialSuccess()
    {
        // Arrange
        var request = new ImportAdminRequestDto
        {
            TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4",
            Scripts = new List<ScriptDto>(),
            TenantTaskRewardScripts = new List<TenantTaskRewardScriptDto> {
                new() { TaskRewardCode = "trw-61715565415562781978379", ScriptId = 1, TenantTaskRewardScriptCode = "TRSC1" }
            },
            EventHandlerScripts = new List<EventHandlerScriptDto>()
        };

        var taskRewardDict = new Dictionary<string, string> { { "trw-0987653456876523", "trw-8765418765487654567" } }; // INVALID not in dict

        // Act
        var result = await _adminService.CreateAdminScripts(request, taskRewardDict);

        // Assert
        Assert.Equal(StatusCodes.Status206PartialContent, result.ErrorCode);
        Assert.Contains("Invalid taskRewardCode", result.ErrorMessage);
    }
    [Fact]
    public async Task CreateAdminScripts_InvalidScripts_ReturnsPartialSuccess()
    {
        // Arrange
        var request = new ImportAdminRequestDto
        {
            TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4",
            TenantTaskRewardScripts = new List<TenantTaskRewardScriptDto>
            {
                new TenantTaskRewardScriptDto
                {
                    TenantTaskRewardScriptCode = "trs-1079177178787187847",
                    TaskRewardCode = "trw-7871877846817871807",
                    ScriptId = 1
                }
            },
            EventHandlerScripts = new List<EventHandlerScriptDto>
            {
                new EventHandlerScriptDto { ScriptId = 1 }
            }
        };

        var rewardCodeMap = new Dictionary<string, string>
        {
            { "trw-7871877846817871807", "trw-787100190919812807717871" }
        };

        _mockTenantTaskRewardScriptRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<TenantTaskRewardScriptModel, bool>>>(), false));
        _mockTenantTaskRewardScriptRepo.Setup(repo => repo.CreateAsync(It.IsAny<TenantTaskRewardScriptModel>()))
                                       .ReturnsAsync(new TenantTaskRewardScriptModelMock());
        _mockMapper.Setup(m => m.Map<TenantTaskRewardScriptModel>(It.IsAny<TenantTaskRewardScriptDto>()))
                   .Returns(new TenantTaskRewardScriptModel());

        _mockEventHandlerScriptRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<EventHandlerScriptModel, bool>>>(), false));
        _mockEventHandlerScriptRepo.Setup(repo => repo.CreateAsync(It.IsAny<EventHandlerScriptModel>()))
                                   .ReturnsAsync(new EventHandlerScriptMockModel());
        _mockMapper.Setup(m => m.Map<EventHandlerScriptModel>(It.IsAny<EventHandlerScriptDto>()))
                   .Returns(new EventHandlerScriptModel());

        // Act
        var result = await _adminService.CreateAdminScripts(request, rewardCodeMap);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(StatusCodes.Status206PartialContent, result.ErrorCode);
        Assert.True(!string.IsNullOrEmpty(result.ErrorMessage));
        Assert.True(result?.ErrorMessage?.Length > 0);
    }
    [Fact]
    public async Task CreateAdminScripts_ThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        var request = new ImportAdminRequestDto
        {
            TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4",
            Scripts = new List<ScriptDto> { new() { ScriptName = "Script1" } }
        };

        _mockScriptRepo.Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<ScriptModel, bool>>>(), false))
            .Throws(new Exception("DB failure"));

        // Act
        var result = await _adminService.CreateAdminScripts(request);

        // Assert
        Assert.Equal(StatusCodes.Status206PartialContent, result.ErrorCode);
    }


}
