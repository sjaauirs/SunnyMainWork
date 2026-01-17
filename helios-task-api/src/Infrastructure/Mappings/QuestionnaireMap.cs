using SunnyRewards.Helios.Common.Core.Mappings;
using SunnyRewards.Helios.Task.Core.Domain.Models;
using System.Diagnostics.CodeAnalysis;

namespace SunnyRewards.Helios.Task.Infrastructure.Mappings
{
    [ExcludeFromCodeCoverage]
    public class QuestionnaireMap : BaseMapping<QuestionnaireModel>
    {
        public QuestionnaireMap()
        {
            Table("questionnaire");
            Schema("task");
            Id(x => x.QuestionnaireId).Column("questionnaire_id").GeneratedBy.Identity();
            Map(x => x.QuestionnaireCode).Column("questionnaire_code");
            Map(x => x.TaskRewardId).Column("task_reward_id");
            Map(x => x.CtaTaskExternalCode).Column("cta_task_external_code");
            Map(x => x.ConfigJson).Column("config_json").CustomSqlType("jsonb").CustomType<StringAsJsonb>();
            Map(x => x.CreateTs).Column("create_ts");
            Map(x => x.UpdateTs).Column("update_ts");
            Map(x => x.DeleteNbr).Column("delete_nbr");
            Map(x => x.UpdateUser).Column("update_user");
            Map(x => x.CreateUser).Column("create_user");
        }
    }
}
