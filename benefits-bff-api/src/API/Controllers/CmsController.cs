using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sunny.Benefits.Bff.Api.Filters;
using Sunny.Benefits.Bff.Core.Domain.Dtos;
using Sunny.Benefits.Bff.Infrastructure.Services.Interfaces;
using Sunny.Benefits.Cms.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;

namespace Sunny.Benefits.Bff.Api.Controllers
{
    [Route("/api/v1/")]
    [ApiController]
    [Authorize]
    [ServiceFilter(typeof(ValidateLanguageCodeAttribute))]

    public class CmsController : ControllerBase
    {
        private readonly ILogger<CmsController> _cmsLogger;
        private readonly ICmsService _cmsService;
        private const string className=nameof(CmsController);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cmsLogger"></param>
        /// <param name="cmsService"></param>
        public CmsController(ILogger<CmsController> cmsLogger, ICmsService cmsService)
        {
            _cmsLogger = cmsLogger;
            _cmsService = cmsService;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="getComponentListRequestDto"></param>
        /// <returns></returns>
        [HttpPost("component-list")]
        public async Task<ActionResult<GetComponentListResponseDto>> GetComponentList(GetComponentListRequestDto getComponentListRequestDto)
        {
            const string methodName = nameof(GetComponentList);
            // Generate unique request ID for tracing this request through all logs
            var requestId = Guid.NewGuid().ToString("N")[..16];
            var requestStopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            try
            {
                _cmsLogger.LogInformation("{ClassName}.{MethodName} - Started for TenantCode: {TenantCode}, RequestId: {RequestId}", 
                    className, methodName, getComponentListRequestDto.TenantCode, requestId);
                var response = await _cmsService.GetCmsComponentList(getComponentListRequestDto, requestId);
                requestStopwatch.Stop();
                
                // Comprehensive timing summary - RequestId is included as structured property for easy filtering
                _cmsLogger.LogInformation(
                    "⏱️ OVERALL TIMING SUMMARY - TotalTime: {TotalMs}ms, TenantCode: {TenantCode}, ComponentName: {ComponentName}, ComponentsReturned: {Count}, RequestId: {RequestId}",
                    requestStopwatch.ElapsedMilliseconds, getComponentListRequestDto.TenantCode, 
                    getComponentListRequestDto.ComponentName, response?.Components?.Count ?? 0, requestId);
                
                _cmsLogger.LogInformation("{ClassName}.{MethodName} - Completed for TenantCode: {TenantCode}, TotalTime: {TotalMs}ms, RequestId: {RequestId}",
                    className, methodName, getComponentListRequestDto.TenantCode, requestStopwatch.ElapsedMilliseconds, requestId);
                return response != null && response.Components != null ? Ok(response) : NotFound();
            }
            catch (Exception ex)
            {
                requestStopwatch.Stop();
                _cmsLogger.LogError(ex, "{ClassName}.{MethodName} - Error for TenantCode: {TenantCode}, ErrorCode:{ErrorCode}, ERROR:{ErrorMessage}, TotalTime: {TotalMs}ms, RequestId: {RequestId}", 
                    className, methodName, getComponentListRequestDto.TenantCode, StatusCodes.Status500InternalServerError, ex.Message, requestStopwatch.ElapsedMilliseconds, requestId);
                return new GetComponentListResponseDto();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="findStoreRequestDTO"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("find-store")]
        public async Task<ActionResult<List<StoreResponseMockDTO>>> FindStoreMock(FindStoreRequestDTO findStoreRequestDTO)
        {
            const string methodName = nameof(FindStoreMock);
            try
            {
                _cmsLogger.LogInformation("{ClassName}.{MethodName} - Started processing Find StoreMock with Latitude :{Latitude},Longitude :{Longitude}", 
                    className,methodName,findStoreRequestDTO.coords.Latitude, findStoreRequestDTO.coords.Longitude);
                var response = await _cmsService.FindStoreMock(findStoreRequestDTO);
                return response;
            }
            catch (Exception ex)
            {
                _cmsLogger.LogError(ex, "{ClassName}.{MethodName} - Error occured while Finding StoreMock, ErrorCode:{ErrorCode}, ERROR: {ErrorMessage}",
                    className, methodName, StatusCodes.Status500InternalServerError, ex.Message);
                return new List<StoreResponseMockDTO>();
            }
            finally
            {
                _cmsLogger.LogInformation("{ClassName}.{MethodName} - Ended with  Latitude :{Latitude},Longitude :{Longitude}", 
                    className, methodName, findStoreRequestDTO.coords.Latitude, findStoreRequestDTO.coords.Longitude);
            }
        }
        [HttpPost("faq")]
        public async Task<ActionResult<FaqSectionResponseDto>> GetFAQList(FaqRetriveRequestDto faqRetriveRequestDto)
        {
            const string methodName = nameof(GetFAQList);
            try
            {
                _cmsLogger.LogInformation("{ClassName}.{MethodName} - Started processing Get FAQList with TenantCode: {TenantCode}", 
                    className,methodName ,faqRetriveRequestDto.TenantCode);

                var response = await _cmsService.GetFaqSection(faqRetriveRequestDto);
                return response != null &&
                                       (
                                           !string.IsNullOrEmpty(response.HtmlContentUrl) ||
                                           (response.FaqSections != null && response.FaqSections.Count > 0)
                                       )
                                       ? Ok(response)
                                       : NotFound();
            }
            catch (Exception ex)
            {
                _cmsLogger.LogError(ex, "{ClassName}.{MethodName} - Error occured while retreiving FAQList for TenantCode: {TenantCode}," +
                    "ErrorCode:{ErrorCode}, ERROR: {ErrorMessage}", className,methodName ,faqRetriveRequestDto.TenantCode, StatusCodes.Status500InternalServerError, ex.Message);
                return new FaqSectionResponseDto
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = ex.Message,
                };

            }
            finally
            {
                _cmsLogger.LogInformation("{ClassName}.{MethodName} - Ended with TenantCode: {TenantCode}", className, methodName, faqRetriveRequestDto.TenantCode);
            }
        }

        /// <summary>
        /// Gets the list of components for a tenant based on component type name and language code
        /// </summary>
        /// <param name="request">Request containing tenant code and list of component codes.</param>
        /// <returns>List of Components.</returns>
        [HttpPost("get-tenant-components-by-type-name")]
        public async Task<IActionResult> GetTenantComponentsByTypeName([FromBody] GetTenantComponentByTypeNameRequestDto request)
        {
            const string methodName = nameof(GetTenantComponentsByTypeName);
            try
            {

                _cmsLogger.LogInformation("{ClassName}.{MethodName}: Get Components request started with TenantCode: {TenantCode}, ComponentTypeName: {ComponentTypeName}",
                   className, methodName, request.TenantCode, request.ComponentTypeName);


                var response = await _cmsService.GetTenantComponentsByTypeName(request);

                if (response.ErrorCode != null)
                {
                    _cmsLogger.LogError(
                        "{ClassName}.{MethodName}: Error occurred while fetching components. Request: {RequestJson}, Response: {ResponseJson}, ErrorCode: {ErrorCode}",
                        className, methodName, request.ToJson(), response.ToJson(), response.ErrorCode
                    );
                    return StatusCode((int)response.ErrorCode, response);
                }

                _cmsLogger.LogInformation(
                    "{ClassName}.{MethodName}: Successfully fetched {ComponentCount} components for TenantCode: {TenantCode}, ComponentName: {ComponentTypeName}",
                    className, methodName, response.Components?.Count, request.TenantCode, request.ComponentTypeName
                );

                return Ok(response);
            }
            catch (Exception ex)
            {
                _cmsLogger.LogError(
                    ex,
                    "{ClassName}.{MethodName}: An error occurred while fetching components. ErrorMessage: {ErrorMessage}, ErrorCode: {ErrorCode}",
                    className, methodName, ex.Message, StatusCodes.Status500InternalServerError
                );

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new GetComponentsResponseDto { ErrorCode = StatusCodes.Status500InternalServerError }
                );
            }
        }

