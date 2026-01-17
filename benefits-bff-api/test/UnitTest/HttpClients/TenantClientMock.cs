using Moq;
using Sunny.Benefits.Bff.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;
using Sunny.Benefits.Bff.UnitTest.Fixtures.MockDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sunny.Benefits.Bff.UnitTest.HttpClients
{
    public class TenantClientMock : Mock<ITenantClient>
    {
        public TenantClientMock()
        {
          
            Setup(c => c.Post<TenantDto>("tenant/get-by-tenant-code", It.IsAny<GetTenantCodeRequestDto>()))
            .ReturnsAsync(new TenantMockDto());

           

        }
    }
}
