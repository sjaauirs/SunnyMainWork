using SunnyRewards.Helios.User.Core.Domain.Dtos;

namespace SunnyRewards.Helios.User.UnitTest.Fixtures.MockDto
{
    public class GetConsumerResponseMockDto : GetConsumerResponseDto
    {
        public GetConsumerResponseMockDto()
        {
            Consumer = new ConsumerDto()
            {
                ConsumerId = 1,
                PersonId = 1,
                TenantCode = "12345",
                ConsumerCode = "Cmr-56735eryr4527ju878",
                Registered = true,
                Eligible = true,
                RegistrationTs = DateTime.Now,
                EligibleStartTs = DateTime.Now,
                EligibleEndTs = DateTime.Now,
                CreateTs = DateTime.Now,
                UpdateTs = DateTime.Now,
                CreateUser = "sunny",
                UpdateUser = "sunny rewards",
                DeleteNbr = 1,
            };
        }
    }
}
