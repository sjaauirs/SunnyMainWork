using NHibernate;

namespace SunnyRewards.Helios.ETL.Common.Nhibernate.Interfaces
{
    public interface ICustomerDbSession
    {
        /// <summary>
        /// 
        /// 
        /// </summary>
        ISession Session { get; }
    }
}
