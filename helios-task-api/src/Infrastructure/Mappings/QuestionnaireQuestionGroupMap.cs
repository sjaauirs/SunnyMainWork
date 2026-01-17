using SunnyRewards.Helios.Common.Core.Mappings;
using SunnyRewards.Helios.Task.Core.Domain.Models;
using System.Diagnostics.CodeAnalysis;

namespace SunnyRewards.Helios.Task.Infrastructure.Mappings
{
    [ExcludeFromCodeCoverage]
    public class QuestionnaireQuestionGroupMap : BaseMapping<QuestionnaireQuestionGroupModel>
    {
        public QuestionnaireQuestionGroupMap()
        {
            Table("questionnaire_question_group");
            Schema("task");
            Id(x => x.QuestionnaireQuestionGroupId).Column("questionnaire_question_group_id ").GeneratedBy.Identity();
            Map(x => x.QuestionnaireId).Column("questionnaire_id ");
            Map(x => x.QuestionnaireQuestionId).Column("questionnaire_question_id ");
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
