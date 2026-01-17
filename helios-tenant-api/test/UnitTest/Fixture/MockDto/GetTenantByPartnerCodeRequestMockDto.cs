using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Tenant.UnitTest.Fixture.MockDto
{
    public class GetTenantByPartnerCodeRequestMockDto: GetTenantByPartnerCodeRequestDto
    {
        public GetTenantByPartnerCodeRequestMockDto()
        {
            PartnerCode = "par-6f222db8ad104cfdbaf59d3c334b2586";
        }
      
    }
}
