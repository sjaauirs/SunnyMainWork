using Moq;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.UnitTest.Fixtures.MockDtos;
using SunnyRewards.Helios.User.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.UnitTest.Helpers.HttpClientsMock
{
    public class UserClientMock : Mock<IUserClient>
    {
        public UserClientMock()
        {

            Setup(client => client.Post<GetConsumerResponseDto>("consumer/get-consumer", It.IsAny<GetConsumerRequestDto>()))
               .ReturnsAsync(new GetConsumerResponseMockDto());

            Setup(client => client.Post<GetConsumerByMemIdResponseDto>("consumer/get-consumer-by-memnbr", It.IsAny<GetConsumerByMemIdRequestDto>()))
                .ReturnsAsync(new GetConsumerByMemNbrResponseMockDto());
        }
    }
}