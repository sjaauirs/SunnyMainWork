using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.ETL.Common.Domain.Models;
using SunnyRewards.Helios.ETL.Common.Extensions;
using SunnyRewards.Helios.ETL.Common.Repositories.Interfaces;

namespace SunnyRewards.Helios.ETL.Common.Repositories
{
    public class AuditTrailRepo : BaseRepo<AuditTrailModel>, IAuditTrailRepo
    {
        private readonly NHibernate.ISession _session;
        private readonly ILogger<BaseRepo<AuditTrailModel>> _logger;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseLogger"></param>
        /// <param name="session"></param>
        public AuditTrailRepo(ILogger<BaseRepo<AuditTrailModel>> logger, NHibernate.ISession session) : base(logger, session)
        {
            _session = session;
            _logger = logger;
        }

        public async Task<long?> PostAuditTrail(AuditTrailModel model)
        {
            _logger.LogInformation("PostAuditTrail: audit: {audit}", model.ToJson());

            try
            {
                using (var transaction = _session.BeginTransaction())
                {
                    try
                    {
                        model.CreateTs = DateTime.UtcNow;
                        var result = await _session.SaveAsync(model);
                        transaction.Commit();

                        return Convert.ToInt64(result);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Unable to post audit trail(L1): {message}, audit: {audit}", ex.Message, model.ToJson());
                        transaction.Rollback();

                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to post audit trail(L2): {message}, audit: {audit}", ex.Message, model.ToJson());
                throw;
            }
        }
    }
}
