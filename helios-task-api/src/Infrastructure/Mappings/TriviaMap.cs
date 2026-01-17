using SunnyRewards.Helios.Common.Core.Mappings;
using SunnyRewards.Helios.Task.Core.Domain.Models;

namespace SunnyRewards.Helios.Task.Infrastructure.Mappings
{
    public class TriviaMap : BaseMapping<TriviaModel>
    {
        public TriviaMap()
        {
            Table("trivia");
            Schema("task");
            Id(x => x.TriviaId).Column("trivia_id").GeneratedBy.Identity();
            Map(x => x.TriviaCode).Column("trivia_code");
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
