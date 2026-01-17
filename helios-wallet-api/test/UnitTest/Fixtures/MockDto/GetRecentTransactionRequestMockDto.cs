using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Wallet.UnitTest.Fixtures.MockDto
{
    public class GetRecentTransactionRequestMockDto : GetRecentTransactionRequestDto
    {
        public GetRecentTransactionRequestMockDto()
        {
            WalletId = 2;
            Count = 7;
            ConsumerCode = "consumer-code";
            skipTransactionType=new List<string> { "TYPE_A", "TYPE_B" };
        }
    }
}
