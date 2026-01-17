using SunnyRewards.Helios.Task.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Task.UnitTest.Fixtures.MockDtos
{
    public class GetTaskRewardRequestMockDto : GetTaskRewardRequestDto
    {
        public GetTaskRewardRequestMockDto()
        {
            TaskRewardCodes = new List<string>
            {
               "trw-8a154edc602c49efb210d67a7bfe22b4",
                "trw-8a154edc602c49efb210d67a7bfe22b4",
                "trw-8a154edc602c49efb210d67a7bfe22b4"
            };

        }
    }
}
