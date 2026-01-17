using Microsoft.AspNetCore.Http;
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
using SunnyRewards.Helios.Cohort.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using System.Dynamic;
using Xunit;
using TaskAsync = System.Threading.Tasks.Task;

namespace SunnyRewards.Helios.Admin.UnitTest.Controllers
{
    public class CohortControllerUnitTest
    {
        private readonly Mock<ILogger<CohortService>> _cohortServiceLogger;
        private readonly Mock<ILogger<CohortController>> _controllerLogger;
        private readonly Mock<ICohortClient> _cohortClient;
        private readonly ICohortService _cohortService;
        private readonly CohortController _cohortController;

        public CohortControllerUnitTest()
        {
            _controllerLogger = new Mock<ILogger<CohortController>>();
            _cohortServiceLogger = new Mock<ILogger<CohortService>>();
            _cohortClient = new CohortMockClient();
            _cohortService = new CohortService(_cohortServiceLogger.Object, _cohortClient.Object);
            _cohortController = new CohortController(_controllerLogger.Object, _cohortService);
        }

        [Fact]
        public async TaskAsync CreateCohort_Success_ReturnsOkResult()
        {
            // Arrange
            var createCohortRequestDto = new CreateCohortRequestMockDto();

            var responseModel = new CreateCohortResponseDto()
            {
                ErrorCode = null,
                Cohort = new CohortDto() {CohortId =10 }
            };
            _cohortClient.Setup(client => client.Post<CreateCohortResponseDto>(Constant.CreateCohortAPIUrl, It.IsAny<CreateCohortRequestDto>()))
               .ReturnsAsync(responseModel);

            // Act
            var result = await _cohortController.CreateCohort(createCohortRequestDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);

        }

        [Fact]
        public async TaskAsync CreateCohort_ErrorInService_ReturnsError()
        {
            // Arrange
            var createCohortRequestDto = new CreateCohortRequestMockDto();

            var responseModel = new CreateCohortResponseDto()
            {
                ErrorCode = 409
            };
            _cohortClient.Setup(client => client.Post<CreateCohortResponseDto>(Constant.CreateCohortAPIUrl, It.IsAny<CreateCohortRequestDto>()))
               .ReturnsAsync(responseModel);

            // Act
            var result = await _cohortController.CreateCohort(createCohortRequestDto);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(409, statusCodeResult.StatusCode);
        }



        [Fact]
        public async TaskAsync CreateCohort_ThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var createCohortRequestDto = new CreateCohortRequestMockDto();
            _cohortClient.Setup(client => client.Post<BaseResponseDto>(Constant.CreateCohortAPIUrl, It.IsAny<CreateCohortRequestDto>()))
               .ThrowsAsync(new Exception("Testing"));

            // Act
            var result = await _cohortController.CreateCohort(createCohortRequestDto);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);

        }

        [Fact]
        public async TaskAsync GetCohort_ShouldReturnOk_WhenCohortsRetrievedSuccessfully()
        {
            // Arrange
            var responseDto = new GetCohortsResponseDto() { ErrorCode = null, Cohorts = new List<CohortDto>() { new CohortDto() { CohortId = 10 } } };
            _cohortClient.Setup(client => client.Get<GetCohortsResponseDto>(Constant.CreateCohortAPIUrl, It.IsAny<Dictionary<string, long>>()))
                .ReturnsAsync(responseDto);

            // Act
            var result = await _cohortController.GetCohort();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(responseDto, okResult.Value);
        }

        [Fact]
        public async TaskAsync GetCohort_ShouldReturnError_WhenServiceReturnsErrorCode()
        {
            // Arrange
            var responseDto = new GetCohortsResponseDto() { ErrorCode = StatusCodes.Status404NotFound, Cohorts = null };
            _cohortClient.Setup(client => client.Get<GetCohortsResponseDto>(Constant.CreateCohortAPIUrl, It.IsAny<Dictionary<string, long>>()))
                .ReturnsAsync(responseDto);

            // Act
            var result = await _cohortController.GetCohort();

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status404NotFound, statusCodeResult.StatusCode);
            Assert.Equal(responseDto, statusCodeResult.Value);
        }

        [Fact]
        public async TaskAsync GetCohort_ShouldReturnInternalServerError_WhenExceptionIsThrown()
        {
            // Arrange
            _cohortClient.Setup(client => client.Get<GetCohortsResponseDto>(Constant.CreateCohortAPIUrl, It.IsAny<Dictionary<string, long>>()))
               .ThrowsAsync(new Exception("Testing"));


            // Act
            var result = await _cohortController.GetCohort();

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);

            var responseDto = Assert.IsType<GetCohortsResponseDto>(statusCodeResult.Value);
            Assert.Equal(StatusCodes.Status500InternalServerError, responseDto.ErrorCode);
        }

      
    }
}
