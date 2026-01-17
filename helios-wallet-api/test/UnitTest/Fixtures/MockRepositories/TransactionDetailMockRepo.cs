using Moq;
using SunnyRewards.Helios.Wallet.Core.Domain.Models;
using SunnyRewards.Helios.Wallet.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Wallet.UnitTest.Fixtures.MockModel;
using System.Linq.Expressions;

namespace SunnyRewards.Helios.Wallet.UnitTest.Fixtures.MockRepositories
{
    public class TransactionDetailMockRepo : Mock<ITransactionDetailRepo>
    {
        public TransactionDetailMockRepo()
        {
            Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<TransactionDetailModel, bool>>>(),false)).ReturnsAsync(new TransactionDetailMockModel());
            Setup(x => x.FindAsync(It.IsAny<Expression<Func<TransactionDetailModel, bool>>>(), false))
                .ReturnsAsync( new List<TransactionDetailModel> () { new TransactionDetailMockModel() });

            Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<TransactionDetailModel, bool>>>(), false))
            .ReturnsAsync(new List<TransactionDetailModel>
            {
                new TransactionDetailModel
                {
                    TransactionDetailId = 5001,
                    ConsumerCode = "cmr-a6404a0b542749c4a014f81bc8932d68"
                }
            });

        }
    }
}
