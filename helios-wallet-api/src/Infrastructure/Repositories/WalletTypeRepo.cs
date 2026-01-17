using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Common.Core.Repositories;
using SunnyRewards.Helios.Wallet.Core.Domain.Models;
using SunnyRewards.Helios.Wallet.Infrastructure.Repositories.Interfaces;

namespace SunnyRewards.Helios.Wallet.Infrastructure.Repositories
{
    public class WalletTypeRepo : BaseRepo<WalletTypeModel>, IWalletTypeRepo
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseLogger"></param>
        /// <param name="session"></param>
        public WalletTypeRepo(ILogger<BaseRepo<WalletTypeModel>> baseLogger, NHibernate.ISession session) : base(baseLogger, session)
        {
        }
    }
}
