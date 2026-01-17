using Sunny.Benefits.Bff.Core.Domain.Dtos;
using SunnyRewards.Helios.NotificationService.Core.Domain.Dtos;

namespace Sunny.Benefits.Bff.Infrastructure.Services.Interfaces
{
    public interface INotificationService
    {
        Task<GetTenantNotificationCategoryResponseDto> GetNotificationCategoryByTenant(string tenantCode);
        Task<ConsumerNotificationPrefResponseDto> GetConsumerNotificationPref(string tenantCode, string consumerCode);
        Task<ConsumerNotificationPrefResponseDto> CreateConsumerNotificationPref(CreateConsumerNotificationPrefRequestDto createConsumerNotificationPrefRequestDto);
        Task<ConsumerNotificationPrefResponseDto> UpdateCustomerNotificationPref(UpdateConsumerNotificationPrefRequestDto updateConsumerNotificationPrefRequestDto);
    }
}
