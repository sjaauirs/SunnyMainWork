using Google.Api;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Infrastructure.Services;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using System.Net.Http;

namespace SunnyRewards.Helios.Admin.Api.Controllers
{
    [Route("api/v1/")]
    [ApiController]
    public class TenantTaskRewardScriptController : ControllerBase
    {
        private readonly ILogger<TenantTaskRewardScriptController> _logger;
        private readonly ITenantTaskRewardScriptService _tenantTaskRewardScriptService;

        private const string className = nameof(TenantTaskRewardScriptController);
        public TenantTaskRewardScriptController(ILogger<TenantTaskRewardScriptController> logger, ITenantTaskRewardScriptService tenantTaskRewardScriptService)
        {
            _logger = logger;
            _tenantTaskRewardScriptService = tenantTaskRewardScriptService;
        }
        [HttpPost("tenant-task-reward-script")]
        public async Task<ActionResult<BaseResponseDto>> CreateTenantTaskRewardScript(TenantTaskRewardScriptRequestDto requestDto)
        {
            const string methodName = nameof(CreateTenantTaskRewardScript);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName}: Request started with TenantCode: {TenantCode}", className, methodName, requestDto.TenantCode);
                var response = await _tenantTaskRewardScriptService.PostTenantTaskRewardScriptRequest(requestDto);

                if (response.ErrorCode != null)
                {
                    _logger.LogError("{ClassName}.{MethodName}: Error occurred while creating TenantTaskRewardScript. Request: {RequestData}, Response: {ResponseData}, ErrorCode: {ErrorCode}", className, methodName, requestDto.ToJson(), response.ToJson(), response.ErrorCode);
                    return StatusCode((int)response.ErrorCode, response);
                }

                _logger.LogInformation("{ClassName}.{MethodName}: TenantTaskRewardScript created Successful, with TenantCode: {TenantCode}", className, methodName, requestDto.TenantCode);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: An error occurred while create TenantTaskRewardScript. Error Message: {ErrorMessage}, ErrorCode: {ErrorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = "TenantTaskRewardScript Not Created" });
                }
        }
        [HttpPut(("tenant-task-reward-script"))]
        public async Task<ActionResult<BaseResponseDto>> UpdateTenantTaskRewardScript(UpdateTenantTaskRewardScriptRequestDto requestDto)
        {
            const string methodName = nameof(UpdateTenantTaskRewardScript);
            try
            {

                _logger.LogInformation("{ClassName}.{MethodName}: Request started with TenantCode: {TenantCode}", className, methodName, requestDto.TenantCode);
                var response = await _tenantTaskRewardScriptService.UpdateTenantTaskRewardScriptRequest(requestDto);

                if (response.ErrorCode != null)
                {
                    _logger.LogError("{ClassName}.{MethodName}: Error occurred while updating TenantTaskRewardScript. Request: {RequestData}, Response: {ResponseData}, ErrorCode: {ErrorCode}", className, methodName, requestDto.ToJson(), response.ToJson(), response.ErrorCode);
                    return StatusCode((int)response.ErrorCode, response);
                }

                _logger.LogInformation("{ClassName}.{MethodName}: TenantTaskRewardScript updated Successful, with TenantCode: {TenantCode}", className, methodName, requestDto.TenantCode);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: An error occurred while updating TenantTaskRewardScript. Error Message: {ErrorMessage}, ErrorCode: {ErrorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError, ErrorMessage = "TenantTaskRewardScript Not Updated" });
            }
        }
    }
}
