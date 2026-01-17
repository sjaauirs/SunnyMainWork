using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SunnyRewards.Helios.User.Api.Controllers;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Models;
using SunnyRewards.Helios.User.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.User.Infrastructure.Services;
using SunnyRewards.Helios.User.Infrastructure.Services.Interfaces;
using Xunit;

namespace SunnyRewards.Helios.User.UnitTest.Controllers
{
    public class ConsumerActivityControllerTest
    {
        private readonly Mock<ILogger<ConsumerActivityController>> _controllerLogger;
        private readonly Mock<ILogger<ConsumerActivityService>> _serviceLogger;
        private readonly Mock<IConsumerActivityRepo> _consumerActivityRepo;
        private readonly IConsumerActivityService _consumerActivityService;
        private readonly Mock<IMapper> _mapper;
        private readonly ConsumerActivityController _consumerActivityController;
        public ConsumerActivityControllerTest()
        {
            _controllerLogger = new Mock<ILogger<ConsumerActivityController>>();
            _serviceLogger = new Mock<ILogger<ConsumerActivityService>>();
            _consumerActivityRepo = new Mock<IConsumerActivityRepo>();
            _mapper = new Mock<IMapper>();
            _consumerActivityService = new ConsumerActivityService(_serviceLogger.Object, _consumerActivityRepo.Object, _mapper.Object);
            _consumerActivityController = new ConsumerActivityController(_controllerLogger.Object, _consumerActivityService);
        }

        [Fact]
        public async Task PostConsumerActivityAsync_Should_Return_Ok_Response()
        {
            // Arrange 
            var requestDto = GetConsumerActivityRequestDto();
            _consumerActivityRepo.Setup(x => x.CreateAsync(It.IsAny<ConsumerActivityModel>())).ReturnsAsync(new ConsumerActivityModel());
            // Act 
            var response = await _consumerActivityController.CreateConsumerActivityAsync(requestDto);

            // Assert
            Assert.NotNull(response);
            var okObjectResponse = response as OkObjectResult;
            Assert.Equal(StatusCodes.Status200OK, okObjectResponse?.StatusCode);
        }
        [Fact]
        public async Task PostConsumerActivityAsync_Should_Return_Internal_Server_Error_Response()
        {
            // Arrange 
            var requestDto = new ConsumerActivityRequestDto();
            _consumerActivityRepo.Setup(x => x.CreateAsync(It.IsAny<ConsumerActivityModel>())).Throws<ArgumentException>();

            // Act 
            var response = await _consumerActivityController.CreateConsumerActivityAsync(requestDto);

            // Assert
            Assert.NotNull(response);
            var objectResponse = response as ObjectResult;
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResponse?.StatusCode);
        }
        private ConsumerActivityRequestDto GetConsumerActivityRequestDto()
        {
            return new ConsumerActivityRequestDto()
            {
                TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4",
                ConsumerCode = "cmr-58b6d93e24284a0db8bbe7fd782362ed",
                ActivitySource = "BenifitsApp",
                ActivityType = "ProductSearch",
                ActivityJson = "{\"data\":{\"productSearchActivityData\":{\"upc\":\"<product UPC barcode string scanned by consumer>\",\"productSearchResult\":\"<result from FIS endpoint>\"}},\"activityType\":\"PRODUCT_SEARCH\"}"
            };
        }
    }
}
