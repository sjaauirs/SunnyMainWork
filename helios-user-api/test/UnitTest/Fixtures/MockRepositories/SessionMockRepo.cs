using Moq;
using NSubstitute;
using SunnyRewards.Helios.User.Core.Domain.Models;
using SunnyRewards.Helios.User.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.User.UnitTest.Fixtures.MockModels;
using System.Linq.Expressions;

namespace SunnyRewards.Helios.User.UnitTest.Fixtures.MockRepositories
{
    public class SessionMockRepo : Mock<NHibernate.ISession>
    {
        public SessionMockRepo()
        {
            
        }
    }
}