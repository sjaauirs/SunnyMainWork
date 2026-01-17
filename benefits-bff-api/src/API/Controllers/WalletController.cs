using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sunny.Benefits.Bff.Api.Filters;
using Sunny.Benefits.Bff.Infrastructure.Services.Interfaces;
using SunnyBenefits.Fis.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;

namespace Sunny.Benefits.Bff.Api.Controllers
{
    [Route("/api/v1/")]
    [ApiController]
    [Authorize]

    public class WalletController : ControllerBase
    {
        private readonly ILogger<WalletController> _walletLogger;
        private readonly IWalletService _walletService;
        private const string className = nameof(WalletController);

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
        /// <param name="findConsumerWalletRequestDto"></param>
        /// <returns></returns>
        [HttpPost("wallets")]
        public async Task<ActionResult> GetWallets(FindConsumerWalletRequestDto findConsumerWalletRequestDto)
        {
            const string methodName = nameof(GetWallets);
            try
            {
                _walletLogger.LogInformation("{ClassName}.{MethodName} - Started processing GetWallets ConsumerCode : {ConsumerCode}", className, methodName, findConsumerWalletRequestDto.ConsumerCode);
                var response = await _walletService.GetWallets(findConsumerWalletRequestDto);
                return response.ErrorCode switch
                {
                    404 => NotFound(),
                    _ => Ok(response)
                };
            }
            catch (Exception ex)
            {
                _walletLogger.LogError(ex, "{ClassName}.{MethodName} - Error occured while processing GetWallets with ConsumerCode: {ConsumerCode} - ErrorCode:{ErrorCode}, ERROR:{ErrorMessage}",
                    className, methodName,findConsumerWalletRequestDto.ConsumerCode, StatusCodes.Status500InternalServerError, ex.Message);
                throw new InvalidOperationException("ERROR: GetWallets API", ex);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="postGetTransactionsRequestDto"></param>
        /// <returns></returns>
        [HttpPost("transactions")]
        [ServiceFilter(typeof(ValidateLanguageCodeAttribute))]
        public async Task<ActionResult> GetTransactions(PostGetTransactionsRequestDto postGetTransactionsRequestDto)
        {
            const string methodName = nameof(GetTransactions);
            try
            {
                _walletLogger.LogInformation("{ClassName}.{MethodName} - Started processing GetTransactions with ConsumerCode : {ConsumerCode},  WalletId : {WalletId}",
                    className,methodName, postGetTransactionsRequestDto.ConsumerCode, postGetTransactionsRequestDto.WalletId);
                var response = await _walletService.GetTransactions(postGetTransactionsRequestDto);
                return response.ErrorCode switch
                {
                    404 => NotFound(),
                    403 => StatusCode(StatusCodes.Status403Forbidden, response),
                    _ => Ok(response)
                };
            }
            catch (Exception ex)
            {
                _walletLogger.LogError(ex, "{ClassName}.{MethodName} - Error occured while processing GetTransactions ConsumerCode : {ConsumerCode},  WalletId : {WalletId} - ErrorCode:{ErrorCode}, ERROR:{ErrorMessage}",
                    className, methodName, postGetTransactionsRequestDto.ConsumerCode, postGetTransactionsRequestDto.WalletId, StatusCodes.Status500InternalServerError,ex.Message);
                throw new InvalidOperationException("ERROR: GetTransactions API", ex);
            }
        }

        /// <summary>
        /// Gets the consumer benefits wallet types.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        [HttpPost("consumer-benefits-wallet-types")]
        public async Task<ActionResult<ConsumerBenefitsWalletTypesResponseDto>> GetConsumerBenefitsWalletTypes([FromBody] ConsumerBenefitsWalletTypesRequestDto request)
        {
            const string methodName = nameof(GetConsumerBenefitsWalletTypes);
            try
            {
                _walletLogger.LogInformation("{ClassName}.{MethodName}: Starting retrieval of available consumer wallet types for TenantCode: {TenantCode}, ConsumerCode: {ConsumerCode}.",
                    className, methodName, request.TenantCode, request.ConsumerCode);

                var response = await _walletService.GetConsumerBenefitsWalletTypesAsync(request);

                if (response.ErrorCode != null)
                {
                    _walletLogger.LogError("{ClassName}.{MethodName}: Retrieval of consumer wallet types failed for TenantCode: {TenantCode}, ConsumerCode: {ConsumerCode}. ErrorCode: {ErrorCode}. Request Data: {RequestData}, Response Data: {ResponseData}",
                        className, methodName, request.TenantCode, request.ConsumerCode, response.ErrorCode, request.ToJson(), response.ToJson());
                    return StatusCode((int)response.ErrorCode, response);
                }

                _walletLogger.LogInformation("{ClassName}.{MethodName}: Successfully retrieved consumer wallet types for TenantCode: {TenantCode}, ConsumerCode: {ConsumerCode}. Response Data: {ResponseData}",
                    className, methodName, request.TenantCode, request.ConsumerCode, response.ToJson());

                return Ok(response);
            }
            catch (Exception ex)
            {
                _walletLogger.LogError(ex, "{ClassName}.{MethodName}: Exception during retrieval of consumer wallet types for TenantCode: {TenantCode}, ConsumerCode: {ConsumerCode}. ErrorMessage: {ErrorMessage}",
                    className, methodName, request.TenantCode, request.ConsumerCode, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new ConsumerBenefitsWalletTypesResponseDto { ErrorCode = StatusCodes.Status500InternalServerError });
            }
        }
    }
}
