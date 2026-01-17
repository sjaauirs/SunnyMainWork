using Moq;
using Sunny.Benefits.Bff.Core.Domain.Dtos;
using Sunny.Benefits.Bff.Infrastructure.HttpClients.Interfaces;
using Sunny.Benefits.Bff.UnitTest.Fixtures.MockDtos;

namespace Sunny.Benefits.Bff.UnitTest.HttpClients
{
    public class ValidicClientMock : Mock<IValidicClient>
    {
        public ValidicClientMock()
        {
            Setup(client => client.Post<CreateValidicUserResponseDto>("organizations/test-org-id/users?token=test-token", It.IsAny<CreateValidicUserRequestDto>()))
                .ReturnsAsync(new CreateValidicUserResponseMockDto());
        }
    }
}
