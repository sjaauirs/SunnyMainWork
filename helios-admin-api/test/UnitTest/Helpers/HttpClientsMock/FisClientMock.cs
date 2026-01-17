using Moq;
using SunnyBenefits.Fis.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Infrastructure.HttpClients.Interfaces;

namespace SunnyRewards.Helios.Admin.UnitTest.Helpers.HttpClientsMock
{

    public class FisClientMock : Mock<IFisClient>
    {
        public FisClientMock()
        {
            Setup(client => client.Post<ConsumerAccountDto>("create-consumer-account", It.IsAny<CreateConsumerAccountRequestDto>()))
               .ReturnsAsync(new ConsumerAccountDto()
               {
                   ConsumerAccountId = 1
               });

            Setup(client => client.Post<GetConsumerAccountResponseDto>("get-consumer-account", It.IsAny<GetConsumerAccountRequestDto>()))
                .ReturnsAsync(new GetConsumerAccountResponseDto());

        }
    }
}
