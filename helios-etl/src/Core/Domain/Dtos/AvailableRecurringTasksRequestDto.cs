using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos
{
    public class AvailableRecurringTasksRequestDto
    {
        public string TenantCode { get; set; } = string.Empty;

        public string ConsumerCode { get; set; } = string.Empty!;

        public DateTime? TaskAvailabilityTs { get; set; }=DateTime.UtcNow;
    }
}
