using SunnyRewards.Helios.Sweepstakes.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Admin.UnitTest.Fixtures.MockDtos
{
    public class SweepstakesInstanceRequestMockDto : SweepstakesInstanceRequestDto
    {
        public SweepstakesInstanceRequestMockDto()
        {
            new SweepstakesInstanceRequestDto
            {
                SweepstakesId = 1,
                InstanceTs = DateTime.Now,
                CreateUser = "test"
            };
        }
    }
}
