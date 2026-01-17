using Microsoft.Extensions.Logging;
using NHibernate.Transform;
using SunnyRewards.Helios.Common.Core.Repositories;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces;
using SunnyRewards.Helios.NotificationService.Core.Domain.Models;
using System.Collections;

namespace SunnyRewards.Helios.ETL.Infrastructure.Repositories
{
    public class NotificationRuleRepository : BaseRepo<NotificationRuleModel>, INotificationRuleRepository
    {
        private readonly NHibernate.ISession _session;
        private readonly ILogger<BaseRepo<NotificationRuleModel>> _logger;
        public NotificationRuleRepository(ILogger<BaseRepo<NotificationRuleModel>> baseLogger, NHibernate.ISession session) : base(baseLogger, session)
        {
            _session = session;
            _logger = baseLogger;
        }

        public IList<Dictionary<string, object>> ExecuteQuery(string sqlQuery)
        {
            if (string.IsNullOrEmpty(sqlQuery))
                throw new ArgumentException("Query cannot be null or empty.", nameof(sqlQuery));

            try
            {
                var query = _session.CreateSQLQuery(sqlQuery);
                query.SetResultTransformer(Transformers.AliasToEntityMap);

                var result = query.List<IDictionary>().Cast<Hashtable>()
                    .Select(ht => ht.Cast<DictionaryEntry>()
                        .ToDictionary(de => (string)de.Key, de => de.Value!)) // Convert Hashtable to Dictionary<string, object>
                    .ToList();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing query: {Query}", sqlQuery);
                throw new InvalidOperationException("An error occurred while executing the SQL query.", ex);
            }
        }


    }
}
