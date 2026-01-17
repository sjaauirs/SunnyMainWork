using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sunny.Benefits.Bff.Core.Domain.Dtos;
using Sunny.Benefits.Bff.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.enums;

namespace Sunny.Benefits.Bff.Api.Controllers
{
    [Route("api/v1/consumer")]
    [ApiController]
    [Authorize]
    public class ConsumerController : ControllerBase
    {
        private readonly ILogger<ConsumerController> _logger;
        private readonly IConsumerService _consumerService;
        private const string className = nameof(ConsumerController);
        public ConsumerController(ILogger<ConsumerController> logger, IConsumerService consumerService)
        {
            _logger = logger;
            _consumerService = consumerService;
        }

        // <summary>
        /// Updates the consumer details asynchronously.
        /// </summary>
        /// <param name="consumerRequestDto">The consumer id and data transfer object containing consumer details to be updated.</param>
        /// <returns>
        /// An <see cref="ActionResult"/> containing the result of the update operation.
        /// If successful, returns an HTTP 200 (OK) response with the updated consumer details.
        /// If an error occurs, returns an appropriate HTTP status code and error details.
        /// </returns>
        [HttpPut("{consumerId}")]
        public async Task<ActionResult> UpdateConsumerAsync(long consumerId, [FromBody] ConsumerRequestDto consumerRequestDto)
        {
            const string methodName = nameof(UpdateConsumerAsync);
            _logger.LogInformation("{ClassName}.{MethodName} : Started updating consumer with ConsumerCode:{Code} and TenantCode:{Tenant}",
                        className, methodName, consumerRequestDto.ConsumerCode, consumerRequestDto.TenantCode);
            try
            {
                var response = await _consumerService.UpdateConsumerAsync(consumerId, consumerRequestDto);
                if (response.ErrorCode != null)
                {
                    var errorCode = response.ErrorCode;
                    _logger.LogError("{ClassName}.{MethodName}: Error occurred while updating consumer with ConsumerCode:{Code} and TenantCode:{Tenant}, ErrorCode: {ErrorCode} and Error Message: {ErrorMessage}",
                        className, methodName, consumerRequestDto.ConsumerCode, consumerRequestDto.TenantCode, response.ErrorCode, response.ErrorMessage);
                    return StatusCode((int)errorCode, response);
                }
                _logger.LogInformation("{ClassName}.{MethodName} : Successfully updated consumer with ConsumerCode:{Code} and TenantCode:{Tenant}",
                  className, methodName, consumerRequestDto.ConsumerCode, consumerRequestDto.TenantCode);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} : Error occurred while updating consumer with ConsumerCode:{Code} and TenantCode:{Tenant},ERROR:{Msg}",
                        className, methodName, consumerRequestDto.ConsumerCode, consumerRequestDto.TenantCode, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new ConsumerResponseDto()
                {
                    ErrorCode = StatusCodes.Status500InternalServerError
                });
            }
        }

        [HttpPut("deactivate-consumer")]
        public async Task<ActionResult<ConsumerResponseDto>> DeactivateConsumer([FromBody] DeactivateConsumerRequestDto requestDto)
        {
            const string methodName = nameof(DeactivateConsumer);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName}: API - Started for ConsumerCode: {ConsumerCode}", className, methodName, requestDto.ConsumerCode);

                var response = await _consumerService.DeactivateConsumer(requestDto);

                if (response.ErrorCode != null)
                {
                    _logger.LogError("{ClassName}.{MethodName}: Error deactivating consumer. Request: {RequestData}, Response: {ResponseData}, ErrorCode: {ErrorCode}",
                        className, methodName, requestDto.ToJson(), response.ToJson(), response.ErrorCode);

                    return StatusCode((int)response.ErrorCode, response);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: ERROR - msg: {Msg}, Error Code:{ErrorCode}",
                    className, methodName, ex.Message, StatusCodes.Status500InternalServerError);

                return StatusCode(StatusCodes.Status500InternalServerError, new ConsumerResponseDto
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = ex.Message
                });
            }
        }

        [HttpPut("reactivate-consumer")]
        public async Task<ActionResult<ConsumerResponseDto>> ReactivateConsumer([FromBody] ReactivateConsumerRequestDto requestDto)
        {
            const string methodName = nameof(ReactivateConsumer);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName}: API - Started for ConsumerCode: {ConsumerCode}", className, methodName, requestDto.ConsumerCode);

                var response = await _consumerService.ReactivateConsumer(requestDto);

                if (response.ErrorCode != null)
                {
                    _logger.LogError("{ClassName}.{MethodName}: Error reactivating consumer. Request: {RequestData}, Response: {ResponseData}, ErrorCode: {ErrorCode}",
                        className, methodName, requestDto.ToJson(), response.ToJson(), response.ErrorCode);

                    return StatusCode((int)response.ErrorCode, response);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: ERROR - msg: {Msg}, Error Code:{ErrorCode}",
                    className, methodName, ex.Message, StatusCodes.Status500InternalServerError);

                return StatusCode(StatusCodes.Status500InternalServerError, new ConsumerResponseDto
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = "An unexpected error occurred while processing the request."
                });
            }
        }

        /// <summary>
        /// Retrieves consumer attributes based on the provided request data.
        /// </summary>
        /// <param name="consumerAttributesRequestDto">The request data containing consumer attribute filters.</param>
        /// <returns>A response containing the consumer attributes or an error message.</returns>
        [HttpPost("consumer-attributes")]
        public async Task<ActionResult<ConsumerAttributesResponseDto>> ConsumerAttributes([FromBody] ConsumerAttributesRequestDto consumerAttributesRequestDto)
        {
            const string methodName = nameof(ConsumerAttributes);
            try
            {
                _logger.LogInformation("{className}.{methodName}:ConsumerAttributes API - Started TenantCode: {tenantCode}", className, methodName, consumerAttributesRequestDto.TenantCode);
                var response = await _consumerService.ConsumerAttributes(consumerAttributesRequestDto);


                if (response.ErrorCode != null)
                {
                    _logger.LogError("{ClassName}.{MethodName}: Error Retrieving consumer Attributes. Request: {RequestData}, Response: {ResponseData}, ErrorCode: {ErrorCode}",
                        className, methodName, consumerAttributesRequestDto.ToJson(), response.ToJson(), response.ErrorCode);

                    return StatusCode((int)response.ErrorCode, response);
                }
                return Ok(response);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: ERROR - msg: {Msg}, Error Code:{ErrorCode}",
                  className, methodName, ex.Message, StatusCodes.Status500InternalServerError);

                return StatusCode(StatusCodes.Status500InternalServerError, new ConsumerAttributesResponseDto
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = "An unexpected error occurred while processing the request."
                });
            }
        }

        /// <summary>
        /// Updates the subscription status of a consumer.
        /// </summary>
        /// <param name="requestDto"></param>
        /// <returns></returns>
        [HttpPost("consumer-subscription-status")]
        public async Task<ActionResult<BaseResponseDto>> UpdateConsumerSubscriptionStatus([FromBody] ConsumerSubscriptionStatusRequestDto requestDto)
        {
            const string methodName = nameof(UpdateConsumerSubscriptionStatus);
            try
            {
                _logger.LogInformation("{className}.{methodName}:ConsumerAttributes API - Started TenantCode: {tenantCode}", className, methodName, requestDto.TenantCode);
                var response = await _consumerService.UpdateConsumerSubscriptionStatus(requestDto);
                if (response.ErrorCode != null)
                {
                    _logger.LogError("{ClassName}.{MethodName}: Error occurred while updating consumer subscription status. Request: {RequestData}, Response: {ResponseData}, ErrorCode: {ErrorCode}", className, methodName, requestDto.ToJson(), response.ToJson(), response.ErrorCode);
                    return StatusCode((int)response.ErrorCode, response);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{className}.{methodName}: API - Error: Error Code:{errorCode} and ERROR - msg: {msg}", className, methodName, StatusCodes.Status500InternalServerError, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponseDto()
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = ex.Message
                });
            }
        }


    }
}
