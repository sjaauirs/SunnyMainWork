using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Wallet.UnitTest.Fixtures.MockDto
{
    public class FindConsumerWalletByWalletTypeRequestMockDto : FindConsumerWalletByWalletTypeRequestDto
    {
        public FindConsumerWalletByWalletTypeRequestMockDto()
        {
            ConsumerCode = "cmr-6a516aa13ad44c139511c607e2087cf4";
            WalletTypeCode = "wat-6a20e80ea6114472b1c0bec2c86f68dc";
        }
    }
}
