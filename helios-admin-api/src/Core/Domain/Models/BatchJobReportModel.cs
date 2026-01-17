using SunnyRewards.Helios.Common.Core.Domain.Models;

namespace SunnyRewards.Helios.Admin.Core.Domain.Models
{
    public class BatchJobReportModel : BaseModel
    {
        public virtual int BatchJobReportId { get; set; }  // Primary Key

        public virtual string BatchJobReportCode { get; set; } = String.Empty; // Format: "brc-<guid>"

        public virtual string JobType { get; set; } = String.Empty; // Example: "MONTXN", "NONMONTXN"

        public virtual string JobResultJson { get; set; } = String.Empty;  // JSON data as a string
        public virtual string ValidationJson { get; set; } = String.Empty;  // JSON data as a string

    }
}
