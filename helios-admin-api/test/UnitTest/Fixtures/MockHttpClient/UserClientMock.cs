using Moq;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;
using SunnyRewards.Helios.Admin.UnitTest.Fixtures.MockDtos;
using SunnyRewards.Helios.User.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.UnitTest.Fixtures.MockHttpClient
{
    public class UserClientMock : Mock<IUserClient>
    {
        public UserClientMock()
        {
            Setup(x => x.Post<GetConsumerResponseDto>("consumer/get-consumer", It.IsAny<GetConsumerRequestDto>()))
                .ReturnsAsync(new GetConsumerResponseMockDto());
           
            Setup(x => x.Post<GetConsumerByMemIdResponseDto>("consumer/get-consumer-by-memnbr", It.IsAny<string>()))
                .ReturnsAsync(new GetConsumerByMemNbrResponseMockDto());
        }
    }
}
