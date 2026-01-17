using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Sunny.Benefits.Bff.Api.Controllers;
using Sunny.Benefits.Bff.Core.Domain.Constants;
using Sunny.Benefits.Bff.Core.Domain.Dtos;
using Sunny.Benefits.Bff.Infrastructure.Helpers.Interface;
using Sunny.Benefits.Bff.Infrastructure.HttpClients.Interfaces;
using Sunny.Benefits.Bff.Infrastructure.Repositories.Interfaces;
using Sunny.Benefits.Bff.Infrastructure.Services;
using Sunny.Benefits.Bff.Infrastructure.Services.Interfaces;
using Sunny.Benefits.Bff.UnitTest.Fixtures.MockDtos;
using Sunny.Benefits.Bff.UnitTest.Fixtures.MockModels;
using Sunny.Benefits.Bff.UnitTest.HttpClients;
using Sunny.Benefits.Cms.Core.Domain.Dtos;
using SunnyRewards.Helios.Cohort.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using Xunit;

namespace Sunny.Benefits.Bff.UnitTest.Controllers
{
    public class CmsControllerUnitTest
    {
        private readonly Mock<ILogger<CmsController>> _cmsControllerLogger;
        private readonly Mock<ILogger<CmsService>> _cmsServiceLogger;
        private readonly ICmsService _cmsService;
        private readonly CmsController _cmsController;
        private readonly Mock<ICmsClient> _cmsClientMock;
        private readonly Mock<ICommonHelper> _commonHelperMock;
        private readonly Mock<ICohortConsumerService> _cohortConsumerServiceMock;

        public CmsControllerUnitTest()
        {
            _cmsControllerLogger = new Mock<ILogger<CmsController>>();
            _cmsServiceLogger = new Mock<ILogger<CmsService>>();
            _cmsClientMock = new CmsClientMock();
            _cohortConsumerServiceMock = new Mock<ICohortConsumerService>();
            _commonHelperMock = new Mock<ICommonHelper>();

            _cmsService = new CmsService(_cmsServiceLogger.Object, _cmsClientMock.Object, _cohortConsumerServiceMock.Object, _commonHelperMock.Object);
            _cmsController = new CmsController(_cmsControllerLogger.Object, _cmsService);
        }

        [Fact]
        public async Task Should_GetComponentList_cmsController()
        {
            var getComponentListRequestMockDto = new GetComponentListRequestMockDto();
            var response = await _cmsController.GetComponentList(getComponentListRequestMockDto);
            var result = response.Result as OkObjectResult;
            Assert.True(result?.StatusCode == 200);
        }
        [Fact]
        public async Task Should_GetforYouComponentList_cmsController()
        {
          
                // Arrange
                var mockCohortConsumerService = new Mock<ICohortConsumerService>();
                var mockLogger = new Mock<ILogger<CmsService>>();

                // Create a mock subclass to override GetComponentList

            var requestDto = new GetComponentListRequestDto
            {
                TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4",
                ConsumerCode = "test",
                ComponentName=CmsConstants.FOR_YOU
            };
              

                var component = new ComponentDto
                {
                    MetadataJson = "{\"tags\": [\"cohort:diabetes\"]}"
                };

                var getComponentListResponse = new GetComponentListResponseDto
                {
                    Components = new List<ComponentDto> { component }
                };

            // Mock GetComponentList to return the component
            _cmsClientMock.Setup(x => x.Post<GetComponentListResponseDto>("cms/component-list", It.IsAny<GetComponentListRequestDto>()))
      .ReturnsAsync(new GetComponentListResponseMockDto());
            

                // Mock consumer cohort service to return a matching cohort
                var cohortResponse = new CohortConsumerResponseDto
                {
                    ConsumerCohorts = new List<CohortConsumersDto>
            {
                new CohortConsumersDto { CohortName = "diabetes" }
            }
                };

                mockCohortConsumerService
                    .Setup(x => x.GetConsumerCohorts(It.IsAny<GetConsumerByCohortsNameRequestDto>(), It.IsAny<string>()))
                    .ReturnsAsync(cohortResponse);

            // Act
            var response = await _cmsController.GetComponentList(requestDto);

            // Assert
            var result = response.Result as OkObjectResult;
            Assert.True(result?.StatusCode == 200);


        }

