using Sunny.Benefits.Bff.Core.Domain.Dtos;

namespace Sunny.Benefits.Bff.UnitTest.Fixtures.MockDtos
{
    public class GetTenantNotificationCategoryResponseMockDto : GetTenantNotificationCategoryResponseDto
    {
        public GetTenantNotificationCategoryResponseMockDto()
        {
            var TenantNotificationCategoryList = new List<TenantNotificationCategoryDto>
            {
                new TenantNotificationCategoryDto
                {
                    TenantNotificationCategoryId = 1,
                    TenantNotificationCategoryCode = "ten-ecada21e57154928a2bb959e8365b8b4",
                    NotificationCategoryId = 1,
                    NotificationCategoryName = "REWARDS"
                }
            };
        }
    }
}
