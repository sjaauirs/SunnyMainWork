using SunnyRewards.Helios.Common.Core.Mappings;
using SunnyRewards.Helios.Task.Core.Domain.Models;
using System.Diagnostics.CodeAnalysis;

namespace SunnyRewards.Helios.Task.Infrastructure.Mappings
{
    public class TaskRewardCollectionMap : BaseMapping<TaskRewardCollectionModel>
    {
        public TaskRewardCollectionMap()
        {
            Table("task_reward_collection");
            Schema("task");

            Id(x => x.TaskRewardCollectionId).Column("task_reward_collection_id").GeneratedBy.Identity();
            Map(x => x.ParentTaskRewardId).Column("parent_task_reward_id").Not.Nullable();
            Map(x => x.ChildTaskRewardId).Column("child_task_reward_id").Not.Nullable();
            Map(x => x.UniqueChildCode).Column("unique_child_code").Not.Nullable();
            Map(x => x.ConfigJson).Column("config_json").CustomSqlType("jsonb").CustomType<StringAsJsonb>();
            Map(x => x.CreateTs).Column("create_ts");
            Map(x => x.UpdateTs).Column("update_ts");
            Map(x => x.DeleteNbr).Column("delete_nbr");
            Map(x => x.UpdateUser).Column("update_user");
            Map(x => x.CreateUser).Column("create_user");
        }
    }

}
