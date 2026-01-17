using Microsoft.AspNetCore.Mvc;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;
using SunnyRewards.Helios.Wallet.Infrastructure.Services.Interfaces;

namespace SunnyRewards.Helios.Wallet.Api.Controllers
{
    [Route("api/v1/consumer-wallet/")]
    [ApiController]
    public class ConsumerWalletController : ControllerBase
    {
        private readonly ILogger<ConsumerWalletController> _consumerWalletLogger;
        private readonly IConsumerWalletService _consumerWalletService;
        const string className = nameof(ConsumerWalletController);
        /// <summary>
        /// Get Consumer Data Constructor
        /// </summary>
        /// <param name="consumerWalletLogger"></param>
        /// <param name="consumerWalletService"></param>
        public ConsumerWalletController(ILogger<ConsumerWalletController> consumerWalletLogger, IConsumerWalletService consumerWalletService)
        {
            _consumerWalletLogger = consumerWalletLogger;
            _consumerWalletService = consumerWalletService;
        }

        [HttpPost("find-consumer-wallet")]
        public async Task<ActionResult<FindConsumerWalletResponseDto>> FindConsumerWallet([FromBody] FindConsumerWalletRequestDto consumerWalletRequestDto)
        {
            const string methodName = nameof(FindConsumerWallet);
            try
            {
                _consumerWalletLogger.LogInformation("{className}.{methodName}: API - Started with ConsumerCode : {ConsumerCode}:", className, methodName, consumerWalletRequestDto.ConsumerCode);
                var response = await _consumerWalletService.GetConsumerWallet(consumerWalletRequestDto);

                return response != null ? Ok(response) : NotFound();
            }
            catch (Exception ex)
            {

                _consumerWalletLogger.LogError(ex, "{className}.{methodName}: API - ERROR Msg:{msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);

                return new FindConsumerWalletResponseDto();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="consumerWalletDataDto"></param>
        /// <returns></returns>
        [HttpPost("post-consumer-wallets")]
        public async Task<ActionResult<List<ConsumerWalletDataResponseDto>>> PostConsumerWallets([FromBody] IList<ConsumerWalletDataDto> consumerWalletDataDto)
        {
            const string methodName = nameof(PostConsumerWallets);
            try
            {
                _consumerWalletLogger.LogInformation("{className}.{methodName}: API - Started", className, methodName);
                var response = await _consumerWalletService.PostConsumerWallets(consumerWalletDataDto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _consumerWalletLogger.LogError(ex, "{className}.{methodName}: API - ERROR Msg:{msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return new List<ConsumerWalletDataResponseDto>();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="consumerWalletByWalletTypeRequestDto"></param>
        /// <returns></returns>
        [HttpPost("find-consumer-wallet-by-wallet-type")]
        public async Task<ActionResult<FindConsumerWalletResponseDto>> GetConsumerWalletsByWalletType([FromBody] FindConsumerWalletByWalletTypeRequestDto consumerWalletByWalletTypeRequestDto)
        {
            const string methodName = nameof(GetConsumerWalletsByWalletType);
            try
            {
                _consumerWalletLogger.LogInformation("{className}.{methodName}: API - Started with Consumer code: {consumcerCode}, WalletTypeCode : {walletTypeCode}:", className, methodName,
                    consumerWalletByWalletTypeRequestDto.ConsumerCode, consumerWalletByWalletTypeRequestDto.WalletTypeCode);
                var response = await _consumerWalletService.GetConsumerWalletsByWalletType(consumerWalletByWalletTypeRequestDto);
                return response.ErrorCode switch
                {
                    404 => NotFound(response),
                    _ => Ok(response)
                };
            }
            catch (Exception ex)
            {
                _consumerWalletLogger.LogError(ex, "{className}.{methodName}: API - ERROR Msg:{msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, new FindConsumerWalletResponseDto()
                {
                    ErrorMessage = ex.Message,
                    ErrorDescription = ex.InnerException?.Message
                });
            }
        }

        /// <summary>
        /// Retrieves all wallets associated with a given consumer.
        /// </summary>
        /// <param name="consumerWalletRequestDto">Contains TenantCode and ConsumerCode.</param>
        /// <returns>A list of wallets for the specified consumer.</returns>
        [HttpPost("get-all-consumer-wallets")]
        public async Task<ActionResult<ConsumerWalletResponseDto>> GetAllConsumerWallets([FromBody] GetConsumerWalletRequestDto consumerWalletRequestDto)
        {
            const string methodName = nameof(GetAllConsumerWallets);
            try
            {
                _consumerWalletLogger.LogInformation("{className}.{methodName}: API - Started for ConsumerCode : {ConsumerCode} and TenantCode: {TenantCode}", className, methodName, consumerWalletRequestDto.ConsumerCode, consumerWalletRequestDto.TenantCode);

                var response = await _consumerWalletService.GetAllConsumerWalletsAsync(consumerWalletRequestDto);

                if (response.ErrorCode != null)
                {
                    _consumerWalletLogger.LogError("{ClassName}.{MethodName}: Retrieval of consumer wallets failed for TenantCode: {TenantCode}, ConsumerCode: {ConsumerCode}. ErrorCode: {ErrorCode}. Request Data: {RequestData}, Response Data: {ResponseData}",
                    className, methodName, consumerWalletRequestDto.TenantCode, consumerWalletRequestDto.ConsumerCode, response.ErrorCode, consumerWalletRequestDto.ToJson(), response.ToJson());
                    return StatusCode((int)response.ErrorCode, response);
                }

                _consumerWalletLogger.LogInformation("{ClassName}.{MethodName}: Successfully retrieved wallets for TenantCode: {TenantCode}, ConsumerCode: {ConsumerCode}. Response Data: {ResponseData}",
                className, methodName, consumerWalletRequestDto.TenantCode, consumerWalletRequestDto.ConsumerCode, response.ToJson());

                return Ok(response);
            }
            catch (Exception ex)
            {
                _consumerWalletLogger.LogError(ex, "{className}.{methodName}: API - ERROR Msg:{msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, new ConsumerWalletResponseDto
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = "An error occurred while retrieving consumer wallets."
                });
            }
        }

        /// <summary>
        /// Retrieves all wallets associated with a given consumer.
        /// </summary>
        /// <param name="consumerWalletRequestDto">Contains TenantCode and ConsumerCode.</param>
        /// <returns>A list of wallets for the specified consumer.</returns>
        [HttpPost("get-all-consumer-reedemable-wallets")]
        public async Task<ActionResult<ConsumerWalletResponseDto>> GetAllConsumerRedeemableWallets([FromBody] FindConsumerWalletRequestDto consumerWalletRequestDto)
        {
            const string methodName = nameof(GetAllConsumerRedeemableWallets);
            try
            {
                _consumerWalletLogger.LogInformation("{className}.{methodName}: API - Started for ConsumerCode : {ConsumerCode}", className, methodName, consumerWalletRequestDto.ConsumerCode);

                var response = await _consumerWalletService.GetAllConsumerRedeemableWalletsAsync(consumerWalletRequestDto);

                if (response.ErrorCode != null)
                {
                    _consumerWalletLogger.LogError("{ClassName}.{MethodName}: Retrieval of consumer wallets failed for  ConsumerCode: {ConsumerCode}. ErrorCode: {ErrorCode}. Request Data: {RequestData}, Response Data: {ResponseData}",
                    className, methodName,  consumerWalletRequestDto.ConsumerCode, response.ErrorCode, consumerWalletRequestDto.ToJson(), response.ToJson());
                    return StatusCode((int)response.ErrorCode, response);
                }

                _consumerWalletLogger.LogInformation("{ClassName}.{MethodName}: Successfully retrieved wallets for ConsumerCode: {ConsumerCode}. Response Data: {ResponseData}",
                className, methodName, consumerWalletRequestDto.ConsumerCode, response.ToJson());

                return Ok(response);
            }
            catch (Exception ex)
            {
                _consumerWalletLogger.LogError(ex, "{className}.{methodName}: API - ERROR Msg:{msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, new ConsumerWalletResponseDto
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = "An error occurred while retrieving consumer wallets."
                });
            }
        }
    }
}


