using SunnyRewards.Helios.User.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Admin.UnitTest.Fixtures.MockDtos
{
    public class GetConsumerRequestMockDto: GetConsumerRequestDto
    {
        public GetConsumerRequestMockDto()
        {
            ConsumerCode = "cmr-c457c5257c59451d8a93ea941a9f2e0a";
        }
    }
}
