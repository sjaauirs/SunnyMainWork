using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using Microsoft.Extensions.DependencyInjection;
using NHibernate;

namespace SunnyRewards.Helios.User.Infrastructure.ReadReplica
{
    public static class NhibernateReadReplicaExtension
    {
        /// <summary>
        /// Adds NHibernate read replica session factory to the service collection.
        /// Only registers if connection string is provided.
        /// </summary>
        /// <typeparam name="T">The mapping class type to scan for mappings.</typeparam>
        /// <param name="services">The service collection.</param>
        /// <param name="readReplicaConnectionString">The read replica database connection string. If null or empty, read replica is not registered.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddNhibernateReadReplica<T>(
            this IServiceCollection services,
            string? readReplicaConnectionString)
        {
            if (string.IsNullOrWhiteSpace(readReplicaConnectionString))
            {
                // No read replica configured - skip registration
                return services;
            }

            var sessionFactory = CreateSessionFactory<T>(readReplicaConnectionString);

            services.AddScoped<IReadOnlySession>(provider =>
            {
                var session = sessionFactory.OpenSession();
                return new ReadOnlySessionWrapper(session);
            });

            return services;
        }

        /// <summary>
        /// Creates a new NHibernate session factory for the read replica using PostgreSQL.
        /// </summary>
        private static ISessionFactory CreateSessionFactory<T>(string connectionString)
        {
            return Fluently.Configure()
                .Database(PostgreSQLConfiguration.Standard
                    .ConnectionString(connectionString))
                .Mappings(m => m.FluentMappings.AddFromAssemblyOf<T>())
                .BuildSessionFactory();
        }
    }
}
