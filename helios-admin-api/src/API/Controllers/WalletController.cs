using Microsoft.AspNetCore.Mvc;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Api.Controllers
{
    [Route("api/v1/wallet")]
    [ApiController]
    public class WalletController : ControllerBase
    {
        private readonly ILogger<WalletController> _walletLogger;
        public readonly IWalletService _walletService;

        const string className = nameof(WalletController);

        public WalletController(ILogger<WalletController> logger, IWalletService walletService)
        {
            _walletLogger = logger;
            _walletService = walletService;
        }
        /// <summary>
        /// Gets all the master wallets based on the given tenant code
        /// </summary>
        /// <param name="tenantCode"></param>
        /// <returns>List of master wallets</returns>
        [HttpGet("master-wallets/{tenantCode}")]
        public async Task<ActionResult<GetAllMasterWalletsResponseDto>> GetMasterWallets(string tenantCode)
        {
            const string methodName = nameof(GetMasterWallets);
            try
            {
                _walletLogger.LogInformation("{ClassName}.{MethodName}: Fetching Master wallets for TenantCode:{tenantCode}", className, methodName, tenantCode);
                var response = await _walletService.GetMasterWallets(tenantCode);
                if (response.ErrorCode != null)
                {
                    _walletLogger.LogError("{ClassName}.{MethodName}: Master wallets fetching failed for TenantCode: {TenantCode}. ErrorCode: {ErrorCode}. Request Data: {RequestData}, Response Data: {ResponseData}",
                        className, methodName, tenantCode, response.ErrorCode, tenantCode, response.ToJson());
                    return StatusCode((int)response.ErrorCode, response);
                }

                _walletLogger.LogInformation("{ClassName}.{MethodName}: Master wallets fetched successfully for TenantCode: {TenantCode}. Response Data: {ResponseData}",
                    className, methodName, tenantCode, response.ToJson());
                return Ok(response);

            }
            catch (Exception ex)
            {
                _walletLogger.LogError(ex, "{ClassName}.{MethodName}: Exception during Master wallets Fetching for TenantCode: {TenantCode}. ErrorCode: {ErrorCode}, ErrorMessage: {ErrorMessage}",
                    className, methodName, tenantCode, StatusCodes.Status500InternalServerError, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new GetAllMasterWalletsResponseDto { ErrorCode = StatusCodes.Status500InternalServerError });
            }
        }
        /// <summary>
        /// Creates new wallet with the given data
        /// </summary>
        /// <param name="walletRequestDto">Request contains data to create wallet</param>
        /// <returns>Base response(200: when wallet creates, 404: if walletType not found, 409: if Wallet already exist</returns>
        [HttpPost("wallet")]
        public async Task<ActionResult<BaseResponseDto>> CreateWallet(WalletRequestDto walletRequestDto)
        {
            const string methodName = nameof(CreateWallet);
            try
            {
                var walletResponse = await _walletService.CreateWallet(walletRequestDto);
                if (walletResponse?.ErrorCode != null)
                {
                    _walletLogger.LogError("{ClassName}.{MethodName} - Error occurred while Create Wallet with Request: {TenantCode},ErrorCode:{ErrorCode}",
                       className, methodName, walletRequestDto.ToJson(), walletResponse.ErrorCode);

                    return StatusCode((int)walletResponse.ErrorCode, walletResponse);
                }
                _walletLogger.LogInformation("{ClassName}.{MethodName} - Wallet Created successfully with, WalletCode:{WalletCode}, TenantCode:{TenantCode}", className, methodName, walletRequestDto.WalletCode, walletRequestDto.TenantCode);
                return Ok(walletResponse);
            }
            catch (Exception ex)
            {
                _walletLogger.LogError(ex, "{ClassName}.{MethodName} - Error occurred while creating Wallet with, WalletCode:{WalletCode}, TenantCode: {TenantCode},ErrorCode:{ErrorCode},ERROR:{Msg}",
                       className, methodName, walletRequestDto.WalletCode, walletRequestDto.TenantCode, StatusCodes.Status500InternalServerError, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponseDto()
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = ex.Message
                });
            }
        }
        /// <summary>
        /// Create tenant master wallets.
        /// </summary>
        /// <param name="createTenantMasterWalletsRequest">The request DTO containing tenant and app information.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        [HttpPost("create-tenant-master-wallets")]
        public async Task<IActionResult> CreateTenantMasterWallets([FromBody] CreateTenantMasterWalletsRequestDto createTenantMasterWalletsRequest)
        {
            const string methodName = nameof(CreateTenantMasterWallets);
            try
            {
                _walletLogger.LogInformation("{ClassName}.{MethodName}: Request started with TenantCode: {TenantCode}", className, methodName, createTenantMasterWalletsRequest.TenantCode);

                var response = await _walletService.CreateTenantMasterWallets(createTenantMasterWalletsRequest);

                if (response.ErrorCode != null)
                {
                    _walletLogger.LogError("{ClassName}.{MethodName}: Error occurred while creating tenant master wallets. Request: {RequestData}, Response: {ResponseData}, ErrorCode: {ErrorCode}", className, methodName, createTenantMasterWalletsRequest.ToJson(), response.ToJson(), response.ErrorCode);
                    return StatusCode((int)response.ErrorCode, response);
                }

                _walletLogger.LogInformation("{ClassName}.{MethodName}: Successfully created tenant master wallets for TenantCode: {TenantCode}", className, methodName, createTenantMasterWalletsRequest.TenantCode);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _walletLogger.LogError(ex, "{ClassName}.{MethodName}: An error occurred while creating tenant master wallets. Error Message: {ErrorMessage}, ErrorCode: {ErrorCode}, StackTrace: {StackTrace}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError, ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError });
            }
        }
    }
}
