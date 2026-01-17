using Microsoft.AspNetCore.Mvc;
using SunnyBenefits.Fis.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;

namespace SunnyRewards.Helios.Admin.Api.Controllers
{
    [Route("api/v1/")]
    [ApiController]
    public class TenantAccountController : ControllerBase
    {
        private readonly ILogger<TenantAccountController> _logger;
        private readonly ITenantAccountService _tenantAccountService;
        private const string className = nameof(TenantAccountController);

        public TenantAccountController(ILogger<TenantAccountController> logger, ITenantAccountService tenantAccountService)
        {
            _logger = logger;
            _tenantAccountService = tenantAccountService;
        }
        /// <summary>
        ///  Creates tenant account if not exists
        /// </summary>
        /// <param name="createTenantAccountRequestDto"> The create tenant account request dto</param>
        /// <returns></returns>
        [HttpPost("tenant-account")]
        public async Task<IActionResult> CreateTenantAccount(CreateTenantAccountRequestDto createTenantAccountRequestDto)
        {
            const string methodName = nameof(CreateTenantAccount);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName}: Request started with TenantCode: {TenantCode}", className, methodName, createTenantAccountRequestDto.TenantAccount.TenantCode);

                var response = await _tenantAccountService.CreateTenantAccount(createTenantAccountRequestDto);

                if (response.ErrorCode != null)
                {
                    _logger.LogError("{ClassName}.{MethodName}: Error occurred while creating tenantaccount. Request: {RequestData}, Response: {ResponseData}, ErrorCode: {ErrorCode}", className, methodName, createTenantAccountRequestDto.ToJson(), response.ToJson(), response.ErrorCode);
                    return StatusCode((int)response.ErrorCode, response);
                }

                _logger.LogInformation("{ClassName}.{MethodName}: Tenant account created successfully, TenantCode: {TenantCode}", className, methodName, createTenantAccountRequestDto.TenantAccount.TenantCode);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: An error occurred while create tenant account. Error Message: {ErrorMessage}, ErrorCode: {ErrorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError });
            }
        }
        /// <summary>
        /// Gets the TenantAccount from the database
        /// </summary>
        /// <param name="tenantCode"></param>
        /// <returns>TenantAccount as response. If not found 404, any Exception 500</returns>
        [HttpGet("tenant-account/{tenantCode}")]
        public async Task<ActionResult<GetTenantAccountResponseDto>> GetTenantAccountDetails(string tenantCode)
        {
            const string methodName = nameof(GetTenantAccountDetails);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName}: Fetching Tenant Account details for TenantCode:{tenantCode}", className, methodName, tenantCode);
                var response = await _tenantAccountService.GetTenantAccount(tenantCode);
                if (response.ErrorCode != null)
                {
                    _logger.LogError("{ClassName}.{MethodName}: Tenant account fetching failed for TenantCode: {TenantCode}. ErrorCode: {ErrorCode}. Request Data: {RequestData}, Response Data: {ResponseData}",
                        className, methodName, tenantCode, response.ErrorCode, tenantCode.ToJson(), response.ToJson());
                    return StatusCode((int)response.ErrorCode, new GetTenantAccountResponseDto
                    {
                        ErrorCode = response.ErrorCode,
                        ErrorMessage = response.ErrorMessage
                    });
                }

                _logger.LogInformation("{ClassName}.{MethodName}: Tenant account fetched successfully for TenantCode: {TenantCode}. Response Data: {ResponseData}",
                    className, methodName, tenantCode, response.ToJson());
                return Ok(response);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: Exception during tenant account Fetching for TenantCode: {TenantCode}. ErrorCode: {ErrorCode}, ErrorMessage: {ErrorMessage}",
                    className, methodName, tenantCode, StatusCodes.Status500InternalServerError, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new GetTenantAccountResponseDto { ErrorCode = StatusCodes.Status500InternalServerError });
            }
        }
        /// <summary>
        /// This controller will update the tenant account
        /// </summary>
        /// <param name="tenantCode">tenant code</param>
        /// <param name="tenantAccount">Data contains to update the Tenant account</param>
        /// <returns>If 200: Updated TenantAccount, 404: TenantAccountNotFound, 400:tenantCode mismatch, 500: any exception</returns>
        [HttpPut("tenant-account/{tenantCode}")]
        public async Task<ActionResult<TenantAccountUpdateResponseDto>> UpdateTenantAccount(string tenantCode, [FromBody] TenantAccountRequestDto tenantAccount)
        {
            const string methodName = nameof(UpdateTenantAccount);
            try
            {
                _logger.LogInformation("{className}.{methodName}: Updating Tenant Account started with TenantCode: {Tenant}.", className, methodName, tenantCode);
                var response = await _tenantAccountService.UpdateTenantAccount(tenantCode, tenantAccount);
                if (response.ErrorCode != null)
                {
                    _logger.LogError("{className}.{methodName}: Failed to update Tenant Account. TenantCode: {TenantCode}, Error: {ErrorMessage}.",
                        className, methodName, tenantCode, response.ErrorMessage);
                    return StatusCode((int)response.ErrorCode, response);
                }
                _logger.LogInformation("{className}.{methodName}: Successfully updated Tenant Account. TenantCode: {TenantCode}", className, methodName, tenantCode);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{className}.{methodName}: An unexpected error occurred while updating Tenant Account. TenantCode: {TenantCode}, Error: {ErrorMessage}.",
                                 className, methodName, tenantCode, ex.Message);

                return StatusCode(StatusCodes.Status500InternalServerError,
                    new TenantAccountUpdateResponseDto
                    {
                        ErrorCode = StatusCodes.Status500InternalServerError,
                        ErrorMessage = "An unexpected error occurred while updating Tenant Account.",
                    });
            }
        }
        /// <summary>
        /// Creates the new TenantAccount in the database
        /// </summary>
        /// <param name="requestDto">request contains data to create new Tenant Account</param>
        /// <returns>base response</returns>
        [HttpPost("save-tenant-account")]
        public async Task<ActionResult<BaseResponseDto>> SaveTenantAccount([FromBody] TenantAccountRequestDto requestDto)
        {
            const string methodName = nameof(SaveTenantAccount);
            try
            {
                var tenantAccountResponse = await _tenantAccountService.SaveTenantAccount(requestDto);
                if (tenantAccountResponse?.ErrorCode != null)
                {
                    _logger.LogError("{ClassName}.{MethodName} - Error occurred while Saving tenant account with Request: {TenantCode},ErrorCode:{ErrorCode}",
                       className, methodName, requestDto.ToJson(), tenantAccountResponse.ErrorCode);

                    return StatusCode((int)tenantAccountResponse.ErrorCode, tenantAccountResponse);
                }
                _logger.LogInformation("{ClassName}.{MethodName} - TenantAccount Created successfully with TenantCode:{TenantCode}",
                   className, methodName, requestDto.TenantCode);

                return Ok(tenantAccountResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Error occurred while creating tenant account with,TenantCode: {TenantCode},ErrorCode:{ErrorCode},ERROR:{Msg}",
                       className, methodName, requestDto.TenantCode, StatusCodes.Status500InternalServerError, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponseDto()
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = ex.Message
                });

            }
        }
    }
}
