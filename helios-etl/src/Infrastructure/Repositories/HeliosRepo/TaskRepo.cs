using SunnyRewards.Helios.ETL.Common.Repositories;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces;
using Microsoft.Extensions.Logging;

namespace SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo
{
    public class TaskRepo : BaseRepo<ETLTaskModel>, ITaskRepo
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseLogger"></param>
        /// <param name="session"></param>
        public TaskRepo(ILogger<BaseRepo<ETLTaskModel>> baseLogger, NHibernate.ISession session) : base(baseLogger, session)
        {
        }
    }
}