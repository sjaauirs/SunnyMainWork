using SunnyBenefits.Fis.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sunny.Benefits.Bff.UnitTest.Fixtures.MockDtos
{
    public class PostSearchStoresResponseMockDto: PostSearchStoresResponseDto
    {
        public PostSearchStoresResponseMockDto() 
        {
            Stores = new List<StoreDto>
            {
                new StoreDto()
                {
                 Address ="phase-8",
                 County="mi", 
                 City="mohali", 
                 State ="panjab",
                 Country="india", 
                 PostalCode ="pc-232",
                 StoreDescription="sunny", 
                 Organization ="rewards",
                 Latitude =1.1,
                 Longitude =1.1,
                 StoreAttributes= new List<StoreAttributeDto>
                 {
                     new StoreAttributeDto() 
                     {
                         AttributeName="sunny rewards",
                         AttributeValue="sunny value",
                     }
                 },
                 Hours= new List<string>
                 {

                 },
                 Distance =1.2,
                 Duration="1.2", 
                 IsOpen =true,
                 CloseDescription ="close",
    }
            };
        }
    }
}
