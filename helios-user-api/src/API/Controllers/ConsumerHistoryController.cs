using Microsoft.AspNetCore.Mvc;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Models;
using SunnyRewards.Helios.User.Infrastructure.Services.Interfaces;


namespace SunnyRewards.Helios.User.Api.Controllers
{
    [Route("/api/v1/")]
    [ApiController]
    public class ConsumerHistoryController : ControllerBase
    {
        private readonly ILogger<ConsumerHistoryController> _logger;
        private readonly IConsumerHistoryService _consumerHistoryService;

        /// <summary>
        /// Get Consumer Data Constructor
        /// </summary>
        /// <param name="consumerHistoryLogger"></param>
        /// <param name="consumerHistoryService"></param>
        public ConsumerHistoryController(ILogger<ConsumerHistoryController> consumerHistoryLogger, IConsumerHistoryService consumerHistoryService)
        {
            _logger = consumerHistoryLogger;
            _consumerHistoryService = consumerHistoryService;
        }
        const string className = nameof(ConsumerHistoryController);
        
        [HttpPost("consumer-history")]
        public async Task<ActionResult<BaseResponseDto>> InsertConsumerHistory([FromBody] IList<ConsumerDto> consumers)
        {
            const string methodName = nameof(InsertConsumerHistory);

            try
            {
                var response = await _consumerHistoryService.InsertConsumerHistory(consumers);

                if (response.ErrorCode != null)
                {
                    _logger.LogError(
                        "{ClassName}.{MethodName}: Failed to insert consumer history. ErrorCode: {ErrorCode}, Message: {ErrorMessage}",
                        className, methodName, response.ErrorCode, response.ErrorMessage
                    );

                    return StatusCode((int)response.ErrorCode, response);
                }

                _logger.LogInformation(
                    "{ClassName}.{MethodName}: Successfully inserted consumer history for {Count} consumers.",
                    className, methodName, consumers?.Count ?? 0
                );

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "{ClassName}.{MethodName}: Unhandled exception occurred. Error Code: {ErrorCode}, Message: {Message}",
                    className, methodName, StatusCodes.Status500InternalServerError, ex.Message
                );

                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponseDto
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = ex.Message
                });
            }
        }

    }
}