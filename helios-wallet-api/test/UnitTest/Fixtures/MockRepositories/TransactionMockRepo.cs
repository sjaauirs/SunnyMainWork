using Moq;
using SunnyRewards.Helios.Wallet.Core.Domain.Models;
using SunnyRewards.Helios.Wallet.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Wallet.UnitTest.Fixtures.MockModel;
using System.Linq.Expressions;

namespace SunnyRewards.Helios.Wallet.UnitTest.Fixtures.MockRepositories
{
    public class TransactionMockRepo : Mock<ITransactionRepo>
    {
        public TransactionMockRepo()
        {
            Setup(x => x.GetMaxTransactionIdByWallet(It.IsAny<long>())).ReturnsAsync(4);
            Setup(repo => repo.FindOneAsync(It.IsAny<Expression<Func<TransactionModel, bool>>>(), false))
                           .ReturnsAsync(new TransactionMockModel());
            Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<TransactionModel, bool>>>(), false)).ReturnsAsync(new List<TransactionModel>() {
             new TransactionMockModel()});
            Setup(x => x.GetTotalAmountForConsumerByTransactionDetailType(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<string>())).ReturnsAsync(25);
            Setup(x => x.GetConsumerWalletTransactions(It.IsAny<string>())).ReturnsAsync(new List<TransactionModel>() {
             new TransactionMockModel()});
            Setup(x => x.GetConsumerWalletTopTransactions(It.IsAny<List<long>>(), It.IsAny<int?>(),null))
            .ReturnsAsync(new List<TransactionModel>
            {
                new TransactionModel
                {
                    TransactionId = 101,
                    WalletId = 1,
                    TransactionDetailId = 5001,
                    DeleteNbr = 0,
                    Balance = 100.00,
                    CreateTs = DateTime.UtcNow
                },
                new TransactionModel
                {
                    TransactionId = 100,
                    WalletId = 2,
                    TransactionDetailId = 5002,
                    DeleteNbr = 0,
                    Balance = 50.00,
                    CreateTs = DateTime.UtcNow.AddMinutes(-10)
                }
            });
        }
    }
}
