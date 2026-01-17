using Moq;
using SunnyRewards.Helios.Sweepstakes.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.UnitTest.Fixtures.MockDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Admin.UnitTest.Helpers.HttpClientsMock
{
    public class SweepstakesClientMock : Mock<ISweepstakesClient>
    {
        public SweepstakesClientMock()
        {
            Setup(client => client.Post<SweepstakesInstanceResponseDto>("sweepstakes/create-sweepstakes-instance", It.IsAny<SweepstakesInstanceRequestDto>()))
               .ReturnsAsync(new SweepstakesInstanceResponseDto { SweepstakesInstanceId = 1 });

        }
    }
}
