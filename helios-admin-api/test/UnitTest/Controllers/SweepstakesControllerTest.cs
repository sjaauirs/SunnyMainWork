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
    public class SweepstakesControllerTest
    {

        private readonly Mock<ILogger<SweepstakesService>> _sweepstakesServiceLogger;
        private readonly Mock<ILogger<SweepstakesController>> _controllerLogger;
        private readonly Mock<ISweepstakesClient> _sweepStakesClient;
        private readonly ISweepstakesService _sweepStakeservice;
        private readonly SweepstakesController _tenantSweepStakesController;

        public SweepstakesControllerTest()
        {
            _controllerLogger = new Mock<ILogger<SweepstakesController>>();
            _sweepstakesServiceLogger = new Mock<ILogger<SweepstakesService>>();
            _sweepStakesClient = new SweepstakesClientMock();
            _sweepStakeservice = new SweepstakesService(_sweepstakesServiceLogger.Object, _sweepStakesClient.Object);
            _tenantSweepStakesController = new SweepstakesController(_controllerLogger.Object, _sweepStakeservice);
        }
        [Fact]
        public async TaskAsync CreateCohort_Success_ReturnsOkResult()
        {
            // Arrange
            var sweepstakesRequestDto = new SweepstakesMockDto();
            _sweepStakesClient.Setup(client => client.Post<BaseResponseDto>(Constant.CreateSweepStakesUrl, It.IsAny<SweepstakesRequestDto>()))
                .ReturnsAsync(new BaseResponseDto());

            // Act
            var result = await _tenantSweepStakesController.CreateSweepstakes(sweepstakesRequestDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);

        }

        [Fact]
        public async TaskAsync CreateCohort_ErrorInService_ReturnsError()
        {
            // Arrange
            var sweepstakesRequestDto = new SweepstakesMockDto();

            var responseModel = new BaseResponseDto()
            {
                ErrorCode = 409
            };
            _sweepStakesClient.Setup(client => client.Post<BaseResponseDto>(Constant.CreateSweepStakesUrl, It.IsAny<SweepstakesRequestDto>()))
                .ReturnsAsync(responseModel);

            // Act
            var result = await _tenantSweepStakesController.CreateSweepstakes(sweepstakesRequestDto);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(409, statusCodeResult.StatusCode);
        }



        [Fact]
        public async TaskAsync CreateCohort_ThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var sweepstakesRequestDto = new SweepstakesMockDto();
            _sweepStakesClient.Setup(client => client.Post<BaseResponseDto>(Constant.CreateSweepStakesUrl, It.IsAny<SweepstakesRequestDto>()))
               .ThrowsAsync(new Exception("Testing"));

            // Act
            var result = await _tenantSweepStakesController.CreateSweepstakes(sweepstakesRequestDto);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);

        }
    }
}
