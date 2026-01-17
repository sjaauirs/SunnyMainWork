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
using Xunit;
using TaskAsync = System.Threading.Tasks.Task;

namespace SunnyRewards.Helios.Admin.UnitTest.Controllers
{
    public class CohortTenantTaskRewardControllerTest
    {
        private readonly Mock<ILogger<CohortTenantTaskRewardService>> _cohortServiceLogger;
        private readonly Mock<ILogger<CohortTenantTaskRewardController>> _controllerLogger;
        private readonly Mock<ICohortClient> _cohortClient;
        private readonly ICohortTenantTaskRewardService _cohortService;
        private readonly CohortTenantTaskRewardController _cohortController;

        public CohortTenantTaskRewardControllerTest()
        {
            _controllerLogger = new Mock<ILogger<CohortTenantTaskRewardController>>();
            _cohortServiceLogger = new Mock<ILogger<CohortTenantTaskRewardService>>();
            _cohortClient = new CohortMockClient();
            _cohortService = new CohortTenantTaskRewardService(_cohortServiceLogger.Object, _cohortClient.Object);
            _cohortController = new CohortTenantTaskRewardController(_controllerLogger.Object, _cohortService);
        }

        [Fact]
        public async TaskAsync CreateCohort_Success_ReturnsOkResult()
        {
            // Arrange
            var createCohortRequestDto = new CreateCohortTenantTaskRewardMockDto();

            var responseModel = new CreateCohortTenantTaskRewardResponseDto()
            {
                ErrorCode = null , CohortTenantTaskRewardDto = new CohortTenantTaskRewardDto() { CohortId =10, CohortTenantTaskRewardId = 11}
            };

            _cohortClient.Setup(client => client.Post<CreateCohortTenantTaskRewardResponseDto>(Constant.CreateCohortTenantTaskRewardAPIUrl, It.IsAny<CreateCohortTenantTaskRewardDto>()))
   .ReturnsAsync(responseModel);


            // Act
            var result = await _cohortController.CreateCohortTenantTaskReward(createCohortRequestDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);

        }

        [Fact]
        public async TaskAsync CreateCohort_ErrorInService_ReturnsError()
        {
            // Arrange
            var createCohortRequestDto = new CreateCohortTenantTaskRewardMockDto();

            var responseModel = new CreateCohortTenantTaskRewardResponseDto()
            {
                ErrorCode = 409
            };
            _cohortClient.Setup(client => client.Post<CreateCohortTenantTaskRewardResponseDto>(Constant.CreateCohortTenantTaskRewardAPIUrl, It.IsAny<CreateCohortTenantTaskRewardDto>()))
               .ReturnsAsync(responseModel);

            // Act
            var result = await _cohortController.CreateCohortTenantTaskReward(createCohortRequestDto);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(409, statusCodeResult.StatusCode);
        }



        [Fact]
        public async TaskAsync CreateCohort_ThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var createCohortRequestDto = new CreateCohortTenantTaskRewardMockDto();
            _cohortClient.Setup(client => client.Post<BaseResponseDto>(Constant.CreateCohortAPIUrl, It.IsAny<CreateCohortRequestDto>()))
               .ThrowsAsync(new Exception("Testing"));

            // Act
            var result = await _cohortController.CreateCohortTenantTaskReward(createCohortRequestDto);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);

        }


        [Fact]
        public async TaskAsync GetCohort_ShouldReturnOk_WhenCohortsRetrievedSuccessfully()
        {
            // Arrange
            var request = new GetCohortTenantTaskRewardRequestDto() { TenantCode = "tenantCode123" };
            var responseDto = new GetCohortTenantTaskRewardResponseDto() { ErrorCode = null, CohortTenantTaskRewards = new List<CohortTenantTaskRewardDto>() { new CohortTenantTaskRewardDto() { CohortId = 10 } } };
            _cohortClient.Setup(client => client.Post<GetCohortTenantTaskRewardResponseDto>(Constant.GetCohortTenantTaskRewardAPIUrl, It.IsAny<GetCohortTenantTaskRewardRequestDto>()))
                .ReturnsAsync(responseDto);

            // Act
            var result = await _cohortController.GetCohortTenantTaskReward(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(responseDto, okResult.Value);
        }

        [Fact]
        public async TaskAsync GetCohort_ShouldReturnError_WhenServiceReturnsErrorCode()
        {
            // Arrange
            var request = new GetCohortTenantTaskRewardRequestDto() { TenantCode = "tenantCode123" };
            var responseDto = new GetCohortTenantTaskRewardResponseDto() { ErrorCode = 404, CohortTenantTaskRewards = null };
            _cohortClient.Setup(client => client.Post<GetCohortTenantTaskRewardResponseDto>(Constant.GetCohortTenantTaskRewardAPIUrl, It.IsAny<GetCohortTenantTaskRewardRequestDto>()))
                .ReturnsAsync(responseDto);

            // Act
            var result = await _cohortController.GetCohortTenantTaskReward(request);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status404NotFound, statusCodeResult.StatusCode);
            Assert.Equal(responseDto, statusCodeResult.Value);
        }

        [Fact]
        public async TaskAsync GetCohort_ShouldReturnInternalServerError_WhenExceptionIsThrown()
        {
            // Arrange
            var request = new GetCohortTenantTaskRewardRequestDto() { TenantCode = "tenantCode123" };
            
            _cohortClient.Setup(client => client.Post<GetCohortTenantTaskRewardResponseDto>(Constant.GetCohortTenantTaskRewardAPIUrl, It.IsAny<GetCohortTenantTaskRewardRequestDto>()))
                .ThrowsAsync(new Exception("Testing"));

            // Act
            var result = await _cohortController.GetCohortTenantTaskReward(request);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);

            var responseDto = Assert.IsType<GetCohortTenantTaskRewardResponseDto>(statusCodeResult.Value);
            Assert.Equal(StatusCodes.Status500InternalServerError, responseDto.ErrorCode);
        }
    }
}