        [Fact]
        public async Task Should_GetComponentList_NotFound_cmsController()
        {
            var getComponentListRequestMockDto = new GetComponentListRequestMockDto();
            var cmsServiceMock = new Mock<ICmsService>();
            cmsServiceMock.Setup(x => x.GetComponentList(getComponentListRequestMockDto, It.IsAny<string>()))
          .ReturnsAsync(new GetComponentListResponseMockDto()
          {
              Components = null
          });
            var cmsController = new CmsController(_cmsControllerLogger.Object, cmsServiceMock.Object);
            var response = await cmsController.GetComponentList(getComponentListRequestMockDto);
            var result = response.Result as NotFoundResult;
            Assert.True(result?.StatusCode == 404);
        }

        [Fact]
        public async Task Should_GetComponentList_Catch_Exception_cmsController()
        {
            var getComponentListRequestMockDto = new GetComponentListRequestMockDto();
            var cmsServiceMock = new Mock<ICmsService>();
            cmsServiceMock.Setup(x => x.GetComponentList(getComponentListRequestMockDto, It.IsAny<string>()))
          .ThrowsAsync(new Exception("Simulated exception"));
            var cmsController = new CmsController(_cmsControllerLogger.Object, cmsServiceMock.Object);
            var response = await cmsController.GetComponentList(getComponentListRequestMockDto);
            var result = response.Result as OkObjectResult;
            Assert.True(result == null);
        }

        [Fact]
        public async Task Should_GetComponentList_Service()
        {
            var getComponentListRequestMockDto = new GetComponentListRequestMockDto();
            var response = await _cmsService.GetComponentList(getComponentListRequestMockDto);
            Assert.True(response.Components != null);
        }

        [Fact]
        public async Task Should_GetComponentList_NotFound_Service()
        {
            var getComponentListRequestMockDto = new GetComponentListRequestMockDto();
            var cmsClientMock = new Mock<ICmsClient>();
            cmsClientMock.Setup(x => x.Post<GetComponentListResponseDto>("cms/component-list", It.IsAny<GetComponentListRequestDto>()))
          .ReturnsAsync(new GetComponentListResponseMockDto()
          {
              Components = null,
              ErrorCode = 404
          });
            var cmsService = new CmsService(_cmsServiceLogger.Object, cmsClientMock.Object, _cohortConsumerServiceMock.Object, _commonHelperMock.Object);
            var response = await cmsService.GetComponentList(getComponentListRequestMockDto);
            var result = response.Components == null;
            Assert.True(response.ErrorCode == 404);
        }

        [Fact]
        public async Task Should_GetComponentList_Catch_Exception_Service()
        {
            var getComponentListRequestMockDto = new GetComponentListRequestMockDto();
            var cmsClientMock = new Mock<ICmsClient>();
            cmsClientMock.Setup(x => x.Post<GetComponentListResponseDto>("cms/component-list", It.IsAny<GetComponentListRequestDto>()))
          .ThrowsAsync(new Exception("Simulated exception"));
            var cmsService = new CmsService(_cmsServiceLogger.Object, cmsClientMock.Object, _cohortConsumerServiceMock.Object, _commonHelperMock.Object);
            await Assert.ThrowsAsync<Exception>(async () => await cmsService.GetComponentList(getComponentListRequestMockDto));
        }

        [Fact]
        public async System.Threading.Tasks.Task Should_GetFaqRetrival()
        {
            // Arrange
            var getComponentListRequestMockDto = new FaqRetriveRequestMockDto
            {
                TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4"
            };

            var sectionListResponse = new GetComponentListResponseDto
            {
                Components = new List<ComponentDto>
                {
                    new ComponentDto
                    {
                        ComponentName = "Activating Your Card",
                        DataJson = "{\"data\":{\"details\":{\"SectionCollectionName\":\"section-collection-1\"}}}"
                    }
                }
            };
            var itemListResponse = new GetComponentListResponseDto
            {
                Components = new List<ComponentDto>
                {
            new ComponentDto
            {
                ComponentName = "Question about activating your card",
                DataJson = "{\"data\":{\"details\":{\"HeaderText\":\"How do I activate my card?\",\"DescriptionText\":\"You can activate your card by...\"}}}"
            }
                }
            };
            _cmsClientMock.Setup(client => client.Post<GetComponentListResponseDto>("cms/component-list", It.Is<ComponentListRequestDto>(r => r.ComponentName == CmsConstants.FAQ_SECTION)))
            .ReturnsAsync(sectionListResponse);

            _cmsClientMock.Setup(client => client.Post<GetComponentListResponseDto>("cms/component-list", It.Is<ComponentListRequestDto>(r => r.ComponentName == "section-collection-1")))
              .ReturnsAsync(itemListResponse);

            // Act
            var response = await _cmsController.GetFAQList(getComponentListRequestMockDto);
            var result = response.Result as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);

