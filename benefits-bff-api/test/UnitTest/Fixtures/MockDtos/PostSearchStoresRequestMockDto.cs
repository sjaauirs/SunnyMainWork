using SunnyBenefits.Fis.Core.Domain.Dtos;

namespace Sunny.Benefits.Bff.UnitTest.Fixtures.MockDtos
{
    public class PostSearchStoresRequestMockDto : PostSearchStoresRequestDto
    {
        public PostSearchStoresRequestMockDto()
        {
            TenantCode = Guid.NewGuid().ToString("N");
            ConsumerCode = Guid.NewGuid().ToString("N");
            Latitude = 39.14981849031349;
            Longitude = -75.85184955752078;
            Radius = 60;
        }
    }
}
