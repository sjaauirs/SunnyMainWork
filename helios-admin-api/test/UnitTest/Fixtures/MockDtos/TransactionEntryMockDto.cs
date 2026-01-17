using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.UnitTest.Fixtures.MockDtos
{
    public class TransactionEntryMockDto:TransactionEntryDto
    {
        public TransactionEntryMockDto() 
        {
            Transaction = new TransactionDto
            {
                TransactionId = 101,
                WalletId = 555,
                Balance = 100.50,
                CreateTs = DateTime.UtcNow,
            };
            TransactionDetail = new TransactionDetailDto
            {
                TransactionDetailId = 101,
                RewardDescription = "Sample reward",
            };
            TransactionWalletType = new WalletTypeDto
            {
                WalletTypeId = 5,
                WalletTypeCode = "REWARD",
                WalletTypeName = "Reward Wallet"
            };
        }
    }
}
