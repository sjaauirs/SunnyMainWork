using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SunnyRewards.Helios.Admin.Api.Controllers;
using SunnyRewards.Helios.Admin.Core.Domain.Constants;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Admin.UnitTest.Fixtures.MockDtos;
using SunnyRewards.Helios.Admin.UnitTest.Helpers.HttpClientsMock;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Sweepstakes.Core.Domain.Dtos;
using Xunit;
using TaskAsync = System.Threading.Tasks.Task;
namespace SunnyRewards.Helios.Admin.UnitTest.Controllers
{
    public class TenantSweepstakesControllerTest
    {

        private readonly Mock<ILogger<TenantSweepstakesService>> _sweepstakesServiceLogger;
        private readonly Mock<ILogger<TenantSweepstakesController>> _controllerLogger;
        private readonly Mock<ISweepstakesClient> _sweepStakesClient;
        private readonly ITenantSweepstakesService _sweepStakeservice;
        private readonly TenantSweepstakesController _tenantSweepStakesController;

        public TenantSweepstakesControllerTest()
        {
            _controllerLogger = new Mock<ILogger<TenantSweepstakesController>>();
            _sweepstakesServiceLogger = new Mock<ILogger<TenantSweepstakesService>>();
            _sweepStakesClient = new SweepstakesClientMock();
            _sweepStakeservice = new TenantSweepstakesService(_sweepstakesServiceLogger.Object, _sweepStakesClient.Object);
            _tenantSweepStakesController = new TenantSweepstakesController(_controllerLogger.Object, _sweepStakeservice);
        }
        [Fact]
        public async TaskAsync CreateCohort_Success_ReturnsOkResult()
        {
            // Arrange
            var tenantSweepstakesRequestDto = new TenantSweepstakesRequestDto() { SweepstakesCode = "Test", TenantSweepstakes = new TenantSweepstakesMockDto() };
            _sweepStakesClient.Setup(client => client.Post<BaseResponseDto>(Constant.CreateTenantSweepStakesUrl, It.IsAny<TenantSweepstakesRequestDto>()))
               .ReturnsAsync(new BaseResponseDto());
            // Act
            var result = await _tenantSweepStakesController.CreateTenantSweepstakes(tenantSweepstakesRequestDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);

        }

        [Fact]
        public async TaskAsync CreateCohort_ErrorInService_ReturnsError()
        {
            // Arrange
            var tenantSweepstakesRequestDto = new TenantSweepstakesRequestDto() { SweepstakesCode = "Test", TenantSweepstakes = new TenantSweepstakesMockDto() };

            var responseModel = new BaseResponseDto()
            {
                ErrorCode = 409
            };
            _sweepStakesClient.Setup(client => client.Post<BaseResponseDto>(Constant.CreateTenantSweepStakesUrl, It.IsAny<TenantSweepstakesRequestDto>()))
               .ReturnsAsync(responseModel);

            // Act
            var result = await _tenantSweepStakesController.CreateTenantSweepstakes(tenantSweepstakesRequestDto);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(409, statusCodeResult.StatusCode);
        }



        [Fact]
        public async TaskAsync CreateCohort_ThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var tenantSweepstakesRequestDto = new TenantSweepstakesRequestDto() { SweepstakesCode = "Test", TenantSweepstakes = new TenantSweepstakesMockDto() };
            _sweepStakesClient.Setup(client => client.Post<BaseResponseDto>(Constant.CreateTenantSweepStakesUrl, It.IsAny<TenantSweepstakesRequestDto>()))
               .ThrowsAsync(new Exception("Testing"));

            // Act
            var result = await _tenantSweepStakesController.CreateTenantSweepstakes(tenantSweepstakesRequestDto);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);

        }
    }
}

