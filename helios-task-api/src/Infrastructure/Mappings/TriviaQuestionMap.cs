using SunnyRewards.Helios.Common.Core.Mappings;
using SunnyRewards.Helios.Task.Core.Domain.Models;

namespace SunnyRewards.Helios.Task.Infrastructure.Mappings
{
    public class TriviaQuestionMap : BaseMapping<TriviaQuestionModel>
    {
        public TriviaQuestionMap()
        {
            Table("trivia_question");
            Schema("task");
            Id(x => x.TriviaQuestionId).Column("trivia_question_id").GeneratedBy.Identity();
            Map(x => x.TriviaQuestionCode).Column("trivia_question_code");
            Map(x => x.TriviaJson).Column("trivia_json").CustomSqlType("jsonb").CustomType<StringAsJsonb>();
            Map(x => x.QuestionExternalCode).Column("question_external_code");
            Map(x => x.CreateTs).Column("create_ts");
            Map(x => x.UpdateTs).Column("update_ts");
            Map(x => x.DeleteNbr).Column("delete_nbr");
            Map(x => x.UpdateUser).Column("update_user");
            Map(x => x.CreateUser).Column("create_user");

        }
    }
}
