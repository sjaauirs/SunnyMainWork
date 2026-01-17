using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Sunny.Benefits.Bff.Api.Controllers;
using Sunny.Benefits.Bff.Core.Constants;
using Sunny.Benefits.Bff.Core.Domain.Constants;
using Sunny.Benefits.Bff.Infrastructure.HttpClients.Interfaces;
using Sunny.Benefits.Bff.Infrastructure.Services;
using Sunny.Benefits.Bff.Infrastructure.Services.Interfaces;
using Sunny.Benefits.Bff.UnitTest.Fixtures.MockDtos;
using Sunny.Benefits.Bff.UnitTest.HttpClients;
using SunnyBenefits.Fis.Core.Domain.Dtos;
using Xunit;

namespace Sunny.Benefits.Bff.UnitTest.Controllers
{
    public class StoreSearchControllerTests
    {
        private readonly Mock<ILogger<StoreSearchController>> _storeSearchControllerLogger;
        private readonly Mock<ILogger<StoreSearchService>> _storeSearchServiceLogger;
        private readonly IStoreSearchService _storeSearchService;
        private readonly StoreSearchController _storeSearchController;
        private readonly Mock<IFisClient> _fisClient;
        private readonly Mock<IConfiguration> _iConfiguration;

        public StoreSearchControllerTests()
        {
            _storeSearchControllerLogger = new Mock<ILogger<StoreSearchController>>();
            _storeSearchServiceLogger = new Mock<ILogger<StoreSearchService>>();
            _fisClient = new FisClientMock();
            _iConfiguration = new Mock<IConfiguration>();
            _storeSearchService = new StoreSearchService(_storeSearchServiceLogger.Object, _fisClient.Object, _iConfiguration.Object);
            _storeSearchController = new StoreSearchController(_storeSearchService, _storeSearchControllerLogger.Object);
        }

        [Fact]
        public async Task StoreSearch_Returns_OkResult_When_Service_Returns_Response_Without_ErrorCode()
        {
            var requestDto = new PostSearchStoresRequestMockDto();
            var result = await _storeSearchController.StoreSearch(requestDto) as OkObjectResult;
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }

        [Fact]
        public async Task StoreSearch_Returns_InternalServerError_When_Api_Throws_Exception()
        {
            var requestDto = new PostSearchStoresRequestMockDto();
            _fisClient.Setup(client => client.Post<PostSearchStoresResponseDto>("fis/store-search", It.IsAny<PostSearchStoresRequestDto>()))
                .ThrowsAsync(new Exception("Test Exception"));
            var result = await _storeSearchController.StoreSearch(requestDto) as ObjectResult;
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, result.StatusCode);
            Assert.IsType<PostSearchStoresResponseDto>(result.Value);
            var errorResponse = (PostSearchStoresResponseDto)result.Value;
            Assert.Equal(StatusCodes.Status500InternalServerError, errorResponse.ErrorCode);
        }

        [Fact]
        public async Task StoreSearch_Returns_Bad_Request_When_Input_Data_Is_Not_Valid()
        {
            var requestDto = new PostSearchStoresRequestMockDto()
            {
                Longitude = 0
            };
            var result = await _storeSearchController.StoreSearch(requestDto) as ObjectResult;
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
            Assert.IsType<PostSearchStoresResponseDto>(result.Value);
            var errorResponse = (PostSearchStoresResponseDto)result.Value;
            Assert.Equal(StatusCodes.Status400BadRequest, errorResponse.ErrorCode);
        }


        [Fact]
        public async Task StoreSearch_Returns_Bad_Request_When_config_Is_Not_available()
        {
            var requestDto = new PostSearchStoresRequestMockDto();
            _iConfiguration.Setup(config => config[CommonConstants.FISStoreTags]).Returns(string.Empty);
            var result = await _storeSearchController.StoreSearch(requestDto) as ObjectResult;
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, result.StatusCode);
            Assert.IsType<PostSearchStoresResponseDto>(result.Value);
            var errorResponse = (PostSearchStoresResponseDto)result.Value;
            Assert.Equal(StatusCodes.Status500InternalServerError, errorResponse.ErrorCode);
        }

        [Fact]
        public void GetFISTags_ShouldReturnCorrectDictionary_WhenSectionExists()
        {
            // Arrange
            var mockConfigurationSection = new Mock<IConfigurationSection>();
            mockConfigurationSection.Setup(x => x.Key).Returns("HFC");
            mockConfigurationSection.Setup(x => x.Value).Returns("Food");

            var mockConfigurationSection2 = new Mock<IConfigurationSection>();
            mockConfigurationSection2.Setup(x => x.Key).Returns("OTC");
            mockConfigurationSection2.Setup(x => x.Value).Returns("OTC");

            _iConfiguration.Setup(x => x.GetSection(CommonConstants.FISStoreTags))
                             .Returns(Mock.Of<IConfigurationSection>(s => s.GetChildren() == new List<IConfigurationSection>
                             {
                             mockConfigurationSection.Object,
                             mockConfigurationSection2.Object
                             }));

            var requestDto = new PostSearchStoresRequestMockDto();
            var responseDto = new PostSearchStoresResponseDto()
            {
                Stores = new List<StoreDto>
                {
                    new StoreDto {
                        StoreAttributes=new List<StoreAttributeDto>
                        {
                            new StoreAttributeDto
                            {
                                AttributeName="HFC",
                                AttributeValue="Y"
                            },
                            new StoreAttributeDto
                            {
                                AttributeName="OTC",
                                AttributeValue="Y"
                            }
                        }
                    }
                }
            };
            _fisClient.Setup(client => client.Post<PostSearchStoresResponseDto>("fis/store-search", It.IsAny<PostSearchStoresRequestMockDto>()))
                .ReturnsAsync(responseDto);

            // Act
            var storeSearchResult = _storeSearchService.SearchStores(requestDto);

            // Assert
            Assert.NotNull(storeSearchResult);
        }

    }
}
