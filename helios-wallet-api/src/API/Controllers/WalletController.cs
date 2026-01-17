using Microsoft.AspNetCore.Mvc;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;
using SunnyRewards.Helios.Wallet.Core.Domain.Models;
using SunnyRewards.Helios.Wallet.Infrastructure.Services.Interfaces;

namespace SunnyRewards.Helios.Wallet.Api.Controllers
{
    [Route("api/v1/wallet/")]
    [ApiController]
    public class WalletController : ControllerBase
    {
        private readonly ILogger<WalletController> _walletLogger;
        private readonly IWalletService _walletService;
        const string className = nameof(WalletController);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="walletLogger"></param>
        /// <param name="walletService"></param>
        public WalletController(ILogger<WalletController> walletLogger, IWalletService walletService)
        {
            _walletLogger = walletLogger;
            _walletService = walletService;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="walletId"></param>
        /// <returns></returns>
        [HttpGet("{walletId}")]
        public async Task<ActionResult<WalletDto>> GetWallet(long walletId)
        {
            const string methodName = nameof(GetWallet);
            try
            {
                _walletLogger.LogInformation("{className}.{methodName}: API - Started with WalletId : {walletId}", className, methodName, walletId);
                var response = await _walletService.GetWalletData(walletId);

                return response != null ? Ok(response) : NotFound();
            }
            catch (Exception ex)
            {

                _walletLogger.LogError(ex, "{className}.{methodName}: API - ERROR Msg:{msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);

                return new WalletDto();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="walletTypeId"></param>
        /// <returns></returns>
        [HttpGet("walletTypeId")]
        public async Task<ActionResult<WalletTypeDto>> GetWalletType(long walletTypeId)
        {
            const string methodName = nameof(GetWalletType);
            try
            {
                _walletLogger.LogInformation("{className}.{methodName}: API - Started with wallletTypeId : {walletTypeId}", className, methodName, walletTypeId);
                var response = await _walletService.GetWalletType(walletTypeId);

                return response != null ? Ok(response) : NotFound();
            }
            catch (Exception ex)
            {

                _walletLogger.LogError(ex, "{className}.{methodName}: API - ERROR Msg:{msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);

                return new WalletTypeDto();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="walletTypeDto"></param>
        /// <returns></returns>
        [HttpPost("wallet-type-code")]
        public async Task<ActionResult<WalletTypeDto>> GetWalletTypeCode([FromBody] WalletTypeDto walletTypeDto)
        {
            const string methodName = nameof(GetWalletTypeCode);
            try
            {
                _walletLogger.LogInformation("{className}.{methodName}: API - Started with walletTpyeCode : {walletTpyeCode}", className, methodName, walletTypeDto.WalletTypeCode);
                var response = await _walletService.GetWalletTypeCode(walletTypeDto);

                return response != null ? Ok(response) : NotFound();
            }
            catch (Exception ex)
            {

                _walletLogger.LogError(ex, "{className}.{methodName}: API - ERROR Msg:{msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);

                return new WalletTypeDto();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="postRewardRequestDto"></param>
        /// <returns></returns>
        [HttpPost("reward")]
        public async Task<ActionResult<PostRewardResponseDto>> PostReward(PostRewardRequestDto postRewardRequestDto)
        {
            const string methodName = nameof(PostReward);
            try
            {

                _walletLogger.LogInformation("{className}.{methodName}: API - Started With ConsumerCode : {ConsumerCode}", className, methodName, postRewardRequestDto.ConsumerCode);

                var response = await _walletService.RewardDetailsOuter(postRewardRequestDto);

                if (response.ErrorCode != null)
                {
                    _walletLogger.LogError("{className}.{methodName}: API Post Reward Error For Consumer Code:{consumerCode}. ERROR Code:{errorCode}", className, methodName, postRewardRequestDto.ConsumerCode, response.ErrorCode);
                    return StatusCode(Convert.ToInt32(response.ErrorCode), response);
                }

                return response.TransactionDetail != null && response.SubEntry != null && response.AddEntry != null ? Ok(response) : NotFound();
            }
            catch (Exception ex)
            {

                _walletLogger.LogError(ex, "{className}.{methodName}: API - ERROR Msg:{msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return new PostRewardResponseDto();

            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="redeemStartRequestDto"></param>
        /// <returns></returns>
        [HttpPost("redeem-start")]
        public async Task<ActionResult<PostRedeemStartResponseDto>> RedeemStart([FromBody] PostRedeemStartRequestDto redeemStartRequestDto)
        {
            const string methodName = nameof(RedeemStart);
            try
            {
                _walletLogger.LogInformation("{className}.{methodName}: API - Started with ConsumerCode : {ConsumerCode}:", className, methodName, redeemStartRequestDto.ConsumerCode);

                var response = await _walletService.RedeemStartOuter(redeemStartRequestDto);
                if (response.ErrorCode != null)
                {
                    _walletLogger.LogError("{className}.{methodName}: API Redeem start Error For Consumer Code:{consumerCode}. ERROR Code:{errorCode}", className, methodName, redeemStartRequestDto.ConsumerCode, response.ErrorCode);
                    var errorCode = response.ErrorCode;
                    return StatusCode((int)errorCode, response);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {

                _walletLogger.LogError(ex, "{className}.{methodName}: API - ERROR Msg:{msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);

                return new PostRedeemStartResponseDto();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="redeemCompleteRequestDto"></param>
        /// <returns></returns>
        [HttpPost("redeem-complete")]
        public async Task<ActionResult<PostRedeemCompleteResponseDto>> RedeemComplete([FromBody] PostRedeemCompleteRequestDto redeemCompleteRequestDto)
        {
            const string methodName = nameof(RedeemComplete);
            try
            {

                _walletLogger.LogInformation("{className}.{methodName}: API - Started with ConsumerCode : {ConsumerCode}:", className, methodName, redeemCompleteRequestDto.ConsumerCode);

                var response = await _walletService.RedeemCompleteOuter(redeemCompleteRequestDto);
                if (response.ErrorCode != null)
                {
                    _walletLogger.LogError("{className}.{methodName}: API Redeem Complete Error For Consumer Code:{consumerCode}. ERROR Code:{errorCode}", className, methodName, redeemCompleteRequestDto.ConsumerCode, response.ErrorCode);
                    var errorCode = response.ErrorCode;
                    return StatusCode((int)errorCode, response);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {

                _walletLogger.LogError(ex, "{className}.{methodName}: API - ERROR Msg:{msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);

                return new PostRedeemCompleteResponseDto();
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="redeemFailRequestDto"></param>
        /// <returns></returns>
        [HttpPost("redeem-fail")]
        public async Task<ActionResult<PostRedeemFailResponseDto>> RedeemFail([FromBody] PostRedeemFailRequestDto redeemFailRequestDto)
        {
            const string methodName = nameof(RedeemFail);
            try
            {

                _walletLogger.LogInformation("{className}.{methodName}: API - Started with ConsumerCode : {ConsumerCode}:", className, methodName, redeemFailRequestDto.ConsumerCode);

                var response = await _walletService.RedeemFailOuter(redeemFailRequestDto);
                if (response.ErrorCode != null)
                {
                    _walletLogger.LogError("{className}.{methodName}: API Redeem Fail Error For Consumer Code:{consumerCode}. ERROR Code:{errorCode}", className, methodName, redeemFailRequestDto.ConsumerCode, response.ErrorCode);
                    var errorCode = response.ErrorCode;
                    return StatusCode((int)errorCode, response);
                }

                return Ok(response);

            }
            catch (Exception ex)
            {

                _walletLogger.LogError(ex, "{className}.{methodName}: API - ERROR Msg:{msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);

                return new PostRedeemFailResponseDto();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="findConsumerWalletRequestDto"></param>
        /// <returns></returns>
        [HttpPost("get-wallets")]
        public async Task<ActionResult<WalletResponseDto>> GetWallets(FindConsumerWalletRequestDto findConsumerWalletRequestDto)
        {
            const string methodName = nameof(GetWallets);
            try
            {
                _walletLogger.LogInformation("{className}.{methodName}: API - Started with ConsumerCode : {ConsumerCode}", className, methodName, findConsumerWalletRequestDto.ConsumerCode);
                var response = await _walletService.GetWallets(findConsumerWalletRequestDto);
                return response.ErrorCode switch
                {
                    404 => NotFound(response),
                    _ => Ok(response)
                };
            }
            catch (Exception ex)
            {
                _walletLogger.LogError(ex, "{className}.{methodName}: API - ERROR Msg:{msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                throw;
            }
        }

        [HttpPost("clear-entries-wallet")]
        public async Task<ActionResult<BaseResponseDto>> ClearEntriesWallet(ClearEntriesWalletRequestDto clearEntriesWalletRequestDto)
        {
            const string methodName = nameof(ClearEntriesWallet);
            try
            {
                _walletLogger.LogInformation("{className}.{methodName}: API - Started with tenant code : {TenantCode}", className, methodName, clearEntriesWalletRequestDto.TenantCode);
                var response = await _walletService.ClearEntriesWallet(clearEntriesWalletRequestDto);
                return response.ErrorCode switch
                {
                    404 => NotFound(response),
                    _ => Ok(response)
                };
            }
            catch (Exception ex)
            {
                _walletLogger.LogError(ex, "{className}.{methodName}: API - ERROR Msg:{msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponseDto()
                {
                    ErrorMessage = ex.Message,
                    ErrorDescription = ex.InnerException?.Message
                });
            }
        }
        [HttpPost("update-wallet-balance")]
        public async Task<ActionResult<BaseResponseDto>> UpdateWalletBalance([FromBody] IList<WalletModel> walletModel)
        {
            const string methodName = nameof(UpdateWalletBalance);
            try
            {
                _walletLogger.LogInformation("{className}.{methodName}: API - Started with Wallet Model.", className, methodName);
                var response = await _walletService.UpdateWalletBalance(walletModel);
                return response.ErrorCode switch
                {
                    404 => NotFound(response),
                    _ => Ok(response)
                };
            }
            catch (Exception ex)
            {
                _walletLogger.LogError(ex, "{className}.{methodName}: API - ERROR Msg:{msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponseDto()
                {
                    ErrorMessage = ex.Message,
                    ErrorDescription = ex.InnerException?.Message
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

        /// <summary>
        /// Gets all the wallet types available in database
        /// </summary>
        /// <returns>List of wallet types</returns>
        [HttpGet("wallet-types")]
        public async Task<ActionResult<GetWalletTypeResponseDto>> GetAllWalletTypes()
        {
            const string methodName = nameof(GetAllWalletTypes);
            try
            {
                _walletLogger.LogInformation("{ClassName}.{MethodName}: Started fetching all the wallet types", className, methodName);
                var response = await _walletService.GetAllWalletTypes();
                if (response?.WalletTypes?.Count == 0)
                {
                    _walletLogger.LogWarning("{ClassName}.{MethodName}: No wallet types are available", className, methodName);
                }
                _walletLogger.LogInformation("{ClassName}.{MethodName}: Successfully fetched all the wallet types", className, methodName);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _walletLogger.LogError(ex, "{ClassName}.{MethodName}: An error occurred while get all walletTypes. Error Message: {ErrorMessage}, ErrorCode: {ErrorCode}, StackTrace: {StackTrace}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError, ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, new GetWalletTypeResponseDto { ErrorCode = StatusCodes.Status500InternalServerError });
            }
        }
        /// <summary>
        /// Creates WalletType with the given data
        /// </summary>
        /// <param name="walletTypeDto">WalletType request to create walletType</param>
        /// <returns>BaseResponse with status codes</returns>
        [HttpPost("wallet-type")]
        public async Task<ActionResult<BaseResponseDto>> CreateWalletType([FromBody] WalletTypeDto walletTypeDto)
        {
            const string methodName = nameof(CreateWalletType);
            try
            {
                var walletTypeResponse = await _walletService.CreateWalletType(walletTypeDto);
                if (walletTypeResponse?.ErrorCode != null)
                {
                    _walletLogger.LogError("{ClassName}.{MethodName} - Error occurred while Create WalletType with Request: {walletTypeRequest},ErrorCode:{ErrorCode}",
                       className, methodName, walletTypeDto.ToJson(), walletTypeResponse.ErrorCode);
                    return StatusCode((int)walletTypeResponse.ErrorCode, walletTypeResponse);
                }
                _walletLogger.LogInformation("{ClassName}.{MethodName} - WalletType Created successfully with WalletTypeCode:{WalletTypeCode}",
                   className, methodName, walletTypeDto.WalletTypeCode);

                return Ok(walletTypeResponse);
            }
            catch (Exception ex)
            {
                _walletLogger.LogError(ex, "{ClassName}.{MethodName} - Error occurred while creating WalletType with,WalletTypeCode:{WalletTypeCode},ErrorCode:{ErrorCode},ERROR:{Msg}",
                       className, methodName, walletTypeDto.WalletTypeCode, StatusCodes.Status500InternalServerError, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponseDto()
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = ex.Message
                });

            }
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

        [HttpPost("get-applicable-transfer-rule")]
        public async Task<ActionResult<MaxWalletTransferRuleResponseDto>> GetWalletTypeTransferRule([FromBody] GetWalletTypeTransferRule getWalletTypeTransferRule)
        {
            const string methodName = nameof(GetWalletTypeTransferRule);
            try
            {
                _walletLogger.LogInformation("{className}.{methodName}: API - Started with TenantCode : {TenantCode}", className, methodName, getWalletTypeTransferRule.TenantCode);
                var response = await _walletService.GetWalletTypeTransferRule(getWalletTypeTransferRule);

                if (response.ErrorCode != null)
                {
                    _walletLogger.LogError("{ClassName}.{MethodName}: Transfer Rule fetching failed for TenantCode: {TenantCode}. ErrorCode: {ErrorCode}. Request Data: {RequestData}, Response Data: {ResponseData}",
                        className, methodName, getWalletTypeTransferRule.TenantCode, response.ErrorCode, getWalletTypeTransferRule.ToJson(), response.ToJson());
                    return StatusCode((int)response.ErrorCode, response);
                }

                _walletLogger.LogInformation("{ClassName}.{MethodName}: Master wallets fetched successfully for TenantCode: {TenantCode}. Response Data: {ResponseData}",
                    className, methodName, getWalletTypeTransferRule.TenantCode, response.ToJson());
                return Ok(response);
            }
            catch (Exception ex)
            {

                _walletLogger.LogError(ex, "{className}.{methodName}: API - ERROR Msg:{msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);

                return new MaxWalletTransferRuleResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError , ErrorMessage = ex.Message };
            }
        }
        /// <summary>
        /// Imports wallet types by calling the service layer and returns appropriate HTTP response.
        /// </summary>
        /// <param name="walletTypeRequestDto">DTO containing wallet types to import.</param>
        /// <returns>ActionResult with import status and details.</returns>
        [HttpPost("import-wallet-types")]
        public async Task<IActionResult> ImportWalletTypesAsync([FromBody] ImportWalletTypeRequestDto walletTypeRequestDto)
        {
            const string methodName = nameof(ImportWalletTypesAsync);
            _walletLogger.LogInformation("{ClassName}.{MethodName} - Import request received with {Count} wallet types.",
                className, methodName, walletTypeRequestDto.WalletTypes.Count);

            try
            {
                var response = await _walletService.ImportWalletTypesAsync(walletTypeRequestDto);

                if (response.ErrorCode != null)
                {
                    _walletLogger.LogWarning("{ClassName}.{MethodName} - Import completed with partial errors. Response: {Response}",
                        className, methodName, response.ToJson());
                    return StatusCode((int)response.ErrorCode, response);
                }

                _walletLogger.LogInformation("{ClassName}.{MethodName} - Import completed successfully with all wallet types.",
                    className, methodName);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _walletLogger.LogError(ex, "{ClassName}.{MethodName} - Import failed due to an unexpected error.",
                    className, methodName);
                return StatusCode(StatusCodes.Status500InternalServerError, new ImportWalletTypeResponseDto
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = ex.Message
                });
            }
        }

    }
}


