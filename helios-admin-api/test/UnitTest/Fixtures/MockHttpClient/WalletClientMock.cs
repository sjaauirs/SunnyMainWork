using Moq;
using SunnyRewards.Helios.Admin.Core.Domain.Constants;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.UnitTest.Fixtures.MockDtos;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.UnitTest.Fixtures.MockHttpClient
{
    public class WalletClientMock : Mock<IWalletClient>
    {
        public WalletClientMock()
        {
            Setup(x => x.Post<PostRewardResponseDto>("wallet/reward", It.IsAny<PostRewardRequestDto>()))
                .ReturnsAsync(new PostRewardResponseMockDto());

            Setup(x => x.Post<BaseResponseDto>(Constant.CreateTenantMasterWalletsAPIUrl, It.IsAny<CreateTenantMasterWalletsRequestDto>()))
                .ReturnsAsync(new BaseResponseDto());
        }
    }
}
