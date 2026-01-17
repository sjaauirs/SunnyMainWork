using SunnyRewards.Helios.ETL.Common.Domain.Models;

namespace SunnyRewards.Helios.ETL.Core.Domain.Models
{
    public class ETLRewardTypeModel : BaseModel
    {
        public virtual long RewardTypeId { get; set; }
        public virtual string? RewardTypeCode { get; set; }
        public virtual string? RewardTypeName { get; set; }
        public virtual string? RewardTypeDescription { get; set; }
    }
}
