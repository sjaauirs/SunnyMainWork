using Moq;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.UnitTest.Fixtures.MockDtos;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.UnitTest.Helpers.HttpClientsMock
{
    public class TenantClientMock : Mock<ITenantClient>
    {
        public TenantClientMock()
        {
            Setup(c => c.Post<GetTenantByPartnerCodeResponseDto>("tenant/get-by-partner-code", It.IsAny<GetTenantByPartnerCodeRequestDto>()))
                .ReturnsAsync(new GetTenantByPartnerCodeResponseMockDto());
            Setup(c => c.Post<TenantDto>(It.IsAny<string>(), It.IsAny<GetTenantCodeRequestDto>()))
                .ReturnsAsync(new TenantDto() { TenantId = 1, TenantCode = "TestCode" });
        }
    }
}