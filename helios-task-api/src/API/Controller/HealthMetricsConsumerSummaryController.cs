using Microsoft.AspNetCore.Mvc;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Infrastructure.Services;
using SunnyRewards.Helios.Task.Infrastructure.Services.Interface;

namespace SunnyRewards.Helios.Task.Api.Controller
{
    [Route("api/v1/")]
    [ApiController]
    public class HealthMetricsConsumerSummaryController : ControllerBase
    {
        private readonly ILogger<HealthMetricsConsumerSummaryController> _taskLogger;
        private readonly IHealthMetricsConsumerSummaryService _consumerHealthMatricsService;
        const string className = nameof(HealthMetricsConsumerSummaryController);
       

        public HealthMetricsConsumerSummaryController(ILogger<HealthMetricsConsumerSummaryController> consumerTaskLogger, IHealthMetricsConsumerSummaryService consumerHealthMatricsService)
        {
            _taskLogger = consumerTaskLogger;
            _consumerHealthMatricsService = consumerHealthMatricsService;
        }
        [HttpPost("get-health-metrics")]
        public async Task<ActionResult> getHealthMetrics( [FromBody] HealthMetricsRequestDto tenantData)
        {
            const string methodName = nameof(getHealthMetrics);
            try
            {
                _taskLogger.LogInformation("{className}.{methodName}: API - Enter with tenantCode {tenantCode}", className, methodName, tenantData.tenantCode);

                var response = await _consumerHealthMatricsService.getHealthMetrics(tenantData.tenantCode);
                return response != null && response.HealthMetricsQueryStartTsMap != null ? Ok(response) : NotFound();
            }
            catch (Exception ex)
            {
                _taskLogger.LogError(ex, "{className}.{methodName}: API -  ERROR:{msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
               return StatusCode(StatusCodes.Status500InternalServerError, new HealthMetricsSummaryDto() { ErrorCode = StatusCodes.Status500InternalServerError });
                
            }
        }
    }
}
