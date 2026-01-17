using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Admin.Core.Domain.Constants;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Cohort.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services
{
    public class CohortTenantTaskRewardService : ICohortTenantTaskRewardService
    {
        public readonly ILogger<CohortTenantTaskRewardService> _logger;
        private readonly ICohortClient _cohortClient;
        private const string _className = nameof(CohortService);
        public CohortTenantTaskRewardService(ILogger<CohortTenantTaskRewardService> logger, ICohortClient cohortClient)
        {
            _logger = logger;
            _cohortClient = cohortClient;
        }

        /// <summary>
        /// Creates a Cohort Tenant Task Reward based on the provided request data.
        /// Logs the start, success, and any errors during the creation process.
        /// </summary>
        /// <param name="createCohortTenantTaskRewardDto">DTO containing details for creating a Cohort Tenant Task Reward.</param>
        /// <returns>200 OK with the response data, error-specific status code, or 500 Internal Server Error.</returns>
        public async Task<CreateCohortTenantTaskRewardResponseDto> CreateCohortTenantTaskReward(CreateCohortTenantTaskRewardDto createCohortTenantTaskRewardDto)
        {
            const string methodName = nameof(CreateCohortTenantTaskReward);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName}: Create CohortTenantTaskReward process started for CohortCode: {CohortCode}, TenantCode: {TenantCode}", _className, methodName, createCohortTenantTaskRewardDto.CohortCode,
                    createCohortTenantTaskRewardDto.CohortTenantTaskReward.TenantCode);

                var cohortResponse = await _cohortClient.Post<CreateCohortTenantTaskRewardResponseDto>(Constant.CreateCohortTenantTaskRewardAPIUrl, createCohortTenantTaskRewardDto);
                if (cohortResponse.ErrorCode != null)
                {
                    _logger.LogWarning("{ClassName}.{MethodName}: Error occurred while creating CohortTenantTaskReward, CohortCode: {CohortCode}, ErrorCode: {ErrorCode}", _className, methodName, createCohortTenantTaskRewardDto.CohortCode, cohortResponse.ErrorCode);
                    return cohortResponse;
                }

                _logger.LogInformation("{ClassName}.{MethodName}: CohortTenantTaskReward created successfully, CohortCode: {CohortCode}", _className, methodName, createCohortTenantTaskRewardDto.CohortCode);
                return cohortResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: Exception occurred while creating CohortTenantTaskReward. ErrorMessage: {ErrorMessage}, StackTrace: {StackTrace}, TenantCode: {TenantCode}", _className, methodName, ex.Message, ex.StackTrace,
                    createCohortTenantTaskRewardDto.CohortTenantTaskReward.TenantCode);
                throw;
            }
        }

        /// <summary>
        /// Retrieves Cohort Tenant Task Reward based on the provided request data.
        /// Logs the start, success, and any errors during the retrieval process.
        /// </summary>
        /// <param name="tenantTaskReward">DTO containing tenant task reward request details.</param>
        /// <returns>200 OK with the response data, error-specific status code, or 500 Internal Server Error.</returns>
        public async Task<GetCohortTenantTaskRewardResponseDto> GetCohortTenantTaskReward(GetCohortTenantTaskRewardRequestDto tenantTaskReward)
        {
            const string methodName = nameof(GetCohortTenantTaskReward);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName}: Get CohortTenantTaskReward process started for  TenantCode: {TenantCode}", _className, methodName,tenantTaskReward.TenantCode);

                var cohortResponse = await _cohortClient.Post<GetCohortTenantTaskRewardResponseDto>(Constant.GetCohortTenantTaskRewardAPIUrl, tenantTaskReward);
                if (cohortResponse.ErrorCode != null)
                {
                    _logger.LogWarning("{ClassName}.{MethodName}: Error occurred while getting CohortTenantTaskReward for  TenantCode: {TenantCode}, ErrorCode: {ErrorCode}", _className, methodName, tenantTaskReward.TenantCode, cohortResponse.ErrorCode);
                    return cohortResponse;
                }

                _logger.LogInformation("{ClassName}.{MethodName}: CohortTenantTaskReward returned successfully, for  TenantCode: {TenantCode}", _className, methodName, tenantTaskReward.TenantCode);
                return cohortResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: Exception occurred while creating CohortTenantTaskReward. ErrorMessage: {ErrorMessage}, StackTrace: {StackTrace}, TenantCode: {TenantCode}", _className, methodName, ex.Message, ex.StackTrace,
                 tenantTaskReward.TenantCode);
                throw;
            }
        }

        /// <summary>
        /// Updates the cohort tenant task reward.
        /// </summary>
        /// <param name="updateCohortTenantTaskRewardDto">The update cohort tenant task reward dto.</param>
        /// <returns></returns>
        public async Task<UpdateCohortTenantTaskRewardResponseDto> UpdateCohortTenantTaskReward(UpdateCohortTenantTaskRewardRequestDto updateCohortTenantTaskRewardDto)
        {
            const string methodName = nameof(CreateCohortTenantTaskReward);
            try
            {
                _logger.LogInformation("{ClassName}.{MethodName}: Update CohortTenantTaskReward process started for CohortCode: {CohortCode}, TenantCode: {TenantCode}", _className, methodName, updateCohortTenantTaskRewardDto.CohortCode,
                    updateCohortTenantTaskRewardDto.CohortTenantTaskReward.TenantCode);

                var cohortResponse = await _cohortClient.Put<UpdateCohortTenantTaskRewardResponseDto>(Constant.CreateCohortTenantTaskRewardAPIUrl, updateCohortTenantTaskRewardDto);
                if (cohortResponse.ErrorCode != null)
                {
                    _logger.LogWarning("{ClassName}.{MethodName}: Error occurred while updating CohortTenantTaskReward, CohortCode: {CohortCode}, ErrorCode: {ErrorCode}", _className, methodName, updateCohortTenantTaskRewardDto.CohortCode, cohortResponse.ErrorCode);
                    return cohortResponse;
                }

                _logger.LogInformation("{ClassName}.{MethodName}: CohortTenantTaskReward updated successfully, CohortCode: {CohortCode}", _className, methodName, updateCohortTenantTaskRewardDto.CohortCode);
                return cohortResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName}: Exception occurred while updating CohortTenantTaskReward. ErrorMessage: {ErrorMessage}, StackTrace: {StackTrace}, TenantCode: {TenantCode}", _className, methodName, ex.Message, ex.StackTrace,
                    updateCohortTenantTaskRewardDto.CohortTenantTaskReward.TenantCode);
                throw;
            }
        }
    }
}
