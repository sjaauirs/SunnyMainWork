using Sunny.Benefits.Bff.Core.Domain.Dtos;
using SunnyRewards.Helios.NotificationService.Core.Domain.Dtos;

namespace Sunny.Benefits.Bff.UnitTest.Fixtures.MockDtos
{
    public class GetNotificationCategoriesMockDto : GetAllNotificationCategoriesResponseDto
    {
        public GetNotificationCategoriesMockDto()
        {
            NotificationCategoriesList = new List<NotificationCategoryDto>
            {
                new NotificationCategoryDto
                {
                    NotificationCategoryId = 1,
                    NotificationCategoryCode = "ntc-ecada21e57154928a2bb959e8365b8b4",
                    CategoryName = "REWARDS",
                    Description = "REWARDS"
                }
            };
        }
    }
}
