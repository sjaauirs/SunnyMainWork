using SunnyRewards.Helios.Task.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Task.UnitTest.Fixtures.MockDtos
{
    public class FindConsumerTaskRequestMockDto : FindConsumerTaskRequestDto
    {
        public FindConsumerTaskRequestMockDto()
        {
            ConsumerCode = "cmr-c69905fc68ce4f36851f877bae38f22e";
            TaskStatus = "in_progress";
        }
    }
}
