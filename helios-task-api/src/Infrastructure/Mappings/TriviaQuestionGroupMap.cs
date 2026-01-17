using SunnyRewards.Helios.Common.Core.Mappings;
using SunnyRewards.Helios.Task.Core.Domain.Models;

namespace SunnyRewards.Helios.Task.Infrastructure.Mappings
{
    public class TriviaQuestionGroupMap : BaseMapping<TriviaQuestionGroupModel>
    {
        public TriviaQuestionGroupMap()
        {
            Table("trivia_question_group");
            Schema("task");
            Id(x => x.TriviaQuestionGroupId).Column("trivia_question_group_id").GeneratedBy.Identity();
            Map(x => x.TriviaId).Column("trivia_id");
            Map(x => x.TriviaQuestionId).Column("trivia_question_id");
            Map(x => x.SequenceNbr).Column("sequence_nbr");
            Map(x => x.CreateTs).Column("create_ts");
            Map(x => x.UpdateTs).Column("update_ts");
            Map(x => x.DeleteNbr).Column("delete_nbr");
            Map(x => x.UpdateUser).Column("update_user");
            Map(x => x.CreateUser).Column("create_user");
            Map(x => x.ValidStartTs).Column("valid_start_ts");
            Map(x => x.ValidEndTs).Column("valid_end_ts");
        }
    }
}
