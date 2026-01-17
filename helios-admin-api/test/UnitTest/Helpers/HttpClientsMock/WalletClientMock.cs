using Moq;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.UnitTest.Fixtures.MockDtos;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.UnitTest.Helpers.HttpClientsMock
{
    public class WalletClientMock : Mock<IWalletClient>
    {
        public WalletClientMock()
        {
           Setup(client => client.Post<PostRewardResponseDto>("wallet/reward", It.IsAny<PostRewardRequestDto>()))
              .ReturnsAsync(new PostRewardResponseMockDto());
        }
    }
}