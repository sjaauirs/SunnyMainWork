using Microsoft.AspNetCore.Mvc;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Api.Controllers
{
    [Route("api/v1/tenant-task-category")]
    [ApiController]
    public class TenantTaskCategoryController : ControllerBase
    {
        private readonly ILogger<TenantTaskCategoryController> _tenantTaskLogger;
        private readonly ITenantTaskCategoryService _tenantTaskService;
        private const string className = nameof(TenantTaskCategoryController);

        public TenantTaskCategoryController(ILogger<TenantTaskCategoryController> tenantTaskLogger, ITenantTaskCategoryService tenantTaskService)
        {
            _tenantTaskLogger = tenantTaskLogger;
            _tenantTaskService = tenantTaskService;
        }
        [HttpPost]
        public async Task<IActionResult> CreateTenantTaskCategory(TenantTaskCategoryRequestDto requestDto)
        {
            const string methodName = nameof(CreateTenantTaskCategory);
            try
            {
                _tenantTaskLogger.LogInformation("{ClassName}.{MethodName}: Request started with TenantCode: {TenantCode}", className, methodName, requestDto.TenantCode);
                var response = await _tenantTaskService.CreateTenantTaskCategory(requestDto);

                if (response.ErrorCode != null)
                {
                    _tenantTaskLogger.LogError("{ClassName}.{MethodName}: Error occurred while creating TenantTaskCategory. Request: {RequestData}, Response: {ResponseData}, ErrorCode: {ErrorCode}", className, methodName, requestDto.ToJson(), response.ToJson(), response.ErrorCode);
                    return StatusCode((int)response.ErrorCode, response);
                }

                _tenantTaskLogger.LogInformation("{ClassName}.{MethodName}: TenantTaskCategory created Successful, with TenantCode: {TenantCode}", className, methodName, requestDto.TenantCode);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _tenantTaskLogger.LogError(ex, "{ClassName}.{MethodName}: An error occurred while create TenantTaskCategory. Error Message: {ErrorMessage}, ErrorCode: {ErrorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError });
            }
        }
    }
}
