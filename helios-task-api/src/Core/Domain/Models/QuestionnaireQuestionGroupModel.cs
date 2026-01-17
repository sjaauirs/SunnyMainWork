using SunnyRewards.Helios.Common.Core.Domain.Models;

namespace SunnyRewards.Helios.Task.Core.Domain.Models
{
    public class QuestionnaireQuestionGroupModel : BaseModel
    {
        public virtual long QuestionnaireQuestionGroupId { get; set; }
        public virtual long QuestionnaireId { get; set; }
        public virtual long QuestionnaireQuestionId { get; set; }
        public virtual int SequenceNbr { get; set; }
        public virtual DateTime ValidStartTs { get; set; }
        public virtual DateTime ValidEndTs { get; set; }
    }
}
