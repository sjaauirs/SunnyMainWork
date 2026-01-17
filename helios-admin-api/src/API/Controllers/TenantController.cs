using Microsoft.AspNetCore.Mvc;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Api.Controllers
{
    [Route("api/v1/tenant")]
    [ApiController]
    public class TenantController : ControllerBase
    {
        private readonly ILogger<TenantController> _tenantLogger;
        private readonly ITenantService _tenantService;
        private const string className = nameof(TenantController);

        public TenantController(ILogger<TenantController> tenantLogger,
            ITenantService tenantService)
        {
            _tenantLogger = tenantLogger;
            _tenantService = tenantService;
        }

        /// <summary>
        /// Creates the tenant.
        /// </summary>
        /// <param name="createTenantRequestDto">The create tenant request dto.</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CreateTenant([FromBody] CreateTenantRequestDto createTenantRequestDto)
        {
            const string methodName = nameof(CreateTenant);
            try
            {
                _tenantLogger.LogInformation("{ClassName}.{MethodName}: Request started with TenantCode: {TenantCode}", className, methodName, createTenantRequestDto.Tenant.TenantCode);

                var response = await _tenantService.CreateTenant(createTenantRequestDto);

                if (response.ErrorCode != null)
                {
                    _tenantLogger.LogError("{ClassName}.{MethodName}: Error occurred while creating tenant. Request: {RequestData}, Response: {ResponseData}, ErrorCode: {ErrorCode}", className, methodName, createTenantRequestDto.ToJson(), response.ToJson(), response.ErrorCode);
                    return StatusCode((int)response.ErrorCode, response);
                }

                _tenantLogger.LogInformation("{ClassName}.{MethodName}: Tenant created successful, TenantCode: {TenantCode}", className, methodName, createTenantRequestDto.Tenant.TenantCode);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _tenantLogger.LogError(ex, "{ClassName}.{MethodName}: An error occurred while create tenant. Error Message: {ErrorMessage}, ErrorCode: {ErrorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError });
            }
        }

        /// <summary>
        /// Retrieves consumer and person details based on the tenant code provided.
        /// </summary>
        /// <param name="consumerByTenantRequestDto">Contains tenant code, search term, and pagination parameters.</param>
        /// <returns>A paginated list of consumer and person details that match the search criteria.</returns>
        /// <remarks>This method performs optional search, and pagination.</remarks>
        [HttpPost("consumers")]
        public async Task<IActionResult> GetAllConsumers([FromBody] GetConsumerByTenantRequestDto consumerByTenantRequestDto)
        {
            const string methodName = nameof(GetAllConsumers);
            try
            {
                _tenantLogger.LogInformation("{className}.{methodName}: API - Started with TenantCode : {TenantCode}", className, methodName, consumerByTenantRequestDto.TenantCode);

                var response = await _tenantService.GetConsumersByTenantCode(consumerByTenantRequestDto);

                if (response.ErrorCode != null)
                {
                    _tenantLogger.LogError("{ClassName}.{MethodName}: Error occurred while creating tenant. Request: {RequestData}, Response: {ResponseData}, ErrorCode: {ErrorCode}", className, methodName, consumerByTenantRequestDto.ToJson(), response.ToJson(), response.ErrorCode);
                    return StatusCode((int)response.ErrorCode, response);
                }

                _tenantLogger.LogInformation("{ClassName}.{MethodName}: Tenant created successful, TenantCode: {TenantCode}", className, methodName, consumerByTenantRequestDto.TenantCode);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _tenantLogger.LogError(ex, "{ClassName}.{MethodName}: An error occurred while create tenant. Error Message: {ErrorMessage}, ErrorCode: {ErrorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError });
            }
        }
        [HttpPut("{tenantCode}")]
        public async Task<ActionResult<UpdateTenantResponseDto>> UpdateTenant(string tenantCode, [FromBody] UpdateTenantDto updateTenant)
        {
            const string methodName = nameof(UpdateTenant);
            try
            {
                _tenantLogger.LogInformation("{ClassName}.{MethodName}: Request started with TenantCode: {TenantCode}", className, methodName, tenantCode);

                var response = await _tenantService.UpdateTenant(tenantCode, updateTenant);

                if (response.ErrorCode != null)
                {
                    _tenantLogger.LogError("{ClassName}.{MethodName}: Error occurred while Updating tenant. Request: {RequestData}, Response: {ResponseData}, ErrorCode: {ErrorCode}", className, methodName, updateTenant.ToJson(), response.ToJson(), response.ErrorCode);
                    return StatusCode((int)response.ErrorCode, response);
                }

                _tenantLogger.LogInformation("{ClassName}.{MethodName}: Tenant Updated successfully, TenantCode: {TenantCode}", className, methodName, updateTenant.TenantCode);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _tenantLogger.LogError(ex, "{ClassName}.{MethodName}: An error occurred while create tenant. Error Message: {ErrorMessage}, ErrorCode: {ErrorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, new UpdateTenantResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError, ErrorMessage = ex.Message });
            }
        }
        [HttpGet("tenants")]
        public async Task<ActionResult<TenantsResponseDto>> GetTenants()
        {
            const string methodName = nameof(GetTenants);
            try
            {
                _tenantLogger.LogInformation("{ClassName}.{MethodName}: API Started fetching all Tenants", className, methodName);
                var response = await _tenantService.GetTenants();
                if (response.ErrorCode != null)
                {
                    var errorCode = response.ErrorCode;
                    _tenantLogger.LogError("{className}.{methodName}: API - Error occurred while processing all Tenants, Error Code: {ErrorCode}, Error Msg: {msg}", className, methodName, response.ErrorCode, response.ErrorMessage);
                    return StatusCode((int)errorCode, response);
                }
                _tenantLogger.LogInformation("{className}.{methodName}: API - Successfully fetched all Tenants", className, methodName);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _tenantLogger.LogError(ex, "{className}.{methodName}: ERROR Msg:{msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, new TenantsResponseDto() { ErrorMessage = ex.Message });
            }
        }

        /// <summary>
        /// Gets the by tenant code.
        /// </summary>
        /// <param name="tenantCode">The tenant code.</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<TenantResponseDto>> GetByTenantCode([FromQuery] string tenantCode)
        {
            const string methodName = nameof(GetByTenantCode);
            try
            {
                _tenantLogger.LogInformation("GetByTenantCode API - Started with Request : {TenantCode}", tenantCode);
                var response = await _tenantService.GetTenantDetails(tenantCode);

                if (response.ErrorCode != null)
                {
                    _tenantLogger.LogError("{ClassName}.{MethodName}: Error occurred while fetching tenant details. Request: {RequestData}, Response: {ResponseData},  ErrorCode: {ErrorCode}", className, methodName, Request.ToJson(), response.ToJson(), response.ErrorCode);
                    return StatusCode((int)response.ErrorCode, response);
                }
                _tenantLogger.LogInformation("{ClassName}.{MethodName}: Successfully fetched tenant details.", className, methodName);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _tenantLogger.LogError(ex, "{ClassName}.{MethodName}: An unexpected error occurred while fetching tenant data. Error Message: {ErrorMessage}", className, methodName, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new TenantResponseDto
                    {
                        ErrorCode = StatusCodes.Status500InternalServerError,
                        ErrorMessage = "An unexpected error occurred while retrieving tenant data. Please try again later."
                    });
            }
        }
    }
}
