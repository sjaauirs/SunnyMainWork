using SunnyRewards.Helios.Cohort.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces
{
    public interface ICohortTenantTaskRewardService
    {
        /// <summary>
        /// Creates a Cohort Tenant Task Reward based on the provided request data.
        /// Logs the start, success, and any errors during the creation process.
        /// </summary>
        /// <param name="createCohortTenantTaskRewardDto">DTO containing details for creating a Cohort Tenant Task Reward.</param>
        /// <returns>200 OK with the response data, error-specific status code, or 500 Internal Server Error.</returns>
        Task<CreateCohortTenantTaskRewardResponseDto> CreateCohortTenantTaskReward(CreateCohortTenantTaskRewardDto createCohortTenantTaskRewardDto);

        /// <summary>
        /// Retrieves Cohort Tenant Task Reward based on the provided request data.
        /// Logs the start, success, and any errors during the retrieval process.
        /// </summary>
        /// <param name="tenantTaskReward">DTO containing tenant task reward request details.</param>
        /// <returns>200 OK with the response data, error-specific status code, or 500 Internal Server Error.</returns>
        Task<GetCohortTenantTaskRewardResponseDto> GetCohortTenantTaskReward(GetCohortTenantTaskRewardRequestDto tenantTaskReward);


        /// <summary>
        /// Updates the cohort tenant task reward.
        /// </summary>
        /// <param name="updateCohortTenantTaskRewardDto">The update cohort tenant task reward dto.</param>
        /// <returns></returns>
        Task<UpdateCohortTenantTaskRewardResponseDto> UpdateCohortTenantTaskReward(UpdateCohortTenantTaskRewardRequestDto updateCohortTenantTaskRewardDto);
    }
}
