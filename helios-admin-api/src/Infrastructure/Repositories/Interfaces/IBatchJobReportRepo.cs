using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Core.Domain.Models;
using SunnyRewards.Helios.Common.Core.Repositories.Interfaces;

namespace SunnyRewards.Helios.Admin.Infrastructure.Repositories.HeliosRepo.Interfaces
{
    public interface IBatchJobReportRepo : IBaseRepo<BatchJobReportModel>
    {
        Task<PaginatedBatchJobReport> GetPaginatedJobReport(string searchByJobName, int skip, int pageSize);
    }
}
