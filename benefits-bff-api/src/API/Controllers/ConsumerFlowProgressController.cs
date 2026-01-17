using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sunny.Benefits.Bff.Core.Domain.Dtos;
using Sunny.Benefits.Bff.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.User.Core.Domain.Dtos;

namespace Sunny.Benefits.Bff.Api.Controllers
{
    [Route("api/v1/flows/")]
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
        /// <summary>
        /// Gets consumer flow progress for the given request.
        /// </summary>
        /// <param name="consumerFlowProgressRequest">Request containing ConsumerCode and TenantCode.</param>
        /// <returns>
        /// 200 with progress details on success, or appropriate error code with error details.
        /// </returns>
        /// <response code="200">Consumer flow progress retrieved successfully.</response>
        /// <response code="400">Invalid request payload.</response>
        /// <response code="500">Internal server error.</response>
        [HttpPost("get-user-flow-status")]
        public async Task<IActionResult> GetConsumerFlowProgressAsync([FromBody] GetConsumerFlowRequestDto consumerFlowProgressRequest)
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

                return StatusCode(StatusCodes.Status500InternalServerError, new SunnyRewards.Helios.User.Core.Domain.Dtos.ConsumerFlowProgressResponseDto
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
        [HttpPost("update-status")]
        public async Task<ActionResult> UpdateConsumerOnboardingFlowAsync([FromBody] UpdateConsumerFlowRequestDto consumerOnboardingFlowRequestDto)
        {
            const string methodName = nameof(UpdateConsumerOnboardingFlowAsync);
            _logger.LogInformation("{_className}.{MethodName} : Started updating consumer onboarding flow status with ConsumerCode:{Code} and TenantCode:{Tenant}", _className, methodName, consumerOnboardingFlowRequestDto.ConsumerCode, consumerOnboardingFlowRequestDto.TenantCode);
            try
            {
                var response = await _consumerFlowProgressService.UpdateConsumerFlowStatusAsync(consumerOnboardingFlowRequestDto);
                if (response.ErrorCode != null)
                {
                    var errorCode = response.ErrorCode;
                    _logger.LogError("{_className}.{MethodName}: Error occurred while updating consumer with ConsumerCode:{Code} and TenantCode:{Tenant} flow step {FlowStep}, ErrorCode: {ErrorCode} and Error Message: {ErrorMessage}",
                        _className, methodName, consumerOnboardingFlowRequestDto.ConsumerCode, consumerOnboardingFlowRequestDto.TenantCode, consumerOnboardingFlowRequestDto.CurrentStepId, response.ErrorCode, response.ErrorMessage);
                    return StatusCode((int)errorCode, response);
                }
                _logger.LogInformation("{_className}.{MethodName} : Successfully updated consumer flow progress with ConsumerCode:{Code} and TenantCode:{Tenant} {FlowStep} {Status}",
                  _className, methodName, consumerOnboardingFlowRequestDto.ConsumerCode, consumerOnboardingFlowRequestDto.TenantCode, consumerOnboardingFlowRequestDto.CurrentStepId, consumerOnboardingFlowRequestDto.Status);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{_className}.{MethodName} : Error occurred while updating consumer flow progress with ConsumerCode:{Code} and TenantCode:{Tenant} flow step {FlowStep},ERROR:{Msg}",
                        _className, methodName, consumerOnboardingFlowRequestDto.ConsumerCode, consumerOnboardingFlowRequestDto.TenantCode, consumerOnboardingFlowRequestDto.CurrentStepId, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new ConsumerFlowProgressResponseDto()
                {
                    ErrorCode = StatusCodes.Status500InternalServerError
                });
            }
        }
    }
}
