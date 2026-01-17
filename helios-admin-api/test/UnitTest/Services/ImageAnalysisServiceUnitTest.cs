using Google.Cloud.Vision.V1;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Infrastructure.Helpers;
using SunnyRewards.Helios.Admin.Infrastructure.Helpers.Interface;
using SunnyRewards.Helios.Admin.Infrastructure.Services;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SunnyRewards.Helios.Admin.UnitTest.Services
{
    public class ImageAnalysisServiceUnitTest
    {
        
            private readonly Mock<ILogger<ImageSearchService>> _loggerMock;
            private readonly Mock<ImageAnnotatorClient> _imageAnnotatorClientMock;
            private readonly Mock<ISecretHelper> _secretMock;
            private readonly ImageSearchService _imageAnalysisService;

            public ImageAnalysisServiceUnitTest()
            {
                // Mocking the logger
                _loggerMock = new Mock<ILogger<ImageSearchService>>();

                // Mocking the ImageAnnotatorClient
                _imageAnnotatorClientMock = new Mock<ImageAnnotatorClient>();
            _secretMock = new Mock<ISecretHelper>();
            _secretMock.Setup(s => s.GetAwsFireBaseCredentialKey()).ReturnsAsync("valid-json-credentials");

            // Mock the image annotator client (you would mock responses for it)
          
            // Initialize the service with mocked dependencies
            _imageAnalysisService = new ImageSearchService(_loggerMock.Object, _secretMock.Object);
            }

            [Fact]
            public void AnalyzeImage_ShouldReturnImageSearchResponse_WhenAnalysisIsSuccessful()
            {
                // Arrange
                var imageRequest = new ImageSearchRequestDto
                {
                    Base64Image = "xyvvv",
                      
                };

                var labelAnnotations = new List<EntityAnnotation>
        {
            new EntityAnnotation { Description = "Label 1" },
            new EntityAnnotation { Description = "Label 2" }
        };
               
              

                var visionResponse = new AnnotateImageResponse
                {
                    LabelAnnotations = { labelAnnotations },
                };

                // Mocking the client behavior
                _imageAnnotatorClientMock
                    .Setup(client => client.AnnotateAsync(It.IsAny<AnnotateImageRequest>(),null))
                    .ReturnsAsync(visionResponse);

                // Act
                var result =  _imageAnalysisService.AnalyzeImage(imageRequest);

                // Assert
                Assert.NotNull(result);
               
            }

           }
    }

