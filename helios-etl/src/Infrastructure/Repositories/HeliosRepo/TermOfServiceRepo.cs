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
    public class TermOfServiceRepo : BaseRepo<ETLTermsOfServiceModel>, ITermOfServiceRepo
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseLogger"></param>
        /// <param name="session"></param>
        public TermOfServiceRepo(ILogger<BaseRepo<ETLTermsOfServiceModel>> baseLogger, NHibernate.ISession session) : base(baseLogger, session)
        {
        }
    }
}
