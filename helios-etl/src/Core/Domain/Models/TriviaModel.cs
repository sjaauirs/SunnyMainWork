using SunnyRewards.Helios.ETL.Common.Domain.Models;

namespace SunnyRewards.Helios.ETL.Core.Domain.Models
{
    public class TriviaModel : BaseModel
    {
        public virtual long TriviaId { get; set; }
        public virtual string? TriviaCode { get; set; }
        public virtual long TaskRewardId { get; set; }
        public virtual string? CtaTaskExternalCode { get; set; }
        public virtual string? ConfigJson { get; set; }
    }
}
