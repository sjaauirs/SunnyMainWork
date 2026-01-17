using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;

namespace Sunny.Benefits.Bff.UnitTest.Fixtures.MockDtos
{
    public class PostGetTransactionsResponseMockDto : PostGetTransactionsResponseDto
    {
        public PostGetTransactionsResponseMockDto()
        {
            Transactions = new List<TransactionEntryDto>
                {
                  new TransactionEntryDto()
                  {
                     Transaction= new TransactionDto()
                     {
                            TransactionId = 5,
                            WalletId = 3,
                            TransactionCode = "ten67766mhggh",
                            TransactionType = "update",
                            PreviousBalance = 800,
                            TransactionAmount = 100,
                            Balance = 400,
                            PrevWalletTxnCode = "pre455rfmmfmg-67",
                            TransactionDetailId = 45,
                            CreateTs = DateTime.Now
                     },
                     TransactionDetail= new TransactionDetailDto()
                     {
                            TransactionDetailId = 1,
                            TransactionDetailType = "sunny",
                            ConsumerCode = "c457c5257c59451d8a93ea941a9f2e0a",
                            TaskRewardCode = "Tas567565kb54",
                            Notes = "five",
                            RedemptionRef = "sucess",
                            RedemptionItemDescription = "update task",
                            CreateTs = DateTime.Now
                     }
                  }
            };
           
        }
    }
}
