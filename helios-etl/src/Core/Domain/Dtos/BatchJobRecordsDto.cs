using System.Text.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using SunnyRewards.Helios.ETL.Core.Domain.Dtos.Json;
using Newtonsoft.Json.Linq;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;

namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos
{
    public class BatchJobRecordsDto
    {
        public string? JobType { get; set; }
        public string? JobResultJson { get; set; }
        public string? ValidationJson { get; set; }


        public void SetJobResultDetails(JobResultDetails details)
        {
            if (details != null)
            {
                JobResultJson = JsonSerializer.Serialize(details);
            }
        }
    }

    public class BatchJobReport
    {
        public required long BatchJobReportId { get; set; }
        public required string BatchJobReportCode { get; set; }
        public required string JobType { get; set; }
        public required string JobResultJson { get; set; }
        public  string? ValidationJson { get; set; }

    }

    public class BatchJobReportResponseDto : BaseResponseDto
    {
        public BatchJobReport? jobReport { get; set; }
    }

    public class BatchJobDetailReportDto
    {
        public long BatchJobDetailReportId { get; set; }
        public required long BatchJobReportId { get; set; }
        public required int FileNum { get; set; }
        public required int RecordNum { get; set; }
        public required string RecordResultJson { get; set; }
    }

    public class BatchJobDetailReportResponseDto : BaseResponseDto
    {
        public IList<BatchJobDetailReportDto>? BatchJobDetails { get; set; }
    }

    public class BatchJobDetailReportRequestDto
    {
        public List<BatchJobDetailReportDto>? BatchJobDetailReportDtos { get; set; }
    }


}

