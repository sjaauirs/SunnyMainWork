using Sunny.Benefits.Bff.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sunny.Benefits.Bff.UnitTest.Fixtures.MockDtos
{
    public class FindStoreRequestMockDTO : FindStoreRequestDTO
    {
        public FindStoreRequestMockDTO()
        {

            coords = new Coords()
            {
                Accuracy = 2,
                Altitude = 2,
                AltitudeAccuracy = 1.1,
                Heading = 1,
                Latitude = 0.1,
                Longitude = 0.2,
                Speed = 2,
            };
            Mocked = true;
           Timestamp = 1;
        }
    }
}
