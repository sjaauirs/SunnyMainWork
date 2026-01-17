using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SunnyBenefits.Health.Core.Domains.Dtos;
using SunnyRewards.Helios.Admin.Core.Domain.Constants;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services
{
    public class HealthMetricService : IHealthMetricService
    {
        private readonly ILogger<HealthMetricService> _logger;
        private readonly IHealthClient _healthClient;
        private const string className = nameof(HealthMetricService);
        public HealthMetricService(ILogger<HealthMetricService> logger, IHealthClient healthClient)
        {
            _logger = logger;
            _healthClient = healthClient;
        }

        /// <summary>
        /// Saves the health metrics.
        /// </summary>
        /// <param name="healthMetricMessageRequestDto">The health metric message request dto.</param>
        /// <returns></returns>
        public async Task<BaseResponseDto> SaveHealthMetrics(HealthMetricMessageRequestDto healthMetricMessageRequestDto)
        {
            const string methodName = nameof(SaveHealthMetrics);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName}: Started processing health metrics.", className, methodName);

                if (healthMetricMessageRequestDto == null || healthMetricMessageRequestDto.HealthMetricMessages == null || !healthMetricMessageRequestDto.HealthMetricMessages.Any())
                {
                    _logger.LogWarning("{ClassName}.{MethodName}: The request is null or contains no health metric messages.", className, methodName);

                    return new BaseResponseDto
                    {
                        ErrorCode = StatusCodes.Status400BadRequest,
                        ErrorMessage = "Request is null or contains no health metric messages."
                    };
                }

                _logger.LogInformation("{ClassName}.{MethodName}: Calling the Health Metrics API at {ApiUrl} with {MessageCount} messages.",
                                        className, methodName, Constant.HealthMetricsAPIUrl, healthMetricMessageRequestDto.HealthMetricMessages.Count);

                var healthMetricResponse = await _healthClient.Post<BaseResponseDto>(Constant.HealthMetricsAPIUrl, healthMetricMessageRequestDto);

                if (healthMetricResponse.ErrorCode != null)
                {
                    _logger.LogError("{ClassName}.{MethodName}: API returned an error. ErrorCode: {ErrorCode}, ErrorMessage: {ErrorMessage}",
                                      className, methodName, healthMetricResponse.ErrorCode, healthMetricResponse.ErrorMessage);

                    return healthMetricResponse;
                }

                _logger.LogInformation("{ClassName}.{MethodName}: Successfully processed health metrics. API response: {Response}",
                                        className, methodName, healthMetricResponse);

                return healthMetricResponse;
            }
            catch (Exception ex)
            {
                const int errorCode = StatusCodes.Status500InternalServerError;
                _logger.LogError(ex, "{ClassName}.{MethodName}: Unexpected error occurred. ErrorCode: {ErrorCode}, ErrorMessage: {ErrorMessage}",
                                  className, methodName, errorCode, ex.Message);
                return new BaseResponseDto
                {
                    ErrorCode = errorCode,
                    ErrorMessage = ex.Message
                };
            }
        }
    }
}
