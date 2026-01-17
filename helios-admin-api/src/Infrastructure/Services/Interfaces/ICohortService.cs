using SunnyRewards.Helios.Cohort.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using System.Dynamic;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces
{
    public interface ICohortService
    {
        /// <summary>
        /// Creates a cohort based on the provided request data.
        /// Logs the start, success, and any errors during the process, returning appropriate status codes.
        /// </summary>
        /// <param name="createCohortRequestDto">DTO containing details for creating a cohort.</param>
        /// <returns>200 OK with the response data, error-specific status code, or 500 Internal Server Error.</returns>
        Task<CreateCohortResponseDto> CreateCohort(CreateCohortRequestDto createCohortRequestDto);

        /// <summary>
        /// Retrieves cohort data with logging for success and failure cases.
        /// Logs start, success, and errors; returns appropriate status codes based on the response or exceptions.
        /// </summary>
        /// <param name="cohortLogger">Logger for logging cohort retrieval details.</param>
        /// <returns>200 OK with cohort data, error-specific status code, or 500 Internal Server Error.</returns>
        Task<GetCohortsResponseDto> GetCohort();

        /// <summary>
        /// Updates the cohort.
        /// </summary>
        /// <param name="updateCohortRequestDto">The update cohort request dto.</param>
        /// <returns></returns>
        Task<UpdateCohortResponseDto> UpdateCohort(UpdateCohortRequestDto updateCohortRequestDto);


    }
}
