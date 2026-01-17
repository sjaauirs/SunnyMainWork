using Microsoft.AspNetCore.Mvc;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;
using SunnyRewards.Helios.Tenant.Infrastructure.Services.Interfaces;

namespace SunnyRewards.Helios.Tenant.Api.Controllers
{
    [Route("api/v1/tenant/")]
    [ApiController]
    public class TenantController : ControllerBase
    {
        private readonly ILogger<TenantController> _tenantLogger;
        private readonly ITenantService _tenantService;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tenantLogger"></param>
        /// <param name="tenantService"></param>
        public TenantController(ILogger<TenantController> tenantLogger,
            ITenantService tenantService)
        {
            _tenantLogger = tenantLogger;
            _tenantService = tenantService;
        }
        const string className = nameof(TenantController);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tenantByPartnerCodeRequestDto"></param>
        /// <returns></returns>
        [HttpPost("get-by-partner-code")]
        public async Task<ActionResult<GetTenantByPartnerCodeResponseDto>> GetByPartnerCode([FromBody] GetTenantByPartnerCodeRequestDto tenantByPartnerCodeRequestDto)
        {
            const string methodName = nameof(GetByPartnerCode);
            try
            {
                _tenantLogger.LogInformation("{ClassName}.{methodName} API: - Started with Request :\n{PartnerCode}", className, methodName, tenantByPartnerCodeRequestDto.PartnerCode);
                var response = await _tenantService.GetTenantByPartnerCode(tenantByPartnerCodeRequestDto);

                return response.Tenant != null ? Ok(response) : NotFound();
            }
            catch (Exception ex)
            {
                _tenantLogger.LogError(ex, "{className}.{methodName}: ERROR Msg:{msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return new GetTenantByPartnerCodeResponseDto();
            }
        }

        /// <summary>
        /// Get Tenant by enc_key_id
        /// </summary>
        /// <param name="GetTenantByEncKeyIdRequestDto"></param>
        /// <returns></returns>
        [HttpPost("get-by-enc-key-id")]
        public async Task<ActionResult<GetTenantByEncKeyIdResponseDto>> GetTenantByEncKeyId([FromBody] GetTenantByEncKeyIdRequestDto getTenantByEncKeyIdRequestDto)
        {
            const string methodName = nameof(GetTenantByEncKeyId);
            try
            {
                _tenantLogger.LogInformation("{className}.{methodName}: API - Started with Request :\n{EncryptionKeyId}", className, methodName, getTenantByEncKeyIdRequestDto.EncKeyId);
                var response = await _tenantService.GetTenantByEncKeyId(getTenantByEncKeyIdRequestDto);

                if (response.ErrorCode != null)
                {
                    var errorCode = response.ErrorCode;
                    _tenantLogger.LogError("{className}.{methodName}: API - ERROR: {ErrorCode}, Error Msg:{msg}", className, methodName, response.ErrorCode, response.ErrorMessage);
                    return StatusCode((int)errorCode, response);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _tenantLogger.LogError(ex, "{className}.{methodName}: ERROR Msg:{msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, new GetTenantByEncKeyIdResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="getTenantCodeRequestDto"></param>
        /// <returns></returns>
        [HttpPost("get-by-tenant-code")]
        public async Task<ActionResult<TenantDto>> GetByTenantCode([FromBody] GetTenantCodeRequestDto getTenantCodeRequestDto)
        {
            const string methodName = nameof(GetByTenantCode);
            try
            {
                _tenantLogger.LogInformation("{className}.{methodName}: API - Started with Request :\n{TenantCode}", className, methodName, getTenantCodeRequestDto.TenantCode);
                var response = await _tenantService.GetByTenantCode(getTenantCodeRequestDto);

                return response.TenantCode != null ? Ok(response) : NotFound();
            }
            catch (Exception ex)
            {
                _tenantLogger.LogError(ex, "{className}.{methodName}: ERROR Msg:{msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return new TenantDto();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="apiKey"></param>
        /// <returns></returns>
        [HttpPost("validate-api-key")]
        public async Task<ActionResult<bool>> ValidateApiKey([FromBody] string apiKey)
        {
            const string methodName = nameof(ValidateApiKey);
            try
            {
                _tenantLogger.LogInformation("{className}.{methodName}: API - Started with Request :\n{apiKey}", className, methodName, apiKey);
                var response = await _tenantService.ValidateApiKey(apiKey);

                return response;
            }
            catch (Exception ex)
            {
                _tenantLogger.LogError(ex, "{className}.{methodName}: ERROR Msg:{msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return false;
            }
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

        [HttpGet("get-tenants")]
        public async Task<ActionResult<TenantsResponseDto>> GetAllTenants()
        {
            const string methodName = nameof(GetAllTenants);
            try
            {
                _tenantLogger.LogInformation("{ClassName}.{MethodName}: API Started fetching all Tenants", className, methodName);
                var response = await _tenantService.GetAllTenants();
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
                    _tenantLogger.LogError("{ClassName}.{MethodName}: Error occurred while fetching tenant details. Request: {RequestData}, Response: {ResponseData},  ErrorCode: {ErrorCode}", className, methodName, tenantCode, response.ToJson(), response.ErrorCode);
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
