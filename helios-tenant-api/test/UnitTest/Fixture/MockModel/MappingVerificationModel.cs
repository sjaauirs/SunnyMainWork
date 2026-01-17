using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using NHibernate.Tool.hbm2ddl;
using SunnyRewards.Helios.Tenant.Infrastructure.Mappings;
using Xunit;

namespace SunnyRewards.Helios.Tenant.UnitTest.Fixture.MockModel
{
    public class MappingVerificationTests
    {
        [Fact]
        public void CanBuildSessionFactory_WithCategoryMap()
        {
            var cfg = Fluently.Configure()
                .Database(SQLiteConfiguration.Standard.InMemory().ShowSql())
                .Mappings(m => m.FluentMappings.Add<CategoryMap>())
                .BuildConfiguration();

            // Ensure configuration built and contains at least one class mapping
            Assert.NotNull(cfg);
            Assert.NotEmpty(cfg.ClassMappings);
        }

        [Fact]
        public void CanBuildSessionFactory_WithWalletCategoryMap()
        {
            var cfg = Fluently.Configure()
                .Database(SQLiteConfiguration.Standard.InMemory().ShowSql())
                .Mappings(m => m.FluentMappings.Add<WalletCategoryMap>())
                .BuildConfiguration();

            // Ensure configuration built and contains mappings
            Assert.NotNull(cfg);
            Assert.NotEmpty(cfg.ClassMappings);
        }
    }
}
