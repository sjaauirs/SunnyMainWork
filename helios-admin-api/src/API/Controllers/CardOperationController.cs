using Microsoft.AspNetCore.Mvc;
using SunnyBenefits.Fis.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;

namespace SunnyRewards.Helios.Admin.Api.Controllers
{
    [Route("api/v1")]
    [ApiController]
    public class CardOperationController : ControllerBase
    {
        private readonly ILogger<CardOperationController> _logger;
        private readonly ICardOperationService _cardOperationService;
        private const string className = nameof(CardOperationController);

        public CardOperationController(ILogger<CardOperationController> logger, ICardOperationService cardOperationService)
        {
            _logger = logger;
            _cardOperationService = cardOperationService;
        }

        /// <summary>
        /// Get the status of consumer cards status based on the provided consumer code and tenant code.
        /// </summary>
        /// <param name="getCardStatusRequestDto"></param>
        /// <returns></returns>
        [HttpPost("get-consumer-cards-status")]
        public async Task<ActionResult<List<ConsumerCardStatusDto>>> GetConsumerCardsStatus([FromBody] GetCardStatusRequestDto getCardStatusRequestDto)
        {
            const string methodName = nameof(GetConsumerCardsStatus);
            const string errorMessage = "{ClassName}.{MethodName} - Error occurred while executing cards status,TenantCode:{TenantCode} and ConsumerCode : {ConsumerCode}" +
                " - ErrorCode:{ErrorCode},ERROR: {ErrorMessage}";
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName} - Started processing get Card Statuses by consumer code. ConsumerCode: {ConsumerCode}", className, methodName, getCardStatusRequestDto.ConsumerCode);
                var response = await _cardOperationService.GetConsumerCardsStatus(getCardStatusRequestDto);
                if (response.ErrorCode != null)
                {
                    var errorCode = response.ErrorCode;
                    _logger.LogError(errorMessage, className, methodName, getCardStatusRequestDto.TenantCode, getCardStatusRequestDto.ConsumerCode, response.ErrorCode, response.ErrorMessage);
                    return StatusCode((int)errorCode, response);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Error occurred while processing get proxy entries by consumer code. ConsumerCode: {ConsumerCode}, ErrorCode:{Code},ERROR:{Msg}",
                    className, methodName, getCardStatusRequestDto.ConsumerCode, StatusCodes.Status500InternalServerError, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

    }
}
