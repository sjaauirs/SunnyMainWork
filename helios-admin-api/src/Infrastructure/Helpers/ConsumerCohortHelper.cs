using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Admin.Infrastructure.Helpers.Interface;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Cohort.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Extensions;
using SunnyRewards.Helios.User.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Infrastructure.Helpers
{
    public class ConsumerCohortHelper : IConsumerCohortHelper
    {
        private const string startLogTemplate = "{className}.{methodName}: Started processing.. Request: {Request}";
        private const string endLogTemplate = "{className}.{methodName}: Ended processing successfully.";
        private const string errorLogTemplate = "{className}.{methodName}: ERROR Msg:{msg}, Error Code:{errorCode}";
        private readonly ILogger<ConsumerCohortHelper> _logger;
        private readonly ICohortClient _cohortClient;
        const string className = nameof(ConsumerCohortHelper);
        public ConsumerCohortHelper(ILogger<ConsumerCohortHelper> logger, ICohortClient cohortClient)
        {
            _logger = logger;
            _cohortClient = cohortClient;

        }

        public async Task<CohortsResponseDto> GetConsumerCohorts(ConsumerCohortsRequestDto requestDto)
        {
            //var requestDto = new ConsumerCohortsRequestDto() { ConsumerCode = consumer.ConsumerCode!, TenantCode = consumer.TenantCode! };
            const string methodName = nameof(GetConsumerCohorts);
            _logger.LogInformation(startLogTemplate, className, methodName, requestDto.ToJson());

            try
            {
                var response = await _cohortClient.Post<CohortsResponseDto>("consumer-cohorts", requestDto);
                _logger.LogInformation(endLogTemplate, className, methodName);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, errorLogTemplate, className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return new CohortsResponseDto { ErrorCode = StatusCodes.Status500InternalServerError, ErrorMessage = "Internal Server Error" };
            }

        }


        /// <summary>
        /// Method to call cohort API for Add consumer from cohort_consumer
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<BaseResponseDto> AddConsumerCohort(CohortConsumerRequestDto cohortConsumerRequestDto)
        {
            const string methodName = nameof(AddConsumerCohort);
            try
            {
                if (cohortConsumerRequestDto == null)
                {
                    _logger.LogWarning("{ClassName}.{MethodName}: Invalid data format for adding consumer to cohort", className, methodName);
                    return new BaseResponseDto
                    {
                        ErrorCode = StatusCodes.Status400BadRequest,
                        ErrorMessage = "Invalid data format"
                    };
                }

                _logger.LogInformation("{ClassName}.{MethodName}: API call to add consumer to cohort started", className, methodName);

                var cohortResponse = await _cohortClient.Post<BaseResponseDto>("add-consumer", cohortConsumerRequestDto);

                if (cohortResponse.ErrorCode != null)
                {
                    _logger.LogWarning("{ClassName}.{MethodName}: Error while adding consumer to cohort. ErrorCode: {ErrorCode}", className, methodName, cohortResponse.ErrorCode);
                    return cohortResponse;
                }

                _logger.LogInformation("{ClassName}.{MethodName}: Consumer successfully added to cohort", className, methodName);
                return cohortResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: Exception occurred while adding to cohort. ErrorMessage: {ErrorMessage}", className, methodName, ex.Message);
                return new BaseResponseDto
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = ex.Message
                };
            }
        }



        /// <summary>
        /// Method to call cohort API for Remove consumer from cohort_consumer
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<BaseResponseDto> RemoveConsumerCohort(CohortConsumerRequestDto cohortConsumerRequestDto)
        {
            const string methodName = nameof(RemoveConsumerCohort);
            try
            {
                if (cohortConsumerRequestDto == null)
                {
                    _logger.LogWarning("{ClassName}.{MethodName}: Invalid data format for removing consumer from cohort", className, methodName);
                    return new BaseResponseDto
                    {
                        ErrorCode = StatusCodes.Status400BadRequest,
                        ErrorMessage = "Invalid data format"
                    };
                }

                _logger.LogInformation("{ClassName}.{MethodName}: API call to remove consumer from cohort started", className, methodName);

                var cohortResponse = await _cohortClient.Post<BaseResponseDto>("remove-consumer", cohortConsumerRequestDto);

                if (cohortResponse.ErrorCode != null)
                {
                    _logger.LogWarning("{ClassName}.{MethodName}: Error while removing consumer from cohort. ErrorCode: {ErrorCode}", className, methodName, cohortResponse.ErrorCode);
                    return cohortResponse;
                }

                _logger.LogInformation("{ClassName}.{MethodName}: Consumer successfully removed from cohort", className, methodName);
                return cohortResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: Exception occurred while removing from cohort. ErrorMessage: {ErrorMessage}", className, methodName, ex.Message);
                return new BaseResponseDto
                {
                    ErrorCode = StatusCodes.Status500InternalServerError,
                    ErrorMessage = ex.Message
                };
            }
        }
    }
}
