using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Tenant.Core.Domain.Dtos
{
    public class CustomerResponseDto : BaseResponseDto
    {
       public CustomerDto customer { get; set; } = new CustomerDto();
    }
}
