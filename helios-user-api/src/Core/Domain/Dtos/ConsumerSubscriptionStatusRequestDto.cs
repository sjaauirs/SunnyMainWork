using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;

namespace SunnyRewards.Helios.User.Core.Domain.Dtos
{
    public class ConsumerSubscriptionStatusRequestDto
    {
        public string TenantCode { get; set; } = string.Empty;
        public string ConsumerCode { get; set; } = string.Empty;
        public ConsumerSubscriptionStatusDetailDto[] ConsumerSubscriptionStatuses { get; set; } = new ConsumerSubscriptionStatusDetailDto[0];
    }
}
