using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using SunnyRewards.Helios.Admin.Api.Controllers;
using SunnyRewards.Helios.Admin.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TaskAlias = System.Threading.Tasks.Task;
using Microsoft.AspNetCore.Mvc;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Core.Domain.Models;
using SunnyRewards.Helios.Admin.UnitTest.Fixtures.MockDtos;
using Xunit;
using SunnyRewards.Helios.Wallet.Core.Domain.Models;
using System.Linq.Expressions;
using SunnyRewards.Helios.Admin.UnitTest.Fixtures.MockModels;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients;

namespace SunnyRewards.Helios.Admin.UnitTest.Controllers
{
    public class TenantTaskRewardScriptControllerUnitTest
    {
        private readonly Mock<ILogger<TenantTaskRewardScriptController>> _controllerLogger;
        private readonly Mock<ILogger<TenantTaskRewardScriptService>> _serviceLogger;
        private readonly Mock<IMapper> _mapper;
        private readonly Mock<NHibernate.ISession> _session;
        private readonly Mock<ITenantTaskRewardScriptRepo> _repo;
        private readonly Mock<IScriptRepo> _scriptRepo;
        private readonly Mock<IConsumerTaskService> _consumerTaskService;
        private readonly Mock<ITaskClient> _taskClient;
        private readonly Mock<IUserContextService> _usercontextClient;
        private readonly ITenantTaskRewardScriptService _TenantTaskRewardScriptService;
        private readonly TenantTaskRewardScriptController _TenantTaskRewardScriptController;

