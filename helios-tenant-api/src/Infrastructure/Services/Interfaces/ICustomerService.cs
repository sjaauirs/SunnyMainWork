using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Tenant.Infrastructure.Services.Interfaces
{
    public interface ICustomerService
    {

        /// <summary>
        /// Get customer by tenant code
        /// </summary>
        /// <param name="tenantCode"></param>
        /// <returns></returns>
        Task<CustomerResponseDto> GetSponsorCustomerByTenant(string tenantCode);

        /// <summary>
        ///  Get customerDetail by CustomerCode
        /// </summary>
        /// <param name="customerRequestDto"></param>
        /// <returns></returns>
        Task<CustomerResponseDto> GetTenantCustomerDetails(CustomerRequestDto customerRequestDto);

        Task<CustomersReponseDto> GetAllCustomers();
        Task<SponsorsResponseDto> GetAllSponsors();
        Task<BaseResponseDto> CreateCustomer(CreateCustomerDto customerRequestDto);
        Task<BaseResponseDto> CreateSponsor(CreateSponsorDto createSponsorDto);

        /// <summary>
        /// Get tenant sponsor customer by tenant code
        /// </summary>
        /// <param name="tenantCode"></param>
        /// <returns></returns>
        Task<TenantSponsorCustomerResponseDto> GetTenantSponsorCustomer(string tenantCode);

        Task<CustomerSponsorTenantsResponseDto> GetCustomerSponsorTenants(CustomerSponsorTenantsRequestDto requestDtos);
    }
}
