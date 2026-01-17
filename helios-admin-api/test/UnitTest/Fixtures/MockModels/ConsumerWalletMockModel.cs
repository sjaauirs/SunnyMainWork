using SunnyRewards.Helios.Wallet.Core.Domain.Models;

namespace SunnyRewards.Helios.Admin.UnitTest.Fixtures.MockModels
{
    public class ConsumerWalletMockModel : ConsumerWalletModel
    {
        public ConsumerWalletMockModel()
        {
            ConsumerWalletId = 1;
            WalletId = 3;
            TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4";
            ConsumerCode = "cus-04c211b4339348509eaa870cdea59600";
            ConsumerRole = "Admin";
            EarnMaximum = (decimal)600.0;
            CreateTs = DateTime.Now;
            UpdateTs = DateTime.Now;
            CreateUser = "sunny";
            UpdateUser = "sunny rewards";
            DeleteNbr = 0;
        }
        public static List<ConsumerWalletModel> consumerWallets()
        {
            return new List<ConsumerWalletModel>()
            {
                new ConsumerWalletMockModel()
            };
        }
    }
}
