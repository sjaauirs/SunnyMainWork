using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Sunny.Benefits.Bff.Core.Domain.Constants;
using Sunny.Benefits.Bff.Infrastructure.HttpClients.Interfaces;
using Sunny.Benefits.Bff.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Cohort.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sunny.Benefits.Bff.Infrastructure.Services
{
    public class CohortConsumerService : ICohortConsumerService
    {
        private readonly ILogger<CohortConsumerService> _logger;
        private readonly ICohortClient _cohortClient;
        private const string className = nameof(CohortConsumerService);

        private const string startLogTemplate = "{className}.{methodName}: Started processing.. Request: {Request}";
        private const string endLogTemplate = "{className}.{methodName}: Ended processing successfully.";
        private const string errorLogTemplate = "{className}.{methodName}: ERROR Msg:{msg}, Error Code:{errorCode}";

        public CohortConsumerService(ILogger<CohortConsumerService> logger, ICohortClient cohortClient)
        {
            _logger = logger;
            _cohortClient = cohortClient;
        }      

        /// <summary>
        /// Retrieves consumer cohorts based on the given request parameters.
        /// If specific cohorts are not found, returns a 404 status code if no relevant data is found.
        /// </summary>
        /// <param name="requestDto">The request containing TenantCode and ConsumerCode.</param>
        /// <returns>
        /// A <see cref="CohortsResponseDto"/> containing the list of cohorts or an error message with an appropriate status code.
        /// </returns>
        /// <exception cref="Exception">Throws an exception if an unexpected error occurs.</exception>
        public async Task<CohortConsumerResponseDto> GetConsumerCohorts(GetConsumerByCohortsNameRequestDto requestDto, string? requestId = null)
        {
            const string methodName = nameof(GetConsumerCohorts);
            requestId ??= Guid.NewGuid().ToString("N")[..16]; // Generate if not provided
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            try
            {
                var response = await _cohortClient.Post<CohortConsumerResponseDto>(CohortConstants.ConsumerCohorts, requestDto);
                stopwatch.Stop();
                
                _logger.LogInformation("{ClassName}.{MethodName} - Cohort API call completed, TenantCode: {TenantCode}, ConsumerCode: {ConsumerCode}, HttpCallTime: {ElapsedMs}ms, EnrolledCohortsCount: {Count}, RequestId: {RequestId}",
                    className, methodName, requestDto?.TenantCode, requestDto?.ConsumerCode, stopwatch.ElapsedMilliseconds, response?.ConsumerCohorts?.Count ?? 0, requestId);
                return response;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "{ClassName}.{MethodName} - Cohort API call failed, TenantCode: {TenantCode}, ConsumerCode: {ConsumerCode}, ERROR: {Msg}, ErrorCode: {ErrorCode}, HttpCallTime: {ElapsedMs}ms, RequestId: {RequestId}",
                    className, methodName, requestDto?.TenantCode, requestDto?.ConsumerCode, ex.Message, StatusCodes.Status500InternalServerError, stopwatch.ElapsedMilliseconds, requestId);
                return new CohortConsumerResponseDto { ErrorCode = StatusCodes.Status500InternalServerError, ErrorMessage = "Internal Server Error" };
            }

        }

        public async Task<CohortsResponseDto> GetConsumerAllCohorts(string tenantCode, string consumerCode)
        {
            var requestDto = new ConsumerCohortsRequestDto() { ConsumerCode = consumerCode, TenantCode = tenantCode };
            const string methodName = nameof(GetConsumerAllCohorts);
            _logger.LogInformation(startLogTemplate, className, methodName, requestDto.ToJson());

            try
            {
                var response = await _cohortClient.Post<CohortsResponseDto>("consumer-cohorts", requestDto);
                
                // Handle 404 as "no cohorts" - this is expected when consumer has no enrolled cohorts
                if (response?.ErrorCode == StatusCodes.Status404NotFound)
                {
                    _logger.LogInformation("{ClassName}.{MethodName} - No cohorts found for TenantCode: {TenantCode}, ConsumerCode: {ConsumerCode}. Returning empty list.", 
                        className, methodName, tenantCode, consumerCode);
                    return new CohortsResponseDto { Cohorts = new List<CohortDto>() };
                }
                
                // Ensure Cohorts is never null
                if (response != null && response.Cohorts == null)
                {
                    response.Cohorts = new List<CohortDto>();
                }
                
                _logger.LogInformation(endLogTemplate, className, methodName);
                return response ?? new CohortsResponseDto { Cohorts = new List<CohortDto>() };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, errorLogTemplate, className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return new CohortsResponseDto { ErrorCode = StatusCodes.Status500InternalServerError, ErrorMessage = "Internal Server Error", Cohorts = new List<CohortDto>() };
            }

        }
    }

}
