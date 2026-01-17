using Microsoft.AspNetCore.DataProtection.KeyManagement;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Tenant.UnitTest.Fixture.MockDto
{
    public class ValidateApiKeyMockDto: TenantDto
    {
        public ValidateApiKeyMockDto()
        {
            ApiKey ="asdfghhhj";
        }
    }
}
