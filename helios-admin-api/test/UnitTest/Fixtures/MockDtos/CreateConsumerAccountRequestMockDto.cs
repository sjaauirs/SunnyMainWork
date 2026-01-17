using SunnyBenefits.Fis.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.UnitTest.Fixtures.MockDtos
{
    public class CreateConsumerAccountRequestMockDto : CreateConsumerAccountRequestDto
    {
        public CreateConsumerAccountRequestMockDto()
        {
            TenantCode = Guid.NewGuid().ToString("N"); ;
            ConsumerCode = Guid.NewGuid().ToString("N");
            ProxyNumber = "12345";
        }
    }
}
