using SunnyRewards.Helios.ETL.Common.Domain.Models;

namespace SunnyRewards.Helios.ETL.Core.Domain.Models
{
    public class ETLQuestionnaireQuestionModel : BaseModel
    {
        public virtual long QuestionnaireQuestionId { get; set; }
        public virtual string? QuestionnaireQuestionCode { get; set; }
        public virtual string? QuestionnaireJson { get; set; }
        public virtual string? QuestionExternalCode { get; set; }
    }
}
