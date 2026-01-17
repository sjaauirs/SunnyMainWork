using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Task.UnitTest.Fixtures.MockDtos
{
    public class TaskTypeResponseMockDto : TaskTypeResponseDto
    {
        public TaskTypeResponseMockDto()
        {
            TaskTypeId = 2;
            TaskTypeCode = "typ-cdfjdxjfvj5654656";
            TaskTypeName = "Test";
            TaskTypeDescription = "Test Done";
        }
    }
}
