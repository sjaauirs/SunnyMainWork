using SunnyRewards.Helios.User.Core.Domain.Models;

namespace SunnyRewards.Helios.User.UnitTest.Fixtures.MockModels
{
    public class PersonRoleMockModel : PersonRoleModel
    {
        public PersonRoleMockModel()
        {
            PersonRoleId = 2;
            PersonId = 2;
            RoleId = 2;
            CreateTs = DateTime.UtcNow;
            UpdateTs = DateTime.UtcNow;
            CreateUser = "Parshant";
            UpdateUser = "Parshant Sood";
            DeleteNbr = 0;
        }
    }
}
