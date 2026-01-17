using Moq;
using SunnyRewards.Helios.Admin.Core.Domain.Constants;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.UnitTest.Fixtures.MockDtos;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.UnitTest.Fixtures.MockHttpClient
{
    public class TenantClientMock : Mock<ITenantClient>
    {
        public TenantClientMock()
        {
            Setup(x => x.Post<GetTenantByPartnerCodeResponseDto>("tenant/get-by-partner-code", It.IsAny<string>()))
                .ReturnsAsync(new GetTenantByPartnerCodeResponseDto());
            Setup(x => x.Post<BaseResponseDto>(Constant.CreateTenantAPIUrl, It.IsAny<CreateTenantRequestDto>()))
                .ReturnsAsync(new BaseResponseDto());
        }
    }
}
