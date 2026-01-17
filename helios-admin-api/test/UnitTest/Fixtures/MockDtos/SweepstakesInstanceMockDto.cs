using SunnyRewards.Helios.Sweepstakes.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Admin.UnitTest.Fixtures.MockDtos
{
    public class SweepstakesInstanceMockDto : SweepstakesInstanceDto
    {
        public SweepstakesInstanceMockDto()
        {
            SweepstakesInstanceId = 1;
            SweepstakesInstanceCode = new Guid().ToString();
            InstanceTs = DateTime.UtcNow;
            PrizeDescriptionJson = string.Empty;
        }
    }
}
