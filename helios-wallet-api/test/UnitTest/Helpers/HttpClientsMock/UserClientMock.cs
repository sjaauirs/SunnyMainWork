using Moq;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.Wallet.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Wallet.UnitTest.Fixtures.MockDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Wallet.UnitTest.Helpers.HttpClientsMock
{
    public class UserClientMock :  Mock<IUserClient>
    {
        public UserClientMock()
        {
            Setup(client => client.Post<GetConsumerResponseDto>("consumer/get-consumer", It.IsAny<GetConsumerRequestDto>()))
              .ReturnsAsync(new GetConsumerResponseMockDto());
        }
       
    }
}
