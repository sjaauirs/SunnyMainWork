using Sunny.Benefits.Bff.Core.Domain.Dtos;

namespace Sunny.Benefits.Bff.UnitTest.Fixtures.MockDtos
{
    public class AppMetadataMockDto : AppMetadata
    {
        public AppMetadataMockDto()
        {
            ConsumerCode = "cmr-f9c419da974c4bbb99eab99fd3b490e0";
            Env = "test";
            Role = "admin";
            TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4";
        }
    }
}
