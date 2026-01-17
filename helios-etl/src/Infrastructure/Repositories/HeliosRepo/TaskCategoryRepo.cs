using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.ETL.Common.Repositories;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo
{
   
    public class TaskCategoryRepo : BaseRepo<ETLTaskCategoryModel>, ITaskCategoryRepo
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseLogger"></param>
        /// <param name="session"></param>
        public TaskCategoryRepo(ILogger<BaseRepo<ETLTaskCategoryModel>> baseLogger, NHibernate.ISession session) : base(baseLogger, session)
        {
        }
    }
}
