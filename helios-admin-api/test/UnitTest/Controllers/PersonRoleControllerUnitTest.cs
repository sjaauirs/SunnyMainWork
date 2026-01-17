using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SunnyRewards.Helios.Admin.Api.Controllers;
using SunnyRewards.Helios.Admin.Core.Domain.Constants;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;
using Xunit;
using TaskAlias = System.Threading.Tasks.Task;

namespace SunnyRewards.Helios.Admin.UnitTest.Controllers
{
    public class PersonRoleControllerUnitTest
    {
        private readonly Mock<ILogger<PersonRoleController>> _controllerLogger;
        private readonly Mock<ILogger<PersonRoleService>> _personRoleServiceLogger;
        private readonly Mock<IUserClient> _userClient;
        private readonly Mock<IAuth0Service> _auth0Service;
        private readonly IPersonRoleService _personRoleService;
        private readonly PersonRoleController _personRoleController;

        public PersonRoleControllerUnitTest()
        {
            _controllerLogger = new Mock<ILogger<PersonRoleController>>();
            _personRoleServiceLogger = new Mock<ILogger<PersonRoleService>>();
            _userClient = new Mock<IUserClient>();
            _auth0Service = new Mock<IAuth0Service>();
            _personRoleService = new PersonRoleService(_personRoleServiceLogger.Object, _userClient.Object, _auth0Service.Object);
            _personRoleController = new PersonRoleController(_controllerLogger.Object, _personRoleService);
        }
        [Fact]
        public async TaskAlias GetPersonRoles_Should_Return_Success_When_personRoles_Are_Fetched_Successfully()
        {
            // Arrange
            var requestDto = new GetPersonRolesRequestDto
            {
                Email = "test-email"
            };

            _userClient.Setup(x => x.Post<GetPersonRolesResponseDto>(Constant.PersonRoles, It.IsAny<GetPersonRolesRequestDto>()))
                    .ReturnsAsync(new GetPersonRolesResponseDto
                    {
                        PersonRoles = new List<PersonRoleDto>
                        {
                        new PersonRoleDto(),
                        new PersonRoleDto()
                        }
                    });


            // Act
            var result = await _personRoleController.GetPersonRoles(requestDto);

            // Assert
            Assert.NotNull(result);
            var actionResult = Assert.IsType<ActionResult<GetPersonRolesResponseDto>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }
        [Fact]
        public async TaskAlias GetPersonRoles_Should_Return_NotFound_When_PersonRoles_Not_Found()
        {
            // Arrange
            var requestDto = new GetPersonRolesRequestDto
            {
                Email = "test-email"
            };
            _userClient.Setup(x => x.Post<GetPersonRolesResponseDto>(Constant.PersonRoles, It.IsAny<GetPersonRolesRequestDto>())).ReturnsAsync(new GetPersonRolesResponseDto
            {
                ErrorCode = StatusCodes.Status404NotFound
            });

            // Act
            var result = await _personRoleController.GetPersonRoles(requestDto);

            // Assert
            Assert.NotNull(result);
            var actionResult = Assert.IsType<ActionResult<GetPersonRolesResponseDto>>(result);
            var notFoundResult = Assert.IsType<ObjectResult>(actionResult.Result);
            Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
        }
        [Fact]
        public async TaskAlias GetPersonRoles_Should_Return_InternalServerError_When_Exception_Thrown()
        {
            // Arrange
            var requestDto = new GetPersonRolesRequestDto
            {
                Email = "test-email"
            };
            _userClient.Setup(x => x.Post<GetPersonRolesResponseDto>(Constant.PersonRoles, It.IsAny<GetPersonRolesRequestDto>())).ThrowsAsync(new Exception("test-exception"));

            // Act
            var result = await _personRoleController.GetPersonRoles(requestDto);

            // Assert
            Assert.NotNull(result);
            var actionResult = Assert.IsType<ActionResult<GetPersonRolesResponseDto>>(result);
            var errorResult = Assert.IsType<ObjectResult>(actionResult.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, errorResult.StatusCode);
        }
    }
}
