using Microsoft.Extensions.Logging;
using Moq;
using SunnyRewards.Helios.Admin.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services;
using SunnyRewards.Helios.Common.Core.Helpers.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SunnyBenefits.Fis.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Api.Controllers;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos.Enums;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Core.Domain.Models;
using SunnyRewards.Helios.Admin.Infrastructure.Repositories;
using SunnyRewards.Helios.Admin.UnitTest.Fixtures.MockDtos;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Helpers;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;
using Xunit;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using System.Linq.Expressions;

namespace SunnyRewards.Helios.Admin.UnitTest.Services
{
    public class CohortConsumerTaskServiceUnitTest
    {
        private readonly Mock<ILogger<CohortConsumerTaskService>> _mockLogger;
        private readonly Mock<ITenantTaskRewardScriptRepo> _mockTenantTaskRewardScriptRepo;
        private readonly Mock<IHeliosScriptEngine> _mockHeliosScriptEngine;
        private readonly Mock<IScriptRepo> _mockScriptRepo;
        private readonly Mock<ITaskRewardScriptResultRepo> _mockTaskRewardScriptResultRepo;
        private readonly Mock<ICohortConsumerService> _mockCohortConsumerService;
        private readonly Mock<IConsumerLoginService> _mockConsumerLoginService;
        private readonly Mock<ITaskService> _mockTaskService;
        private readonly Mock<NHibernate.ISession> _mockSession;

        private readonly CohortConsumerTaskService _service;

        public CohortConsumerTaskServiceUnitTest()
        {
            _mockLogger = new Mock<ILogger<CohortConsumerTaskService>>();
            _mockTenantTaskRewardScriptRepo = new Mock<ITenantTaskRewardScriptRepo>();
            _mockHeliosScriptEngine = new Mock<IHeliosScriptEngine>();
            _mockScriptRepo = new Mock<IScriptRepo>();
            _mockTaskRewardScriptResultRepo = new Mock<ITaskRewardScriptResultRepo>();
            _mockCohortConsumerService = new Mock<ICohortConsumerService>();
            _mockConsumerLoginService = new Mock<IConsumerLoginService>();
            _mockTaskService = new Mock<ITaskService>();
            _mockSession = new Mock<NHibernate.ISession>();

            _service = new CohortConsumerTaskService(
                _mockLogger.Object,
                _mockTenantTaskRewardScriptRepo.Object,
                _mockHeliosScriptEngine.Object,
                _mockScriptRepo.Object,
                _mockTaskRewardScriptResultRepo.Object,
                _mockTaskService.Object,
                _mockCohortConsumerService.Object,
                _mockSession.Object,
                _mockConsumerLoginService.Object
            );
        }
        [Fact]
        public async System.Threading.Tasks.Task UpdateConsumerTask_Should_ValidCheck_For_Pre_And_Post_Consumer_Task_State()
        {
            // Arrange
            var consumerTaskUpdateRequestDto = new FindConsumerTasksByIdResponseDto
            {
                TaskRewardDetail = new TaskRewardDetailDto
                {
                    TaskReward = new TaskRewardDto
                    {
                        TenantCode = "tenant1",
                        TaskRewardCode = "reward1"
                    }
                }
            };
            var consumer = new ConsumerDto { ConsumerCode = "consumer1" };
            var scriptType = "TASK_COMPLETE_PRE";
            var request = new TaskUpdateRequestMockDto();

            _mockTenantTaskRewardScriptRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TenantTaskRewardScriptModel, bool>>>(), false)).ReturnsAsync(new TenantTaskRewardScriptModel
            {
                ScriptId = 1,
                TenantCode = "valid",
                ScriptType = nameof(ScriptTypes.TASK_COMPLETE_PRE),
                DeleteNbr = 0
            });
            _mockScriptRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ScriptModel, bool>>>(), false)).ReturnsAsync(new ScriptModel
            {
                ScriptId = 1,
                ScriptJson = MockScriptJsonDto(),
                ScriptSource = "xyz",
                DeleteNbr = 0
            });
            _mockHeliosScriptEngine.Setup(x => x.ExecuteScript(It.IsAny<ScriptContext>(), It.IsAny<ScriptArgumentContext>(), It.IsAny<string>())).Returns(new ScriptExecutionResultDto { ErrorMessage = "", ResultCode = 0 });
            _mockTaskRewardScriptResultRepo
               .Setup(session => session.CreateAsync(It.IsAny<TaskRewardScriptResultModel>()))
               .ReturnsAsync(It.IsAny<TaskRewardScriptResultModel>());

            // Act
            var response = await _service.TaskCompletionPrePostScriptCheck(consumerTaskUpdateRequestDto, consumer, scriptType);

            Assert.True(response);
        }

        [Fact]
        public async System.Threading.Tasks.Task UpdateConsumerTask_Should_InValidCheck_For_Pre_And_Post_Consumer_Task_State()
        {
            // Arrange
            var consumerTaskUpdateRequestDto = new FindConsumerTasksByIdResponseDto
            {
                TaskRewardDetail = new TaskRewardDetailDto
                {
                    TaskReward = new TaskRewardDto
                    {
                        TenantCode = "tenant1",
                        TaskRewardCode = "reward1"
                    }
                }
            };
            var consumer = new ConsumerDto { ConsumerCode = "consumer1" };
            var scriptType = "TASK_COMPLETE_PRE";
            var request = new TaskUpdateRequestMockDto();

            _mockTenantTaskRewardScriptRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TenantTaskRewardScriptModel, bool>>>(), false)).ReturnsAsync(new TenantTaskRewardScriptModel
            {
                ScriptId = 1,
                TenantCode = "valid",
                ScriptType = nameof(ScriptTypes.TASK_COMPLETE_PRE),
                DeleteNbr = 0
            });
            _mockScriptRepo.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ScriptModel, bool>>>(), false)).ReturnsAsync(new ScriptModel
            {
                ScriptId = 1,
                ScriptJson = MockScriptJsonDto(),
                ScriptSource = "xyz",
                DeleteNbr = 0
            });
            _mockHeliosScriptEngine.Setup(x => x.ExecuteScript(It.IsAny<ScriptContext>(), It.IsAny<ScriptArgumentContext>(), It.IsAny<string>())).Returns(new ScriptExecutionResultDto { ErrorMessage = "", ResultCode = 2 });
            _mockTaskRewardScriptResultRepo
               .Setup(session => session.CreateAsync(It.IsAny<TaskRewardScriptResultModel>()))
               .ReturnsAsync(It.IsAny<TaskRewardScriptResultModel>());

            // Act
            var response = await _service.TaskCompletionPrePostScriptCheck(consumerTaskUpdateRequestDto, consumer, scriptType);

            Assert.False(response);
        }
        public string MockScriptJsonDto()
        {
            // Arrange
            var scriptDto = new ScriptJsonDto
            {
                Args = new List<Argument>
    {
        new Argument { ArgName = "consumerDto", ArgType = "Object" },
        new Argument { ArgName = "taskRewardDetailDto", ArgType = "object" }
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
}
