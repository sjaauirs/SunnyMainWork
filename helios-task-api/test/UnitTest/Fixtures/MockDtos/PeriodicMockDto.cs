using SunnyRewards.Helios.Task.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Task.UnitTest.Fixtures.MockDtos
{
    public class PeriodicMockDto:PeriodicDto
    {
        public PeriodicMockDto() 
        {
            period = "MONTH";
            periodRestartDate = 20;
        }
    }
}
