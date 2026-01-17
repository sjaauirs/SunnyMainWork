using Microsoft.AspNetCore.Mvc;
using Sunny.Benefits.Cms.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;

namespace SunnyRewards.Helios.Admin.Api.Controllers
{
    [Route("api/v1/component")]
    [ApiController]
    public class ComponentController : ControllerBase
    {
        private readonly ILogger<ComponentController> _componentControllerLogger;
        private readonly IComponentService _componentService;
        private const string className = nameof(ComponentController);

        public ComponentController(ILogger<ComponentController> componentControllerLogger, IComponentService componentService)
        {
            _componentControllerLogger = componentControllerLogger;
            _componentService = componentService;
        }
        /// <summary>
        /// end point to create new component
        /// </summary>
        /// <param name="requestDto">request contains data to create new component</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CreateComponent(ComponentRequestDto requestDto)
        {
            const string methodName = nameof(CreateComponent);
            try
            {
                _componentControllerLogger.LogInformation("{ClassName}.{MethodName}: Request started with Tenant Code:{TenantCode}", className, methodName, requestDto.TenantCode);
                var response = await _componentService.CreateComponent(requestDto);

                if (response.ErrorCode != null)
                {
                    _componentControllerLogger.LogError("{ClassName}.{MethodName}: Error occurred while creating Component. Request: {RequestData}, Response: {ResponseData}, ErrorCode: {ErrorCode}", className, methodName, requestDto.ToJson(), response.ToJson(), response.ErrorCode);
                    return StatusCode((int)response.ErrorCode, response);
                }

                _componentControllerLogger.LogInformation("{ClassName}.{MethodName}: Component created Successful, with TenantCode: {TenantCode}", className, methodName, requestDto.TenantCode);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _componentControllerLogger.LogError(ex, "{ClassName}.{MethodName}: An error occurred while create Component. Error Message: {ErrorMessage}, ErrorCode: {ErrorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError });
            }
        }
        /// <summary>
        /// Retrieves all the component Types
        /// </summary>
        /// <returns>List of All componentTypes</returns>
        [HttpGet("component-types")]
        public async Task<IActionResult> GetAllComponentTypes()
        {
            const string methodName = nameof(GetAllComponentTypes);
            try
            {
                _componentControllerLogger.LogInformation("{ClassName}.{MethodName}: Request started.", className, methodName);
                var response = await _componentService.GetAllComponentTypes();

                if (response.ErrorCode != null)
                {
                    _componentControllerLogger.LogError("{ClassName}.{MethodName}: Error occurred while fetching ComponentTypes. Response: {ResponseData}, ErrorCode: {ErrorCode}", className, methodName, response.ToJson(), response.ErrorCode);
                    return StatusCode((int)response.ErrorCode, response);
                }

                _componentControllerLogger.LogInformation("{ClassName}.{MethodName}: ComponentTypes fetched Successfully", className, methodName);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _componentControllerLogger.LogError(ex, "{ClassName}.{MethodName}: An error occurred while fetching ComponentTypes. Error Message: {ErrorMessage}, ErrorCode: {ErrorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, new GetAllComponentTypesResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError });
            }
        }
        /// <summary>
        ///  Fetch all the Components which are specific to the given request
        /// </summary>
        /// <param name="requestDto">request contains tenant code</param>
        /// <returns></returns>
        [HttpPost("get-all-components")]
        public async Task<IActionResult> GetAllComponents(GetAllComponentsRequestDto requestDto)
        {
            const string methodName = nameof(GetAllComponents);
            try
            {
                _componentControllerLogger.LogInformation("{ClassName}.{MethodName}: Request started with Tenant Code:{TenantCode}", className, methodName, requestDto.TenantCode);
                var response = await _componentService.GetAllComponents(requestDto);

                if (response.ErrorCode != null)
                {
                    _componentControllerLogger.LogError("{ClassName}.{MethodName}: Error occurred while fetching Components. Request: {RequestData}, Response: {ResponseData}, ErrorCode: {ErrorCode}", className, methodName, requestDto.ToJson(), response.ToJson(), response.ErrorCode);
                    return StatusCode((int)response.ErrorCode, response);
                }

                _componentControllerLogger.LogInformation("{ClassName}.{MethodName}: Components Retrieved Successfully, with TenantCode: {TenantCode}", className, methodName, requestDto.TenantCode);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _componentControllerLogger.LogError(ex, "{ClassName}.{MethodName}: An error occurred while fetching Components. Error Message: {ErrorMessage}, ErrorCode: {ErrorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, new GetAllComponentsResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError });
            }
        }
        /// <summary>
        /// Updates the component
        /// </summary>
        /// <param name="requestDto">request contains data to update component</param>
        /// <returns>Updated Component</returns>
        [HttpPut]
        public async Task<IActionResult> UpdateComponent(ComponentRequestDto requestDto)
        {
            const string methodName = nameof(UpdateComponent);
            try
            {
                _componentControllerLogger.LogInformation("{ClassName}.{MethodName}: Request started with Tenant Code:{TenantCode}", className, methodName, requestDto.TenantCode);
                var response = await _componentService.UpdateComponent(requestDto);

                if (response.ErrorCode != null)
                {
                    _componentControllerLogger.LogError("{ClassName}.{MethodName}: Error occurred while Updating Component. Request: {RequestData}, Response: {ResponseData}, ErrorCode: {ErrorCode}", className, methodName, requestDto.ToJson(), response.ToJson(), response.ErrorCode);
                    return StatusCode((int)response.ErrorCode, response);
                }

                _componentControllerLogger.LogInformation("{ClassName}.{MethodName}: Component Updated Successful, with TenantCode: {TenantCode}", className, methodName, requestDto.TenantCode);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _componentControllerLogger.LogError(ex, "{ClassName}.{MethodName}: An error occurred while update Component. Error Message: {ErrorMessage}, ErrorCode: {ErrorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, new UpdateComponentResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError });
            }
        }
    }
}
