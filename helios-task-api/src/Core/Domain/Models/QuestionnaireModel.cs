using SunnyRewards.Helios.Common.Core.Domain.Models;

namespace SunnyRewards.Helios.Task.Core.Domain.Models
{
    public class QuestionnaireModel : BaseModel
    {
        public virtual long QuestionnaireId { get; set; }
        public virtual string? QuestionnaireCode { get; set; }
        public virtual long TaskRewardId { get; set; }
        public virtual string? CtaTaskExternalCode { get; set; }
        public virtual string? ConfigJson { get; set; }
    }
}
