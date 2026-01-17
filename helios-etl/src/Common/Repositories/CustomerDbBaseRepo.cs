using SunnyRewards.Helios.ETL.Common.Domain.Models;
using SunnyRewards.Helios.ETL.Common.Nhibernate.Interfaces;
using Microsoft.Extensions.Logging;
using NHibernate;

namespace SunnyRewards.Helios.ETL.Common.Repositories
{
    /// <summary>
    /// 
    /// </summary>
    public class CustomerDbBaseRepo<T> : BaseRepo<T> where T : BaseModel
    {
        /// <summary>
        /// This will be injected in .net core dependencies and will automatically auto wired here.
        /// </summary>
        private readonly ISession _session;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="session"></param>
        public CustomerDbBaseRepo(ICustomerDbSession customerDbSession, ILogger<BaseRepo<T>> logger) : 
            base(logger, customerDbSession.Session)
        {
            _session = customerDbSession.Session;
        }
    }
}
