using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Admin.Core.Domain.Constants;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services
{
    public class CustomerService : ICustomerService
    {
        public readonly ILogger<CustomerService> _logger;
        private readonly ITenantClient _tenantClient;

        public const string className = nameof(CustomerService);

        public CustomerService(ILogger<CustomerService> logger, ITenantClient tenantClient)
        {
            _logger = logger;
            _tenantClient = tenantClient;
        }

        public async Task<BaseResponseDto> CreateCustomer(CreateCustomerDto customerRequestDto)
        {
            return await _tenantClient.Post<BaseResponseDto>(Constant.Customer, customerRequestDto);
        }

        public async Task<CustomersReponseDto> GetCustomers()
        {
            IDictionary<string, long> parameters = new Dictionary<string, long>();
            return await _tenantClient.Get<CustomersReponseDto>(Constant.Customers, parameters);
        }

        public async Task<TenantSponsorCustomerResponseDto> GetTenantSponsorCustomer(string tenantCode)
        {
            IDictionary<string, long> parameters = new Dictionary<string, long>();
            return await _tenantClient.Get<TenantSponsorCustomerResponseDto>($"{Constant.GetTenantSponsorCustomer}/{tenantCode}", parameters);
        }
    }
}
