using Moq;
using NHibernate;
using NHibernate.Linq;

namespace SunnyRewards.Helios.Wallet.UnitTest.Fixtures.MockRepositories
{
    public class MockSession : Mock<ISession>
    {
        public MockSession()
        {
            DefaultValue = DefaultValue.Mock;
            DefaultValueProvider = DefaultValueProvider.Mock;
        }
    }
    public class NhProvider : Mock<INhQueryProvider>
    {
        public NhProvider()
        {
            DefaultValue = DefaultValue.Mock;
        }
    }
    public class Q : Mock<IQueryable>
    {
        public Q()
        {
            DefaultValueProvider = DefaultValueProvider.Mock;
        }
    }
}
