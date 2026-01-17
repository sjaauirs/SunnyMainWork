using SunnyRewards.Helios.NotificationService.Core.Domain.Dtos;

namespace Sunny.Benefits.Bff.UnitTest.Fixtures.MockDtos
{
    public class GetConsumerNotificationPrefResponseMockDto : ConsumerNotificationPrefResponseDto
    {
        public GetConsumerNotificationPrefResponseMockDto()
        {
            ConsumerNotificationPrefDto = new SunnyRewards.Helios.NotificationService.Core.Domain.Dtos.ConsumerNotificationPrefDto
            {
                ConsumerNotificationPreferenceId = 12345,
                NotificationConfig = "testConfig",
                UseDefault = false
            };
        }
    }
}
