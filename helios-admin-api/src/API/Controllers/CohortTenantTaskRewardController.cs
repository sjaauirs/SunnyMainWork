using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SunnyRewards.Helios.Admin.Infrastructure.Services;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Cohort.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;

namespace SunnyRewards.Helios.Admin.Api.Controllers
{
    [Route("api/v1/cohort-tenant-task-reward")]
    [ApiController]
    public class CohortTenantTaskRewardController : ControllerBase
    {
        private readonly ILogger<CohortTenantTaskRewardController> _logger;
        private readonly ICohortTenantTaskRewardService _cohortTenantTaskRewardService;
        private const string className = nameof(CohortTenantTaskRewardController);

        public CohortTenantTaskRewardController(ILogger<CohortTenantTaskRewardController> cohortLogger,
           ICohortTenantTaskRewardService cohortTenantTaskRewardService)
        {
            _logger = cohortLogger;
            _cohortTenantTaskRewardService = cohortTenantTaskRewardService;
        }
        /// <summary>
        /// Creates a Cohort Tenant Task Reward based on the provided request data.
        /// Logs the start, success, and any errors during the creation process.
        /// </summary>
        /// <param name="createCohortTenantTaskRewardDto">DTO containing details for creating a Cohort Tenant Task Reward.</param>
        /// <returns>200 OK with the response data, error-specific status code, or 500 Internal Server Error.</returns>
        [HttpPost]
        public async Task<IActionResult> CreateCohortTenantTaskReward([FromBody] CreateCohortTenantTaskRewardDto createCohortTenantTaskRewardDto)
        {
            const string methodName = nameof(CreateCohortTenantTaskReward);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName}: Request started with CohortCode: {CohortCode}", className, methodName, createCohortTenantTaskRewardDto.CohortCode);

                var response = await _cohortTenantTaskRewardService.CreateCohortTenantTaskReward(createCohortTenantTaskRewardDto);

                if (response.ErrorCode != null)
                {
                    _logger.LogError("{ClassName}.{MethodName}: Error occurred during create CohortTenantTaskReward. Request: {RequestData}, Response: {ResponseData}, ErrorCode: {ErrorCode}", className, methodName, createCohortTenantTaskRewardDto.ToJson(), response.ToJson(), response.ErrorCode);
                    return StatusCode((int)response.ErrorCode, response);
                }

                _logger.LogInformation("{ClassName}.{MethodName}: CohortTenantTaskReward created successful for CohortCode: {CohortCode}", className, methodName, createCohortTenantTaskRewardDto.CohortCode);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: An error occurred during CohortTenantTaskReward creating. Error Message: {ErrorMessage}, ErrorCode: {ErrorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, new ExportCohortResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError });
            }
        }

        /// <summary>
        /// Retrieves Cohort Tenant Task Reward based on the provided request data.
        /// Logs the start, success, and any errors during the retrieval process.
        /// </summary>
        /// <param name="tenantTaskReward">DTO containing tenant task reward request details.</param>
        /// <returns>200 OK with the response data, error-specific status code, or 500 Internal Server Error.</returns>
        [HttpPost("cohort-tenant-task-reward")]
        public async Task<ActionResult<GetCohortTenantTaskRewardResponseDto>> GetCohortTenantTaskReward(GetCohortTenantTaskRewardRequestDto tenantTaskReward)
        {
            const string methodName = nameof(GetCohortTenantTaskReward);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName}: Request started to get CohortTenantTaskReward for TenantCode :{tenantCode}", className, methodName, tenantTaskReward.TenantCode);
                var response = await _cohortTenantTaskRewardService.GetCohortTenantTaskReward(tenantTaskReward);
                if (response.ErrorCode != null)
                {
                    _logger.LogError("{ClassName}{methodName}: Error occurred during get GetCohortTenantTaskReward Request: {RequestData}, Response: {ResponseData}, ErrorCode: {ErrorCode}", className, methodName, tenantTaskReward.ToJson(), response.ToJson(), response.ErrorCode);
                    return StatusCode((int)response.ErrorCode, response);
                }

                _logger.LogInformation("{ClassName}.{MethodName}: get GetCohortTenantTaskReward successful for Request: {RequestData}", className, methodName,tenantTaskReward.ToJson());

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetCohort API: Error:{msg}", ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new GetCohortTenantTaskRewardResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError });
            }
        }

        /// <summary>
        /// Updates the cohort tenant task reward.
        /// </summary>
        /// <param name="updateCohortTenantTaskRewardRequestDto">The update cohort tenant task reward request dto.</param>
        /// <returns></returns>
        [HttpPut]
        public async Task<IActionResult> UpdateCohortTenantTaskReward([FromBody] UpdateCohortTenantTaskRewardRequestDto updateCohortTenantTaskRewardRequestDto)
        {
            const string methodName = nameof(CreateCohortTenantTaskReward);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName}: Request started with CohortCode: {CohortCode}", className, methodName, updateCohortTenantTaskRewardRequestDto.CohortCode);

                var response = await _cohortTenantTaskRewardService.UpdateCohortTenantTaskReward(updateCohortTenantTaskRewardRequestDto);

                if (response.ErrorCode != null)
                {
                    _logger.LogError("{ClassName}.{MethodName}: Error occurred during update CohortTenantTaskReward. Request: {RequestData}, Response: {ResponseData}, ErrorCode: {ErrorCode}", className, methodName, updateCohortTenantTaskRewardRequestDto.ToJson(), response.ToJson(), response.ErrorCode);
                    return StatusCode((int)response.ErrorCode, response);
                }

                _logger.LogInformation("{ClassName}.{MethodName}: CohortTenantTaskReward updated successful for CohortCode: {CohortCode}", className, methodName, updateCohortTenantTaskRewardRequestDto.CohortCode);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: An error occurred during CohortTenantTaskReward updating. Error Message: {ErrorMessage}, ErrorCode: {ErrorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return StatusCode(StatusCodes.Status500InternalServerError, new UpdateCohortTenantTaskRewardResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError });
            }
        }
    }
}
