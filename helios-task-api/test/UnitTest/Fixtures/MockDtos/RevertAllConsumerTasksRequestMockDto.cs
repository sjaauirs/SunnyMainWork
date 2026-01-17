using SunnyRewards.Helios.Task.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Task.UnitTest.Fixtures.MockDtos
{
    public class RevertAllConsumerTasksRequestMockDto : RevertAllConsumerTasksRequestDto
    {
        public RevertAllConsumerTasksRequestMockDto()
        {
            TenantCode = Guid.NewGuid().ToString("N");
            ConsumerCode = Guid.NewGuid().ToString("N");
        }
    }
}
