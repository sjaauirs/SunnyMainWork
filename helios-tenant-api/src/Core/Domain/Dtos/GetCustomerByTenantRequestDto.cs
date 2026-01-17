using System.ComponentModel.DataAnnotations;

namespace SunnyRewards.Helios.Tenant.Core.Domain.Dtos
{
    public class GetCustomerByTenantRequestDto
    {
        [Required]
        public required string TenantCode { get; set; }
    }
}
