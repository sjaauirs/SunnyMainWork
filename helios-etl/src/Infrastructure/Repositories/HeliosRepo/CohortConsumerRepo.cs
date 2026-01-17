using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.ETL.Common.Repositories;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces;

namespace SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo
{
    public class CohortConsumerRepo : BaseRepo<ETLCohortConsumerModel>, ICohortConsumerRepo
    {
        private readonly NHibernate.ISession _session;
        private readonly ILogger<BaseRepo<ETLCohortConsumerModel>> _logger;
        private const string _className = nameof(CohortConsumerRepo);

        public CohortConsumerRepo(ILogger<BaseRepo<ETLCohortConsumerModel>> baseLogger, NHibernate.ISession session) : base(baseLogger, session)
        {
            _logger = baseLogger;
            _session = session;
        }

        /// <summary>
        /// Get the cohort consumer task.
        /// </summary>
        /// <param name="tenantCode"></param>
        /// <param name="consumerCode"></param>
        /// <param name="cohortName"></param>
        /// <returns></returns>
        public IQueryable<CohortConsumerTaskDto> GetCohortConsumerTask(string tenantCode,
            string consumerCode, string cohortName)
        {
            const string methodName = nameof(GetCohortConsumerTask);
            try
            {
                var result = from cs in _session.Query<ETLCohortConsumerModel>()
                             join coh in _session.Query<ETLCohortModel>() on cs.CohortId equals coh.CohortId
                             join cttr in _session.Query<ETLCohortTenantTaskRewardModel>() on coh.CohortId equals cttr.CohortId
                             join tr in _session.Query<ETLTaskRewardModel>() on cttr.TaskRewardCode equals tr.TaskRewardCode
                             join td in _session.Query<ETLTaskDetailModel>() on tr.TaskId equals td.TaskId
                             join t in _session.Query<ETLTaskModel>() on tr.TaskId equals t.TaskId
                             where cs.DeleteNbr == 0 &&
                                   coh.DeleteNbr == 0 &&
                                   cttr.DeleteNbr == 0 &&
                                   tr.DeleteNbr == 0 &&
                                   td.DeleteNbr == 0 &&
                                   t.DeleteNbr == 0 &&
                                   cttr.TenantCode == tenantCode &&
                                   cs.ConsumerCode == consumerCode &&
                                   coh.CohortName == cohortName
                             orderby cs.CohortConsumerId descending
                             select new CohortConsumerTaskDto
                             {
                                 ConsumerCode = cs.ConsumerCode,
                                 TenantCode = tr.TenantCode,
                                 CohortName = coh.CohortName,
                                 CohortId = coh.CohortId,
                                 TaskRewardCode = tr.TaskRewardCode,
                                 TaskHeader = td.TaskHeader,
                                 TaskDescription = td.TaskDescription,
                                 TaskExternalCode = tr.TaskExternalCode,
                                 TaskId = t.TaskId,
                                 TaskCode = t.TaskCode,
                                 LanguageCode = td.LanguageCode
                             };

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[{_className}] : [{methodName}] : Error processing {methodName}, request: {tenantCode}", _className, methodName);
                throw;
            }
            finally
            {
                _logger.LogInformation($"[{_className}] : [{methodName}] : Completed processing {methodName}");
            }
        }
    }
}
