using SunnyBenefits.Fis.Core.Domain.Dtos;

namespace Sunny.Benefits.Bff.UnitTest.Fixtures.MockDtos
{
    public class CardReissueRequestMockDto:CardReissueRequestDto
    {
        public CardReissueRequestMockDto()
        {

            TenantCode= Guid.NewGuid().ToString();
            ConsumerCode= Guid.NewGuid().ToString();
        }
    }
}
