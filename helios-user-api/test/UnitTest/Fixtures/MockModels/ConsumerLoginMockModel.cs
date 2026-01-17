using SunnyRewards.Helios.User.Core.Domain.Models;

namespace SunnyRewards.Helios.User.UnitTest.Fixtures.MockModels
{
    public class ConsumerLoginMockModel : ConsumerLoginModel
    {
        public ConsumerLoginMockModel()
        {
            ConsumerLoginId = 1001;
            ConsumerId = 2;
            LoginTs = DateTime.Now;
            RefreshTokenTs = new DateTime(2023, 08, 10, 11, 30, 05);
            LogoutTs = DateTime.UtcNow;
            AccessToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJjb25zdW1lcl9jb2RlIjoiY21yLTNiMjZiNTJmOTIxMzQ3OTE5MzhmMjNhZGU3Njk2NzU1IiwidGVuYW50X2NvZGUiOiJ0ZW4tZWNhZGEyMWU1NzE1NDkyOGEyYmI5NTllODM2NWI4YjQiLCJyb2xlIjoic3Vic2NyaWJlciIsImV4cCI6MTY5MzQ2Njg5NywiZW52IjoiRGV2ZWxvcG1lbnQiLCJpc3MiOiJTdW5ueVJld2FyZHMiLCJhdWQiOiJIZWxpb3MifQ.kLGsFXprhgYUwI2F_cimkBa8y5J-NpAjPX4_0JGnem8";
            CreateTs = DateTime.UtcNow;
            UpdateTs = DateTime.UtcNow;
            CreateUser = "Parshant";
            UpdateUser = "Parshant Sood";
            DeleteNbr = 0;
        }
    }
}