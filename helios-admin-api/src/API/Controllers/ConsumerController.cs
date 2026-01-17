using Microsoft.AspNetCore.Mvc;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.User.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Api.Controllers
{
    [Route("api/v1/")]
    [ApiController]
    public class ConsumerController : ControllerBase
    {
        private readonly ILogger<ConsumerController> _consumerLogger;
        private readonly IConsumerService _consumerService;
        const string className = nameof(ConsumerController);

        /// <summary>
        /// Get Consumer Data Constructor
        /// </summary>
        /// <param name="consumerLogger"></param>
        /// <param name="consumerService"></param>
        public ConsumerController(ILogger<ConsumerController> consumerLogger, IConsumerService consumerService)
        {
            _consumerLogger = consumerLogger;
            _consumerService = consumerService;
        }
        /// <summary>
        /// Retrieves a consumer that matches the given TenantCode and MemNbr.
        /// </summary>
        /// <param name="consumerRequestDto">The request data containing the TenantCode and MemNbr.</param>
        /// <returns>A response containing the consumer details or an error message.</returns>
        [HttpPost("consumer/get-consumer-by-mem-id")]
        public async Task<ActionResult<GetConsumerByMemIdResponseDto>> GetConsumerByMemId([FromBody] GetConsumerByMemIdRequestDto consumerRequestDto)
        {
            const string methodName = nameof(GetConsumerByMemId);
            try
            {
                _consumerLogger.LogInformation("{ClassName}.{MethodName}: API - Started with TenantCode: {Tenant}, MemId: {Memnbr}", className, methodName,
                      consumerRequestDto.TenantCode, consumerRequestDto.MemberId);

                var response = await _consumerService.GetConsumerByMemId(consumerRequestDto);

                if (response.ErrorCode != null)
                {
                    _consumerLogger.LogError("{ClassName}.{MethodName}: API Error Occurred while fetching consumer, ErrorCode:{ErrorCode}", className, methodName, response.ErrorCode);
                    return StatusCode((int)response.ErrorCode, response);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                _consumerLogger.LogError(ex, "{ClassName}.{MethodName}: Error Occurred while fetching consumer ,Error Code:{ErrorCode} and ERROR: {Msg}", className, methodName, StatusCodes.Status500InternalServerError, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new GetConsumerByMemIdResponseDto() { ErrorMessage = ex.Message, ErrorDescription = ex.InnerException?.Message });
            }
        }

        /// <summary>
        /// Retrieves consumer details based on the provided request data.
        /// </summary>
        /// <param name="consumerRequestDto">The request data containing the consumer code.</param>
        /// <returns>A response containing the consumer details or an error message.</returns>
        [HttpPost("consumer/get-consumer")]
        public async Task<ActionResult<GetConsumerResponseDto>> GetConsumer([FromBody] GetConsumerRequestDto consumerRequestDto)
        {
            var response = new GetConsumerResponseDto();
            const string methodName = nameof(GetConsumer);
            try
            {
                if (!string.IsNullOrEmpty(consumerRequestDto.ConsumerCode))
                {
                    _consumerLogger.LogInformation("{className}.{methodName}: API - Started with ConsumerCode :{ConsumerCode}", className, methodName, consumerRequestDto.ConsumerCode);
                    response = await _consumerService.GetConsumerData(consumerRequestDto);
                }
                if (response.ErrorCode != null)
                {
                    _consumerLogger.LogError("{ClassName}.{MethodName}: API Error Occurred while fetching consumer, ErrorCode:{ErrorCode}", className, methodName, response.ErrorCode);
                    return StatusCode((int)response.ErrorCode, response);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                _consumerLogger.LogError(ex, "{className}.{methodName}: Error Code:{errorCode} and ERROR - msg: {msg}", className, methodName, StatusCodes.Status500InternalServerError, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new GetConsumerResponseDto() { ErrorMessage = ex.Message, ErrorDescription = ex.InnerException?.Message });
            }
        }
    }
}
