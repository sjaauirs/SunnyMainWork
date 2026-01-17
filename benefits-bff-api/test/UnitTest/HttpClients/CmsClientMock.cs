using Moq;
using Sunny.Benefits.Bff.Infrastructure.HttpClients.Interfaces;
using Sunny.Benefits.Bff.UnitTest.Fixtures.MockModels;
using Sunny.Benefits.Cms.Core.Domain.Dtos;

namespace Sunny.Benefits.Bff.UnitTest.HttpClients
{
    public class CmsClientMock : Mock<ICmsClient>
    {
        public CmsClientMock()
        {
            Setup(client => client.Post<GetComponentListResponseDto>("cms/component-list", It.IsAny<GetComponentListRequestDto>()))
          .ReturnsAsync(new GetComponentListResponseMockDto());
            Setup(client => client.Post<GetComponentResponseDto>("cms/get-component", It.IsAny<GetComponentRequestDto>()))
          .ReturnsAsync(new GetComponentResponseDto());

        }
    }
}
