using SunnyRewards.Helios.User.Core.Domain.Models;

namespace SunnyRewards.Helios.User.UnitTest.Fixtures.MockModels
{
    public class PersonAddressMockModel : PersonAddressModel
    {
        public PersonAddressMockModel() 
        {
            PersonAddressId = 1;
            AddressTypeId = 1;
            PersonId = 2;
            Line1 = "123 Main St";
            Line2 = "Apt 4B";
            City = "Springfield";
            State = "IL";
            PostalCode = "62701";
            Country = "US";
            IsPrimary = true;
            CreateTs = DateTime.UtcNow;
            UpdateTs = DateTime.UtcNow;
            CreateUser = "testUser";    
            UpdateUser = "testUser";
        }
    }
}
