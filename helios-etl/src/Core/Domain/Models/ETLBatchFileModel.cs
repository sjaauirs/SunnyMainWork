using SunnyRewards.Helios.ETL.Common.Domain.Models;

namespace SunnyRewards.Helios.ETL.Core.Domain.Models
{
    public class ETLBatchFileModel : BaseModel
    {
        public virtual long BatchFileId { get; set; } // Primary key, auto-generated
        public virtual string BatchFileCode { get; set; } = string.Empty; // Format: "bfc-<guid>"
        public virtual string Direction { get; set; } = string.Empty; // INBOUND / OUTBOUND
        public virtual string FileType { get; set; } = string.Empty; // e.g., "NON_MON_TXN", "MON_TXN", etc.
        public virtual string FileName { get; set; } = string.Empty; // File name (processed inbound or generated outbound)
        public virtual DateTime ProcessStartTs { get; set; } // Process start timestamp
        public virtual DateTime? ProcessEndTs { get; set; } // Nullable process end timestamp
    }

}
