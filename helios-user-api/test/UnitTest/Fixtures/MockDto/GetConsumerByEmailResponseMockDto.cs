using NHibernate.Linq.ResultOperators;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.User.UnitTest.Fixtures.MockDto
{
    public class GetConsumerByEmailResponseMockDto : GetConsumerByEmailResponseDto
    {
        public GetConsumerByEmailResponseMockDto()
        {
            Person = new PersonDto
            {
                PersonId = 1,
                PersonCode = "per-91532506c783dd4601e1d27704",
                FirstName = "FirstName",
                LastName = "LastName",
                LanguageCode = "en-US-IND",
                MemberSince = DateTime.UtcNow,
                Email = "mock@example.com",
                City = "chd",
                Country = "india",
                YearOfBirth = 1999,
                PostalCode = "0711122",
                PhoneNumber = "8784738747",
                Region = "Region",
                DOB = DateTime.UtcNow,
                Gender = "Male"
            };

            Consumer = new[]
            {
                new ConsumerDto
                {
                    ConsumerId = 1,
                    PersonId = 1,
                    TenantCode = "ten-91532506c8d468e1d27704",
                    ConsumerCode = "cmr--91532578681e1d27704",
                    RegistrationTs = DateTime.UtcNow,
                    EligibleStartTs = DateTime.UtcNow,
                    EligibleEndTs = DateTime.UtcNow.AddDays(30),
                    Registered = false,
                    Eligible = true,
                    MemberNbr = "69-676-815ec8aefa64",
                    ConsumerAttribute = "{\n  \"testing\": {\n    \"string\": \"string\"\n  }\n \"string\": \"string\"\n  }\n}"
                }
            };

        }
    }
}
