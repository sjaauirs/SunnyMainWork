using SunnyRewards.Helios.ETL.Common.Domain.Models;

namespace SunnyRewards.Helios.ETL.Core.Domain.Models
{
    public class ETLTaskModel : BaseModel
    {
        public virtual long TaskId { get; set; }
        public virtual long TaskTypeId { get; set; }
        public virtual long TaskCategoryId { get; set; }
        public virtual string? TaskCode { get; set; }
        public virtual string? TaskName { get; set; }
        public virtual bool SelfReport { get; set; }
        public virtual bool ConfirmReport { get; set; }
    }
}