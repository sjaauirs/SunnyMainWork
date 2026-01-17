using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Task.UnitTest.Fixtures.MockDtos
{
    public class ConsumerTaskRequestMockDto: ConsumerTaskRequestDto
    {
        public ConsumerTaskRequestMockDto()
        {
            TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4";
            ConsumerCode = "cmr-c457c5257c59451d8a93ea941a9f2e0a";

        }

    }
}
