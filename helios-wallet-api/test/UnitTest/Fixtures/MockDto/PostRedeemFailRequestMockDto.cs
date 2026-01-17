using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Wallet.UnitTest.Fixtures.MockDto
{
    public class PostRedeemFailRequestMockDto : PostRedeemFailRequestDto
    {
        public PostRedeemFailRequestMockDto()
        {
            TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4";
            ConsumerCode = "cmr-c69905fc68ce4f36851f877bae38f22e";
            RedemptionVendorCode = "PRIZE_OUT1233";
            RedemptionAmount = 4000;
            RedemptionRef = "PRIZE_OUT";
            Notes = "Ok";

        }
    }
}
