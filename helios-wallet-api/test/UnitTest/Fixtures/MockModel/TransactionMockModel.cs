using SunnyRewards.Helios.Wallet.Core.Domain.Models;

namespace SunnyRewards.Helios.Wallet.UnitTest.Fixtures.MockModel
{
    public class TransactionMockModel : TransactionModel
    {
        public TransactionMockModel()
        {
            TransactionId = 1;
            TransactionDetailId = 2;
            WalletId = 5;
            TransactionCode = "trans-11703";
            TransactionType = "success";
            PreviousBalance = 300;
            TransactionAmount = 4000;
            Balance = 1000;
            PrevWalletTxnCode = "pre-67656wal";
            CreateTs = DateTime.Now;
            UpdateTs = DateTime.Now;
            CreateUser = "sunny";
            UpdateUser = "sunny rewards";
            DeleteNbr = 0;

        }
        public static List<TransactionModel> transactionModels()
        {
            return new List<TransactionModel>()
            {
                new TransactionMockModel()
            };
        }
    }
}
