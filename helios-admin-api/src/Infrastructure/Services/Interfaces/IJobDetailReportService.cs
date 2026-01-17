using SunnyRewards.Helios.Admin.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces
{
    public interface IJobDetailReportService
    {
        Task<BatchJobDetailReportResponseDto> GetJobDetailReport(JobDetailReportRequestDto jobReportDetailRequestDto);
        Task<BatchJobDetailReportResponseDto> SaveJobDetailReport(BatchJobDetailReportRequestDto batchJobDetailReportRequestDto);
    }
}
