using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces
{
    public interface ICustomerService
    {
        Task<CustomersReponseDto> GetCustomers();
        Task<BaseResponseDto> CreateCustomer(CreateCustomerDto customerRequestDto);

        /// <summary>
        /// Get tenant sponsor customer by tenant code
        /// </summary>
        /// <param name="tenantCode"></param>
        /// <returns></returns>
        Task<TenantSponsorCustomerResponseDto> GetTenantSponsorCustomer(string tenantCode);
    }
}
