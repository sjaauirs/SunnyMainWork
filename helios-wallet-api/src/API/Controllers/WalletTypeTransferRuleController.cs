using Microsoft.AspNetCore.Mvc;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;
using SunnyRewards.Helios.Wallet.Infrastructure.Services;
using SunnyRewards.Helios.Wallet.Infrastructure.Services.Interfaces;

namespace SunnyRewards.Helios.Wallet.Api.Controllers
{
    [Route("api/v1/")]
    [ApiController]
    public class WalletTypeTransferRuleController : ControllerBase
    {
        private readonly ILogger<WalletTypeTransferRuleController> _logger;
        private readonly IWalletTypeTransferRuleService _walletTypeTransferRuleService;
        const string className = nameof(WalletTypeTransferRuleController);

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="walletTypeTransferRuleService"></param>
        public WalletTypeTransferRuleController(IWalletTypeTransferRuleService walletTypeTransferRuleService, ILogger<WalletTypeTransferRuleController> logger)
        {
            _walletTypeTransferRuleService = walletTypeTransferRuleService;
            _logger = logger;
        }

        /// <summary>
        /// Get walletType transfer rule by tenant code
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("export-walletType-transfer-rule")]
        public async Task<ActionResult<ExportWalletTypeTransferRuleResponseDto>> ExportWalletTypeTransferRule([FromBody] ExportWalletTypeTransferRuleRequestDto request)
        {
            const string methodName = nameof(ExportWalletTypeTransferRule);
            try
            {
                _logger.LogInformation("{className}.{methodName}:  API - Started with tenantCode : {TenantCode}", className, methodName, request.TenantCode);
                var response = await _walletTypeTransferRuleService.ExportWalletTypeTransferRules(request);

                return Ok(response);
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, "{className}.{methodName}: API - ERROR Msg:{msg}, Error Code:{errorCode}, request: {request}",
                    className, methodName, ex.Message, StatusCodes.Status500InternalServerError, request.ToJson());

                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponseDto()
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = "Internal Server Error"
                });
            }
        }

        /// <summary>
        /// Import walletType transfer rule per tenant code
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("import-walletType-transfer-rule")]
        public async Task<ActionResult> ImportWalletTypeTransferRule(ImportWalletTypeTransferRuleRequestDto importWalletTypeTransferRuleRequest)
        {
            const string methodName = nameof(ImportWalletTypeTransferRule);
            try
            {
                _logger.LogInformation("{className}.{methodName}:  API - Started", className, methodName);
                var response = await _walletTypeTransferRuleService.ImportWalletTypeTransferRules(importWalletTypeTransferRuleRequest);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{className}.{methodName}: API - ERROR Msg:{msg}, Error Code:{errorCode}, request: {request}",
                    className, methodName, ex.Message, StatusCodes.Status500InternalServerError, importWalletTypeTransferRuleRequest.ToJson());

                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponseDto()
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = "Internal Server Error"
                });
            }
        }
    }
}