            var responseBody = result.Value as FaqSectionResponseDto;
            Assert.NotNull(responseBody);
            Assert.True(result?.StatusCode == 200);
        }
        [Fact]
        public async System.Threading.Tasks.Task Should_GetFaqRetrival_Catch_Exception_In_Service()
        {
            // Arrange
            var faqRetriveRequestDto = new FaqRetriveRequestMockDto
            {
                TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4"
            };

            var cmsServiceLoggerMock = new Mock<ILogger<CmsService>>();
            _cmsClientMock.Setup(client => client.Post<GetComponentListResponseDto>("cms/component-list", It.Is<ComponentListRequestDto>(r => r.ComponentName == CmsConstants.FAQ_SECTION))).ThrowsAsync(new Exception("Simulated exception"));
            await Assert.ThrowsAsync<Exception>(async () => await _cmsService.GetFaqSection(faqRetriveRequestDto));
        }

        [Fact]
        public async Task GetComponent_ShouldReturnOk_WhenNoError()
        {
            // Arrange
            var requestDto = new GetComponentRequestDto { TenantCode = "Tenant123", ComponentName = "Test ComponentName" };

            // Act
            var result = await _cmsController.GetComponent(requestDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task GetComponent_ShouldReturnError_WhenErrorCodeIsNotNull()
        {
            // Arrange
            var requestDto = new GetComponentRequestDto { TenantCode = "Tenant123", ComponentName = "Test ComponentName" };
            _cmsClientMock.Setup(client => client.Post<GetComponentResponseDto>("cms/get-component", It.IsAny<GetComponentRequestDto>()))
              .ReturnsAsync(new GetComponentResponseDto()
              {
                  ErrorCode = StatusCodes.Status404NotFound
              });

            // Act
            var result = await _cmsController.GetComponent(requestDto);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status404NotFound, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task GetComponent_ShouldReturnInternalServerError_WhenExceptionIsThrown()
        {
            // Arrange
            var requestDto = new GetComponentRequestDto { TenantCode = "Tenant123", ComponentName = "Test ComponentName" };
            
            _cmsClientMock.Setup(client => client.Post<GetComponentResponseDto>("cms/get-component", It.IsAny<GetComponentRequestDto>()))
              .ThrowsAsync(new Exception("Testing"));
            // Act
            var result = await _cmsController.GetComponent(requestDto);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
        }


        [Fact]
        public async Task GetTenantComponentsByTypeName_ShouldReturnOk_WhenServiceReturnsComponents()
        {
            // Arrange
            var request = new GetTenantComponentByTypeNameRequestDto
            {
                TenantCode = "TENANT1",
                ComponentTypeName = "Banner"
            };

            var components = new List<ComponentDto> { new ComponentDto { ComponentName = "Banner" } };
            var responseDto = new GetComponentsResponseDto { Components = components };

            _cmsClientMock
                .Setup(x => x.Post<GetComponentsResponseDto>(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(responseDto);

            // Act
            var result = await _cmsController.GetTenantComponentsByTypeName(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<GetComponentsResponseDto>(okResult.Value);
            Assert.NotNull(response.Components);
            Assert.Single(response.Components);
        }

        [Fact]
        public async Task GetTenantComponentsByTypeName_ShouldReturnError_WhenServiceReturnsErrorCode()
        {
            // Arrange
            var request = new GetTenantComponentByTypeNameRequestDto
            {
                TenantCode = "TENANT1",
                ComponentTypeName = "Banner"
            };

            var responseDto = new GetComponentsResponseDto
            {
                ErrorCode = StatusCodes.Status404NotFound,
                ErrorMessage = "Not found"
            };

            _cmsClientMock
                .Setup(x => x.Post<GetComponentsResponseDto>(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(responseDto);

            // Act
            var result = await _cmsController.GetTenantComponentsByTypeName(request);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);
            var response = Assert.IsType<GetComponentsResponseDto>(objectResult.Value);
            Assert.Equal(StatusCodes.Status404NotFound, response.ErrorCode);
        }

        [Fact]
        public async Task GetTenantComponentsByTypeName_ShouldReturnInternalServerError_WhenExceptionThrown()
        {
            // Arrange
            var request = new GetTenantComponentByTypeNameRequestDto
            {
                TenantCode = "TENANT1",
                ComponentTypeName = "Banner"
            };

            _cmsClientMock
                .Setup(x => x.Post<GetComponentsResponseDto>(It.IsAny<string>(), It.IsAny<object>()))
                .ThrowsAsync(new Exception("Simulated exception"));

            // Act
            var result = await _cmsController.GetTenantComponentsByTypeName(request);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
            var response = Assert.IsType<GetComponentsResponseDto>(objectResult.Value);
            Assert.Equal(StatusCodes.Status500InternalServerError, response.ErrorCode);
        }

        [Fact]
        public async Task GetTenantComponentsByTypeName_ShouldApplyCohortFilter_WhenRequested()
        {
            // Arrange
            var request = new GetTenantComponentByTypeNameRequestDto
            {
                TenantCode = "TENANT1",
                ComponentTypeName = "Banner",
                ApplyCohortFilter = true,
                ConsumerCode = "CONSUMER1"
            };

            // Component with cohort tag
            var component = new ComponentDto
            {
                MetadataJson = "{\"tags\": [\"cohort:diabetes\"]}"
            };
            var responseDto = new GetComponentsResponseDto
            {
                Components = new List<ComponentDto> { component }
            };

            _cmsClientMock
                .Setup(x => x.Post<GetComponentsResponseDto>(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(responseDto);

            _cohortConsumerServiceMock
                .Setup(x => x.GetConsumerCohorts(It.IsAny<GetConsumerByCohortsNameRequestDto>(), It.IsAny<string>()))
                .ReturnsAsync(new CohortConsumerResponseDto
                {
                    ConsumerCohorts = new List<CohortConsumersDto>
                    {
                new CohortConsumersDto { CohortName = "diabetes" }
                    }
                });

            // Act
            var result = await _cmsController.GetTenantComponentsByTypeName(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<GetComponentsResponseDto>(okResult.Value);
            Assert.NotNull(response.Components);
            Assert.Single(response.Components);
        }
        [Fact]
        public async Task GetComponentBycode_ShouldReturnComponent_WhenApiResponseIsSuccessful()
        {
            // Arrange
            var request = new GetComponentByCodeRequestDto { componentCode = "test_component" };

            var expectedResponse = new GetComponentByCodeResponseDto
            {
                Component = new ComponentDto { ComponentCode = "test_component" },
                ErrorCode = null
            };

            _cmsClientMock
                .Setup(c => c.Post<GetComponentByCodeResponseDto>(
                    CmsConstants.GET_COMPONENT_BY_CODE_API_URL,
                    request))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _cmsService.GetComponentBycode(request);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.ErrorCode);
            Assert.Equal("test_component", result.Component.ComponentCode);

        
        }

        [Fact]
        public async Task GetComponentBycode_ShouldReturnErrorResponse_WhenApiReturnsError()
        {
            // Arrange
            var request = new GetComponentByCodeRequestDto { componentCode = "invalid_component" };

            var expectedResponse = new GetComponentByCodeResponseDto
            {
                ErrorCode = StatusCodes.Status404NotFound,
                ErrorMessage = "Component not found"
            };

            _cmsClientMock
                .Setup(c => c.Post<GetComponentByCodeResponseDto>(
                    CmsConstants.GET_COMPONENT_BY_CODE_API_URL,
                    request))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _cmsService.GetComponentBycode(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status404NotFound, result.ErrorCode);
            Assert.Equal("Component not found", result.ErrorMessage);
        }

        [Fact]
        public async Task GetComponentBycode_ShouldThrowException_WhenClientThrowsException()
        {
            // Arrange
            var request = new GetComponentByCodeRequestDto { componentCode = "error_component" };

            _cmsClientMock
                .Setup(c => c.Post<GetComponentByCodeResponseDto>(
                    CmsConstants.GET_COMPONENT_BY_CODE_API_URL,
                    request))
                .ThrowsAsync(new Exception("Network error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _cmsService.GetComponentBycode(request));

          
        }

        [Fact]
        public async Task GetTenantComponentsByTypeNames_ShouldReturnOk_WhenAllTypesSucceed()
        {
            // Arrange
            var request = new GetTenantComponentsByTypeNamesRequestDto
            {
                TenantCode = "TENANT1",
                ComponentTypeNames = new List<string> { "Banner", "Card" },
                ApplyCohortFilter = false
            };

            var bannerComponents = new List<ComponentDto> { new ComponentDto { ComponentName = "Banner1" } };
            var cardComponents = new List<ComponentDto> { new ComponentDto { ComponentName = "Card1" } };

            _cmsClientMock
                .Setup(x => x.Post<GetComponentsResponseDto>("component/get-tenant-components-by-type-name", It.Is<GetTenantComponentByTypeNameRequestDto>(r => r.ComponentTypeName == "Banner")))
                .ReturnsAsync(new GetComponentsResponseDto { Components = bannerComponents });

            _cmsClientMock
                .Setup(x => x.Post<GetComponentsResponseDto>("component/get-tenant-components-by-type-name", It.Is<GetTenantComponentByTypeNameRequestDto>(r => r.ComponentTypeName == "Card")))
                .ReturnsAsync(new GetComponentsResponseDto { Components = cardComponents });

            // Act
            var result = await _cmsController.GetTenantComponentsByTypeNames(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<GetTenantComponentsByTypeNamesResponseDto>(okResult.Value);
            Assert.NotNull(response.ComponentsByType);
            Assert.Equal(2, response.ComponentsByType.Count);
            Assert.True(response.ComponentsByType.ContainsKey("Banner"));
            Assert.True(response.ComponentsByType.ContainsKey("Card"));
            Assert.Single(response.ComponentsByType["Banner"]);
            Assert.Single(response.ComponentsByType["Card"]);
        }

        [Fact]
        public async Task GetTenantComponentsByTypeNames_ShouldReturnOk_WhenSomeTypesFail()
        {
            // Arrange
            var request = new GetTenantComponentsByTypeNamesRequestDto
            {
                TenantCode = "TENANT1",
                ComponentTypeNames = new List<string> { "Banner", "InvalidType" },
                ApplyCohortFilter = false
            };

            var bannerComponents = new List<ComponentDto> { new ComponentDto { ComponentName = "Banner1" } };

            _cmsClientMock
                .Setup(x => x.Post<GetComponentsResponseDto>("component/get-tenant-components-by-type-name", It.Is<GetTenantComponentByTypeNameRequestDto>(r => r.ComponentTypeName == "Banner")))
                .ReturnsAsync(new GetComponentsResponseDto { Components = bannerComponents });

            _cmsClientMock
                .Setup(x => x.Post<GetComponentsResponseDto>("component/get-tenant-components-by-type-name", It.Is<GetTenantComponentByTypeNameRequestDto>(r => r.ComponentTypeName == "InvalidType")))
                .ReturnsAsync(new GetComponentsResponseDto { ErrorCode = StatusCodes.Status404NotFound, ErrorMessage = "Not found" });

            // Act
            var result = await _cmsController.GetTenantComponentsByTypeNames(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<GetTenantComponentsByTypeNamesResponseDto>(okResult.Value);
            Assert.NotNull(response.ComponentsByType);
            Assert.Equal(2, response.ComponentsByType.Count);
            Assert.True(response.ComponentsByType.ContainsKey("Banner"));
            Assert.True(response.ComponentsByType.ContainsKey("InvalidType"));
            Assert.Single(response.ComponentsByType["Banner"]);
            Assert.Empty(response.ComponentsByType["InvalidType"]);
        }

        [Fact]
        public async Task GetTenantComponentsByTypeNames_ShouldReturnError_WhenServiceReturnsErrorCode()
        {
            // Arrange
            var request = new GetTenantComponentsByTypeNamesRequestDto
            {
                TenantCode = "TENANT1",
                ComponentTypeNames = new List<string> { "Banner" },
                ApplyCohortFilter = false
            };

            var cmsServiceMock = new Mock<ICmsService>();
            cmsServiceMock
                .Setup(x => x.GetTenantComponentsByTypeNames(It.IsAny<GetTenantComponentsByTypeNamesRequestDto>()))
                .ReturnsAsync(new GetTenantComponentsByTypeNamesResponseDto
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = "Internal server error"
                });

            var cmsController = new CmsController(_cmsControllerLogger.Object, cmsServiceMock.Object);

            // Act
            var result = await cmsController.GetTenantComponentsByTypeNames(request);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
            var response = Assert.IsType<GetTenantComponentsByTypeNamesResponseDto>(objectResult.Value);
            Assert.Equal(StatusCodes.Status500InternalServerError, response.ErrorCode);
        }

        [Fact]
        public async Task GetTenantComponentsByTypeNames_ShouldReturnInternalServerError_WhenExceptionThrown()
        {
            // Arrange
            var request = new GetTenantComponentsByTypeNamesRequestDto
            {
                TenantCode = "TENANT1",
                ComponentTypeNames = new List<string> { "Banner" },
                ApplyCohortFilter = false
            };

            var cmsServiceMock = new Mock<ICmsService>();
            cmsServiceMock
                .Setup(x => x.GetTenantComponentsByTypeNames(It.IsAny<GetTenantComponentsByTypeNamesRequestDto>()))
                .ThrowsAsync(new Exception("Simulated exception"));

            var cmsController = new CmsController(_cmsControllerLogger.Object, cmsServiceMock.Object);

            // Act
            var result = await cmsController.GetTenantComponentsByTypeNames(request);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
            var response = Assert.IsType<GetTenantComponentsByTypeNamesResponseDto>(objectResult.Value);
            Assert.Equal(StatusCodes.Status500InternalServerError, response.ErrorCode);
            Assert.Equal("Simulated exception", response.ErrorMessage);
        }

        [Fact]
        public async Task GetTenantComponentsByTypeNames_ShouldApplyCohortFilter_WhenRequested()
        {
            // Arrange
            var request = new GetTenantComponentsByTypeNamesRequestDto
            {
                TenantCode = "TENANT1",
                ComponentTypeNames = new List<string> { "Banner" },
                ApplyCohortFilter = true,
                ConsumerCode = "CONSUMER1"
            };

            var component = new ComponentDto
            {
                MetadataJson = "{\"tags\": [\"cohort:diabetes\"]}"
            };
            var responseDto = new GetComponentsResponseDto
            {
                Components = new List<ComponentDto> { component }
            };

            _cmsClientMock
                .Setup(x => x.Post<GetComponentsResponseDto>("component/get-tenant-components-by-type-name", It.IsAny<GetTenantComponentByTypeNameRequestDto>()))
                .ReturnsAsync(responseDto);

            _cohortConsumerServiceMock
                .Setup(x => x.GetConsumerCohorts(It.IsAny<GetConsumerByCohortsNameRequestDto>(), It.IsAny<string>()))
                .ReturnsAsync(new CohortConsumerResponseDto
                {
                    ConsumerCohorts = new List<CohortConsumersDto>
                    {
                        new CohortConsumersDto { CohortName = "diabetes" }
                    }
                });

            // Act
            var result = await _cmsController.GetTenantComponentsByTypeNames(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<GetTenantComponentsByTypeNamesResponseDto>(okResult.Value);
            Assert.NotNull(response.ComponentsByType);
            Assert.True(response.ComponentsByType.ContainsKey("Banner"));
        }

        [Fact]
        public async Task GetTenantComponentsByTypeNames_ShouldHandleEmptyComponentTypeNames()
        {
            // Arrange
            var request = new GetTenantComponentsByTypeNamesRequestDto
            {
                TenantCode = "TENANT1",
                ComponentTypeNames = new List<string>(),
                ApplyCohortFilter = false
            };

            // Act
            var result = await _cmsController.GetTenantComponentsByTypeNames(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<GetTenantComponentsByTypeNamesResponseDto>(okResult.Value);
            Assert.NotNull(response.ComponentsByType);
            Assert.Empty(response.ComponentsByType);
        }

    }
}

