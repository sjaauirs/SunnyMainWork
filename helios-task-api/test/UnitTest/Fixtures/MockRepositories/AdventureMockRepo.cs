using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Cfg;
using Microsoft.Extensions.Logging;
using Moq;
using NHibernate;
using SunnyRewards.Helios.Common.Core.Repositories;
using SunnyRewards.Helios.Task.Core.Domain.Models;
using SunnyRewards.Helios.Task.Infrastructure.Repositories;
using SunnyRewards.Helios.Task.Infrastructure.Repositories.Interfaces;
using System.Linq.Expressions;
using Xunit;
using NHibernate.Tool.hbm2ddl;
using SunnyRewards.Helios.Task.Infrastructure.Mappings;
using SunnyRewards.Helios.Task.UnitTest.Fixtures.MockDtos;

namespace SunnyRewards.Helios.Task.UnitTest.Fixtures.MockRepositories
{
    public class AdventureMockRepo : Mock<IAdventureRepo>
    {
        public AdventureMockRepo()
        {
            Setup(x => x.FindAsync(It.IsAny<Expression<Func<AdventureModel, bool>>>(), false)).ReturnsAsync(new List<AdventureModel>() { new AdventureModel() });
            Setup(x => x.GetAllAdventures(It.IsAny<string>())).ReturnsAsync(new List<AdventureModel>() { new AdventureMockModel() });
        }
    }


    public class AdventureRepoTests
    {
        private readonly Mock<ISession> _mockSession;
        private readonly Mock<ILogger<BaseRepo<AdventureModel>>> _mockLogger;
        private readonly AdventureRepo _adventureRepo;

        public AdventureRepoTests()
        {
            _mockSession = new Mock<ISession>();
            _mockLogger = new Mock<ILogger<BaseRepo<AdventureModel>>>();
            _adventureRepo = new AdventureRepo(_mockLogger.Object, _mockSession.Object);
        }

        [Fact]
        public void Constructor_Should_Initialize_AdventureRepo()
        {
            Assert.NotNull(_adventureRepo);
        }
    }

    public class AdventureMapTests : IDisposable
    {
        private readonly ISessionFactory _sessionFactory;
        private readonly ISession _session;

        public AdventureMapTests()
        {
            // Setup in-memory SQLite database
            _sessionFactory = Fluently.Configure()
                .Database(SQLiteConfiguration.Standard.InMemory().ShowSql())
                .Mappings(m => m.FluentMappings.Add<AdventureMap>())
                .ExposeConfiguration(cfg => new SchemaExport(cfg).Create(false, true))
                .BuildSessionFactory();

            _session = _sessionFactory.OpenSession();
        }

        [Fact]
        public void Should_Map_AdventureModel_Correctly()
        {
            using (var transaction = _session.BeginTransaction())
            {
                // Arrange: Create an entity
                var adventure = new AdventureModel
                {
                    AdventureCode = "ADV-001",
                    CreateTs = DateTime.UtcNow,
                    UpdateTs = DateTime.UtcNow,
                    DeleteNbr = 0,
                    CreateUser = "test-user",
                    UpdateUser = "test-user"
                };

                // Act: Save and fetch the entity
                _session.Save(adventure);
                transaction.Commit();
                _session.Clear(); // Clear session cache

                var retrieved = _session.Query<AdventureModel>().FirstOrDefault(x => x.AdventureCode == "ADV-001");

                // Assert: Verify data is persisted correctly
                Assert.NotNull(retrieved);
                Assert.Equal("ADV-001", retrieved.AdventureCode);
            }
        }

        public void Dispose()
        {
            _session.Dispose();
            _sessionFactory.Dispose();
        }
    }

}
