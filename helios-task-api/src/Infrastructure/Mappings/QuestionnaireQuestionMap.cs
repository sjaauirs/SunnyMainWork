using SunnyRewards.Helios.Common.Core.Mappings;
using SunnyRewards.Helios.Task.Core.Domain.Models;
using System.Diagnostics.CodeAnalysis;

namespace SunnyRewards.Helios.Task.Infrastructure.Mappings
{
    [ExcludeFromCodeCoverage]
    public class QuestionnaireQuestionMap : BaseMapping<QuestionnaireQuestionModel>
    {
        public QuestionnaireQuestionMap()
        {
            Table("questionnaire_question");
            Schema("task");
            Id(x => x.QuestionnaireQuestionId).Column("questionnaire_question_id").GeneratedBy.Identity();
            Map(x => x.QuestionnaireQuestionCode).Column("questionnaire_question_code");
            Map(x => x.QuestionnaireJson).Column("questionnaire_json").CustomSqlType("jsonb").CustomType<StringAsJsonb>();
            Map(x => x.QuestionExternalCode).Column("question_external_code");
            Map(x => x.CreateTs).Column("create_ts");
            Map(x => x.UpdateTs).Column("update_ts");
            Map(x => x.DeleteNbr).Column("delete_nbr");
            Map(x => x.UpdateUser).Column("update_user");
            Map(x => x.CreateUser).Column("create_user");
        }

    }
}
