using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces
{
    public interface ITenantService
    {
        /// <summary>
        /// Creates a new tenant and its associated master wallets.
        /// </summary>
        /// <param name="createTenantRequest">The request data for creating a tenant.</param>
        /// <returns></returns>
        Task<BaseResponseDto> CreateTenant(CreateTenantRequestDto createTenantRequest);
        /// <summary>
        /// Retrieves consumer and person details based on the tenant code provided.
        /// </summary>
        /// <param name="consumerByTenantRequestDto">Contains tenant code, search term, and pagination parameters.</param>
        /// <returns>A paginated list of consumer and person details that match the search criteria.</returns>
        /// <remarks>This method performs optional search, and pagination.</remarks>
        Task<ConsumersAndPersonsListResponseDto> GetConsumersByTenantCode(GetConsumerByTenantRequestDto consumerByTenantRequestDto);
        /// <summary>
        /// Get all tenants from the database where delete_nbr is zero
        /// </summary>
        /// <returns>List of tenants</returns>
        Task<TenantsResponseDto> GetTenants();
        /// <summary>
        /// Updates the existing tenant
        /// </summary>
        /// <param name="updateTenantRequest"> request contains data to update tenant</param>
        /// <returns>returns the updated tenant</returns>
        Task<UpdateTenantResponseDto> UpdateTenant(string tenantCode, UpdateTenantDto updateTenantRequest);

        /// <summary>
        /// Get tenant details by tenant code
        /// </summary>
        /// <param name="tenantCode"></param>
        /// <returns></returns>
        Task<TenantResponseDto> GetTenantDetails(string tenantCode);
    }
}
