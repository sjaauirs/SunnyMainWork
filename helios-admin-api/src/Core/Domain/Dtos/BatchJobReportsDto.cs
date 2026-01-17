using FirebaseAdmin.Auth.Multitenancy;
using SunnyRewards.Helios.Admin.Core.Domain.Models;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using System.ComponentModel.DataAnnotations;

namespace SunnyRewards.Helios.Admin.Core.Domain.Dtos
{
    public class BatchJobReportDto
    {
        public long BatchJobReportId { get; internal set; }

        public string BatchJobReportCode { get; internal set; } = "brc-" + Guid.NewGuid().ToString("N");

        [Required]
        public required string JobType { get; set; }

        [Required]
        public required string JobResultJson { get; set; }
        public string? ValidationJson { get; set; }

    }
    public class BatchJobReportResponseDto : BaseResponseDto
    {
        public BatchJobReportDto? jobReport { get; set; }
    }

    public class GetBatchJobReportResponseDto : BaseResponseDto
    {
        public List<BatchJobReportDto> jobReports { get; set; } = new List<BatchJobReportDto>();
        public int RecordCount { get; set; }
    }

    public class PaginatedBatchJobReport
    {
        public int TotalRecords { get; set; }
        public List<BatchJobReportModel> JobReports { get; set; } = new List<BatchJobReportModel>();
    }
}
