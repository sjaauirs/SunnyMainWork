using SunnyRewards.Helios.ETL.Common.Mappings;
using SunnyRewards.Helios.ETL.Core.Domain.Models;

namespace SunnyRewards.Helios.ETL.Infrastructure.Mappings
{
    public class QuestionnaireMap : BaseMapping<ETLQuestionnaireModel>
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
