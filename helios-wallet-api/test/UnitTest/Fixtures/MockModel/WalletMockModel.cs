using SunnyRewards.Helios.Wallet.Core.Domain.Models;

namespace SunnyRewards.Helios.Wallet.UnitTest.Fixtures.MockModel
{
    public class WalletMockModel : WalletModel
    {
        public WalletMockModel()
        {
            WalletId = 4;
            WalletTypeId = 1;
            CustomerCode = "cus-04c211b4339348509eaa870cdea59600";
            SponsorCode = "spo-c008f49aa31f4acd9aa6e2114bfb820e";
            TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4";
            WalletCode = "wal-869553609cc84b71977f30bec1882ab3";
            MasterWallet = true;
            WalletName = "MASTER_FUND";
            Active = true;
            ActiveStartTs = DateTime.Now;
            ActiveEndTs = DateTime.Now;
            Balance = 999800.00;
            EarnMaximum = 2000;
            TotalEarned = 500;
            LeftToEarn = 1500;
            CreateTs = DateTime.Now;
            UpdateTs = DateTime.Now;
            CreateUser = "sunny";
            UpdateUser = "sunny rewards";
            DeleteNbr = 0;
            Xmin = 12345;
        }
       


    }
}

