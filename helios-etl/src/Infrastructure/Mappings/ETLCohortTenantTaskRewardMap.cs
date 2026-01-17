using SunnyRewards.Helios.ETL.Common.Mappings;
using SunnyRewards.Helios.ETL.Core.Domain.Models;

namespace Infrastructure.Mappings
{
    public class ETLCohortTenantTaskRewardMap : BaseMapping<ETLCohortTenantTaskRewardModel>
    {
        public ETLCohortTenantTaskRewardMap()
        {
            Schema("cohort");
            Table("cohort_tenant_task_reward");
            Id(x => x.CohortTenantTaskRewardId).Column("cohort_tenant_task_reward_id ").GeneratedBy.Identity();
            Map(x => x.CohortId).Column("cohort_id");
            Map(x => x.TenantCode).Column("tenant_code");
            Map(x => x.TaskRewardCode).Column("task_reward_code");
            Map(x => x.Recommended).Column("recommended");
            Map(x => x.Priority).Column("priority");
            Map(x => x.CreateTs).Column("create_ts");
            Map(x => x.UpdateUser).Column("update_user");
            Map(x => x.CreateUser).Column("create_user");
            Map(x => x.DeleteNbr).Column("delete_nbr");
        }
    }
}
