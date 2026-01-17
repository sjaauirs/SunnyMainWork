using Moq;
using Sunny.Benefits.Bff.Core.Domain.Dtos;
using Sunny.Benefits.Bff.Infrastructure.HttpClients.Interfaces;
using Sunny.Benefits.Bff.UnitTest.Fixtures.MockDtos;
using SunnyRewards.Helios.NotificationService.Core.Domain.Dtos;

namespace Sunny.Benefits.Bff.UnitTest.HttpClients
{
    public class NotificationClientMock : Mock<INotificationClient>
    {
        public NotificationClientMock()
        {
            // Mock GET for GetNotificationCategoryByTenant
            Setup(client => client.GetId<GetTenantNotificationCategoryResponseDto>(
                It.Is<string>(endpoint => endpoint.Contains("tenant-notification-category/get-all-tenant-notification-category-by-tenant-code")),
                It.IsAny<Dictionary<string, string>>()
            )).ReturnsAsync(new GetTenantNotificationCategoryResponseMockDto());

            // Mock GET for GetAllNotificationCategories
            Setup(client => client.GetId<GetAllNotificationCategoriesResponseDto>(
                It.Is<string>(endpoint => endpoint.Contains("notification-category/all-categories")),
                It.IsAny<Dictionary<string, string>>()
            )).ReturnsAsync(new GetNotificationCategoriesMockDto());

            // Mock GET for GetConsumerNotificationPref
            Setup(client => client.GetId<ConsumerNotificationPrefResponseDto>(
                It.Is<string>(endpoint => endpoint.Contains("consumer-notication-pref")),
                It.IsAny<Dictionary<string, string>>()
            )).ReturnsAsync(new GetConsumerNotificationPrefResponseMockDto());

            // Mock POST for CreateConsumerNotificationPref
            Setup(client => client.Post<ConsumerNotificationPrefResponseDto>(
                "consumer-notication-pref/create-consumer-notification-pref",
                It.IsAny<CreateConsumerNotificationPrefRequestDto>()
            )).ReturnsAsync(new GetConsumerNotificationPrefResponseMockDto());

            // Mock PUT for UpdateCustomerNotificationPref
            Setup(client => client.Put<ConsumerNotificationPrefResponseDto>(
                "consumer-notication-pref/update-consumer-notification-pref",
                It.IsAny<UpdateConsumerNotificationPrefRequestDto>()
            )).ReturnsAsync(new GetConsumerNotificationPrefResponseMockDto());
        }
    }
}
