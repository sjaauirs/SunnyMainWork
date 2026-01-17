using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Task.UnitTest.Fixtures.MockDtos
{
    public class GetRewardTypeConsumerTaskRequestMockDto : GetRewardTypeConsumerTaskRequestDto
    {
        public GetRewardTypeConsumerTaskRequestMockDto()
        {
            TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4";
            ConsumerCode = "cmr-c457c5257c59451d8a93ea941a9f2e0a";
            RewardTypeCode = "trw-8a154edc602c49efb210d67a7bfe22b4";
        }
    }
}