        /// <summary>
        /// Gets multiple lists of components based on tenant code and list of component type names
        /// </summary>
        /// <param name="request">Request containing tenant code and list of component type names</param>
        /// <returns>Dictionary of component type names to their component lists</returns>
        [HttpPost("get-tenant-components-by-type-names")]
        public async Task<IActionResult> GetTenantComponentsByTypeNames([FromBody] GetTenantComponentsByTypeNamesRequestDto request)
        {
            const string methodName = nameof(GetTenantComponentsByTypeNames);
            try
            {
                _cmsLogger.LogInformation(
                    "{ClassName}.{MethodName}: Get Components request started with TenantCode: {TenantCode}, ComponentTypeNames: {ComponentTypeNames}",
                    className, methodName, request.TenantCode, string.Join(", ", request.ComponentTypeNames));

                var response = await _cmsService.GetTenantComponentsByTypeNames(request);

                if (response.ErrorCode != null)
                {
                    _cmsLogger.LogError(
                        "{ClassName}.{MethodName}: Error occurred while fetching components. Request: {RequestJson}, Response: {ResponseJson}, ErrorCode: {ErrorCode}",
                        className, methodName, request.ToJson(), response.ToJson(), response.ErrorCode);
                    return StatusCode((int)response.ErrorCode, response);
                }

                var totalComponents = response.ComponentsByType.Values.Sum(components => components?.Count ?? 0);
                _cmsLogger.LogInformation(
                    "{ClassName}.{MethodName}: Successfully fetched components for TenantCode: {TenantCode}, ComponentTypeNames: {ComponentTypeNames}, TotalComponents: {TotalComponents}, TypesProcessed: {TypeCount}",
                    className, methodName, request.TenantCode, string.Join(", ", request.ComponentTypeNames), totalComponents, response.ComponentsByType.Count);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _cmsLogger.LogError(
                    ex,
                    "{ClassName}.{MethodName}: An error occurred while fetching components. ErrorMessage: {ErrorMessage}, ErrorCode: {ErrorCode}",
                    className, methodName, ex.Message, StatusCodes.Status500InternalServerError);

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new GetTenantComponentsByTypeNamesResponseDto { ErrorCode = StatusCodes.Status500InternalServerError, ErrorMessage = ex.Message }
                );
            }
        }

