using NHibernate;

namespace SunnyRewards.Helios.User.Infrastructure.ReadReplica
{
    public class ReadOnlySessionWrapper : IReadOnlySession
    {
        public ISession Session { get; }

        public ReadOnlySessionWrapper(ISession session)
        {
            Session = session;
        }
    }
}

