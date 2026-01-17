using SunnyRewards.Helios.ETL.Common.Nhibernate.Interfaces;
using NHibernate;

namespace SunnyRewards.Helios.ETL.Common.Nhibernate
{
    /// <summary>
    /// 
    /// </summary>
    public class CustomerDbSession : ICustomerDbSession
    {
        private readonly ISession _session;

        public CustomerDbSession(ISession session)
        {
            _session = session;
        }

        /// <summary>
        /// 
        /// </summary>
        public ISession Session
        { get { return _session; } }
    }
}
