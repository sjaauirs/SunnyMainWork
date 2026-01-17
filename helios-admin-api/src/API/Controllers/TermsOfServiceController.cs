using Microsoft.AspNetCore.Mvc;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Api.Controllers
{
    [Route("api/v1/terms-of-service")]
    [ApiController]
    public class TermsOfServiceController : ControllerBase
    {
        private readonly ILogger<TermsOfServiceController> _logger;
        private readonly ITermsOfServiceService _termsOfService;
        private const string className = nameof(TermsOfServiceController);

        public TermsOfServiceController(ILogger<TermsOfServiceController> logger, ITermsOfServiceService termsOfService)
        {
            _logger = logger;
            _termsOfService = termsOfService;
        }
        [HttpPost]
        public async Task<IActionResult> CreateTermsOfService(CreateTermsOfServiceRequestDto requestDto)
        {
            const string methodName = nameof(CreateTermsOfService);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName}: Request started with TermsOfServiceId: {Id}", className, methodName, requestDto.TermsOfServiceId);
                var response = await _termsOfService.CreateTermsOfService(requestDto);

                if (response.ErrorCode != null)
                {
                    _logger.LogError("{ClassName}.{MethodName}: Error occurred while creating TermsOfService. Request: {RequestData}, Response: {ResponseData}, ErrorCode: {ErrorCode}", className, methodName, requestDto.ToJson(), response.ToJson(), response.ErrorCode);
                    return StatusCode((int)response.ErrorCode, response);
                }

                _logger.LogInformation("{ClassName}.{MethodName}: TermsOfService created Successful, with ServiceId: {Id}", className, methodName, requestDto.TermsOfServiceId);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: An error occurred while create TermsOfService. Error Message: {ErrorMessage}, ErrorCode: {ErrorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError });
            }
        }
    }
}
