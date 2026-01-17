using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Tenant.Infrastructure.Services.Interfaces
{
    public interface ITenantService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tenantByPartnerCodeRequestDto"></param>
        /// <returns></returns>
        Task<GetTenantByPartnerCodeResponseDto> GetTenantByPartnerCode(GetTenantByPartnerCodeRequestDto tenantByPartnerCodeRequestDto);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="getTenantCodeRequestDto"></param>
        /// <returns></returns>
        Task<TenantDto> GetByTenantCode(GetTenantCodeRequestDto getTenantCodeRequestDto);

        /// <summary>
        ///  Get Tenant by encryption key id
        /// </summary>
        /// <param name="getTenantCodeRequestDto"></param>
        /// <returns></returns>
        Task<GetTenantByEncKeyIdResponseDto> GetTenantByEncKeyId(GetTenantByEncKeyIdRequestDto getTenantByEncKeyIdRequestDto);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="apiKey"></param>
        /// <returns></returns>
        Task<bool> ValidateApiKey(string apiKey);


        /// <summary>
        /// Creates the tenant.
        /// </summary>
        /// <param name="createTenantRequest">The create tenant request.</param>
        /// <returns></returns>
        Task<BaseResponseDto> CreateTenant(CreateTenantRequestDto createTenantRequest);
        /// <summary>
        /// Retrives all the tenants from database
        /// </summary>
        /// <returns>List of tenants available in database</returns>
        Task<TenantsResponseDto> GetAllTenants();

        Task<UpdateTenantResponseDto> UpdateTenant(string tenantCode, UpdateTenantDto updateTenantRequest);

        /// <summary>
        /// Get tenant details by tenant code
        /// </summary>
        /// <param name="tenantCode"></param>
        /// <returns></returns>
        Task<TenantResponseDto> GetTenantDetails(string tenantCode);

    }
}