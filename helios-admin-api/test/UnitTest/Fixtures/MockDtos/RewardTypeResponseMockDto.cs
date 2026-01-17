using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Admin.UnitTest.Fixtures.MockDtos
{
    public class RewardTypeResponseMockDto: RewardTypeResponseDto
    {
        public RewardTypeResponseMockDto()
        {
            RewardTypeDto = new TaskRewardTypeDto()
            { RewardTypeId = 1, RewardTypeName = "MONETARY_DOLLARS", RewardTypeCode = "ASFSDAGE", RewardTypeDescription = "TRGNJKGFNDVKJNGFADKJ" };
        }
    }
}
