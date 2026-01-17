using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos
{
    public class MemberImportComprehensiveReportDto
    {
        [Name("Tenant Code")]
        public string? TenantCode { get; set; }
        [Name("File Type")]
        public string? FileType { get; set; }
        
        [Name("File Name")]
        public string? FileName { get; set; }
        
        [Name("Action Name")]
        public string? ActionType { get; set; }

        [Name("Total Records Count")]
        public int TotalRecordsCount { get; set; }

        [Name("Total Valid Records Count")]
        public int TotalValidRecordsCount { get; set; }

        [Name("Invalid Record Count")]
        public int InvalidRecordCount { get; set; }

        [Name("Processed Records Count")]
        public int? ProcessedRecordsCount { get; set; }

        [Name("Successful Record Count")]
        public int? SuccessfulRecordCount { get; set; }
    }

  }

