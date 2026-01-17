using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SunnyBenefits.Fis.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Extensions;

namespace SunnyRewards.Helios.Admin.Api.Controllers
{
    [Route("api/v1")]
    [ApiController]
    public class ConsumerAccountController : ControllerBase
    {
        private readonly ILogger<ConsumerAccountController> _logger;
        private readonly IConsumerAccountService _consumerAccountService;
        private const string className = nameof(ConsumerAccountController);

        public ConsumerAccountController(ILogger<ConsumerAccountController> logger, IConsumerAccountService consumerAccountService)
        {
            _logger = logger;
            _consumerAccountService = consumerAccountService;
        }

        /// <summary>
        /// Updates the consumer account configuration based on the provided consumer account update request.
        /// </summary>
        /// <param name="requestDto">The request data transfer object containing the tenant code, consumer code, and the consumer account configuration.</param>
        /// <returns>
        /// /// An <see cref="IActionResult"/> containing:
        /// A 200 OK response with <see cref="ConsumerAccountUpdateResponseDto"/> if the update is successful.
        /// A specific status code (404 Not Found) with an error response if the update fails validation.
        /// A 500 Internal Server Error response if an unexpected error occurs.
        /// </returns>
        [HttpPatch("consumer-account")]
        public async Task<IActionResult> UpdateConsumerAccountConfig([FromBody] ConsumerAccountUpdateRequestDto requestDto)
        {
            const string methodName = nameof(UpdateConsumerAccountConfig);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName} - Started processing Update consumer account. TenantCode: {TenantCode}, ConsumerCode: {ConsumerCode}", className, methodName, requestDto.TenantCode, requestDto.ConsumerCode);
                var response = await _consumerAccountService.UpdateConsumerAccountConfig(requestDto);
                if (response.ErrorCode != null)
                {
                    _logger.LogError("{ClassName}.{MethodName}: Error occurred during Update Consumer Account. Request: {RequestData}, Response: {ResponseData}, ErrorCode: {ErrorCode}", className, methodName, requestDto.ToJson(), response.ToJson(), response.ErrorCode);
                    return StatusCode((int)response.ErrorCode, response);
                }
                _logger.LogInformation("{ClassName}.{MethodName}: Updated Consumer Account successfully for Tenant Code: {TenantCode}, Consumer Code:{Consumer}", className, methodName, requestDto.TenantCode, requestDto.ConsumerCode);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: An error occurred during Update Consumer Accountt. Error Message: {ErrorMessage}, ErrorCode: {ErrorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, new ConsumerAccountResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError });
            }

        }
    }
}
