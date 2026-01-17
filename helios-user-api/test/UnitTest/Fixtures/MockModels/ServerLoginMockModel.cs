using SunnyRewards.Helios.User.Core.Domain.Models;

namespace SunnyRewards.Helios.User.UnitTest.Fixtures.MockModels
{
    public class ServerLoginMockModel : ServerLoginModel
    {
        public ServerLoginMockModel()
        {
            ServerLoginId = 1001;
            TenantCode = "ce532c17157044d3a20c15d248bd2cbd";
            LoginTs = DateTime.Now;
            RefreshTokenTs = DateTime.UtcNow;
            LogoutTs = null;
            CreateTs = DateTime.UtcNow;
            UpdateTs = DateTime.UtcNow;
            CreateUser = "System";
            UpdateUser = "System";
            DeleteNbr = 0;
        }
    }
}
