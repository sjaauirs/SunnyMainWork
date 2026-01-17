using SunnyRewards.Helios.Sweepstakes.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.UnitTest.Fixtures.MockDtos
{
    internal class TenantSweepstakesMockDto : TenantSweepstakesDto
    {
        public TenantSweepstakesMockDto()
        {
            TenantSweepstakesId = 1;
            SweepstakesId = 1;
            TenantCode = "TestTenantCode";
        }

    }
}
