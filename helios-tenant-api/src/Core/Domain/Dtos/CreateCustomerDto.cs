using System.ComponentModel.DataAnnotations;

namespace SunnyRewards.Helios.Tenant.Core.Domain.Dtos
{
    public class CreateCustomerDto
    {
        public long CustomerId { get; set; }
        [Required]
        public string CustomerCode { get; set; } = string.Empty;
        [Required]
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerDescription { get; set; } = string.Empty;
    }
}
