using Moq;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.UnitTest.Fixtures.MockDtos;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.UnitTest.Fixtures.MockHttpClient
{
    public class TaskClientMock : Mock<ITaskClient>
    {
        public TaskClientMock()
        {
            Setup(x => x.Post<FindConsumerTasksByIdResponseDto>("get-consumer-task-by-task-id", It.IsAny<long>()))
                .ReturnsAsync(new FindConsumerTasksByIdResponseMockDto());
           
            Setup(x=>x.Put<ConsumerTaskDto>("update-consumer-task",It.IsAny<string>())).ReturnsAsync(new ConsumerTaskMockDto());
        }
    }
}
