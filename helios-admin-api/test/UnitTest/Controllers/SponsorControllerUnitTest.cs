using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SunnyRewards.Helios.Admin.Api.Controllers;
using SunnyRewards.Helios.Admin.Core.Domain.Constants;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;
using Xunit;
using TaskAlias = System.Threading.Tasks.Task;

namespace SunnyRewards.Helios.Admin.UnitTest.Controllers
{
    public class SponsorControllerUnitTest
    {
        private readonly Mock<ILogger<SponsorController>> _controllerLogger;
        private readonly Mock<ILogger<SponsorService>> _sponsorServiceLogger;
        private readonly Mock<ITenantClient> _tenantClient;
        private readonly ISponsorService _sponsorService;
        private readonly SponsorController _sponsorController;

        public SponsorControllerUnitTest()
        {
            _controllerLogger = new Mock<ILogger<SponsorController>>();
            _sponsorServiceLogger = new Mock<ILogger<SponsorService>>();
            _tenantClient = new Mock<ITenantClient>();
            _sponsorService = new SponsorService(_sponsorServiceLogger.Object, _tenantClient.Object);
            _sponsorController = new SponsorController(_controllerLogger.Object, _sponsorService);
        }
        [Fact]
        public async TaskAlias GetSponsors_Should_Return_Success_When_Sponsors_Are_Fetched_Successfully()
        {
            // Arrange
            IDictionary<string, long> parameters = new Dictionary<string, long>();
            var mockResponse = new SponsorsResponseDto
            {
                Sponsors = new List<SponsorDto>
                {
                    new SponsorDto { SponsorId = 1, SponsorName = "Sponsor A" },
                    new SponsorDto { SponsorId = 2, SponsorName = "Sponsor B" }
                }
            };

            _tenantClient.Setup(x => x.Get<SponsorsResponseDto>(Constant.Sponsors, parameters)).ReturnsAsync(mockResponse);

            // Act
            var result = await _sponsorController.GetSponsors();

            // Assert
            Assert.NotNull(result);
            var actionResult = Assert.IsType<ActionResult<SponsorsResponseDto>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }
        [Fact]
        public async TaskAlias GetSponsors_Should_Return_NotFound_When_Customers_Not_Found()
        {
            // Arrange
            IDictionary<string, long> parameters = new Dictionary<string, long>();
            _tenantClient.Setup(x => x.Get<SponsorsResponseDto>(Constant.Sponsors, parameters)).ReturnsAsync(new SponsorsResponseDto
            {
                ErrorCode = StatusCodes.Status404NotFound,
                ErrorMessage = "No Sponsors found."
            });

            // Act
            var result = await _sponsorController.GetSponsors();

            // Assert
            Assert.NotNull(result);
            var actionResult = Assert.IsType<ActionResult<SponsorsResponseDto>>(result);
            var notFoundResult = Assert.IsType<ObjectResult>(actionResult.Result);
            Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
        }
        [Fact]
        public async TaskAlias GetSponsors_Should_Return_InternalServerError_When_Exception_Thrown()
        {
            // Arrange

            IDictionary<string, long> parameters = new Dictionary<string, long>();
            _tenantClient.Setup(x => x.Get<SponsorsResponseDto>(Constant.Sponsors, parameters)).ThrowsAsync(new Exception("Simulated Exception"));

            // Act
            var result = await _sponsorController.GetSponsors();

            // Assert
            Assert.NotNull(result);
            var actionResult = Assert.IsType<ActionResult<SponsorsResponseDto>>(result);
            var errorResult = Assert.IsType<ObjectResult>(actionResult.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, errorResult.StatusCode);
        }
        [Fact]
        public async TaskAlias CreateSponsor_Should_Return_Success_When_Sponsor_Is_Created_Successfully()
        {
            // Arrange
            var mockRequest = new CreateSponsorDto { SponsorCode = "SPON123" };
            _tenantClient.Setup(x => x.Post<BaseResponseDto>(Constant.Sponsor, mockRequest)).ReturnsAsync(new BaseResponseDto());

            // Act
            var result = await _sponsorController.CreateSponsor(mockRequest);

            // Assert
            Assert.NotNull(result);
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }
        [Fact]
        public async TaskAlias CreateSponsor_Should_Return_Error_When_Throws_Exception()
        {
            // Arrange
            var mockRequest = new CreateSponsorDto { SponsorCode = "SPON123" };
            _tenantClient.Setup(x => x.Post<BaseResponseDto>(Constant.Sponsor, mockRequest)).ThrowsAsync(new Exception("Simulated exception"));

            // Act
            var result = await _sponsorController.CreateSponsor(mockRequest);

            // Assert
            Assert.NotNull(result);
            var errorResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, errorResult.StatusCode);
        }
        [Fact]
        public async TaskAlias CreateSponsor_Should_Return_Error_When_Sponsor_AlreadyExist()
        {
            // Arrange
            var mockRequest = new CreateSponsorDto { SponsorCode = "SPON123" };
            var mockResponse = new BaseResponseDto
            {
                ErrorCode = StatusCodes.Status409Conflict,
            };
            _tenantClient.Setup(x => x.Post<BaseResponseDto>(Constant.Sponsor, mockRequest)).ReturnsAsync(mockResponse);

            // Act
            var result = await _sponsorController.CreateSponsor(mockRequest);

            // Assert
            Assert.NotNull(result);
            var errorResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status409Conflict, errorResult.StatusCode);
        }
    }
}
