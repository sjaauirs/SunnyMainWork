using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos
{
    public class CohortConsumerTaskDto
    {
        public string? TenantCode { get; set; }
        public string? ConsumerCode { get; set; }
        public string? TaskExternalCode { get; set; }
        public string? TaskStatus { get; set; }
        public long TaskId { get; set; }
        public string? TaskCode { get; set; }
        public string? TaskHeader { get; set; }
        public string? TaskDescription { get; set; }
        public string? CohortName { get; set; }
        public long CohortId { get; set; }
        public string? LanguageCode { get; set; }
        public string? TaskRewardCode { get; set; }
    }
}
