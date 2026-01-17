using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Task.UnitTest.Fixtures.MockDtos
{
    public class PostTaskProgressUpdateRequestMockDto : PostTaskProgressUpdateRequestDto
    {
        public PostTaskProgressUpdateRequestMockDto()
        {
            TaskId = 1;
            ConsumerCode = "cmr-c69905fc68ce4f36851f877bae38f22e";
            ProgressDetail = "Details of progress";
        }
    }
}
