using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Admin.Core.Domain.Constants;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Cohort.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services
{
    public class CohortConsumerService : ICohortConsumerService
    {
        public readonly ILogger<CohortConsumerService> _logger;
        public readonly ICohortClient _cohortClient;
        public const string className = nameof(CohortConsumerService);

        public CohortConsumerService(ILogger<CohortConsumerService> logger, ICohortClient cohortClient)
        {
            _logger = logger;
            _cohortClient = cohortClient;
        }
        /// <summary>
        /// Method to call cohort API for Add consumer from cohort_consumer
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public BaseResponseDto AddConsumerCohort(dynamic request)
        {
            const string methodName = nameof(AddConsumerCohort);
            try
            {
                var cohortConsumerRequestDto = CreateCohortConsumerRequestDto(request);
                if (cohortConsumerRequestDto == null)
                {
                    _logger.LogWarning("{ClassName}.{MethodName}: Invalid data format for adding Consumer To Cohort", className, methodName);
                    return new BaseResponseDto
                    {
                        ErrorCode = StatusCodes.Status400BadRequest,
                        ErrorMessage = "Invalid data format"
                    };
                }
                _logger.LogInformation("{ClassName}.{MethodName}: API Call Add Consumer To Cohort started", className, methodName);

                BaseResponseDto cohortResponse = _cohortClient.Post<BaseResponseDto>("add-consumer", cohortConsumerRequestDto).GetAwaiter().GetResult();
                if (cohortResponse.ErrorCode != null)
                {
                    _logger.LogWarning("{ClassName}.{MethodName}: Error occurred while adding Consumer To Cohort, ErrorCode: {ErrorCode}", className, methodName, cohortResponse.ErrorCode);
                    return cohortResponse;
                }

                _logger.LogInformation("{ClassName}.{MethodName}: Consumer Added to Cohort successfully", className, methodName);
                return cohortResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: Exception occurred while creating cohort. ErrorMessage: {ErrorMessage}, StackTrace: {StackTrace}", className, methodName, ex.Message, ex.StackTrace);
                return new BaseResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError, ErrorMessage = ex.Message };
            }
        }


        /// <summary>
        /// Method to call cohort API for Remove consumer from cohort_consumer
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public BaseResponseDto RemoveConsumerCohort(dynamic request)
        {
            const string methodName = nameof(RemoveConsumerCohort);
            try
            {
                var cohortConsumerRequestDto = CreateCohortConsumerRequestDto(request);
                if (cohortConsumerRequestDto == null)
                {
                    _logger.LogWarning("{ClassName}.{MethodName}: Invalid data format for adding Consumer To Cohort", className, methodName);
                    return new BaseResponseDto
                    {
                        ErrorCode = StatusCodes.Status400BadRequest,
                        ErrorMessage = "Invalid data format"
                    };
                }

                _logger.LogInformation("{ClassName}.{MethodName}: API Call Remove Consumer To Cohort started", className, methodName);

                BaseResponseDto cohortResponse = _cohortClient.Post<BaseResponseDto>("remove-consumer", cohortConsumerRequestDto).GetAwaiter().GetResult();
                if (cohortResponse.ErrorCode != null)
                {
                    _logger.LogWarning("{ClassName}.{MethodName}: Error occurred while removing Consumer from Cohort, ErrorCode: {ErrorCode}", className, methodName, cohortResponse.ErrorCode);
                    return cohortResponse;
                }

                _logger.LogInformation("{ClassName}.{MethodName}: Consumer Removed from Cohort successfully", className, methodName);
                return cohortResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: Exception occurred while removing cohort. ErrorMessage: {ErrorMessage}, StackTrace: {StackTrace}", className, methodName, ex.Message, ex.StackTrace);
                return new BaseResponseDto() { ErrorCode = StatusCodes.Status500InternalServerError, ErrorMessage = ex.Message };
            }
        }

        private CohortConsumerRequestDto? CreateCohortConsumerRequestDto(dynamic request)
        {
            const string methodName = nameof(CreateCohortConsumerRequestDto);
            try
            {
                // Attempt to extract properties safely
                string cohortName = request.CohortName?.ToString();
                string consumerCode = request.ConsumerCode?.ToString();
                string tenantCode = request.TenantCode?.ToString();

                // Validate the extracted values
                if (string.IsNullOrEmpty(cohortName))
                {
                    throw new ArgumentException("CohortName is required and cannot be null or empty.");
                }

                if (string.IsNullOrEmpty(consumerCode))
                {
                    throw new ArgumentException("ConsumerCode is required and cannot be null or empty.");
                }

                if (string.IsNullOrEmpty(tenantCode))
                {
                    throw new ArgumentException("TenantCode is required and cannot be null or empty.");
                }

                // Create and return the DTO if all fields are valid
                return new CohortConsumerRequestDto
                {
                    CohortName = cohortName!,
                    ConsumerCode = consumerCode!,
                    TenantCode = tenantCode!
                };
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: Argument Exception occurred. ErrorMessage: {ErrorMessage}", className, methodName, ex.Message);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: Exception occurred while creating CohortConsumerRequestDto. ErrorMessage: {ErrorMessage}, StackTrace: {StackTrace}", className, methodName, ex.Message, ex.StackTrace);
                return null;
            }
        }
    }
}
