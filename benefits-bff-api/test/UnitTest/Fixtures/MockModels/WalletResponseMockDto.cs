using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;

namespace Sunny.Benefits.Bff.UnitTest.Fixtures.MockModels
{
    public class WalletResponseMockDto : WalletResponseDto
    {
        public WalletResponseMockDto()
        {
            GrandTotal = 100.0;

            walletDetailDto = new WalletDetailDto[]
            {
              new WalletDetailDto
              {
                 Wallet = new WalletDto
                 {
                         WalletId = 4,
                         WalletTypeId = 1,
                         CustomerCode = "cus-04c211b4339348509eaa870cdea59600",
                         SponsorCode = "spo-c008f49aa31f4acd9aa6e2114bfb820e",
                         TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4",
                         WalletCode = "wal-869553609cc84b71977f30bec1882ab3",
                         MasterWallet = true,
                         WalletName = "MASTER_FUND_NO",
                         Active = true,
                         ActiveStartTs = DateTime.Now,
                         ActiveEndTs = DateTime.Now,
                         Balance = 999800.00,
                         EarnMaximum = 2000,
                         TotalEarned = 500,
                         LeftToEarn = 1500,
                         PendingTasksTotalRewardAmount = 500,
                         CreateTs = DateTime.Now,
                         CreateUser = "sunny walletDto"
                 },
                WalletType = new WalletTypeDto
                {
                        WalletTypeCode = "test",
                        WalletTypeId = 1,
                        WalletTypeLabel = "test",
                        WalletTypeName= "test",
                        IsExternalSync= true,
                },
                RecentTransaction = new List<TransactionEntryDto>
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
                            CreateTs = DateTime.Now,
                            
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
                            CreateTs = DateTime.Now,
                            
                     }
                  }
                }
              }
            };
        }
    }
}




