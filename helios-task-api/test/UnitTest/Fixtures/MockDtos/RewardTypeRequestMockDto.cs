using SunnyRewards.Helios.Task.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Task.UnitTest.Fixtures.MockDtos
{
    public class RewardTypeRequestMockDto: RewardTypeRequestDto
    {
        public RewardTypeRequestMockDto()
        {
            TaskId = 1;
            TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4";
        }
    }
}
