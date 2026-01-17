using SunnyRewards.Helios.User.Core.Domain.Models;

namespace SunnyRewards.Helios.User.UnitTest.Fixtures.MockDto
{
    public static class ConsumerLoginTestClass
    {
        public static List<ConsumerLoginModel> ConsumerLoginList()
        {
            var list = new List<ConsumerLoginModel>()
            {
                new ConsumerLoginModel()
                {
                    ConsumerLoginId = 1001,
                    ConsumerId = 2,
                    LoginTs = DateTime.Now,
                    RefreshTokenTs = new DateTime(2023, 08, 10, 11, 30, 05),
                    LogoutTs = DateTime.UtcNow,
                    AccessToken = "ValidToken",
                    CreateTs = DateTime.UtcNow,
                    UpdateTs = DateTime.UtcNow,
                    CreateUser = "Parshant",
                    UpdateUser = "Parshant Sood",
                    DeleteNbr = 0,
                },
                new ConsumerLoginModel()
                {
                    ConsumerLoginId = 1002,
                    ConsumerId = 3,
                    LoginTs = DateTime.Now,
                    RefreshTokenTs = DateTime.UtcNow,
                    LogoutTs = DateTime.UtcNow,
                    AccessToken = "InValidToken",
                    CreateTs = DateTime.UtcNow,
                    UpdateTs = DateTime.UtcNow,
                    CreateUser = "Parshant",
                    UpdateUser = "Parshant Sood",
                    DeleteNbr = 0,
                }
            };
            return list;
        }
    }
}
