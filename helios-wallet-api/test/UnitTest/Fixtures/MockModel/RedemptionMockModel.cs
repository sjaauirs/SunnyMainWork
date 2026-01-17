using SunnyRewards.Helios.Wallet.Core.Domain.Models;

namespace SunnyRewards.Helios.Wallet.UnitTest.Fixtures.MockModel
{
    public class RedemptionMockModel : RedemptionModel
    {
        public RedemptionMockModel()
        {
            RedemptionId = 3;
            SubTransactionId = 2;
            AddTransactionId = 3;
            RevertSubTransactionId = 4;
            RevertAddTransactionId = 2;
            RedemptionStatus = "Complete";
            Notes = "Ok";
            RedemptionItemDescription = "Gift card redeemed";
            RedemptionRef = "PRIZE_OUT";
            RedemptionStartTs = DateTime.Now;
            RedemptionCompleteTs = DateTime.Now;
            RedemptionRevertTs = DateTime.Now;
            CreateTs = DateTime.Now;
            UpdateTs = DateTime.Now;
            CreateUser = "sunny";
            UpdateUser = "sunny rewards";
            DeleteNbr = 0;
            Xmin = 12345;

        }
    }


}
