using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Infrastructure.Services.Interfaces;

namespace SunnyRewards.Helios.User.Api.Controllers
{
    [Route("api/v1/")]
    [ApiController]
    public class ConsumerFlowProgressController : ControllerBase
    {
        private readonly ILogger<ConsumerFlowProgressController> _logger;
        private readonly IConsumerFlowProgressService _consumerFlowProgressService;
        private const string _className = nameof(ConsumerFlowProgressController);

        public ConsumerFlowProgressController(ILogger<ConsumerFlowProgressController> logger, 
            IConsumerFlowProgressService consumerFlowProgressService)
        {
            _logger = logger;
            _consumerFlowProgressService = consumerFlowProgressService;
        }
        [HttpPost("flows/get-user-flow-status")]
        public async Task<IActionResult> GetConsumerFlowProgressAsync([FromBody] ConsumerFlowProgressRequestDto consumerFlowProgressRequest)
        {
            const string methodName = nameof(GetConsumerFlowProgressAsync);

            try
            {
                _logger.LogInformation(
                    "{_className}.{MethodName}: API - GetConsumerFlowProgress request received with ConsumerCode: {ConsumerCode}, TenantCode: {TenantCode}",
                    _className, methodName, consumerFlowProgressRequest.ConsumerCode, consumerFlowProgressRequest.TenantCode);

                var response = await _consumerFlowProgressService.GetConsumerFlowProgressAsync(consumerFlowProgressRequest);

                if (response.ErrorCode != null)
                {
                    _logger.LogError(
                        "{_className}.{MethodName}: Error occurred while fetching consumer flow progress. Request: {RequestData}, Response: {ResponseData}, ErrorCode: {ErrorCode}",
                        _className, methodName, consumerFlowProgressRequest.ToJson(), response.ToJson(), response.ErrorCode);

                    return StatusCode((int)response.ErrorCode, response);
                }

                _logger.LogInformation(
                    "{_className}.{MethodName}: Consumer flow progress retrieval successful. ConsumerCode: {ConsumerCode}, TenantCode: {TenantCode}",
                    _className, methodName, consumerFlowProgressRequest.ConsumerCode, consumerFlowProgressRequest.TenantCode);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex, "{_className}.{MethodName}: Unexpected error occurred. Msg: {Msg}, ErrorCode: {ErrorCode}",
                    _className, methodName, ex.Message, StatusCodes.Status500InternalServerError);

                return StatusCode(StatusCodes.Status500InternalServerError, new ConsumerFlowProgressResponseDto
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = ex.Message
                });
            }
        }

        /// <summary>
        /// Updates the onboarding flow progress status for a consumer based on the provided data.
        /// </summary>
        /// <param name="UpdateFlowStatusRequestDto">The data containing the consumerCode and new onboarding flow progress information.</param>
        /// <returns>A response containing the updated consumer or an error message.</returns>
        [HttpPost("flows/update-flow-status")]
        public async Task<ActionResult<ConsumerFlowProgressResponseDto>> UpdateFlowStatus([FromBody] UpdateFlowStatusRequestDto updateFlowStatusDto)
        {
            const string methodName = nameof(UpdateFlowStatus);
            try
            {
                _logger.LogInformation("{className}.{methodName}: API -  OnBoardingStatus Update Started with ConsumerCode : {consumerCode}", _className, methodName, updateFlowStatusDto.ConsumerCode);

                var response = await _consumerFlowProgressService.UpdateOnboardingStatusFlow(updateFlowStatusDto);
                if (response.ErrorCode != null)
                {
                    _logger.LogError("{ClassName}.{MethodName}: Error occurred while updating OnBoarding Status flow. Request: {RequestData}, Response: {ResponseData}, ErrorCode: {ErrorCode}", _className, methodName, updateFlowStatusDto.ToJson(), response.ToJson(), response.ErrorCode);
                    return StatusCode((int)response.ErrorCode, response);
                }

                _logger.LogInformation("{ClassName}.{MethodName}: OnBoarding Status flow update successful, ConsumerCode: {ConsumerCode}", _className, methodName, response.ConsumerFlowProgress.ConsumerCode ?? "Consumer not found");

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{className}.{methodName}: ERROR - msg: {Msg}, Error Code:{errorCode}", _className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError, ErrorMessage = ex.Message });
            }
        }
    }
}
