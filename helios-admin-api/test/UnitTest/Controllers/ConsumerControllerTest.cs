using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SunnyRewards.Helios.Admin.Api.Controllers;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Admin.UnitTest.Fixtures.MockDtos;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using Xunit;
using TaskAlias = System.Threading.Tasks.Task;
namespace SunnyRewards.Helios.Admin.UnitTest.Controllers
{
    public class ConsumerControllerTest
    {
        private readonly IConsumerService _consumerService;
        private readonly ConsumerController _consumerController;
        private readonly Mock<ILogger<ConsumerController>> _consumerLogger;
        private readonly Mock<IUserClient> _userClient;
        public ConsumerControllerTest()
        {
            _userClient = new Mock<IUserClient>();
            _consumerLogger = new Mock<ILogger<ConsumerController>>();
            _consumerService = new ConsumerService(_userClient.Object);
            _consumerController = new ConsumerController(_consumerLogger.Object, _consumerService);
        }
        [Fact]
        public async TaskAlias Should_Get_Consumer_By_MemNbr()
        {
            // Arrange
            var consumerRequestMockDto = new GetConsumerByMemNbrRequestMockDto();
            _userClient.Setup(x => x.Post<GetConsumerByMemIdResponseDto>(It.IsAny<string>(), It.IsAny<GetConsumerByMemIdRequestDto>()))
                .ReturnsAsync(new GetConsumerByMemIdResponseDto());

            // Act
            var consumerResponseMockDto = await _consumerController.GetConsumerByMemId(consumerRequestMockDto);

            // Assert
            var result = consumerResponseMockDto.Result as OkObjectResult;
            Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }

        [Fact]
        public async TaskAlias Should_Not_Get_Consumer_By_MemNbr()
        {
            // Arrange
            var consumerRequestMockDto = new GetConsumerByMemNbrRequestMockDto();
            _userClient.Setup(x => x.Post<GetConsumerByMemIdResponseDto>(It.IsAny<string>(), It.IsAny<GetConsumerByMemIdRequestDto>())).ReturnsAsync(new GetConsumerByMemIdResponseDto()
            {
                ErrorCode = StatusCodes.Status404NotFound
            });
            // Act
            var response = await _consumerController.GetConsumerByMemId(consumerRequestMockDto);

            // Assert
            var result = response.Result as ObjectResult;
            Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status404NotFound, result.StatusCode);
        }

        [Fact]
        public async TaskAlias Should_Catch_GetConsumerByMemNbr_Controller_Level_Exception()
        {
            // Arrange
            _userClient.Setup(x => x.Post<GetConsumerByMemIdResponseDto>(It.IsAny<string>(), It.IsAny<GetConsumerByMemIdRequestDto>()))
                .ThrowsAsync(new Exception("intended exception"));
            var consumerRequestMockDto = new GetConsumerByMemNbrRequestMockDto();

            // Act
            var response = await _consumerController.GetConsumerByMemId(consumerRequestMockDto);

            // Assert
            var result = response.Result as ObjectResult;
            Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, result.StatusCode);
        }
        [Fact]
        public async TaskAlias Should_Get_Consumer()
        {
            // Arrange
            var consumerRequestMockDto = new GetConsumerRequestMockDto();
            _userClient.Setup(x => x.Post<GetConsumerResponseDto>(It.IsAny<string>(), It.IsAny<GetConsumerRequestDto>())).ReturnsAsync(new GetConsumerResponseMockDto());

            // Act
            var response = await _consumerController.GetConsumer(consumerRequestMockDto);

            // Assert
            var result = response.Result as OkObjectResult;
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }

        [Fact]
        public async TaskAlias Should_Not_Get_Consumer_For_InValid_ConsumerCode()
        {
            // Arrange
            GetConsumerRequestMockDto consumerRequestMockDto = new GetConsumerRequestMockDto();
            _userClient.Setup(x => x.Post<GetConsumerResponseDto>(It.IsAny<string>(), It.IsAny<GetConsumerRequestDto>())).ReturnsAsync(new GetConsumerResponseDto()
            {
                ErrorCode = StatusCodes.Status404NotFound
            });

            // Act
            var response = await _consumerController.GetConsumer(consumerRequestMockDto);

            // Assert
            var result = response.Result as ObjectResult;
            Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status404NotFound, result.StatusCode);
        }

        [Fact]
        public async TaskAlias Should_Catch_GetConsumer_Controller_Level_Exception()
        {
            // Arrange
            var dto = new GetConsumerRequestMockDto();
            _userClient.Setup(x => x.Post<GetConsumerResponseDto>(It.IsAny<string>(), It.IsAny<GetConsumerRequestDto>()))
                  .ThrowsAsync(new Exception("intended exception"));

            // Act
            var response = await _consumerController.GetConsumer(dto);

            // Assert
            var result = response.Result as ObjectResult;
            Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, result.StatusCode);
        }
    }
}
