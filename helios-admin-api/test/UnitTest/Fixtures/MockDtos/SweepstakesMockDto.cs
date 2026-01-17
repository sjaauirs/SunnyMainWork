using SunnyRewards.Helios.Sweepstakes.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.UnitTest.Fixtures.MockDtos
{
    internal class SweepstakesMockDto : SweepstakesRequestDto
    {
        public SweepstakesMockDto()
        {
            SweepstakesId = 1;
            SweepstakesCode = "TestSweepstakesCode";
            SweepstakesName = "TestSweepstakesName";
            SweepstakesDescription = "TestSweepstakesDescription";
            StartTs = DateTime.Now;
            EndTs = DateTime.Now.AddDays(4);
        }
    }
}
