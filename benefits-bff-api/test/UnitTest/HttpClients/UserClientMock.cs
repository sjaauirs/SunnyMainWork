using Moq;
using Sunny.Benefits.Bff.Infrastructure.Repositories.Interfaces;
using Sunny.Benefits.Bff.UnitTest.Fixtures.MockDtos;
using SunnyRewards.Helios.User.Core.Domain.Dtos;

public class UserClientMock : Mock<IUserClient>
{
    public UserClientMock()
    {
        Setup(client => client.GetId<GetConsumerByEmailResponseDto>("consumer/get-consumers-by-email?email=", It.IsAny<Dictionary<string, string>>()))
          .ReturnsAsync(new GetConsumerByEmailResponseMockDto());
        Setup(client => client.Post<ConsumerActivityResponseDto>("consumer-activity", It.IsAny<ConsumerActivityRequestDto>()))
          .ReturnsAsync(new ConsumerActivityResponseDto());
    }
}