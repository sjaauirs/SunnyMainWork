using SunnyRewards.Helios.User.Core.Domain.Models;

namespace SunnyRewards.Helios.User.UnitTest.Fixtures.MockModels
{
    public class RoleMockModel : RoleModel
    {
        public RoleMockModel()
        {
            RoleId = 2;
            RoleCode = "rol-46c2740cafc44869a8b1f822bf5fa712";
            RoleName = "subscriber";
            RoleDescription = "Policy and Care subscriber (HoF)";
            CreateTs = DateTime.UtcNow;
            UpdateTs = DateTime.UtcNow;
            CreateUser = "Parshant";
            UpdateUser = "Parshant Sood";
            DeleteNbr = 0;
        }
    }
}
