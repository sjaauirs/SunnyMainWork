using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.UnitTest.Fixtures.MockDtos
{
    public class PostRewardResponseMockDto : PostRewardResponseDto
    {
        public PostRewardResponseMockDto()
        {
            SubEntry = new TransactionDto()
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

            };
            AddEntry = new TransactionDto()
            {
                    TransactionId = 7,
                    WalletId = 8,
                    TransactionCode = "ten666mhggh",
                    TransactionType = "update",
                    PreviousBalance = 80,
                    TransactionAmount = 10,
                    Balance = 40,
                    PrevWalletTxnCode = "pre455mmfmg-67",
                    TransactionDetailId = 5,
                    CreateTs = DateTime.Now,
            };
            TransactionDetail = new TransactionDetailDto()
            {
                TransactionDetailId = 1,
                TransactionDetailType = "sunny",
                ConsumerCode = "cmr-c457c5257c59451d8a93ea941a9f2e0a",
                TaskRewardCode = "Tas567565kb54",
                Notes = "five",
                RedemptionRef = "sucesss",
                RedemptionItemDescription = "update task",

            };


        }
    }
}
