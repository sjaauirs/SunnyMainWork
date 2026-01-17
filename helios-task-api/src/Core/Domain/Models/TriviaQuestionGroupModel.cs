using SunnyRewards.Helios.Common.Core.Domain.Models;

namespace SunnyRewards.Helios.Task.Core.Domain.Models
{
    public class TriviaQuestionGroupModel : BaseModel
    {
        public virtual long TriviaQuestionGroupId { get; set; }
        public virtual long TriviaId { get; set; }
        public virtual long TriviaQuestionId { get; set; }
        public virtual int SequenceNbr { get; set; }
        public virtual DateTime ValidStartTs { get; set; }
        public virtual DateTime ValidEndTs { get; set; }
    }
}
