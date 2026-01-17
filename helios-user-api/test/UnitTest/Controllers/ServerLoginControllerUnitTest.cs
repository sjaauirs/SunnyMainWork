using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NHibernate;
using SunnyRewards.Helios.Common.Core.Repositories;
using SunnyRewards.Helios.User.Api.Controllers;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Models;
using SunnyRewards.Helios.User.Infrastructure.Mappings;
using SunnyRewards.Helios.User.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.User.Infrastructure.Services;
using SunnyRewards.Helios.User.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.User.UnitTest.Fixtures.MockDto;
using SunnyRewards.Helios.User.UnitTest.Fixtures.MockModels;
using SunnyRewards.Helios.User.UnitTest.Fixtures.MockRepositories;
using System.Linq.Expressions;
using Xunit;

namespace SunnyRewards.Helios.User.UnitTest.Controllers
{
    public class ServerLoginControllerUnitTest
    {
        private readonly Mock<ILogger<ServerLoginController>> _serverLoginControllerLogger;
        private readonly Mock<ILogger<ServerLoginService>> _serverLoginServiceLogger;
        private readonly Mock<IServerLoginRepo> _serverLoginRepo;
        private readonly Mock<NHibernate.ISession> _session;

        private readonly IServerLoginService _serverLoginService;
        private readonly ServerLoginController _serverLoginController;
        public ServerLoginControllerUnitTest()
        {
            _serverLoginControllerLogger = new Mock<ILogger<ServerLoginController>>();
            _serverLoginServiceLogger = new Mock<ILogger<ServerLoginService>>();
            _serverLoginRepo = new ServerLoginMockRepo();
            _session = new Mock<NHibernate.ISession>();
            _serverLoginService = new ServerLoginService(_serverLoginServiceLogger.Object, _session.Object, _serverLoginRepo.Object);
            _serverLoginController = new ServerLoginController(_serverLoginControllerLogger.Object, _serverLoginService);
        }

        [Fact]
        public async Task ServerLogin_Should_Returns_Bad_Request_Result()
        {
            // Arrange
            var requestDto = new ServerLoginRequestDto()
            {
                TenantCode = string.Empty
            };

            var expectedResponseDto = new ServerLoginResponseDto()
            {
                ErrorCode = StatusCodes.Status400BadRequest,
                ErrorMessage = "Please provide a valid Tenant Code"
            };

            // Act
            var result = await _serverLoginController.ServerLogin(requestDto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
            var badRequestResult = result.Result as BadRequestObjectResult;
            var responseDto = badRequestResult?.Value as ServerLoginResponseDto;
            Assert.Equal(expectedResponseDto.ErrorCode, responseDto?.ErrorCode);
            Assert.Equal(expectedResponseDto.ErrorMessage, responseDto?.ErrorMessage);
        }

        [Fact]
        public async Task ServerLogin_Should_Returns_Existing_Api_Token()
        {
            // Arrange
            var requestDto = new ServerLoginRequestMockDto();
            var expectedDto = new ServerLoginMockModel();

            // Act
            var result = await _serverLoginController.ServerLogin(requestDto);

            // Assert
            Assert.IsType<OkObjectResult>(result.Result);
            var okObjectResult = result.Result as OkObjectResult;
            var responseDto = okObjectResult?.Value as ServerLoginResponseDto;
            Assert.Equal(StatusCodes.Status200OK, okObjectResult?.StatusCode);
            Assert.Equal(expectedDto.ApiToken, responseDto?.ApiToken);
        }

        [Fact]
        public async Task ServerLogin_Should_Returns_New_API_Token_When_Token_Expired()
        {
            // Arrange
            var requestDto = new ServerLoginRequestMockDto();
            var serverLoginMap = new ServerLoginMap();
            var serverLoginMockModel = new ServerLoginMockModel { RefreshTokenTs = DateTime.UtcNow.AddDays(-1) };
            var serverLoginMockModelList = new List<ServerLoginModel> { serverLoginMockModel };
            var transactionMock = new Mock<ITransaction>();

            _serverLoginRepo.Setup(x => x.FindAsync(
                It.IsAny<Expression<Func<ServerLoginModel, bool>>>(), false))
                .ReturnsAsync(serverLoginMockModelList);
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);

            // Act
            var result = await _serverLoginController.ServerLogin(requestDto);

            // Assert
            Assert.IsType<OkObjectResult>(result.Result);
            var okObjectResult = result.Result as OkObjectResult;
            var responseDto = okObjectResult?.Value as ServerLoginResponseDto;
            Assert.NotNull(serverLoginMap);
            Assert.Equal(StatusCodes.Status200OK, okObjectResult?.StatusCode);
            Assert.NotEqual(serverLoginMockModel.ApiToken, responseDto?.ApiToken);
        }

        [Fact]
        public async Task ServerLogin_Should_Returns_Internal_Server_Error_When_Service_Throws_Exception()
        {
            // Arrange
            var requestDto = new ServerLoginRequestMockDto();
            var serverLoginMockModel = new ServerLoginMockModel { RefreshTokenTs = DateTime.UtcNow.AddDays(-1) };
            var serverLoginMockModelList = new List<ServerLoginModel> { serverLoginMockModel };
            var transactionMock = new Mock<ITransaction>();
            var errorMessage = "Test Exception";
            _serverLoginRepo.Setup(x => x.FindAsync(
                It.IsAny<Expression<Func<ServerLoginModel, bool>>>(), false))
                .ReturnsAsync(serverLoginMockModelList);
            transactionMock.Setup(t => t.CommitAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new Exception(errorMessage));
            _session.Setup(s => s.BeginTransaction()).Returns(transactionMock.Object);

            // Act
            var result = await _serverLoginController.ServerLogin(requestDto);

            // Assert
            Assert.IsType<ObjectResult>(result.Result);
            var objectResult = result.Result as ObjectResult;
            var responseDto = objectResult?.Value as ServerLoginResponseDto;
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult?.StatusCode);
            Assert.Equal(errorMessage, responseDto?.ErrorMessage);
        }

        [Fact]
        public void ServerLoginRepo_Constructor_ShouldInstantiate_WhenDependenciesAreProvided()
        {
            var mockLogger = new Mock<ILogger<BaseRepo<ServerLoginModel>>>();
            var mockSession = new Mock<NHibernate.ISession>();
            var repo = new Infrastructure.Repositories.ServerLoginRepo(mockLogger.Object, mockSession.Object);
            Assert.NotNull(repo);
        }

    }
}
