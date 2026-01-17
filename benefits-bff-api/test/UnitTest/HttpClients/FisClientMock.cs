using Moq;
using NSubstitute;
using Sunny.Benefits.Bff.Infrastructure.HttpClients;
using Sunny.Benefits.Bff.Infrastructure.HttpClients.Interfaces;
using Sunny.Benefits.Bff.UnitTest.Fixtures.MockDtos;
using SunnyBenefits.Fis.Core.Domain.Dtos;
using SunnyBenefits.Fis.Core.Domain.Enums;

namespace Sunny.Benefits.Bff.UnitTest.HttpClients
{
    public class FisClientMock : Mock<IFisClient>
    {
        public FisClientMock()
        {
            Setup(client => client.Post<PostSearchStoresResponseDto>("fis/store-search", It.IsAny<PostSearchStoresRequestDto>()))
            .ReturnsAsync(new PostSearchStoresResponseDto());

            Setup(client => client.Post<ProductSearchResponseDto>("fis/product-search", It.IsAny<PostSearchProductRequestDto>()))
               .ReturnsAsync(new ProductSearchResponseMockDto());      
            
            Setup(client => client.Post<GetConsumerAccountResponseDto>(It.IsAny<string>(), It.IsAny<GetConsumerAccountRequestDto>()))
                .ReturnsAsync(new GetConsumerAccountResponseDto { ConsumerAccount = new ConsumerAccountDto { CardIssueStatus = "NOT_ELIGIBLE"} });
        }
    }
}
