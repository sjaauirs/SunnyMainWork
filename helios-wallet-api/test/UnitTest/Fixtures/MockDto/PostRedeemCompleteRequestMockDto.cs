using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Wallet.UnitTest.Fixtures.MockDto
{
    public class PostRedeemCompleteRequestMockDto : PostRedeemCompleteRequestDto
    {
        public PostRedeemCompleteRequestMockDto()
        {
            ConsumerCode = "cus-04c211b4339348509eaa870cdea59600";
            RedemptionVendorCode = "PRIZE_OUT1233";
            RedemptionRef = "PRIZE_OUT";
        }
    }
}
