using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.UnitTest.Fixtures.MockDtos
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
