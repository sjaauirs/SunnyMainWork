using SunnyRewards.Helios.ETL.Common.Domain.Models;

namespace SunnyRewards.Helios.ETL.Core.Domain.Models
{
    public class ETLTaskTypeModel : BaseModel
    {
        public virtual long TaskTypeId { get; set; }
        public virtual string? TaskTypeCode { get; set; }
        public virtual string? TaskTypeName { get; set; }
        public virtual string? TaskTypeDescription { get; set; }
        public virtual bool IsSubtask { get; set; }
    }
}
