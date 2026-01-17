using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Tenant.Core.Domain.Dtos
{
    public class FlowRequestDto
    {
        public required string TenantCode { get; set; }
        public string? ConsumerCode { get; set; } = string.Empty;
        public List<string>? CohortCodes { get; set; } = [];
        public long? FlowId { get; set; } = 0;
        public string? FlowName { get; set; }
        public DateTime? EffectiveDate { get; set; } = DateTime.UtcNow;
    }
}
