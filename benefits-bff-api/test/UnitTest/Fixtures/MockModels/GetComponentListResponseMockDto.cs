using Sunny.Benefits.Cms.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sunny.Benefits.Bff.UnitTest.Fixtures.MockModels
{
    public class GetComponentListResponseMockDto : GetComponentListResponseDto
    {
        public GetComponentListResponseMockDto()
        {
            Components = new List<ComponentDto>()
            {
                new ComponentDto
                {
                   ComponentId = 2,
                   ComponentTypeId = 3,
                   TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4",
                   ComponentCode = "test1",
                   ComponentOverrideCode = "cov-b9ae10e55a904deea42b0cc626c1f04d",
                   ComponentName = "COMPONENT_LIST",
                   DataJson = "{\r\n  \"data\": {\r\n    \"childrenComponentCodes\": [\r\n      \"com-8e7cc9868a764120ae6e98150d69d12b\",\r\n      \"com-0cf4b70d7ba14f62898b18db2f259a6a\"\r\n    ]\r\n  }\r\n}",
                   MetadataJson = "{\r\n  \"tags\": [\r\n    \"otc\",\r\n    \"vision\"\r\n ,\r\n \"cohort:diabetes\"\r\n ]\r\n}",
                }
            };
        }
    }
}
