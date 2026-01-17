using Moq;
using SunnyRewards.Helios.Wallet.Core.Domain.Models;
using SunnyRewards.Helios.Wallet.Infrastructure.Repositories;
using SunnyRewards.Helios.Wallet.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Wallet.UnitTest.Fixtures.MockModel;
using System.Linq.Expressions;

namespace SunnyRewards.Helios.Wallet.UnitTest.Fixtures.MockRepositories
{
    public class ConsumerWalletMockRepo : Mock<IConsumerWalletRepo>
    {
        public ConsumerWalletMockRepo()
        {
            Setup(x => x.FindAsync(It.IsAny<Expression<Func<ConsumerWalletModel, bool>>>(),false)).ReturnsAsync(new List<ConsumerWalletModel>(){
                new ConsumerWalletMockModel()
            });

            Setup(x => x.GetConsumerWalletsByWalletType(It.IsAny<string>(), It.IsAny<long>())).ReturnsAsync(new List<ConsumerWalletModel>(){
                new ConsumerWalletMockModel()
            });

            Setup(x => x.GetConsumerWalletsExcludingWalletType(It.IsAny<string>(), It.IsAny<long>())).ReturnsAsync(new List<ConsumerWalletModel>(){
                new ConsumerWalletMockModel()
            });

            Setup(x => x.FindOneAsync(It.IsAny<Expression<Func<ConsumerWalletModel, bool>>>(), false))
                            .ReturnsAsync(new ConsumerWalletMockModel());
            Setup(x => x.GetConsumerWalletsWithDetails(It.IsAny<string>(),false, It.IsAny<long?>()))
                .ReturnsAsync(new List<ConsumerWalletDetailsModel>
                {
                    new ConsumerWalletDetailsModel
                    {
                        ConsumerWallet = new ConsumerWalletModel
                        {
                            ConsumerCode = "cmr-a6404a0b542749c4a014f81bc8932d68",
                            TenantCode ="ten-ecada21e57154928a2bb959e8365b8b4",
                            WalletId = 1,
                            DeleteNbr = 0
                        },
                        Wallet = new WalletModel
                        {
                            WalletId = 1,
                            WalletTypeId = 100,
                            DeleteNbr = 0
                        },
                        WalletType = new WalletTypeModel
                        {
                            WalletTypeId = 100,
                            WalletTypeCode = "TYPE100"
                        }
                    },
                    new ConsumerWalletDetailsModel
                    {
                        ConsumerWallet = new ConsumerWalletModel
                        {
                            ConsumerCode = "cmr-a6404a0b542749c4a014f81bc8932d68",
                            TenantCode ="ten-ecada21e57154928a2bb959e8365b8b4",
                            WalletId = 1,
                            DeleteNbr = 0
                        }
                    },
                    new ConsumerWalletDetailsModel
                    {
                        ConsumerWallet = new ConsumerWalletModel
                        {
                            ConsumerCode = "cmr-a6404a0b542749c4a014f81bc8932d68",
                            TenantCode ="ten-ecada21e57154928a2bb959e8365b8b4",
                            WalletId = 1,
                            DeleteNbr = 0
                        },
                        Wallet = new WalletModel
                        {
                            WalletId = 1,
                            WalletTypeId = 100,
                            DeleteNbr = 0
                        }
                    }
                });
        }
    }
}
