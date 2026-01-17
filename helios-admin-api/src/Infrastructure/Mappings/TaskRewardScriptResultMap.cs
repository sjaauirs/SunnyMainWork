using SunnyRewards.Helios.Common.Core.Mappings;
using SunnyRewards.Helios.Admin.Core.Domain.Models;

namespace SunnyRewards.Helios.Admin.Infrastructure.Mappings
{
    public class TaskRewardScriptResultMap : BaseMapping<TaskRewardScriptResultModel>
    {
        public TaskRewardScriptResultMap()
        {
            Schema("admin");
            Table("task_reward_script_result");
            Id(x => x.TaskRewardScriptId).Column("task_reward_script_id").GeneratedBy.Identity();
            Map(x => x.TenantTaskRewardScriptId).Column("tenant_task_reward_script_id");
            Map(x => x.ConsumerCode).Column("consumer_code");
            Map(x => x.ExecutionContextJson).Column("execution_context_json").CustomSqlType("jsonb").CustomType<StringAsJsonb>();
            Map(x => x.ExecutionResultJson).Column("execution_result_json").CustomSqlType("jsonb").CustomType<StringAsJsonb>(); 
            Map(x => x.CreateTs).Column("create_ts");
            Map(x => x.UpdateTs).Column("update_ts");
            Map(x => x.DeleteNbr).Column("delete_nbr");
            Map(x => x.UpdateUser).Column("update_user");
            Map(x => x.CreateUser).Column("create_user");
        }
    }
}
