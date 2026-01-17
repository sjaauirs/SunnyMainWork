using SunnyBenefits.Fis.Core.Domain.Dtos;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sunny.Benefits.Bff.UnitTest.Fixtures.MockDtos
{
    public class ExternalSyncWalletDtoMock : ExternalSyncWalletDto
    {
        public ExternalSyncWalletDtoMock()
        {
            PurseNumber = 12111;
            PurseWalletType = "test";
            Wallet = new WalletDto()
            {
                WalletId = 1,
                WalletTypeId = 1,
                CustomerCode = "customer123",
                SponsorCode = "sponsor456",
                TenantCode = "tenant789",
                WalletCode = "wallet001",
                MasterWallet = true,
                WalletName = "Main Wallet",
                Active = true,
                ActiveStartTs = DateTime.UtcNow,
                ActiveEndTs = DateTime.UtcNow.AddYears(1),
                Balance = 1000.00,
                EarnMaximum = 500.00,
                TotalEarned = 200.00,
                PendingTasksTotalRewardAmount = 100.00,
                LeftToEarn = 300.00,
                CreateTs = DateTime.UtcNow,
                CreateUser = "admin"
            };
        }
    }
}
