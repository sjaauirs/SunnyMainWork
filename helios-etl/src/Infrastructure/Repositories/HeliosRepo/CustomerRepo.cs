using SunnyRewards.Helios.ETL.Common.Repositories;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces;
using Microsoft.Extensions.Logging;
using NHibernate;
using NHibernate.Linq;

namespace SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo
{
    public class CustomerRepo : BaseRepo<CustomerModel>, ICustomerRepo
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseLogger"></param>
        /// <param name="session"></param>
        public CustomerRepo(ILogger<BaseRepo<CustomerModel>> baseLogger, NHibernate.ISession session) : base(baseLogger, session)
        {

        }
    }
}
