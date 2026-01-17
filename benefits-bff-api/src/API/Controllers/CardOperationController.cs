using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sunny.Benefits.Bff.Core.Domain.Dtos;
using Sunny.Benefits.Bff.Infrastructure.Services.Interfaces;
using SunnyBenefits.Fis.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;

namespace Sunny.Benefits.Bff.Api.Controllers
{
    [Route("api/v1/fis")]
    [ApiController]
    [Authorize]
    public class CardOperationController : ControllerBase
    {
        private readonly ILogger<CardOperationController> _logger;
        private readonly ICardOperationService _cardOperationService;
        private readonly ICardReissueService _cardReissueService;
        private readonly IReplaceCardService _replaceCardService;
        private const string className=nameof(CardOperationController);

        public CardOperationController(ICardOperationService cardOperationService, ILogger<CardOperationController> logger, ICardReissueService cardReissueService, IReplaceCardService replaceCardService)
        {
            _cardOperationService = cardOperationService;
            _logger = logger;
            _cardReissueService = cardReissueService;
            _replaceCardService = replaceCardService;
        }

        [HttpPost("card-operation")]
        public async Task<IActionResult> ExecuteCardOperation([FromBody] ExecuteCardOperationRequestDto requestDto)
        {
           const string methodName=nameof(ExecuteCardOperation);
           const string errorMessage = "{ClassName}.{MethodName} - Error occurred while processing Card Operation for TenantCode:{TenantCode} and ConsumerCode : {ConsumerCode}, " +
                "ErrorCode:{ErrorCode}, ERROR: {ErrorMessage}";

            _logger.LogInformation("{ClassName}.{MethodName} Started processing Card Operation with TenantCode:{TenantCode} and ConsumerCode : {ConsumerCode}.", className,methodName,requestDto.TenantCode,requestDto.ConsumerCode);
            try
            {
                var response = await _cardOperationService.ExecuteCardOperation(requestDto);
                if (response.ErrorCode != null)
                {
                    var errorCode = response.ErrorCode;
                    _logger.LogError(errorMessage, className, methodName, requestDto.TenantCode, requestDto.ConsumerCode, response.ErrorCode, response.ErrorMessage);
                    return StatusCode((int)errorCode, response);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, errorMessage,
                    className, methodName, requestDto.TenantCode, requestDto.ConsumerCode, StatusCodes.Status500InternalServerError, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new ExecuteCardOperationResponseDto()
                {
                    ErrorCode = StatusCodes.Status500InternalServerError
                });
            }
        }

        [HttpPost("card-status")]
        public async Task<IActionResult> GetCardStatus([FromBody] GetCardStatusRequestDto requestDto)
        {
           const string methodName=nameof(GetCardStatus);
           const string errorMessage = "{ClassName}.{MethodName} - Error occurred while fetching card status for TenantCode:{TenantCode} and ConsumerCode : {ConsumerCode}, " +
                "ErrorCode:{ErrorCode}, ERROR: {ErrorMessage}";

            _logger.LogInformation("{ClassName}.{MethodName} - Started processing Get CardStatus with TenantCode:{TenantCode} and ConsumerCode : {ConsumerCode}.", 
                className,methodName,requestDto.TenantCode,requestDto.ConsumerCode);
            try
            {
                var response = await _cardOperationService.GetCardStatus(requestDto);
                if (response.ErrorCode != null)
                {
                    var errorCode = response.ErrorCode;
                    _logger.LogError(errorMessage, className,methodName,requestDto.TenantCode,requestDto.ConsumerCode, response.ErrorCode, response.ErrorMessage);
                    return StatusCode((int)errorCode, response);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,errorMessage, className, methodName, requestDto.TenantCode, requestDto.ConsumerCode, StatusCodes.Status500InternalServerError,ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new GetCardStatusResponseDto()
                {
                    ErrorCode = StatusCodes.Status500InternalServerError
                });
            }


        }
        [HttpPost("reissue-card")]
        public async Task<IActionResult> ExecuteCardReissue([FromBody] CardReissueRequestDto cardReissueRequestDto)
        {
            const string methodName= nameof(ExecuteCardReissue);
            const string errorMessage = "{ClassName}.{MethodName} - Error occurred while executing card reissue,TenantCode:{TenantCode} and ConsumerCode : {ConsumerCode}" +
                " - ErrorCode:{ErrorCode},ERROR: {ErrorMessage}";

            _logger.LogInformation("{ClassName}.{MethodName} - Started processing Card Reissue with TenantCode:{TenantCode} and ConsumerCode : {ConsumerCode}.", 
                className,methodName,cardReissueRequestDto.TenantCode,cardReissueRequestDto.ConsumerCode);
            try
            {
                var response = await _cardReissueService.ExecuteCardReissue(cardReissueRequestDto);
                if (response.ErrorCode != null)
                {
                    var errorCode = response.ErrorCode;
                    _logger.LogError(errorMessage, className, methodName, cardReissueRequestDto.TenantCode, cardReissueRequestDto.ConsumerCode, response.ErrorCode,response.ErrorMessage);
                    return StatusCode((int)errorCode, response);

                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,errorMessage, className, methodName, cardReissueRequestDto.TenantCode, cardReissueRequestDto.ConsumerCode, StatusCodes.Status500InternalServerError,ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new ExecuteCardReissueResponseDto()
                {
                    ErrorCode = StatusCodes.Status500InternalServerError
                });
            }

        }
        [HttpPost("replace-card")]
        public async Task<IActionResult> ExecuteCardReplacement([FromBody] ReplaceCardRequestDto requestDto)
        {
            const string methodName= nameof(ExecuteCardReplacement);
            const string errorMessage = "{ClassName}.{MethodName} - Error occurred while executing card replacement for TenantCode:{TenantCode} " +
                "and ConsumerCode : {ConsumerCode} - ErrorCode:{ErrorCode}, ERROR: {ErrorMessage}";

            _logger.LogInformation("{ClassName}.{MethodName} - Started processing Card Replacement with TenantCode:{TenantCode} and ConsumerCode : {ConsumerCode}", 
                className,methodName,requestDto.TenantCode,requestDto.ConsumerCode);
            try
            {
                var response = await _replaceCardService.ExecuteCardReplacement(requestDto);
                if (response.ErrorCode != null)
                {
                    var errorCode = response.ErrorCode;
                    _logger.LogError(errorMessage, className, methodName, requestDto.TenantCode, requestDto.ConsumerCode , response.ErrorCode,response.ErrorMessage);
                    return StatusCode((int)errorCode, response);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, errorMessage, className, methodName, requestDto.TenantCode,requestDto.ConsumerCode , StatusCodes.Status500InternalServerError, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new ExecuteReplaceCardResponseDto()
                {
                    ErrorCode = StatusCodes.Status500InternalServerError
                });
            }
        
        }
    }
}
