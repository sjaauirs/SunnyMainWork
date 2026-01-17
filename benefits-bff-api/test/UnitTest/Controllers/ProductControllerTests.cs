using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Sunny.Benefits.Bff.Api.Controllers;
using Sunny.Benefits.Bff.Infrastructure.HttpClients.Interfaces;
using Sunny.Benefits.Bff.Infrastructure.Repositories.Interfaces;
using Sunny.Benefits.Bff.Infrastructure.Services;
using Sunny.Benefits.Bff.Infrastructure.Services.Interfaces;
using Sunny.Benefits.Bff.UnitTest.HttpClients;
using SunnyBenefits.Fis.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using Xunit;

namespace Sunny.Benefits.Bff.UnitTest.Controllers
{
    public class ProductControllerTests
    {
        private readonly Mock<ILogger<ProductController>> _productControllerLogger;
        private readonly Mock<ILogger<ProductService>> _productServiceLogger;
        private readonly Mock<ILogger<ConsumerActivityService>> _consumerActivityServiceLogger;
        private readonly IProductService _productService;
        private readonly Mock<IFisClient> _fisClient;
        private readonly Mock<IUserClient> _userClient;
        private readonly IConsumerActivityService _consumerActivityService;

        private readonly ProductController _productController;
        public ProductControllerTests()
        {
            _productControllerLogger = new Mock<ILogger<ProductController>>();
            _productServiceLogger = new Mock<ILogger<ProductService>>();
            _consumerActivityServiceLogger = new Mock<ILogger<ConsumerActivityService>>();
            _fisClient = new FisClientMock();
            _userClient = new UserClientMock();
            _consumerActivityService = new ConsumerActivityService(_consumerActivityServiceLogger.Object, _userClient.Object);
            _productService = new ProductService(_productServiceLogger.Object, _fisClient.Object, _consumerActivityService);
            _productController = new ProductController(_productControllerLogger.Object, _productService);
        }

        [Fact]
        public void SearchProduct_MissingProductCode_ReturnsBadRequest()
        {
            var request = new PostSearchProductRequestDto();
            var result = _productController.SearchProduct(request);
            Assert.IsType<ObjectResult>(result.Result);
            var objectResult = result.Result as ObjectResult;
            Assert.Equal(StatusCodes.Status400BadRequest, objectResult?.StatusCode);
        }

        [Fact]
        public void SearchProduct_Should_Ok_Response()
        {
            // Arrange
            var request = new PostSearchProductRequestDto { ConsumerCode = "cmr-d62494c1335b48febef95b21c2a9e6c3", TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4", Upc = "abc123" };
            // Act
            var result = _productController.SearchProduct(request);

            //Assert
            Assert.IsType<OkObjectResult>(result.Result);
            var objectResult = result.Result as OkObjectResult;
            Assert.Equal(StatusCodes.Status200OK, objectResult?.StatusCode);
        }

        [Fact]
        public void SearchProduct_IneligibleProductCode_ReturnsEligibleFalse()
        {
            //Arrange
            var request = new PostSearchProductRequestDto { Upc = "67890" };
            //Act 
            var result = _productController.SearchProduct(request);
            _userClient.Setup(client => client.Post<ConsumerActivityResponseDto>("consumer-activity", It.IsAny<ConsumerActivityRequestDto>()))
          .ReturnsAsync(new ConsumerActivityResponseDto());

            // Assert
            Assert.IsType<ObjectResult>(result.Result);
            var objectResult = result.Result as ObjectResult;
            Assert.Equal(StatusCodes.Status400BadRequest, objectResult?.StatusCode);
        }

        [Fact]
        public void SearchProduct_WhenException_Occurred_ReturnsInternalServer()
        {
            // Arrange
            var request = new PostSearchProductRequestDto { Upc = "12345" };
            Mock<IProductService> productServiceMock = new Mock<IProductService>();
            productServiceMock.Setup(x => x.SearchProduct(It.IsAny<PostSearchProductRequestDto>())).Throws(new Exception("Test Exception"));
            var productController = new ProductController(_productControllerLogger.Object, productServiceMock.Object);

            //Act
            var result = productController.SearchProduct(request);

            // Assert
            Assert.IsType<ObjectResult>(result.Result);
            var objectResult = result.Result as ObjectResult;
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult?.StatusCode);
        }

        [Fact]
        public async void SearchProduct_Exception_Occurred_ReturnsInternalServer()
        {
            // Arrange
            var request = new PostSearchProductRequestDto { ConsumerCode = "cmr-d62494c1335b48febef95b21c2a9e6c3", TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4", Upc = "abc123" };
            Mock<IProductService> productServiceMock = new Mock<IProductService>();
            _fisClient.Setup(x => x.Post<ProductSearchResponseDto>("fis/product-search", It.IsAny<PostSearchProductRequestDto>()))
                            .ThrowsAsync(new Exception("inner exception"));
            // Act & Assert
            await Assert.ThrowsAsync<Exception>(async () => await _productService.SearchProduct(request));
        }
        [Fact]
        public void SearchProduct__ReturnsInternalServer_When_UserClient_Throws_Exception()
        {
            // Arrange
            var request = new PostSearchProductRequestDto { ConsumerCode = "cmr-d62494c1335b48febef95b21c2a9e6c3", TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4", Upc = "abc123" };
            _userClient.Setup(client => client.Post<ConsumerActivityResponseDto>("consumer-activity", It.IsAny<ConsumerActivityRequestDto>())).Throws(new Exception("Testing"));

            // Act
            var result = _productController.SearchProduct(request);

            // Assert
            Assert.IsType<OkObjectResult>(result.Result);
            var objectResult = result.Result as OkObjectResult;
            Assert.Equal(StatusCodes.Status200OK, objectResult?.StatusCode);
        }
        [Fact]
        public void SearchProduct_Return_OkResponse_When_UserClient_Returns_Error()
        {
            // Arrange
            var request = new PostSearchProductRequestDto { ConsumerCode = "cmr-d62494c1335b48febef95b21c2a9e6c3", TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4", Upc = "abc123" };
            _userClient.Setup(client => client.Post<ConsumerActivityResponseDto>("consumer-activity", It.IsAny<ConsumerActivityRequestDto>())).
                ReturnsAsync(new ConsumerActivityResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError });
            //Act
            var result = _productController.SearchProduct(request);

            // Assert
            Assert.IsType<OkObjectResult>(result.Result);
            var objectResult = result.Result as OkObjectResult;
            Assert.Equal(StatusCodes.Status200OK, objectResult?.StatusCode);
        }
    }
}
