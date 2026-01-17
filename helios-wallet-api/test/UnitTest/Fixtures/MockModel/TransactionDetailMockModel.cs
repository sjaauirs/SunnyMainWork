using SunnyRewards.Helios.Wallet.Core.Domain.Models;

namespace SunnyRewards.Helios.Wallet.UnitTest.Fixtures.MockModel
{
    public class TransactionDetailMockModel : TransactionDetailModel
    {
        public TransactionDetailMockModel()
        {
            TransactionDetailId = 2;
            TransactionDetailType = "Withdrawal Transaction";
            ConsumerCode = "cus-04c211b4339348509eaa870cdea59600";
            TaskRewardCode = "trans-Rew-9803";
            Notes = "oK";
            RedemptionRef = "PRIZE_OUT";
            CreateTs = DateTime.Now;
            UpdateTs = DateTime.Now;
            CreateUser = "sunny";
            UpdateUser = "sunny rewards";
            RewardDescription = "Test RewardDescription";
            DeleteNbr = 0;
        }
    }
}
