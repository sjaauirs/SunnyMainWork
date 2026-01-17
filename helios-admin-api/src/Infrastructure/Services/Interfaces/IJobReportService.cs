using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces
{
    public interface IJobReportService
    {
        Task<GetBatchJobReportResponseDto> GetJobReports(JobReportRequestDto jobReportRequestDto);

        Task<BatchJobReportResponseDto> SaveJobReport(BatchJobReportDto batchJobReportDto);
    }
}
