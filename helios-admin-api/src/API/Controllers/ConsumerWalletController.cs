using Microsoft.AspNetCore.Mvc;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Api.Controllers
{
    [Route("api/v1")]
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="consumerWalletByWalletTypeRequestDto"></param>
        /// <returns></returns>
        [HttpPost("consumer-wallet/consumer-wallet-by-wallet-type")]
        public async Task<ActionResult<FindConsumerWalletResponseDto>> GetConsumerWalletsByWalletType([FromBody] FindConsumerWalletByWalletTypeRequestDto consumerWalletByWalletTypeRequestDto)
        {
            const string methodName = nameof(GetConsumerWalletsByWalletType);
            try
            {
                _consumerWalletLogger.LogInformation("{ClassName}.{MethodName}: API - Started with Consumer code: {ConsumerCode}, WalletTypeCode : {WalletTypeCode}:", className, methodName,
                    consumerWalletByWalletTypeRequestDto.ConsumerCode, consumerWalletByWalletTypeRequestDto.WalletTypeCode);

                var response = await _consumerWalletService.GetConsumerWalletsByWalletType(consumerWalletByWalletTypeRequestDto);
                if (response.ErrorCode != null)
                {
                    _consumerWalletLogger.LogError("{ClassName}.{MethodName}: consumer wallet fetching failed ErrorCode: {ErrorCode}. Request Data: {RequestData}, Response Data: {ResponseData}",
                        className, methodName, response.ErrorCode, consumerWalletByWalletTypeRequestDto.ToJson(), response.ToJson());
                    return StatusCode((int)response.ErrorCode, response);
                }
                _consumerWalletLogger.LogInformation("{ClassName}.{MethodName}: Consumer wallet fetched successfully. Response Data: {ResponseData}",
                    className, methodName, response.ToJson());
                return Ok(response);
            }
            catch (Exception ex)
            {
                _consumerWalletLogger.LogError(ex, "{ClassName}.{MethodName}: API - ERROR:{Msg}, Error Code:{ErrorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
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
        [HttpPost("consumer-wallet/get-all-consumer-wallets")]
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
        [HttpPost("consumer-wallet/get-all-consumer-redeemable-wallets")]
        public async Task<ActionResult<ConsumerWalletResponseDto>> GetAllConsumerRedeemableWallets([FromBody] FindConsumerWalletRequestDto consumerWalletRequestDto)
        {
            const string methodName = nameof(GetAllConsumerRedeemableWallets);
            try
            {
                _consumerWalletLogger.LogInformation("{className}.{methodName}: API - Started for ConsumerCode : {ConsumerCode}", className, methodName, consumerWalletRequestDto.ConsumerCode);

                var response = await _consumerWalletService.GetAllConsumerRedeemableWalletsAsync(consumerWalletRequestDto);

                if (response.ErrorCode != null)
                {
                    _consumerWalletLogger.LogError("{ClassName}.{MethodName}: Retrieval of consumer wallets failed for ConsumerCode: {ConsumerCode}. ErrorCode: {ErrorCode}. Request Data: {RequestData}, Response Data: {ResponseData}",
                    className, methodName, consumerWalletRequestDto.ConsumerCode, response.ErrorCode, consumerWalletRequestDto.ToJson(), response.ToJson());
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

        /// <summary>
        /// Handles the posting of consumer wallets.
        /// </summary>
        /// <param name="consumerWalletDataDto">The consumer wallet data transfer object containing wallet details.</param>
        /// <returns>
        /// Returns an HTTP 200 response with the consumer wallet data if successful.  
        /// If an error occurs, logs the error and returns an empty list.
        /// </returns>
        /// <exception cref="Exception">Logs and handles any exceptions that occur during execution.</exception>
        [HttpPost("post-consumer-wallets")]
        public async Task<ActionResult<List<ConsumerWalletDataResponseDto>>> PostConsumerWallets([FromBody] IList<ConsumerWalletDataDto> consumerWalletDataDto)
        {
            const string methodName = nameof(PostConsumerWallets);
            try
            {
                _consumerWalletLogger.LogInformation("{className}.{methodName}: API - Started", className, methodName);
                var response = await _consumerWalletService.PostConsumerWallets(consumerWalletDataDto);
                if (response == null || response.Count <= 0)
                {
                    _consumerWalletLogger.LogError("{ClassName}.{MethodName}: Retrieval of consumer wallets failed for TenantCode: {TenantCode}, ConsumerCode: {ConsumerCode}. ErrorCode: {ErrorCode}. Request Data: {RequestData}, Response Data: {ResponseData}",
                    className, methodName, response.ToJson());
                    return StatusCode(StatusCodes.Status400BadRequest, response);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                _consumerWalletLogger.LogError(ex, "{className}.{methodName}: API - ERROR Msg:{msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, new List<ConsumerWalletDataResponseDto>());
            }
        }
    }
}
