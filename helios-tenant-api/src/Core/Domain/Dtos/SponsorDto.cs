using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Tenant.Core.Domain.Dtos
{
    public class SponsorDto
    {
        public long SponsorId { get; set; }
        public long? CustomerId { get; set; }
        public string SponsorCode { get; set; } = string.Empty;
        public string SponsorName { get; set; } = string.Empty;
        public string SponsorDescription { get; set; } = string.Empty;
    }
}
