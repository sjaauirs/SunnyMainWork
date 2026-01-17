using SunnyRewards.Helios.User.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.User.UnitTest.Fixtures.MockDto
{
    public class ConsumerDataMockDto : ConsumerDataDto

    {
        public ConsumerDataMockDto()
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
                MiddleName = "middle name",
            };

            Consumer = new ConsumerDto()
            {
                ConsumerId = 3,
                PersonId = 120,
                TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4",
                ConsumerCode = "cmr-bjuebdf-492f-46i4-bh55-5a2b0134cbc",
                Registered = true,
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
                SubscriberMemberNbr = "78b1bhf61-2e75-4029-8e81-3jjy47654a8",
                RegionCode = "US",
                PlanId = "plan",
                PlanType = "plan",
                SubgroupId = "subgroup",
                SubsciberMemberNbrPrefix = "subsciber",
                MemberNbrPrefix = "member",
                SubscriberOnly = true,
                ConsumerAttribute = "{\n  \"testing\": {\n \"test\": \"find\"\n  }\n}" 
            };

            PersonAddresses = new List<PersonAddressDto>
            {
                new PersonAddressDto
                {
                    PersonAddressId = 1,
                    AddressTypeId = 1001,
                    PersonId = 120,
                    AddressLabel = "Home",
                    Line1 = "123 Main St",
                    Line2 = "Apt 101",
                    City = "New York",
                    State = "NY",
                    PostalCode = "10001",
                    Region = "NorthEast",
                    CountryCode = "US",
                    Country = "United States",
                    Source = "UnitTest",
                    IsPrimary = true,
                    CreateTs = DateTime.UtcNow,
                    UpdateTs = DateTime.UtcNow,
                    CreateUser = "sunny",
                    UpdateUser = "sunny"
                }
            };

            PhoneNumbers = new List<PhoneNumberDto>
            {
                new PhoneNumberDto {
                    PhoneNumberId = 1,
                    PersonId = 1,
                    PhoneTypeId = 101,
                    PhoneNumberCode = "+1",
                    PhoneNumber = "1234567890",
                    IsPrimary = true,
                    IsVerified = true,
                    VerifiedDate = DateTime.UtcNow,
                    Source = "test_source",
                    CreateTs = DateTime.UtcNow,
                    UpdateTs = DateTime.UtcNow,
                    DeleteNbr = 0
                }
            };
        }
    }
}
   