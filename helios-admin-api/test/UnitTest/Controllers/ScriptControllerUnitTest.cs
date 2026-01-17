using Microsoft.Extensions.Logging;
using Moq;
using SunnyRewards.Helios.Admin.Api.Controllers;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using TaskAlias = System.Threading.Tasks.Task;
using SunnyRewards.Helios.Admin.Infrastructure.Repositories;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using Xunit;
using SunnyRewards.Helios.Admin.Core.Domain.Models;
using SunnyRewards.Helios.Admin.UnitTest.Fixtures.MockDtos;
using System.Collections.Immutable;
using SunnyRewards.Helios.Admin.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Models;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using Azure;

namespace SunnyRewards.Helios.Admin.UnitTest.Controllers
{
    public class ScriptControllerUnitTest
    {
        private readonly Mock<ILogger<ScriptController>> _controllerLogger;
        private readonly Mock<ILogger<ScriptService>> _serviceLogger;
        private readonly Mock<IMapper> _mapper;
        private readonly Mock<IScriptRepo> _repo;
        private readonly IScriptService _scriptService;
        private readonly ScriptController _scriptController;

        public ScriptControllerUnitTest()
        {
            // Initialize mocks
            _controllerLogger = new Mock<ILogger<ScriptController>>();
            _serviceLogger = new Mock<ILogger<ScriptService>>();
            _repo = new Mock<IScriptRepo>();
            _mapper = new Mock<IMapper>();
            _scriptService = new ScriptService(_serviceLogger.Object, _repo.Object, _mapper.Object);

            _scriptController = new ScriptController(_controllerLogger.Object, _scriptService);
        }
        [Fact]
        public async TaskAlias GetScript_ShouldReturnOkResult()
        {
            // Arrange
            var scriptdata = new List<ScriptModel>
    {
        new ScriptModel { ScriptId = 1, ScriptCode = "SCR001", ScriptName = "Script One" },
        new ScriptModel { ScriptId = 2, ScriptCode = "SCR002", ScriptName = "Script Two" }
    };
            _repo.Setup(x => x.FindAllAsync()).ReturnsAsync(scriptdata);
            _mapper.Setup(x => x.Map<ScriptDto>(scriptdata))
         .Returns(new ScriptDtoMock());

            // Act
            var result = await _scriptController.GetScript();

            // Assert
            var okResult = result?.Result as OkObjectResult;
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }
        [Fact]
        public async TaskAlias GetScript_ShouldReturn404Result()
        {
            // Arrange
            var scriptdata = new List<ScriptModel>
    {
       
    };
            _repo.Setup(x => x.FindAllAsync()).ReturnsAsync(scriptdata);
            _mapper.Setup(x => x.Map<ScriptDto>(scriptdata))
         .Returns(new ScriptDtoMock());

            // Act
            var result = await _scriptController.GetScript();

            // Assert
            var notfoundResult = result?.Result as NotFoundObjectResult;

            Assert.Equal(StatusCodes.Status404NotFound, notfoundResult.StatusCode);
        }
        [Fact]
        public async TaskAlias GetScript_ShouldReturn500Result()
        {
            // Arrange
            var scriptdata = new List<ScriptModel>
    {
       
    };
            _repo.Setup(x => x.FindAllAsync()).ThrowsAsync(new Exception("Simulated exception"));
            _mapper.Setup(x => x.Map<ScriptDto>(scriptdata))
         .Returns(new ScriptDtoMock());

            // Act
            var result = await _scriptController.GetScript();

            // Assert
            var errorResult = result?.Result as ObjectResult;
            Assert.Equal(StatusCodes.Status500InternalServerError, errorResult.StatusCode);
        }
    }
}
