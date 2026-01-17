using Moq;
using Sunny.Benefits.Bff.Infrastructure.HttpClients;
using Sunny.Benefits.Bff.Infrastructure.HttpClients.Interfaces;
using Sunny.Benefits.Bff.UnitTest.Fixtures.MockDtos;
using Sunny.Benefits.Bff.UnitTest.Fixtures.MockModels;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sunny.Benefits.Bff.UnitTest.HttpClients
{
    public class TaskClientMock: Mock<ITaskClient>
    {
        public TaskClientMock()
        {
            Setup(client => client.Post<GetTaskRewardByCodeResponseDto>("get-task-reward-by-code", It.IsAny<GetTaskRewardByCodeRequestDto>()))
           .ReturnsAsync(new GetTaskRewardByCodeResponseMockDto());


        }
    }
}
