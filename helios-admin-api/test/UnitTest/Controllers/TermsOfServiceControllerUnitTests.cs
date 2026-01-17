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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using Xunit;
using TaskAlias = System.Threading.Tasks.Task;
using SunnyRewards.Helios.Admin.Core.Domain.Constants;
using Microsoft.VisualStudio.TestPlatform.Common;

namespace SunnyRewards.Helios.Admin.UnitTest.Controllers
{
    public class TermsOfServiceControllerUnitTests
    {
        private readonly Mock<ILogger<TermsOfServiceController>> _controllerLogger;
        private readonly Mock<ILogger<TermsOfServiceService>> _termsOfServiceLogger;
        private readonly Mock<ITaskClient> _taskClient;
        private readonly ITermsOfServiceService _termsOfService;
        private readonly TermsOfServiceController _termsOfServiceController;

        public TermsOfServiceControllerUnitTests()
        {
            _controllerLogger = new Mock<ILogger<TermsOfServiceController>>();
            _termsOfServiceLogger = new Mock<ILogger<TermsOfServiceService>>();
            _taskClient = new Mock<ITaskClient>();
            _termsOfService = new TermsOfServiceService(_termsOfServiceLogger.Object, _taskClient.Object);

            _termsOfServiceController = new TermsOfServiceController(_controllerLogger.Object, _termsOfService);
        }
        [Fact]
        public async TaskAlias CreateTaskReward_ShouldReturnOkResult()
        {
            // Arrange
            var requestDto = CreateTermsOfServiceMockDto();
            _taskClient.Setup(x => x.Post<BaseResponseDto>(Constant.CreateTermsOfServiceUrl, It.IsAny<CreateTermsOfServiceRequestDto>()))
                .ReturnsAsync(new BaseResponseDto() { ErrorCode = null });
            // Act
            var result = await _termsOfServiceController.CreateTermsOfService(requestDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Fact]
        public async TaskAlias CreateTaskReward_ShouldReturnErrorResult_WhenCreateTaskReturnsError()
        {
            // Arrange
            var requestDto = CreateTermsOfServiceMockDto();

            _taskClient.Setup(x => x.Post<BaseResponseDto>(Constant.CreateTermsOfServiceUrl, It.IsAny<CreateTermsOfServiceRequestDto>()))
                .ReturnsAsync(new BaseResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError });

            // Act
            var result = await _termsOfServiceController.CreateTermsOfService(requestDto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }
        [Fact]
        public async TaskAlias CreateTaskReward_ShouldThrow_Exception_ErrorResult_WhenCreateTaskReturnsException()
        {
            // Arrange
            var requestDto = CreateTermsOfServiceMockDto();

            _taskClient.Setup(x => x.Post<BaseResponseDto>(Constant.CreateTermsOfServiceUrl, It.IsAny<CreateTermsOfServiceRequestDto>()))
                    .ThrowsAsync(new Exception("An error occurred while Creating the task details."));

            // Act
            var result = await _termsOfServiceController.CreateTermsOfService(requestDto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        }

        private static CreateTermsOfServiceRequestDto CreateTermsOfServiceMockDto()
        {
            return new CreateTermsOfServiceRequestDto
            {
                TermsOfServiceId = 1,
                TermsOfServiceText = "Test",
                LanguageCode = "en",
                CreateUser = "per-915325069cdb42c783dd4601e1d27704"
            };
        }
    }
}
