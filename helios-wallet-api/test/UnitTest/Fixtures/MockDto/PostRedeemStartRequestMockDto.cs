using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Wallet.UnitTest.Fixtures.MockDto
{
    public class PostRedeemStartRequestMockDto : PostRedeemStartRequestDto
    {
        public PostRedeemStartRequestMockDto()
        {
            ConsumerWalletTypeCode = "cnwal-2d62dcaf2aa4424b9ff6c2ddb5895077";
            RedemptionWalletTypeCode = "redwal-3d62dcaf2aa4424b9ff6c2ddb5895077";
            TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4";
            ConsumerCode = "cus-04c211b4339348509eaa870cdea59600";
            RedemptionVendorCode = "PRIZE_OUT1233";
            RedemptionAmount = 5000;
            RedemptionRef = "5";
            RedemptionItemDescription = "Gift card redeemed";
            Notes = "Ok";
            RedemptionItemData = "Dominos";
        }

    }
}
