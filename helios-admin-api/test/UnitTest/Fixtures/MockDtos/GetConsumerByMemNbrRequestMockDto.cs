using SunnyRewards.Helios.User.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.UnitTest.Fixtures.MockDtos
{
    public class GetConsumerByMemNbrRequestMockDto : GetConsumerByMemIdRequestDto
    {
        public GetConsumerByMemNbrRequestMockDto()
        {
            TenantCode = "ten-56735eryr4527ju878";
            MemberId = "12345678";
        }
    }
}
