using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.ETL.Common.Repositories;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces;

namespace SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo
{
    public class WalletTypeRepo : BaseRepo<ETLWalletTypeModel>, IWalletTypeRepo
    {
        public WalletTypeRepo(ILogger<BaseRepo<ETLWalletTypeModel>> baseLogger, NHibernate.ISession session) : base(baseLogger, session)
        {
        }
    }
}
