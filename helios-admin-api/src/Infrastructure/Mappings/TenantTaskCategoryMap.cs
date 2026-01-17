using SunnyRewards.Helios.Common.Core.Mappings;
using SunnyRewards.Helios.Admin.Core.Domain.Models;

namespace SunnyRewards.Helios.Admin.Infrastructure.Mappings
{
    public class TenantTaskRewardScriptMap : BaseMapping<TenantTaskRewardScriptModel>
    {
        public TenantTaskRewardScriptMap()
        {
            Schema("admin");
            Table("tenant_task_reward_script");
            Id(x => x.TenantTaskRewardScriptId).Column("tenant_task_reward_script_id").GeneratedBy.Identity();
            Map(x => x.TenantTaskRewardScriptCode).Column("tenant_task_reward_script_code");
            Map(x => x.TenantCode).Column("tenant_code");
            Map(x => x.TaskRewardCode).Column("task_reward_code");
            Map(x => x.ScriptType).Column("script_type");
            Map(x => x.ScriptId).Column("script_id");
            Map(x => x.CreateTs).Column("create_ts");
            Map(x => x.UpdateTs).Column("update_ts");
            Map(x => x.DeleteNbr).Column("delete_nbr");
            Map(x => x.UpdateUser).Column("update_user");
            Map(x => x.CreateUser).Column("create_user");
        }
    }
}
