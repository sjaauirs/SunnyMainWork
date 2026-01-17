using SunnyRewards.Helios.ETL.Common.Domain.Models;
using SunnyRewards.Helios.ETL.Common.Mappings;
using SunnyRewards.Helios.ETL.Common.Nhibernate;
using SunnyRewards.Helios.ETL.Common.Nhibernate.Interfaces;
using FluentNHibernate;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using Microsoft.Extensions.DependencyInjection;
using NHibernate;

namespace SunnyRewards.Helios.ETL.Common.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public static class NhibernateExtension
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="services"></param>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public static IServiceCollection AddNhibernate<T>(this IServiceCollection services, string connectionString,
            string customerDbconnectionString, string? auditSchemaName = null) where T : IMappingProvider, new()
        {
            try
            {
                if (auditSchemaName != null)
                {
                    // used by Audit Trail service if enabled
                    AuditTrailMap.SchemaName = auditSchemaName;
                }

                var configureSessionFactory = BuildSessionFactory<T>(connectionString);
                services.AddSingleton(configureSessionFactory);
                services.AddScoped(_ => configureSessionFactory.OpenSession());

                // var customerConfigureSessionFactory = BuildSessionFactory<T>(customerDbconnectionString);
                // services.AddSingleton(customerConfigureSessionFactory);
                // services.AddScoped<ICustomerDbSession>(_ => new CustomerDbSession(customerConfigureSessionFactory.OpenSession()));
            }
            catch (Exception ex)
            {
                Console.Write(ex);
            }
            return services;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        private static ISessionFactory BuildSessionFactory<T>(this string connectionString)
        {
            ISessionFactory? sessionFactory = CreateSession<T>(connectionString);
            return sessionFactory;
        }

        /// <summary>
        /// Will create an ORM session
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        private static ISessionFactory CreateSession<T>(string connectionString)
        {

            return Fluently.Configure()
                  .Database(PostgreSQLConfiguration.PostgreSQL83.ConnectionString(connectionString))
                  .Mappings(m =>
                  {
                      m.FluentMappings.AddFromAssemblyOf<T>();
                      m.FluentMappings.AddFromAssemblyOf<BaseMapping<BaseModel>>();
                  })
                  .BuildConfiguration()
                  .BuildSessionFactory();
        }
    }
}
