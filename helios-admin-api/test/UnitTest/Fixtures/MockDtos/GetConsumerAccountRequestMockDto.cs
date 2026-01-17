using SunnyBenefits.Fis.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.UnitTest.Fixtures.MockDtos
{
    public class GetConsumerAccountRequestMockDto : GetConsumerAccountRequestDto
    {
        public GetConsumerAccountRequestMockDto()
        {
            TenantCode = Guid.NewGuid().ToString("N"); ;
            ConsumerCode = Guid.NewGuid().ToString("N");
        }
    }
}
