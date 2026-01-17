using SunnyRewards.Helios.ETL.Common.Domain.Models;

namespace SunnyRewards.Helios.ETL.Core.Domain.Models
{
    public class ETLRedshiftSyncStatusModel : BaseModel
    {
        public virtual long RedshiftSyncStatusId { get; set; }
        public virtual int RecordsProcessed { get; set; }
        public virtual string? ErrorMessage { get; set; }
        public virtual string DataType { get; set; } = string.Empty;
        public virtual long LastMemberImportFileDataId { get; set; }
        public virtual long LastLoadedId { get; set; }
    }
}
