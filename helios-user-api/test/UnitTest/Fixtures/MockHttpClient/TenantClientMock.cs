using Moq;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.User.UnitTest.Fixtures.MockDto;

namespace SunnyRewards.Helios.User.UnitTest.Fixtures.MockHttpClient
{
    public class TenantClientMock : Mock<ITenantClient>
    {
        public TenantClientMock()
        {
            Setup(x => x.Post<GetTenantByTenantCodeResponseDto>("tenant/get-by-tenant-code", It.IsAny<GetTenantByTenantCodeRequestDto>()))
            .ReturnsAsync(new GetTenantByTenantCodeResponseMockDto());

        }
    }
}
