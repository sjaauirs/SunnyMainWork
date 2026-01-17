using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Infrastructure.Services;
using SunnyRewards.Helios.Task.Infrastructure.Services.Interface;

namespace SunnyRewards.Helios.Task.Api.Controller
{
    [Route("api/v1/terms-of-service")]
    [ApiController]
    public class TermsOfServiceController : ControllerBase
    {
        private readonly ILogger<TermsOfServiceController> _logger;
        private readonly ITermsOfServiceService _termsOfServiceService;
        const string className = nameof(TermsOfServiceController);

        public TermsOfServiceController(ILogger<TermsOfServiceController> logger, ITermsOfServiceService termsOfServiceService)
        {
            _logger = logger;
            _termsOfServiceService = termsOfServiceService;
        }
        [HttpPost]
        public async Task<IActionResult> CreateTermsOfService([FromBody] CreateTermsOfServiceRequestDto requestDto)
        {
            const string methodName = nameof(CreateTermsOfService);
            try
            {
                var response = await _termsOfServiceService.CreateTermsOfService(requestDto);
                if (response.ErrorCode != null)
                {
                    _logger.LogError("{ClassName}.{MethodName}: Error occurred while creating TermsOfService,  Request: {RequestData}, Response: {ResponseData}, ErrorCode: {ErrorCode}", className, methodName, requestDto.ToJson(), response.ToJson(), response.ErrorCode);
                    return StatusCode((int)response.ErrorCode, response);
                }
                _logger.LogInformation("{ClassName}.{MethodName}: TermsOfService Create successful for Service Id: {ServiceId}", className, methodName, requestDto.TermsOfServiceId);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: An error occurred during TermsOfDetails Create. Error Message: {ErrorMessage}, ErrorCode: {ErrorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError });
            }
        }
    }
}
