using SunnyRewards.Helios.User.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.UnitTest.Fixtures.MockDtos
{
    public class GetConsumerByMemNbrResponseMockDto: GetConsumerByMemIdResponseDto
    {
        public GetConsumerByMemNbrResponseMockDto()
        {
            Consumer = new ConsumerDto()
            {
                ConsumerId = 2,
                PersonId = 2,
                TenantCode = "ten - ecada21e57154928a2bb959e8365b8b4",
                ConsumerCode = "cmr-c69905fc68ce4f36851f877bae38f22e",
                RegistrationTs = DateTime.UtcNow,
                EligibleStartTs = DateTime.UtcNow,
                EligibleEndTs = DateTime.UtcNow,
                CreateTs = DateTime.UtcNow,
                UpdateTs = DateTime.UtcNow,
                CreateUser = "Sunny",
                UpdateUser = "SunnyReward",
                DeleteNbr = 0,
                Registered = true,
                Eligible = true,
                MemberNbr = "60be2228-04f5-417f-9d33-0d1d78d7cb76",
               ConsumerAttribute = null,

            };
        }
    }
}
