using Microsoft.AspNetCore.Mvc;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;
using SunnyRewards.Helios.Wallet.Infrastructure.Services.Interfaces;

namespace SunnyRewards.Helios.Wallet.Api.Controllers
{
    [Route("api/v1/purse-funding")]
    [ApiController]
    public class PurseFundingController : ControllerBase
    {
        private readonly ILogger<PurseFundingController> _logger;
        private readonly IPurseFundingService _purseFundingService;
        private const string className = nameof(PurseFundingController);
        public PurseFundingController(ILogger<PurseFundingController> logger, IPurseFundingService purseFundingService)
        {
            _logger = logger;
            _purseFundingService = purseFundingService;
        }

        /// <summary>
        /// Execute funding for given purse and funding rule details
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<PurseFundingResponseDto>> PurseFundingAsync([FromBody] PurseFundingRequestDto request)
        {
            const string methodName = nameof(PurseFundingAsync);
            _logger.LogInformation("{ClassName}.{MethodName}: Starting funding to purse for TenantCode: {TenantCode}, ConsumerCode: {ConsumerCode}",
                    className, methodName, request.TenantCode, request.ConsumerCode);
            try
            {
                var response = await _purseFundingService.PurseFundingAsync(request);

                if (response?.ErrorCode != null)
                {
                    _logger.LogError("{ClassName}.{MethodName}: Funding to purse failed for TenantCode: {TenantCode}, ConsumerCode: {ConsumerCode}, ErrorCode: {ErrorCode}, Request Data: {RequestData}, Response Data: {ResponseData}",
                        className, methodName, request.TenantCode, request.ConsumerCode,  response.ErrorCode, request.ToJson(), response.ToJson());
                    return StatusCode(Convert.ToInt32(response.ErrorCode), response);
                }

                _logger.LogInformation("{ClassName}.{MethodName} - Funding completed for TenantCode: {TenantCode}, ConsumerCode: {ConsumerCode}",
                    className, methodName, request.TenantCode, request.ConsumerCode);
                return Ok(response);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} - Error occurred while processing ConsumerCode: {ConsumerCode}. Message: {ErrorMessage}, ErrorCode: {ErrorCode}",
                    className, methodName, request.ConsumerCode, ex.Message, StatusCodes.Status500InternalServerError);

                return StatusCode(StatusCodes.Status500InternalServerError, new PurseFundingResponseDto
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = "An unexpected error occurred."
                });
            }
        }
    }
}
