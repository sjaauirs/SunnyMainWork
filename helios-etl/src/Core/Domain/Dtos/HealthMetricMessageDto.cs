using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos
{
    public class HealthMetricMessageDto
    {
        public string? HealthMetricTypeCode { get; set; }
        public string? ConsumerCode { get; set; }
        public DateTime CaptureTs { get; set; }
        public DateTime OsMetricTs { get; set; }
        public string? DataJson { get; set; }
        public string? TenantCode { get; set; }
    }
}
