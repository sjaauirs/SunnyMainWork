using Moq;
using System;
using SunnyRewards.Helios.Admin.Core.Domain.Constants;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.UnitTest.Fixtures.MockDtos;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using DateTime = System.DateTime;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.UnitTest.Helpers.HttpClientsMock
{
    public class TaskClientMock : Mock<ITaskClient>
    {
        public TaskClientMock()
        {
            Setup(client => client.Post<FindConsumerTasksByIdResponseDto>("get-consumer-task-by-task-id", It.IsAny<FindConsumerTasksByIdRequestDto>()))
               .ReturnsAsync(new FindConsumerTasksByIdResponseMockDto());
            Setup(client => client.Post<RewardTypeResponseDto>("reward-type", It.IsAny<RewardTypeRequestDto>()))
               .ReturnsAsync(new RewardTypeResponseMockDto());

            Setup(client => client.Put<ConsumerTaskDto>("update-consumer-task", It.IsAny<ConsumerTaskDto>()))
                .ReturnsAsync(new ConsumerTaskMockDto());

            Setup(client => client.Put<BaseResponseDto>("update-consumer-task-details", It.IsAny<ConsumerTaskDto>()))
                .ReturnsAsync(new BaseResponseDto());

            Setup(client => client.Post<ExportTaskResponseDto>(Constant.TaskExportAPIUrl, It.IsAny<ExportTaskRequestDto>()))
               .ReturnsAsync(new ExportTaskResponseDto());

            Setup(client => client.Post<ConsumerTaskResponseDto>("get-all-consumer-tasks", It.IsAny<ConsumerTaskRequestDto>()))
                .ReturnsAsync(new ConsumerTaskResponseDto()
                {
                    AvailableTasks = new List<TaskRewardDetailDto>(){
                    new TaskRewardDetailDto(){ Task = new TaskDto(){ } , TaskReward = new TaskRewardDto(){ IsRecurring = true , ValidStartTs = new DateTime(2025, 01, 1, 0, 0, 0, DateTimeKind.Utc) , Expiry = new DateTime(2026, 01, 1, 0, 0, 0, DateTimeKind.Utc) } }
                }
                });
        }
    }
}