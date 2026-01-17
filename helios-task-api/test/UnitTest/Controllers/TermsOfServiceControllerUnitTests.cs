using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Api.Controller;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Models;
using SunnyRewards.Helios.Task.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Task.Infrastructure.Services;
using System.Linq.Expressions;
using Xunit;
using TaskAlias = System.Threading.Tasks.Task;

namespace SunnyRewards.Helios.Task.UnitTest.Controllers
{
    public class TermsOfServiceControllerUnitTests
    {
        private readonly Mock<ILogger<TermsOfServiceController>> _logger;
        private TermsOfServiceController _termsOfServiceController;

        private readonly TermsOfServiceService _termsOfService;
        private readonly Mock<ITermsOfServiceRepo> _tenantTaskCategoryRepoMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogger<TermsOfServiceService>> _serviceLoggerMock;

        public TermsOfServiceControllerUnitTests()
        {
            _logger = new Mock<ILogger<TermsOfServiceController>>();
            _tenantTaskCategoryRepoMock = new Mock<ITermsOfServiceRepo>();
            _mapperMock = new Mock<IMapper>();
            _serviceLoggerMock = new Mock<ILogger<TermsOfServiceService>>();

            // Initialize the service with mocks
            _termsOfService = new TermsOfServiceService(_tenantTaskCategoryRepoMock.Object, _serviceLoggerMock.Object, _mapperMock.Object);
            _termsOfServiceController = new TermsOfServiceController(_logger.Object, _termsOfService);
        }
        [Fact]
        public async TaskAlias CreateTermsOfService_ShouldReturnOk_WhenRequestIsSuccessful()
        {
            // Arrange
            var requestDto = CreateTermsOfServiceMock();

            _tenantTaskCategoryRepoMock.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TermsOfServiceModel, bool>>>(), false));

            _mapperMock
                .Setup(m => m.Map<TermsOfServiceModel>(It.IsAny<CreateTermsOfServiceRequestDto>()))
                .Returns(new TermsOfServiceModel());
            // Act
            var result = await _termsOfServiceController.CreateTermsOfService(requestDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<BaseResponseDto>(okResult.Value);
            Assert.Null(response.ErrorCode);
            Assert.Equal(200, okResult.StatusCode);
        }
        [Fact]
        public async TaskAlias CreateTermsOfService_ShouldReturnError_WhenConflictOccurs()
        {
            // Arrange
            var requestDto = CreateTermsOfServiceMock();

            _tenantTaskCategoryRepoMock.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TermsOfServiceModel, bool>>>(), false))
                .ReturnsAsync(new TermsOfServiceModel
                {
                    TermsOfServiceId = requestDto.TermsOfServiceId
                });
            // Act
            var result = await _termsOfServiceController.CreateTermsOfService(requestDto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status409Conflict, objectResult.StatusCode);
            var response = Assert.IsType<BaseResponseDto>(objectResult.Value);
            Assert.Equal(StatusCodes.Status409Conflict, response.ErrorCode);
        }
        [Fact]
        public async TaskAlias CreateTermsOfService_ShouldThrow_Exception()
        {
            var requestDto = CreateTermsOfServiceMock();
            _tenantTaskCategoryRepoMock.Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TermsOfServiceModel, bool>>>(), false)).ThrowsAsync(new Exception("An error occurred while fetching the task reward."));

            // Act
            var result = await _termsOfServiceController.CreateTermsOfService(requestDto);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);

        }
        private static CreateTermsOfServiceRequestDto CreateTermsOfServiceMock()
        {
            return new CreateTermsOfServiceRequestDto
            {
                TermsOfServiceId = 1,
                TermsOfServiceText = "Something text here",
                LanguageCode = "us-En",
                CreateUser = "per-915325069cdb42c783dd4601e1d27704"
            };
        }
    }
}
