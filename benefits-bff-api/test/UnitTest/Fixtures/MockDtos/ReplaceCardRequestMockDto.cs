using SunnyBenefits.Fis.Core.Domain.Dtos;

namespace Sunny.Benefits.Bff.UnitTest.Fixtures.MockDtos
{
    public class ReplaceCardRequestMockDto : ReplaceCardRequestDto
    {
        public ReplaceCardRequestMockDto() 
        {
            TenantCode = Guid.NewGuid().ToString();
            ConsumerCode = Guid.NewGuid().ToString();
        }
    }
}
