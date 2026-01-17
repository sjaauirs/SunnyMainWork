using SunnyRewards.Helios.Common.Core.Mappings;
using SunnyRewards.Helios.Task.Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Task.Infrastructure.Mappings
{
    public class TaskTypeMap : BaseMapping<TaskTypeModel>
    {
        public TaskTypeMap()
        {
            Schema("task");
            Table("task_type");
            Id(x => x.TaskTypeId).Column("task_type_id").GeneratedBy.Identity();
            Map(x => x.TaskTypeCode).Column("task_type_code");
            Map(x => x.TaskTypeName).Column("task_type_name");
            Map(x => x.TaskTypeDescription).Column("task_type_description");
            Map(x => x.IsSubtask).Column("is_subtask");
            Map(x => x.CreateTs).Column("create_ts");
            Map(x => x.UpdateTs).Column("update_ts");
            Map(x => x.DeleteNbr).Column("delete_nbr");
            Map(x => x.UpdateUser).Column("update_user");
            Map(x => x.CreateUser).Column("create_user");
        }
    }
}
