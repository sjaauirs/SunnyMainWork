using SunnyRewards.Helios.Common.Core.Domain.Models;

namespace SunnyRewards.Helios.Task.Core.Domain.Models
{
    public class QuestionnaireQuestionModel : BaseModel
    {
        public virtual long QuestionnaireQuestionId { get; set; }
        public virtual string? QuestionnaireQuestionCode { get; set; }
        public virtual string? QuestionnaireJson { get; set; }
        public virtual string? QuestionExternalCode { get; set; }
    }
}
