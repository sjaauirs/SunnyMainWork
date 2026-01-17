using SunnyRewards.Helios.Task.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Task.UnitTest.Fixtures.MockDtos
{
    public class GetConsumerSubTaskResponseMockDto : GetConsumerSubTaskResponseDto
    {
        public GetConsumerSubTaskResponseMockDto()
        {
            var ConsumerTaskDto = new ConsumerTaskDto[]
            {
                 new ConsumerTaskDto
                 {
                TenantCode = "ten-ecada21e57154928a2bb959e8365b8b4",
                ConsumerTaskId = 203,
                Progress = 0,
                Notes = "done",
                TaskStartTs = DateTime.UtcNow,
                TaskCompleteTs = DateTime.UtcNow,
                ConsumerCode = "cmr-c69905fc68ce4f36851f877bae38f22e",
                TaskStatus = "completed",
                TaskId = 5,
                CreateTs = DateTime.UtcNow,
                CreateUser = "sunny",
                 }

            };


        }
    }
}
