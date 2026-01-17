using SunnyRewards.Helios.User.Core.Domain.Dtos;

namespace Sunny.Benefits.Bff.UnitTest.Fixtures.MockDtos
{
    public class GetConsumerByEmailResponseMockDto : GetConsumerByEmailResponseDto
    {
        public GetConsumerByEmailResponseMockDto()
        {
            Person = new PersonDto()
            {
                PersonId = 120,
                PersonCode = "per-45i741-df2e-4f5-ab26-670d444f1c",
                FirstName = "sunny",
                LastName = "rewards",
                LanguageCode = "en-US",
                MemberSince = DateTime.UtcNow,
                Email = "sunnyrewards@gmail.com",
                City = "New york",
                Country = "US",
                YearOfBirth = 1998,
                PostalCode = "(555)777-8888",
                PhoneNumber = "97867588788",
                Region = "US",
                DOB = DateTime.UtcNow,
                Gender = "Female",
                CreateTs = DateTime.Now,
                UpdateTs = DateTime.Now,
                CreateUser = "sunny",
                UpdateUser = "sunny rewards",
                DeleteNbr = 1,
            };

            Consumer = new ConsumerDto[]
            {
                new ConsumerDto
                {
                ConsumerId = 3,
                PersonId = 120,
                TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4",
                ConsumerCode = "cmr-bjuebdf-492f-46i4-bh55-5a2b0134cbc",
                Registered = false,
                Eligible = true,
                MemberNbr = "78b1bhf61-2e75-4029-8e81-3jjy47654a8",
                RegistrationTs = DateTime.Now,
                EligibleStartTs = DateTime.Now,
                EligibleEndTs = DateTime.Now,
                CreateTs = DateTime.Now,
                UpdateTs = DateTime.Now,
                CreateUser = "sunny",
                UpdateUser = "sunny rewards",
                DeleteNbr = 1,
                }
            };

        }
    }
}
