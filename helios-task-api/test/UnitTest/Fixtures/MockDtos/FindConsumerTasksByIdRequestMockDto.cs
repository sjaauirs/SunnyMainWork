using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Task.UnitTest.Fixtures.MockDtos
{
    public  class FindConsumerTasksByIdRequestMockDto: FindConsumerTasksByIdRequestDto
    {
        public FindConsumerTasksByIdRequestMockDto()
        {
            ConsumerCode = "cmr-c69905fc68ce4f36851f877bae38f22e";
            TaskId = 1;
            TaskStatus = "completed";
            TaskCode = "tsk-210ddf7876234c11b64668d4246f0b44";
            TaskExternalCode = "NA";
            TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4";
        }
    }

    public class FindConsumerTasksByIdExternalCodeRequestMockDto : FindConsumerTasksByIdRequestDto
    {
        public FindConsumerTasksByIdExternalCodeRequestMockDto()
        {
            ConsumerCode = "cmr-c69905fc68ce4f36851f877bae38f22e";
            TaskId = 0;
            TaskStatus = "completed";
            TaskExternalCode = "NA";
            TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4";
        }
    }
}
