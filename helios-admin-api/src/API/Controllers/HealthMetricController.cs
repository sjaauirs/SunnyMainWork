using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SunnyBenefits.Health.Core.Domains.Dtos;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;

namespace SunnyRewards.Helios.Admin.Api.Controllers
{
    [Route("/api/v1/")]
    [ApiController]
    public class HealthMetricController : ControllerBase
    {
        private readonly ILogger<HealthMetricController> _healthMetricControllerLogger;
        private readonly IHealthMetricService _healthMetricService;

        public HealthMetricController(ILogger<HealthMetricController> healthMetricControllerLogger, IHealthMetricService healthMetricService)
        {
            _healthMetricControllerLogger = healthMetricControllerLogger;
            _healthMetricService = healthMetricService;
        }
        const string className = nameof(HealthMetricController);
    

        /// <summary>
        /// Saves the health metrics.
        /// </summary>
        /// <param name="healthMetricMessageRequestDto">The health metric message request dto.</param>
        /// <returns></returns>
        [HttpPost("health-metrics")]
        public async Task<ActionResult<BaseResponseDto>> SaveHealthMetrics([FromBody] HealthMetricMessageRequestDto healthMetricMessageRequestDto)
        {
            const string methodName = nameof(SaveHealthMetrics);
            try
            {
                _healthMetricControllerLogger.LogInformation("{className}.{methodName}: API - Started to save health metrics", className, methodName);

                var response = await _healthMetricService.SaveHealthMetrics(healthMetricMessageRequestDto);

                if (response.ErrorCode != null)
                {
                    _healthMetricControllerLogger.LogError("{className}.{methodName}: Error occurred while saving health metrics. Request: {request}, Response: {response}, ErrorCode: {errorCode}", className, methodName, healthMetricMessageRequestDto.ToJson(), response.ToJson(), response.ErrorCode);
                    return StatusCode((int)response.ErrorCode, response);
                }

                _healthMetricControllerLogger.LogInformation("{className}.{methodName}: Successfully saved health metrics. Response: {response}", className, methodName, response.ToJson());
                return Ok(response);
            }
            catch (Exception ex)
            {
                const int errorCode = StatusCodes.Status500InternalServerError;
                _healthMetricControllerLogger.LogError(ex, "{className}.{methodName}: ERROR Msg: {msg}, Error Code: {errorCode}", className, methodName, ex.Message, errorCode);
                return StatusCode(errorCode, new BaseResponseDto
                {
                    ErrorMessage = ex.Message,
                    ErrorCode = errorCode
                });
            }
        }
    }
}
