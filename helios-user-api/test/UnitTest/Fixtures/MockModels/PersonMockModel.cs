using SunnyRewards.Helios.User.Core.Domain.Models;

namespace SunnyRewards.Helios.User.UnitTest.Fixtures.MockModels
{
    public class PersonMockModel : PersonModel
    {
        public PersonMockModel()
        {
            PersonId = 2;
            PersonCode = "per-57f237065a454e25b5de870072baf3d4";
            FirstName = "Kailey";
            LastName = "Joseph";
            LanguageCode = "en-US";
            MemberSince = DateTime.UtcNow;
            CreateTs = DateTime.UtcNow;
            UpdateTs = DateTime.UtcNow;
            CreateUser = "sunny";
            UpdateUser = "sunny rewards";
            DeleteNbr = 0;
            Email = "kailey.j@absentis.com";
            City = null;
            Country = "US";
            YearOfBirth = 1990;
            PostalCode = null;
            PhoneNumber = null;
            Region = " ";
            DOB = DateTime.UtcNow;
            Gender = "Male";
            OnBoardingState = "CARD_LAST_4_VERIFIED";
        }
    }
}
