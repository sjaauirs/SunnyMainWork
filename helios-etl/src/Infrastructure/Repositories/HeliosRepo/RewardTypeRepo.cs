using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.ETL.Common.Repositories;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces;

namespace SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo
{
    public class RewardTypeRepo : BaseRepo<ETLRewardTypeModel>, IRewardTypeRepo
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseLogger"></param>
        /// <param name="session"></param>
        public RewardTypeRepo(ILogger<BaseRepo<ETLRewardTypeModel>> baseLogger, NHibernate.ISession session) : base(baseLogger, session)
        {
        }
    }
}