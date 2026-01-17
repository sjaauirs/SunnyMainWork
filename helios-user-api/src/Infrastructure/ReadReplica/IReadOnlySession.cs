using NHibernate;

namespace SunnyRewards.Helios.User.Infrastructure.ReadReplica
{
    public interface IReadOnlySession
    {
        ISession Session { get; }
    }
}

