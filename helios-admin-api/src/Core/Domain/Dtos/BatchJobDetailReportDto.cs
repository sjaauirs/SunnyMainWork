using SunnyRewards.Helios.Admin.Core.Domain.Models;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using System.ComponentModel.DataAnnotations;

namespace SunnyRewards.Helios.Admin.Core.Domain.Dtos
{
    public class BatchJobDetailReportRequestDto 
    {
        public List<BatchJobDetailReportDto>? BatchJobDetailReportDtos { get; set; }
    }
    public class BatchJobDetailReportDto
    {
        public  long BatchJobDetailReportId { get; internal set; }
        [Required] 
        public  long BatchJobReportId { get; set; }
        [Required] 
        public  int FileNum { get; set; }
        [Required] 
        public  int RecordNum { get; set; }
        [Required] 
        public required string RecordResultJson { get; set; }
    }
    public class BatchJobDetailReportResponseDto : BaseResponseDto
    {

        public IList<BatchJobDetailReportDto>? BatchJobDetails { get; set; }
        public int RecordCount { get; set; }
    }

    public class PaginatedBatchJobDetailReport
    {
        public int TotalRecords { get; set; }
        public List<BatchJobDetailReportModel> JobDetailReports { get; set; } = new List<BatchJobDetailReportModel>();
    }
}
