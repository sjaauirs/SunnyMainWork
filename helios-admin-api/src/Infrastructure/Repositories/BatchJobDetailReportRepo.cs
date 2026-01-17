using Microsoft.Extensions.Logging;
using NHibernate.Linq;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Core.Domain.Models;
using SunnyRewards.Helios.Admin.Infrastructure.Repositories.HeliosRepo.Interfaces;
using SunnyRewards.Helios.Common.Core.Repositories;
using SunnyRewards.Helios.User.Core.Domain.Models;

namespace SunnyRewards.Helios.Admin.Infrastructure.Repositories
{

    public class BatchJobDetailReportRepo : BaseRepo<BatchJobDetailReportModel>, IBatchJobDetailReportRepo
    {
        private readonly NHibernate.ISession _session;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseLogger"></param>
        /// <param name="session"></param>
        public BatchJobDetailReportRepo(ILogger<BaseRepo<BatchJobDetailReportModel>> baseLogger, NHibernate.ISession session) : base(baseLogger, session)
        {
            _session = session;
        }

        public async Task<PaginatedBatchJobDetailReport> GetBatchJobDetailsByReportCode(string jobReportCode, int skip, int pageSize)
        {
            var query = from d in _session.Query<BatchJobDetailReportModel>()
                        join j in _session.Query<BatchJobReportModel>() on d.BatchJobReportId equals j.BatchJobReportId
                        where d.DeleteNbr == 0 && j.DeleteNbr == 0 && j.BatchJobReportCode == jobReportCode.Trim()
                        select d;
            int totalRecords = await query.CountAsync();

            query = query.OrderBy(q => q.BatchJobDetailReportId)
                            .Skip(skip)
                            .Take(pageSize);

            return new PaginatedBatchJobDetailReport()
            {
                TotalRecords = totalRecords,
                JobDetailReports= await query.ToListAsync()
            };
        }
    }
}
