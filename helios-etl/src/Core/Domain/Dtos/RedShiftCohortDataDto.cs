using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos
{
    public class RedShiftCohortDataDto
    {
        public long ConsumerCohortImportId { get; set; }
        public string? PartnerCode { get; set; }
        public string? TenantCode { get; set; }
        public string? CustomerCode { get; set; }
        public string? CustomerLabel { get; set; }
        public string? PersonUniqueIdentifier { get; set; }
        public string? CohortName { get; set; }
        public string? Action { get; set; }
        public DateTime CreateTs { get; set; }
        public string? PublishStatus { get; set; }
        public string? PublishingLockId { get; set; }
    }
}
