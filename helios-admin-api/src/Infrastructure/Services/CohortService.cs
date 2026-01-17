using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Admin.Core.Domain.Constants;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Cohort.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using System.Dynamic;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services
{
    public class CohortService : ICohortService
    {
        public readonly ILogger<CohortService> _logger;
        private readonly ICohortClient _cohortClient;
        private const string _className = nameof(CohortService);
        public CohortService(ILogger<CohortService> logger, ICohortClient cohortClient)
        {
            _logger = logger;
            _cohortClient = cohortClient;
        }

        /// <summary>
        /// Creates a cohort based on the provided request data.
        /// Logs the start, success, and any errors during the process, returning appropriate status codes.
        /// </summary>
        /// <param name="createCohortRequestDto">DTO containing details for creating a cohort.</param>
        /// <returns>200 OK with the response data, error-specific status code, or 500 Internal Server Error.</returns>
        public async Task<CreateCohortResponseDto> CreateCohort(CreateCohortRequestDto createCohortRequestDto)
        {
            const string methodName = nameof(CreateCohort); 
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName}: Create cohort process started for CohortCode: {CohortCode}", _className, methodName, createCohortRequestDto.CohortCode);

                var cohortResponse = await _cohortClient.Post<CreateCohortResponseDto>(Constant.CreateCohortAPIUrl, createCohortRequestDto);
                if (cohortResponse.ErrorCode != null)
                {
                    _logger.LogWarning("{ClassName}.{MethodName}: Error occurred while creating cohort, CohortCode: {CohortCode}, ErrorCode: {ErrorCode}", _className, methodName, createCohortRequestDto.CohortCode, cohortResponse.ErrorCode);
                    return cohortResponse;
                }

                _logger.LogInformation("{ClassName}.{MethodName}: Cohort created successfully, CohortCode: {CohortCode}", _className, methodName, createCohortRequestDto.CohortCode);
                return cohortResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: Exception occurred while creating cohort. ErrorMessage: {ErrorMessage}, StackTrace: {StackTrace}", _className, methodName, ex.Message, ex.StackTrace);
                throw;
            }
        }


        /// <summary>
        /// Retrieves cohort data with logging for success and failure cases.
        /// Logs start, success, and errors; returns appropriate status codes based on the response or exceptions.
        /// </summary>
        /// <param name="cohortLogger">Logger for logging cohort retrieval details.</param>
        /// <returns>200 OK with cohort data, error-specific status code, or 500 Internal Server Error.</returns>
        public async Task<GetCohortsResponseDto> GetCohort()
        {
            const string methodName = nameof(GetCohort);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName}: Get cohort started", _className, methodName);

                var cohortResponse = await _cohortClient.Get<GetCohortsResponseDto>(Constant.CreateCohortAPIUrl , new Dictionary<string, long>());
                if (cohortResponse.ErrorCode != null)
                {
                    _logger.LogWarning("{ClassName}.{MethodName}: Error occurred while getting cohort, ErrorCode: {ErrorCode}", _className, methodName, cohortResponse.ErrorCode);
                    return cohortResponse;
                }

                _logger.LogInformation("{ClassName}.{MethodName}: Cohort created successfully", _className, methodName);
                return cohortResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: Exception occurred while creating cohort. ErrorMessage: {ErrorMessage}, StackTrace: {StackTrace}", _className, methodName, ex.Message, ex.StackTrace);
                throw;
            }
        }

        /// <summary>
        /// Updates the cohort.
        /// </summary>
        /// <param name="updateCohortRequestDto">The update cohort request dto.</param>
        /// <returns></returns>
        public async Task<UpdateCohortResponseDto> UpdateCohort(UpdateCohortRequestDto updateCohortRequestDto)
        {
            const string methodName = nameof(UpdateCohort);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName}: Update cohort process started for CohortCode: {CohortCode}", _className, methodName, updateCohortRequestDto.CohortCode);

                var cohortResponse = await _cohortClient.Put<UpdateCohortResponseDto>(Constant.UpdateCohortAPIUrl, updateCohortRequestDto);
                if (cohortResponse.ErrorCode != null)
                {
                    _logger.LogWarning("{ClassName}.{MethodName}: Error occurred while updating cohort, CohortCode: {CohortCode}, ErrorCode: {ErrorCode}", _className, methodName, updateCohortRequestDto.CohortCode, cohortResponse.ErrorCode);
                    return cohortResponse;
                }

                _logger.LogInformation("{ClassName}.{MethodName}: Cohort updated successfully, CohortCode: {CohortCode}", _className, methodName, updateCohortRequestDto.CohortCode);
                return cohortResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: Exception occurred while updating cohort. ErrorMessage: {ErrorMessage}, StackTrace: {StackTrace}", _className, methodName, ex.Message, ex.StackTrace);
                throw;
            }
        }
    }
}
