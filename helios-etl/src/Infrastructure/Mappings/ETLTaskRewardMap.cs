using SunnyRewards.Helios.ETL.Common.Mappings;
using SunnyRewards.Helios.ETL.Core.Domain.Models;

namespace SunnyRewards.Helios.ETL.Infrastructure.Mappings
{
    public class ETLTaskRewardMap : BaseMapping<ETLTaskRewardModel>
    {
        public ETLTaskRewardMap()
        {
            Table("task_reward");
            Schema("task");
            Id(x => x.TaskRewardId).Column("task_reward_id").GeneratedBy.Identity();
            Map(x => x.TaskId).Column("task_id");
            Map(x => x.RewardTypeId).Column("reward_type_id");
            Map(x => x.TenantCode).Column("tenant_code");
            Map(x => x.TaskRewardCode).Column("task_reward_code");
            Map(x => x.Reward).Column("reward").CustomSqlType("jsonb").CustomType<StringAsJsonb>();
            Map(x => x.MinTaskDuration).Column("min_task_duration");
            Map(x => x.MaxTaskDuration).Column("max_task_duration");
            Map(x => x.Expiry).Column("expiry");
            Map(x => x.Priority).Column("priority");
            Map(x => x.CreateTs).Column("create_ts");
            Map(x => x.UpdateTs).Column("update_ts");
            Map(x => x.DeleteNbr).Column("delete_nbr");
            Map(x => x.UpdateUser).Column("update_user");
            Map(x => x.CreateUser).Column("create_user");
            Map(x => x.TaskActionUrl).Column("task_action_url");
            Map(x => x.TaskExternalCode).Column("task_external_code").Not.Nullable();
            Map(x => x.ValidStartTs).Column("valid_start_ts").Nullable();
            Map(x => x.IsRecurring).Column("is_recurring").Nullable();
            Map(x => x.RecurrenceDefinitionJson).Column("recurrence_definition_json").CustomSqlType("jsonb").CustomType<StringAsJsonb>();
            Map(x => x.SelfReport).Column("self_report");
            Map(x => x.TaskCompletionCriteriaJson).Column("task_completion_criteria_json").CustomSqlType("jsonb").CustomType<StringAsJsonb>();
            Map(x => x.ConfirmReport).Column("confirm_report");
            Map(x => x.CompletionEligibilityJson).Column("completion_eligibility_json").CustomSqlType("jsonb").CustomType<StringAsJsonb>();
        }
    }
}