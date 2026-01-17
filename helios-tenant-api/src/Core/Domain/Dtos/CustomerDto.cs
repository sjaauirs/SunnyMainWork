namespace SunnyRewards.Helios.Tenant.Core.Domain.Dtos
{
    public class CustomerDto
    {
        public long CustomerId { get; set; }
        public string CustomerCode { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerDescription { get; set; } = string.Empty;
    }
}
