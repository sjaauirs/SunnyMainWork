using SunnyRewards.Helios.ETL.Common.Domain.Models;

namespace SunnyRewards.Helios.ETL.Core.Domain.Models
{
    public class TriviaQuestionModel : BaseModel
    {
        public virtual long TriviaQuestionId { get; set; }
        public virtual string? TriviaQuestionCode { get; set; }
        public virtual string? TriviaJson { get; set; }
        public virtual string? QuestionExternalCode { get; set; }
    }
}
