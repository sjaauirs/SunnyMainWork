using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class HealthMetricsSummaryDto :BaseResponseDto
    {
        public IDictionary<string, DateTime?> HealthMetricsQueryStartTsMap { get; set; } =new Dictionary<string, DateTime?>();

    }
}
