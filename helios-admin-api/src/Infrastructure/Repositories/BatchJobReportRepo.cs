using Microsoft.Extensions.Logging;
using NHibernate.Linq;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Core.Domain.Models;
using SunnyRewards.Helios.Admin.Infrastructure.Repositories.HeliosRepo.Interfaces;
using SunnyRewards.Helios.Common.Core.Repositories;

namespace SunnyRewards.Helios.Admin.Infrastructure.Repositories
{
    public class BatchJobReportRepo : BaseRepo<BatchJobReportModel>, IBatchJobReportRepo
    {

        private readonly NHibernate.ISession _session;
        public BatchJobReportRepo(ILogger<BaseRepo<BatchJobReportModel>> baseLogger, NHibernate.ISession session) : base(baseLogger, session)
        {
            _session = session;
        }

        public async Task<PaginatedBatchJobReport> GetPaginatedJobReport(string searchByJobName, int skip, int pageSize)
        {
            var query = from j in _session.Query<BatchJobReportModel>()
                        where j.DeleteNbr == 0 && j.JobType.ToLower().Contains(searchByJobName.ToLower())
                        select j;

            int totalRecords = await query.CountAsync();

            query = query.OrderByDescending(q => q.BatchJobReportId) 
                             .Skip(skip)
                             .Take(pageSize);

            return new PaginatedBatchJobReport
            {
                TotalRecords = totalRecords,
                JobReports = await query.ToListAsync()
            };
        }
    }
}
