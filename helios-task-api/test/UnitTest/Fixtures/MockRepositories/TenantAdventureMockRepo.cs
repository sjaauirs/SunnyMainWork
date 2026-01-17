using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Cfg;
using Moq;
using NHibernate;
using SunnyRewards.Helios.Task.Core.Domain.Models;
using SunnyRewards.Helios.Task.Infrastructure.Repositories.Interfaces;
using System.Linq.Expressions;
using Xunit;
using NHibernate.Tool.hbm2ddl;
using SunnyRewards.Helios.Task.Infrastructure.Mappings;

namespace SunnyRewards.Helios.Task.UnitTest.Fixtures.MockRepositories
{
    public class TenantAdventureMockRepo : Mock<ITenantAdventureRepo>
    {
        public TenantAdventureMockRepo()
        {
            Setup(x => x.FindAsync(It.IsAny<Expression<Func<TenantAdventureModel, bool>>>(), false)).ReturnsAsync(new List<TenantAdventureModel>() { new TenantAdventureModel() });
        }
    }



    public class TenantAdventureMapTests : IDisposable
    {
        private readonly ISessionFactory _sessionFactory;
        private readonly ISession _session;

        public TenantAdventureMapTests()
        {
            // Setup in-memory SQLite database
            _sessionFactory = Fluently.Configure()
                .Database(SQLiteConfiguration.Standard.InMemory().ShowSql())
                .Mappings(m => m.FluentMappings.Add<TenantAdventureMap>())
                .ExposeConfiguration(cfg => new SchemaExport(cfg).Create(false, true))
                .BuildSessionFactory();

            _session = _sessionFactory.OpenSession();
        }

        [Fact]
        public void Should_Map_TenantAdventureModel_Correctly()
        {
            using (var transaction = _session.BeginTransaction())
            {
                // Arrange: Create an entity
                var tenantAdventure = new TenantAdventureModel
                {
                    TenantAdventureCode = "TA-123",
                    TenantCode = "TEN-001",
                    AdventureId = 10,
                    CreateTs = DateTime.UtcNow,
                    UpdateTs = DateTime.UtcNow,
                    DeleteNbr = 0,
                    CreateUser = "test-user",
                    UpdateUser = "test-user"
                };

                // Act: Save and fetch the entity
                _session.Save(tenantAdventure);
                transaction.Commit();
                _session.Clear(); // Clear session cache

                var retrieved = _session.Query<TenantAdventureModel>().FirstOrDefault(x => x.TenantAdventureCode == "TA-123");

                // Assert: Verify data is persisted correctly
                Assert.NotNull(retrieved);
                Assert.Equal("TA-123", retrieved.TenantAdventureCode);
                Assert.Equal("TEN-001", retrieved.TenantCode);
                Assert.Equal(10, retrieved.AdventureId);
            }
        }

        public void Dispose()
        {
            _session.Dispose();
            _sessionFactory.Dispose();
        }
    }


}
