using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sunny.Benefits.Bff.Api.Filters;
using Sunny.Benefits.Bff.Core.Domain.Dtos;
using Sunny.Benefits.Bff.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.User.Core.Domain.Dtos;

namespace Sunny.Benefits.Bff.Api.Controllers
{
    [Route("/api/v1/")]
    [ApiController]
    [Authorize]
    public class ConsumerSummaryController : ControllerBase
    {
        private readonly ILogger<ConsumerSummaryController> _logger;
        private readonly IConsumerSummaryService _consumerSummaryService;
        private const string className = nameof(ConsumerSummaryController);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="consumerSummaryLogger"></param>
        /// <param name="consumerSummaryService"></param>
        public ConsumerSummaryController(ILogger<ConsumerSummaryController> consumerSummaryLogger, IConsumerSummaryService consumerSummaryService)
        {
            _logger = consumerSummaryLogger;
            _consumerSummaryService = consumerSummaryService;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="findConsumerconsumerSummaryRequestDto"></param>
        /// <returns></returns>
        [HttpPost("consumer-summary")]
        [ServiceFilter(typeof(ValidateLanguageCodeAttribute))]
        public async Task<ActionResult<ConsumerSummaryResponseDto>> ConsumerSummary(ConsumerSummaryRequestDto consumerSummaryRequestDto)
        {
            const string methodName = nameof(ConsumerSummary);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName} - Started processing ConsumerSummary ConsumerCode : {ConsumerCode}", className, methodName, consumerSummaryRequestDto.consumerCode);
                var response = await _consumerSummaryService.GetConsumerSummary(consumerSummaryRequestDto);
                if (response.ErrorCode != null)
                {
                    _logger.LogError("{ClassName}.{MethodName}: Error occurred while fetching ConsumerSummary. Request: {RequestData}, Response: {ResponseData}, ErrorCode: {ErrorCode}", className, methodName, consumerSummaryRequestDto.ToJson(), response.ToJson(), response.ErrorCode);
                    return StatusCode((int)response.ErrorCode, response);
                }

                _logger.LogInformation("{ClassName}.{MethodName}: ConsumerSummary fetched successful, ConsumerCode: {ConsumerCode}", className, methodName, consumerSummaryRequestDto.consumerCode);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: An error occurred while create tenant. Error Message: {ErrorMessage}, ErrorCode: {ErrorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError , ErrorMessage = ex.Message});
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ConsumerSummaryRequestDto"></param>
        /// <returns></returns>
        [HttpPost("consumer-detail")]
        [ServiceFilter(typeof(ValidateLanguageCodeAttribute))]
        public async Task<ActionResult<GetConsumerByEmailResponseDto>> ConsumerDetail(ConsumerSummaryRequestDto consumerDetailRequestDto)
        {
            const string methodName = nameof(ConsumerDetail);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName} - Started processing ConsumerDetail ConsumerCode : {ConsumerCode}", className, methodName, consumerDetailRequestDto.consumerCode);
                var response = await _consumerSummaryService.GetConsumerDetails(consumerDetailRequestDto);
                if (response.ErrorCode != null)
                {
                    _logger.LogError("{ClassName}.{MethodName}: Error occurred while fetching ConsumerDetail. Request: {RequestData}, Response: {ResponseData}, ErrorCode: {ErrorCode}", className, methodName, consumerDetailRequestDto.ToJson(), response.ToJson(), response.ErrorCode);
                    return StatusCode((int)response.ErrorCode, response);
                }

                _logger.LogInformation("{ClassName}.{MethodName}: ConsumerDetail fetched successful, ConsumerCode: {ConsumerCode}", className, methodName, consumerDetailRequestDto.consumerCode);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: An error occurred while create tenant. Error Message: {ErrorMessage}, ErrorCode: {ErrorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError, ErrorMessage = ex.Message });
            }
        }

    }
}
