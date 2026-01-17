using Sunny.Benefits.Bff.Core.Domain.Dtos;

namespace Sunny.Benefits.Bff.UnitTest.Fixtures.MockDtos
{
    public class PatchUserRequestMockDto : PatchUserRequestDto
    {
        public PatchUserRequestMockDto()
        {
            Email = "sunnyreward@gmail.com";
            UserId = "auth0|6564a33ce3178efd5b0e9892";
            DeviceAttrJson = "testing";
            DeviceId = "123";
            DeviceType = "Mobile";
        }
    }
}
