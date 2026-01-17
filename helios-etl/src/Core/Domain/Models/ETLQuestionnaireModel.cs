using SunnyRewards.Helios.ETL.Common.Domain.Models;

namespace SunnyRewards.Helios.ETL.Core.Domain.Models
{
    public class ETLQuestionnaireModel : BaseModel
    {
        public virtual long QuestionnaireId { get; set; }
        public virtual string? QuestionnaireCode { get; set; }
        public virtual long TaskRewardId { get; set; }
        public virtual string? CtaTaskExternalCode { get; set; }
        public virtual string? ConfigJson { get; set; }
    }
}