        [HttpPost("get-component")]
        public async Task<IActionResult> GetComponent([FromBody] GetComponentRequestDto getComponentRequestDto)
        {
            const string methodName = nameof(GetComponent);
            try
            {
                _cmsLogger.LogInformation("{ClassName}.{MethodName}: Get CMS export request started with TenantCode: {TenantCode}",
                    className, methodName, getComponentRequestDto.TenantCode);

                var response = await _cmsService.GetComponent(getComponentRequestDto);

                if (response.ErrorCode != null)
                {
                    _cmsLogger.LogError("{ClassName}.{MethodName}: Error occurred while fetching component. Request: {RequestData}, Response: {ResponseData}, ErrorCode: {ErrorCode}",
                        className, methodName, getComponentRequestDto.ToJson(), response.ToJson(), response.ErrorCode);
                    return StatusCode((int)response.ErrorCode, response);
                }

                _cmsLogger.LogInformation("{ClassName}.{MethodName}: Successful fetched the component for TenantCode: {TenantCode}",
                    className, methodName, getComponentRequestDto.TenantCode);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _cmsLogger.LogError(ex, "{ClassName}.{MethodName}: An error occurred while fetching component. ErrorMessage: {ErrorMessage}, ErrorCode: {ErrorCode}",
                    className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, new ExportCmsResponseDto { ErrorCode = StatusCodes.Status500InternalServerError });
            }
        }
    }
}
