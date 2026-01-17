using SunnyRewards.Helios.Common.Core.Mappings;
using SunnyRewards.Helios.Task.Core.Domain.Models;

namespace SunnyRewards.Helios.Task.Infrastructure.Mappings
{
    public class TaskMap : BaseMapping<TaskModel>
    {
        public TaskMap()
        {
            Table("task");
            Schema("task");
            Id(x => x.TaskId).Column("task_id").GeneratedBy.Identity();
            Map(x => x.TaskTypeId).Column("task_type_id");
            Map(x => x.TaskCode).Column("task_code");
            Map(x => x.TaskName).Column("task_name");
            Map(x => x.SelfReport).Column("self_report");
            Map(x => x.ConfirmReport).Column("confirm_report");
            Map(x => x.TaskCategoryId).Column("task_category_id").Nullable();
            Map(x => x.CreateTs).Column("create_ts");
            Map(x => x.UpdateTs).Column("update_ts");
            Map(x => x.DeleteNbr).Column("delete_nbr");
            Map(x => x.UpdateUser).Column("update_user");
            Map(x => x.CreateUser).Column("create_user");
            Map(x => x.IsSubtask).Column("is_subtask");
        }
    }
}