        public TenantTaskRewardScriptControllerUnitTest()
        {
            // Initialize mocks
            _controllerLogger = new Mock<ILogger<TenantTaskRewardScriptController>>();
            _serviceLogger = new Mock<ILogger<TenantTaskRewardScriptService>>();
            _repo = new Mock<ITenantTaskRewardScriptRepo>();
            _scriptRepo = new Mock<IScriptRepo>();
            _taskClient = new Mock<ITaskClient>();
            _consumerTaskService = new Mock<IConsumerTaskService>();
            _usercontextClient = new Mock<IUserContextService>();
            _session = new Mock<NHibernate.ISession>();
            _mapper = new Mock<IMapper>();
            _TenantTaskRewardScriptService = new TenantTaskRewardScriptService(_serviceLogger.Object, _repo.Object, _scriptRepo.Object, _session.Object, _mapper.Object, _consumerTaskService.Object, _taskClient.Object, _usercontextClient.Object);
            _TenantTaskRewardScriptController = new TenantTaskRewardScriptController(_controllerLogger.Object, _TenantTaskRewardScriptService);
        }
        [Fact]
        public async TaskAlias CreateTenantTaskRewardScript_ShouldReturnOkResult()
        {
            // Arrange
            var requestDto = CreateMockDto();

            var scriptdata = new ScriptModel { ScriptId = 1, ScriptCode = "SCR001", ScriptName = "Script One" };
            _scriptRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ScriptModel, bool>>>(), false)).ReturnsAsync(scriptdata);
            _repo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TenantTaskRewardScriptModel, bool>>>(), false));
            _taskClient.Setup(client => client.Post<GetTaskRewardByCodeResponseDto>("get-task-reward-by-code", It.IsAny<GetTaskRewardByCodeRequestDto>())).ReturnsAsync(new GetTaskRewardByCodeResponseMockDto()) ;
            _consumerTaskService.Setup(x => x.GetTenantByTenantCode(It.IsAny<string>())).ReturnsAsync(true);
            _mapper.Setup(x => x.Map<TenantTaskRewardScriptModel>(It.IsAny<TenantTaskRewardScriptRequestDto>()))
         .Returns(new TenantTaskRewardScriptModelMock ());
            _repo.Setup(x => x.CreateAsync(It.IsAny<TenantTaskRewardScriptModel>())).ReturnsAsync(new TenantTaskRewardScriptModelMock() );
            // Act
            var result = await _TenantTaskRewardScriptController.CreateTenantTaskRewardScript(requestDto);

            // Assert
            var okResult = result?.Result as OkObjectResult;
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }
        [Fact]
        public async TaskAlias CreateTenantTaskRewardScript_ShouldReturn404Result()
        {
            // Arrange
            var requestDto = CreateMockDto();

            var scriptdata = new ScriptModel { ScriptId = 1, ScriptCode = "SCR001", ScriptName = "Script One" };
            _scriptRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ScriptModel, bool>>>(), false));
            _repo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TenantTaskRewardScriptModel, bool>>>(), false));
            _taskClient.Setup(client => client.Post<GetTaskRewardByCodeResponseDto>("get-task-reward-by-code", It.IsAny<GetTaskRewardByCodeRequestDto>())).ReturnsAsync(new GetTaskRewardByCodeResponseMockDto());
            _consumerTaskService.Setup(x => x.GetTenantByTenantCode(It.IsAny<string>())).ReturnsAsync(true);

            _mapper.Setup(x => x.Map<TenantTaskRewardScriptModel>(It.IsAny<TenantTaskRewardScriptRequestDto>()))
         .Returns(new TenantTaskRewardScriptModelMock());
            _repo.Setup(x => x.CreateAsync(It.IsAny<TenantTaskRewardScriptModel>())).ReturnsAsync(new TenantTaskRewardScriptModelMock() );
            // Act
            var result = await _TenantTaskRewardScriptController.CreateTenantTaskRewardScript(requestDto);

            // Assert
            var notFoundkResult = result?.Result as ObjectResult;
            Assert.Equal(StatusCodes.Status404NotFound, notFoundkResult.StatusCode);
        }
        [Fact]
        public async TaskAlias CreateTenantTaskRewardScript_ShouldReturn500Result()
        {
            // Arrange
            var requestDto = CreateMockDto();

            var scriptdata = new ScriptModel { ScriptId = 1, ScriptCode = "SCR001", ScriptName = "Script One" };
            _scriptRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ScriptModel, bool>>>(), false)).ThrowsAsync(new Exception("Simulated exception"));
            _repo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TenantTaskRewardScriptModel, bool>>>(), false));
            _mapper.Setup(x => x.Map<TenantTaskRewardScriptModel>(It.IsAny<TenantTaskRewardScriptRequestDto>()))
         .Returns(It.IsAny<TenantTaskRewardScriptModel>());
            _repo.Setup(x => x.CreateAsync(It.IsAny<TenantTaskRewardScriptModel>())).ReturnsAsync(new TenantTaskRewardScriptModelMock() );
            // Act
            var result = await _TenantTaskRewardScriptController.CreateTenantTaskRewardScript(requestDto);

            // Assert
            var errorResult = result?.Result as ObjectResult;
            Assert.Equal(StatusCodes.Status500InternalServerError, errorResult.StatusCode);
        }[Fact]
        public async TaskAlias UpdateTenantTaskRewardScript_ShouldReturnOkResult()
        {
            // Arrange
            var requestDto = CreateUpdateMockDto();

            var scriptdata = new ScriptModel { ScriptId = 1, ScriptCode = "SCR001", ScriptName = "Script One" };
            _repo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TenantTaskRewardScriptModel, bool>>>(), false)).ReturnsAsync(new TenantTaskRewardScriptModelMock());
            _scriptRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ScriptModel, bool>>>(), false)).ReturnsAsync(scriptdata);
            _taskClient.Setup(client => client.Post<GetTaskRewardByCodeResponseDto>("get-task-reward-by-code", It.IsAny<GetTaskRewardByCodeRequestDto>())).ReturnsAsync(new GetTaskRewardByCodeResponseMockDto());
            _consumerTaskService.Setup(x => x.GetTenantByTenantCode(It.IsAny<string>())).ReturnsAsync(true);

            _repo.Setup(repo => repo.FindOneAsync(x => x.TaskRewardCode == requestDto.TaskRewardCode && x.DeleteNbr == 0
              && x.ScriptType != null && x.ScriptType.ToLower() == requestDto.ScriptType.ToLower(), false)); 
            _mapper.Setup(x => x.Map<TenantTaskRewardScriptModel>(It.IsAny<UpdateTenantTaskRewardScriptRequestDto>()))
         .Returns(new TenantTaskRewardScriptModelMock ());
            _repo.Setup(x => x.UpdateAsync(It.IsAny<TenantTaskRewardScriptModel>())).ReturnsAsync(new TenantTaskRewardScriptModelMock() );
            // Act
            var result = await _TenantTaskRewardScriptController.UpdateTenantTaskRewardScript(requestDto);

            // Assert
            var okResult = result?.Result as OkObjectResult;
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }
        [Fact]
        public async TaskAlias UpdateTenantTaskRewardScript_ShouldReturn404Result()
        {
            // Arrange
            var requestDto = CreateUpdateMockDto();

            var scriptdata = new ScriptModel { ScriptId = 1, ScriptCode = "SCR001", ScriptName = "Script One" };
            _scriptRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ScriptModel, bool>>>(), false));
            _repo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TenantTaskRewardScriptModel, bool>>>(), false)).ReturnsAsync(new TenantTaskRewardScriptModelMock());
            _mapper.Setup(x => x.Map<TenantTaskRewardScriptModel>(It.IsAny<UpdateTenantTaskRewardScriptRequestDto>()))
         .Returns(new TenantTaskRewardScriptModelMock());
            _repo.Setup(x => x.UpdateAsync(It.IsAny<TenantTaskRewardScriptModel>())).ReturnsAsync(new TenantTaskRewardScriptModelMock() );
            // Act
            var result = await _TenantTaskRewardScriptController.UpdateTenantTaskRewardScript(requestDto);

            // Assert
            var notFoundkResult = result?.Result as ObjectResult;
            Assert.Equal(StatusCodes.Status404NotFound, notFoundkResult.StatusCode);
        }
        [Fact]
        public async TaskAlias UpdateTenantTaskRewardScript_ShouldReturn500Result()
        {
            // Arrange
            var requestDto = CreateUpdateMockDto();

            var scriptdata = new ScriptModel { ScriptId = 1, ScriptCode = "SCR001", ScriptName = "Script One" };
            _scriptRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ScriptModel, bool>>>(), false)).ThrowsAsync(new Exception("Simulated exception"));
            _repo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TenantTaskRewardScriptModel, bool>>>(), false));
            _mapper.Setup(x => x.Map<TenantTaskRewardScriptModel>(It.IsAny<UpdateTenantTaskRewardScriptRequestDto>()))
         .Returns(It.IsAny<TenantTaskRewardScriptModel>());
            _repo.Setup(x => x.UpdateAsync(It.IsAny<TenantTaskRewardScriptModel>())).ReturnsAsync(new TenantTaskRewardScriptModelMock() );
            // Act
            var result = await _TenantTaskRewardScriptController.UpdateTenantTaskRewardScript(requestDto);

            // Assert
            var errorResult = result?.Result as ObjectResult;
            Assert.Equal(StatusCodes.Status500InternalServerError, errorResult.StatusCode);
        }
        private static TenantTaskRewardScriptRequestDto CreateMockDto()
        {

            return new TenantTaskRewardScriptRequestDto()
            {

                TenantCode = "Tenant1",
                TaskRewardCode = "TR001",
                ScriptType = "Type1",
                ScriptCode = "SCR001",

            };
        }
        private static UpdateTenantTaskRewardScriptRequestDto CreateUpdateMockDto()
        {

            return new UpdateTenantTaskRewardScriptRequestDto()
            {

                TenantCode = "Tenant1",
                TenantTaskRewardScriptCode="test1",
                TaskRewardCode = "TR001",
                ScriptType = "Type1",
                ScriptCode = "SCR001",
            };
        }
    }
}
