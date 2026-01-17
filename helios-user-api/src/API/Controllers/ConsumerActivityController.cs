using Microsoft.AspNetCore.Mvc;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Infrastructure.Services.Interfaces;

namespace SunnyRewards.Helios.User.Api.Controllers
{
    [Route("api/v1/consumer-activity")]
    [ApiController]
    public class ConsumerActivityController : ControllerBase
    {
        private readonly IConsumerActivityService _consumerActivityService;
        private readonly ILogger<ConsumerActivityController> _logger;
        private const string className = nameof(ConsumerActivityController);

        public ConsumerActivityController(ILogger<ConsumerActivityController> logger, IConsumerActivityService consumerActivityService)
        {
            _consumerActivityService = consumerActivityService;
            _logger = logger;
        }
        /// <summary>
        /// Handles the creation of a consumer activity
        /// </summary>
        /// <param name="consumerActivityRequestDto">The request DTO containing consumer activity details, including TenantCode and ConsumerCode.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> containing either the result of the operation on success
        /// or a response with an error code and message on failure.
        /// </returns>
        /// <exception cref="Exception">
        /// Logs and handles any exceptions that occur during the process, returning a 500 status code with error details.
        /// </exception>

        [HttpPost]
        public async Task<IActionResult> CreateConsumerActivityAsync([FromBody] ConsumerActivityRequestDto consumerActivityRequestDto)
        {
            const string methodName = nameof(CreateConsumerActivityAsync);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName}: Started processing for TenantCode:{TenantCode},ConsumerCode:{ConsumerCode}",
                        className, methodName, consumerActivityRequestDto.TenantCode, consumerActivityRequestDto.ConsumerCode);

                var response = await _consumerActivityService.CreateConsumerActivityAsync(consumerActivityRequestDto);

                _logger.LogInformation("{ClassName}.{MethodName}: Successfully created consumer activity for TenantCode:{Code},ConsumerCode:{Consumer}",
                        className, methodName, consumerActivityRequestDto.TenantCode, consumerActivityRequestDto.ConsumerCode);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: Error processing for TenantCode:{TenantCode},ConsumerCode:{ConsumerCode},ErrorCode:{ErrorCode},ERROR:{Error}",
                       className, methodName, consumerActivityRequestDto.TenantCode, consumerActivityRequestDto.ConsumerCode, StatusCodes.Status500InternalServerError, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new ConsumerActivityResponseDto()
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = ex.Message
                });
            }
        }
    }
}
