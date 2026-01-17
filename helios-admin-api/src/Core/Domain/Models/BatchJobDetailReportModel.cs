
using SunnyRewards.Helios.Common.Core.Domain.Models;

namespace SunnyRewards.Helios.Admin.Core.Domain.Models
{
    public class BatchJobDetailReportModel : BaseModel {

        public virtual long BatchJobDetailReportId { get; set; }
        public virtual long BatchJobReportId { get; set; }
        public virtual int FileNum { get; set; }
        public virtual int RecordNum { get; set; }
        public virtual string RecordResultJson { get; set; } = String.Empty;

    }
}
