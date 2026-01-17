using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Sunny.Benefits.Bff.Api.Controllers;
using Sunny.Benefits.Bff.Infrastructure.HttpClients.Interfaces;
using Sunny.Benefits.Bff.Infrastructure.Services;
using Sunny.Benefits.Bff.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Sunny.Benefits.Bff.UnitTest.Controllers
{
    public class ImageSearchControllerUnitTest
    {
        private readonly Mock<IImageSearchService> _mockImageSearchService;
        private readonly Mock<ILogger<ImageSearchController>> _mockLogger;
        private readonly ImageSearchController _controller;
        private readonly Mock<IAdminClient> _mockAdminClient;
        private readonly Mock<ILogger<ImageSearchService>> _mockServiceLogger;
        private readonly ImageSearchService _service;

        public ImageSearchControllerUnitTest()
        {
            // Setup mock services
            _mockImageSearchService = new Mock<IImageSearchService>();
            _mockLogger = new Mock<ILogger<ImageSearchController>>();
            _mockAdminClient = new Mock<IAdminClient>();
            _mockServiceLogger = new Mock<ILogger<ImageSearchService>>();

            // Initialize the service with the mocked dependencies
            _service = new ImageSearchService(_mockServiceLogger.Object, _mockAdminClient.Object);

            // Initialize the controller with mocks
            _controller = new ImageSearchController( _mockLogger.Object,_mockImageSearchService.Object);
        }

        [Fact]
        public async Task AnalyzeImage_ReturnsOkResult_WhenImageSearchResponseDtoIsValid()
        {
            // Arrange
            var ImageSearchRequestDto = new ImageSearchRequestDto
            {
Base64Image="xyz"
            }; 

            _mockImageSearchService
                .Setup(service => service.AnalyseImageSearch(It.IsAny<ImageSearchRequestDto>()))
                .ReturnsAsync(new ImageSearchResponseDto { lables= new List<string> { "test", "test2" } });

            // Act
            var result = await _controller.AnalyzeImage(ImageSearchRequestDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result); // Ensure the result is Ok()
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode); // Ensure the response is the correct DTO
        }

        [Fact]
        public async Task AnalyzeImage_ReturnsNotFoundResult_WhenImageSearchResponseDtoIsNull()
        {
            // Arrange

            var ImageSearchRequestDto = new ImageSearchRequestDto
            {
Base64Image="xyz"
            };

            _mockImageSearchService
                .Setup(service => service.AnalyseImageSearch(It.IsAny<ImageSearchRequestDto>()))
                .ReturnsAsync(It.IsAny<ImageSearchResponseDto>());

            // Act
            var result = await _controller.AnalyzeImage(ImageSearchRequestDto);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result); // Ensure the result is NotFound
            Assert.Null(notFoundResult.Value); // Ensure there's no data in the result
        }

        [Fact]
        public async Task AnalyseImageSearch_ReturnsImageSearchResponseDto_WhenSuccessful()
        {
            // Arrange

            var ImageSearchRequestDto = new ImageSearchRequestDto
            {
Base64Image="xyz"
            };
            var responseDto = new ImageSearchResponseDto
            {
                lables = new List<string> { "test", "test2" }
            };
            

            _mockAdminClient
                .Setup(client => client.Post<ImageSearchResponseDto>(It.IsAny<string>(), It.IsAny<ImageSearchRequestDto>()))
                .ReturnsAsync(responseDto);

            // Act
            var result = await _service.AnalyseImageSearch(ImageSearchRequestDto);

            // Assert
            Assert.NotNull(result); // Ensure the result is not null
            Assert.Equal(responseDto, result); // Ensure the returned result matches the expected response
        }

        [Fact]
        public async Task AnalyseImageSearch_ReturnsError_WhenImageSearchResponseDtoIsNull()
        {
            // Arrange

            var ImageSearchRequestDto = new ImageSearchRequestDto
            {
Base64Image="xyz"
            };
            var errorResponse = new ImageSearchResponseDto
            {
                ErrorCode = StatusCodes.Status404NotFound,
                ErrorMessage = "No data found"
            };

            _mockAdminClient
                .Setup(client => client.Post<ImageSearchResponseDto>(It.IsAny<string>(), It.IsAny<ImageSearchRequestDto>()));

            // Act
            var result = await _service.AnalyseImageSearch(ImageSearchRequestDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status404NotFound, result.ErrorCode);
            Assert.Equal("No data found", result.ErrorMessage);
        }
        [Fact]
        public async Task AnalyseImageSearch_ThrowsException_WhenExceptionOccurs()
        {
            // Arrange
            var ImageSearchRequestDto = new ImageSearchRequestDto
            {
Base64Image="xyz"
            };
            _mockAdminClient
                .Setup(client => client.Post<ImageSearchResponseDto>(It.IsAny<string>(), It.IsAny<ImageSearchRequestDto>()))
                .ThrowsAsync(new Exception("Something went wrong"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _service.AnalyseImageSearch(ImageSearchRequestDto));
            Assert.Equal("Something went wrong", exception.Message);
        }
        [Fact]
        public async Task AnalyzeImage_ReturnsInternalServerError_WhenExceptionOccurs()
        {
            // Arrange
            var ImageSearchRequestDto = new ImageSearchRequestDto
            {
Base64Image="xyz"
            };
            _mockImageSearchService
                .Setup(service => service.AnalyseImageSearch(It.IsAny<ImageSearchRequestDto>()))
                .ThrowsAsync(new System.Exception("Something went wrong"));

            // Act
            var result = await _controller.AnalyzeImage(ImageSearchRequestDto);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result); // Ensure the result is an ObjectResult
            Assert.Equal(500, statusCodeResult.StatusCode); // Ensure the status code is 500 (Internal Server Error)
        }
    }
}
