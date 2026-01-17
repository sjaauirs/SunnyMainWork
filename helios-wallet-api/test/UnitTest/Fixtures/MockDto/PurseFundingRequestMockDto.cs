using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Wallet.UnitTest.Fixtures.MockDto
{
    public class PurseFundingRequestMockDto : PurseFundingRequestDto
    {
        public PurseFundingRequestMockDto()
        {
            TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4";
            ConsumerCode = "cmr-1ab1f990bdf44ca789bc6963a18004c2";
            MasterWalletType = "wat-4b364fg722f04034cv732b355d84f479";
            ConsumerWalletType = "wat-4b364fg722f04034cv732b355d84f479";
            Amount = 10;
            TransactionDetailType = "BENEFITS";
            RewardDescription = "Upon onboarding, deposit $100 into the OTC Wallet";
            RuleNumber = 4;
        }
    }
}
