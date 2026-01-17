using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;
using SunnyRewards.Helios.Tenant.Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Tenant.UnitTest.Fixture.MockDto
{
    internal class CustomerRequestMockDto : CustomerRequestDto
    {
        public CustomerRequestMockDto()
        {
            CustomerCode = "cus-8d9e6f00eec8436a8251d55ff74b1642";
            CustomerLabel = "label";
        }
    }
}
