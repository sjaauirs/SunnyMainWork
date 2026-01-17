using Microsoft.AspNetCore.Mvc;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Api.Controllers
{
    [Route("api/v1/")]
    [ApiController]
    public class SponsorController : ControllerBase
    {
        private readonly ILogger<SponsorController> _sponsorLogger;
        public readonly ISponsorService _sponsorService;

        const string className = nameof(SponsorController);

        public SponsorController(ILogger<SponsorController> sponsorLogger, ISponsorService sponsorService)
        {
            _sponsorLogger = sponsorLogger;
            _sponsorService = sponsorService;
        }

        /// <summary>
        /// Retrieves all sponsors from the database.
        /// Logs the operation progress and handles errors if they occur.
        /// </summary>
        /// <returns>
        /// An ActionResult containing either a successful response with a list of sponsors 
        /// </returns>
        [HttpGet("sponsors")]
        public async Task<ActionResult<SponsorsResponseDto>> GetSponsors()
        {
            const string methodName = nameof(GetSponsors);
            try
            {
                _sponsorLogger.LogInformation("{ClassName}.{MethodName}: API Started fetching all Sponsors", className, methodName);
                var response = await _sponsorService.GetSponsors();
                if (response.ErrorCode != null)
                {
                    var errorCode = response.ErrorCode;
                    _sponsorLogger.LogError("{className}.{methodName}: API - Error occurred while processing all sponsors, Error Code: {ErrorCode}, Error Msg: {msg}", className, methodName, response.ErrorCode, response.ErrorMessage);
                    return StatusCode((int)errorCode, response);
                }
                _sponsorLogger.LogInformation("{className}.{methodName}: API - Successfully fetched all sponsors", className, methodName);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _sponsorLogger.LogError(ex, "{ClassName}.{MethodName}: An error occurred while fetching all Sponsors. Error Message: {ErrorMessage}, ErrorCode: {ErrorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, new SponsorsResponseDto() { ErrorMessage = ex.Message });
            }
        }

        /// <summary>
        /// Creates a new sponsor for a specific customer in the database.
        /// Logs the operation progress, including request details and success/failure states.
        /// </summary>
        /// <param name="requestDto">
        /// The request data containing the customer code and sponsor details 
        /// </param>
        /// <returns>
        /// An IActionResult containing either a successful response with sponsor creation details 
        /// </returns>
        [HttpPost("sponsor")]
        public async Task<IActionResult> CreateSponsor([FromBody] CreateSponsorDto requestDto)
        {
            const string methodName = nameof(CreateSponsor);
            try
            {
                _sponsorLogger.LogInformation("{ClassName}.{MethodName}: Request started with SponsorCode:{Sponsor}", className, methodName, requestDto.SponsorCode);

                var response = await _sponsorService.CreateSponsor(requestDto);

                if (response.ErrorCode != null)
                {
                    _sponsorLogger.LogError("{ClassName}.{MethodName}: Error occurred while creating Sponsor. Request: {RequestData}, Response: {ResponseData}, ErrorCode: {ErrorCode}", className, methodName, requestDto.ToJson(), response.ToJson(), response.ErrorCode);
                    return StatusCode((int)response.ErrorCode, response);
                }

                _sponsorLogger.LogInformation("{ClassName}.{MethodName}: Sponsor created successful, SponsorCode: {SponsorCode}", className, methodName, requestDto.SponsorCode);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _sponsorLogger.LogError(ex, "{ClassName}.{MethodName}: An error occurred while create Sponsor. Error Message: {ErrorMessage}, ErrorCode: {ErrorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError });
            }
        }
    }
}
