using SunnyBenefits.Fis.Core.Domain.Dtos;

namespace Sunny.Benefits.Bff.UnitTest.Fixtures.MockDtos
{
    public class GetCardStatusRequestMockDto : GetCardStatusRequestDto
    {
        public GetCardStatusRequestMockDto()
        {
            TenantCode = Guid.NewGuid().ToString();
            ConsumerCode = Guid.NewGuid().ToString();
        }
    }
}
