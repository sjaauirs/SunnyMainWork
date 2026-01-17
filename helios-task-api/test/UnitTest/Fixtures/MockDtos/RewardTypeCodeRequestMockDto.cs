using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Task.UnitTest.Fixtures.MockDtos
{
    public class RewardTypeCodeRequestMockDto : RewardTypeCodeRequestDto
    {
        public RewardTypeCodeRequestMockDto() 
        {
            RewardTypeCode = "Code001";
        }
    }
}
