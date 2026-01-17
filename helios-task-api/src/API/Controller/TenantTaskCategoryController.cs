using Microsoft.AspNetCore.Mvc;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Infrastructure.Services.Interface;

namespace SunnyRewards.Helios.Task.Api.Controller
{
    [Route("api/v1/")]
    [ApiController]
    public class TenantTaskCategoryController : ControllerBase
    {
        private readonly ILogger<TenantTaskCategoryController> _logger;
        private readonly ITenantTaskCategoryService _tenantTaskCategoryService;
        public const string className = nameof(TenantTaskCategoryController);

        public TenantTaskCategoryController(ILogger<TenantTaskCategoryController> logger, ITenantTaskCategoryService tenantTaskCategoryService)
        {
            _logger = logger;
            _tenantTaskCategoryService = tenantTaskCategoryService;
        }
        [HttpPost("tenant-task-category")]
        public async Task<IActionResult> CreateTenantTaskCategory([FromBody] TenantTaskCategoryRequestDto requestDto)
        {
            const string methodName = nameof(CreateTenantTaskCategory);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName}: Request started with Tenant Code: {TenantCode}", className, methodName, requestDto.TenantCode);
                var response = await _tenantTaskCategoryService.CreateTenantTaskCategory(requestDto);
                if (response.ErrorCode != null)
                {
                    _logger.LogError("{ClassName}.{MethodName}: Error occurred during Tenant Task Category Create. Request: {RequestData}, Response: {ResponseData}, ErrorCode: {ErrorCode}", className, methodName, requestDto.ToJson(), response.ToJson(), response.ErrorCode);
                    return StatusCode((int)response.ErrorCode, response);
                }

                _logger.LogInformation("{ClassName}.{MethodName}: Tenant Task Category Create successful for Tenant Code: {TenantCode}", className, methodName, requestDto.TenantCode);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: An error occurred during Tenant Task Category import. Error Message: {ErrorMessage}, ErrorCode: {ErrorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError });
            }
        }

        /// <summary>
        /// Updates an existing TenantTaskCategory based on the provided request data.
        /// </summary>
        /// <param name="requestDto">The request data containing the details to update.</param>
        /// <returns>A response DTO indicating success or failure.</returns>
        [HttpPut("tenant-task-category/{tenantTaskCategoryId}")]
        public async Task<IActionResult> UpdateTenantTaskCategory(long tenantTaskCategoryId, [FromBody] TenantTaskCategoryDto requestDto)
        {
            const string methodName = nameof(UpdateTenantTaskCategory);

            if (tenantTaskCategoryId != requestDto.TenantTaskCategoryId)
            {
                _logger.LogInformation("{ClassName}.{MethodName}: Mismatch between TenantTaskCategoryId in query parameter and object.", className, methodName);
                
                return StatusCode(StatusCodes.Status400BadRequest, "Mismatch between TenantTaskCategoryId in query parameter and object.");
            }

            var response = await _tenantTaskCategoryService.UpdateTenantTaskCategory(requestDto);
            if (response.ErrorCode != null)
            {
                return StatusCode((int)response.ErrorCode, response);
            }

            return Ok(response);
        }
    }
}
