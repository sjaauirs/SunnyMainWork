using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Tenant.Core.Domain.Dtos
{
    public class CustomersReponseDto : BaseResponseDto
    {
        public List<CustomerDto>? Customers { get; set; }
    }
}
