using Moq;
using Sunny.Benefits.Bff.Core.Domain.Dtos;
using Sunny.Benefits.Bff.Infrastructure.HttpClients;
using Sunny.Benefits.Bff.Infrastructure.HttpClients.Interfaces;
using Sunny.Benefits.Bff.UnitTest.Fixtures.MockDtos;
using Sunny.Benefits.Bff.UnitTest.Fixtures.MockModels;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;

namespace Sunny.Benefits.Bff.UnitTest.HttpClients
{
    public class WalletClientMock: Mock<IWalletClient>
    {
        public WalletClientMock()
        {
            Setup(client => client.Post<WalletResponseDto>("wallet/get-wallets", It.IsAny<FindConsumerWalletRequestDto>()))
            .ReturnsAsync(new WalletResponseMockDto());

            Setup(client => client.Post<PostGetTransactionsResponseDto>("transaction/get-transactions", It.IsAny<PostGetTransactionsRequestDto>()))
           .ReturnsAsync(new PostGetTransactionsResponseMockDto());

            Setup(client => client.Post<TransactionBySectionResponseDto>("transaction/get-transactions", It.IsAny<PostGetTransactionsRequestDto>()))
              .ReturnsAsync(new TransactionBySectionResponseMockDto());
           
        }
    }
}