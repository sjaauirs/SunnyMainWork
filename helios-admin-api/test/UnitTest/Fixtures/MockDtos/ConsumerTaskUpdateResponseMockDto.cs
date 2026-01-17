using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.UnitTest.Fixtures.MockDtos
{
    public class ConsumerTaskUpdateResponseMockDto : ConsumerTaskUpdateResponseDto
    {
        public ConsumerTaskUpdateResponseMockDto()
        {
            ConsumerTask = new ConsumerTaskDto()
            {
                ConsumerTaskId = 2,
                TaskId = 2,
                Progress = 1,
                Notes = "note",
                TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4",
                ConsumerCode = "cmr-c457c5257c59451d8a93ea941a9f2e0a",
                TaskStatus = "IN_PROGRESS",
                TaskCompleteTs = DateTime.UtcNow,
                TaskStartTs = DateTime.UtcNow,
                CreateTs = DateTime.UtcNow,
                CreateUser = "sunny",
            };
        }
    }
}
