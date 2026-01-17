using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Cohort.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;

namespace SunnyRewards.Helios.Admin.Api.Controllers
{
    [Route("api/v1/cohort/")]
    [ApiController]
    public class CohortController : ControllerBase
    {
        private readonly ILogger<CohortController> _logger;
        private readonly ICohortService _cohortService;
        private const string className = nameof(CohortController);
        public CohortController(ILogger<CohortController> logger, ICohortService cohortService)
        {
            _logger = logger;
            _cohortService = cohortService;
        }

        /// <summary>
        /// Creates a cohort based on the provided request data.
        /// Logs the start, success, and any errors during the process, returning appropriate status codes.
        /// </summary>
        /// <param name="createCohortRequestDto">DTO containing details for creating a cohort.</param>
        /// <returns>200 OK with the response data, error-specific status code, or 500 Internal Server Error.</returns>
        [HttpPost]
        public async Task<IActionResult> CreateCohort([FromBody] CreateCohortRequestDto createCohortRequestDto)
        {
            const string methodName = nameof(CreateCohort);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName}: Request started with CohortCode: {CohortCode}", className, methodName, createCohortRequestDto.CohortCode);

                var response = await _cohortService.CreateCohort(createCohortRequestDto);

                if (response.ErrorCode != null)
                {
                    _logger.LogError("{ClassName}.{MethodName}: Error occurred during create cohort. Request: {RequestData}, Response: {ResponseData}, ErrorCode: {ErrorCode}", className, methodName, createCohortRequestDto.ToJson(), response.ToJson(), response.ErrorCode);
                    return StatusCode((int)response.ErrorCode, response);
                }

                _logger.LogInformation("{ClassName}.{MethodName}: Cohort created successful for CohortCode: {CohortCode}", className, methodName, createCohortRequestDto.CohortCode);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: An error occurred during cohort creating. Error Message: {ErrorMessage}, ErrorCode: {ErrorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, new ExportCohortResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError });
            }
        }

        /// <summary>
        /// Retrieves cohort data with logging for success and failure cases.
        /// Logs start, success, and errors; returns appropriate status codes based on the response or exceptions.
        /// </summary>
        /// <param name="cohortLogger">Logger for logging cohort retrieval details.</param>
        /// <returns>200 OK with cohort data, error-specific status code, or 500 Internal Server Error.</returns>
        [HttpGet]
        public async Task<ActionResult<GetCohortsResponseDto>> GetCohort()
        {
            const string methodName = nameof(CreateCohort);
            try
            {
                _logger.LogInformation("GetCohort: Request started to get all cohort");
                var response = await _cohortService.GetCohort();
                if (response.ErrorCode != null)
                {
                    _logger.LogError("{ClassName}{methodName}: Error occurred during get Cohort , Response: {ResponseData}, ErrorCode: {ErrorCode}", className, methodName, response.ToJson(), response.ErrorCode);
                    return StatusCode((int)response.ErrorCode, response);
                }

                _logger.LogInformation("{ClassName}.{MethodName}: get Cohort successful", className, methodName);

                return Ok(response); 
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetCohort API: Error:{msg}", ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new GetCohortsResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError });
            }
        }

        /// <summary>
        /// Updates the cohort.
        /// </summary>
        /// <param name="updateCohortRequestDto">The update cohort request dto.</param>
        /// <returns></returns>
        [HttpPut]
        public async Task<IActionResult> UpdateCohort([FromBody] UpdateCohortRequestDto updateCohortRequestDto)
        {
            const string methodName = nameof(UpdateCohort);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName}: Request started with CohortCode: {CohortCode}", className, methodName, updateCohortRequestDto.CohortCode);

                var response = await _cohortService.UpdateCohort(updateCohortRequestDto);

                if (response.ErrorCode != null)
                {
                    _logger.LogError("{ClassName}.{MethodName}: Error occurred during create cohort. Request: {RequestData}, Response: {ResponseData}, ErrorCode: {ErrorCode}", className, methodName, updateCohortRequestDto.ToJson(), response.ToJson(), response.ErrorCode);
                    return StatusCode((int)response.ErrorCode, response);
                }

                _logger.LogInformation("{ClassName}.{MethodName}: Cohort updated successful for CohortCode: {CohortCode}", className, methodName, updateCohortRequestDto.CohortCode);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: An error occurred during cohort updating. Error Message: {ErrorMessage}, ErrorCode: {ErrorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, new UpdateCohortResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError });
            }
        }
    }
}
