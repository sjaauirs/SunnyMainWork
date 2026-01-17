using Sunny.Benefits.Cms.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sunny.Benefits.Bff.UnitTest.Fixtures.MockDtos
{
    public class GetComponentListRequestMockDto : GetComponentListRequestDto
    {
        public GetComponentListRequestMockDto()
        {
            TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4";
            ComponentName = "test";

            MetaData = new MetaData
            {
                Tags = new List<string>
                {
                  "otc",
                  "vision"
                }
            };
        }
    }
}
