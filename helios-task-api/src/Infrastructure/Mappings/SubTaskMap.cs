using SunnyRewards.Helios.Common.Core.Mappings;
using SunnyRewards.Helios.Task.Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Task.Infrastructure.Mappings
{
    public class SubTaskMap : BaseMapping<SubTaskModel>
    {
        public SubTaskMap() 
        {

            Table("subtask");
            Schema("task");
            Id(x => x.SubTaskId).Column("subtask_id").GeneratedBy.Identity();
            Map(x => x.ParentTaskRewardId).Column("parent_task_reward_id");
            Map(x => x.ChildTaskRewardId).Column("child_task_reward_id");
            Map(x => x.ConfigJson).Column("config_json").CustomSqlType("jsonb").CustomType<StringAsJsonb>();
            Map(x => x.UpdateTs).Column("update_ts");
            Map(x => x.CreateTs).Column("create_ts");
            Map(x => x.UpdateUser).Column("update_user");
            Map(x => x.CreateUser).Column("create_user");
            Map(x => x.DeleteNbr).Column("delete_nbr");
           
        }
    }
}
