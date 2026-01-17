using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Wallet.UnitTest.Fixtures.MockDto
{
    public class PostRewardRequestMockDto : PostRewardRequestDto
    {
        public PostRewardRequestMockDto()
        {
            MasterWalletTypeCode = "wal-ff13d3db5ad04c64a9a75c2eef359ba7";
            ConsumerWalletTypeCode = "wal-ff13d3db5ad04c64a9a75c2eef359ba7";
            TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4";
            ConsumerCode = "cmr-c69905fc68ce4f36851f877bae38f22e";
            TaskRewardCode = "trw-d8d0f148beef43bcaf471789deda87be";
            RewardDescription = "Test RewardDescription";
            RewardAmount = 500;
        }
    }
}
