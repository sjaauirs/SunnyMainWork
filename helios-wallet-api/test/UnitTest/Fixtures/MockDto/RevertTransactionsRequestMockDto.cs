using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Wallet.UnitTest.Fixtures.MockDto
{
    public class RevertTransactionsRequestMockDto : RevertTransactionsRequestDto
    {
        public RevertTransactionsRequestMockDto()
        {
            TenantCode = Guid.NewGuid().ToString("N");
            ConsumerCode = Guid.NewGuid().ToString("N");
        }
    }
}
