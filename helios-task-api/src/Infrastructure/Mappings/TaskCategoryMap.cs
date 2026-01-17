using SunnyRewards.Helios.Common.Core.Mappings;
using SunnyRewards.Helios.Task.Core.Domain.Models;

namespace SunnyRewards.Helios.Task.Infrastructure.Mappings
{
    public class TaskCategoryMap : BaseMapping<TaskCategoryModel>
    {
        public TaskCategoryMap()
        {
            Schema("task");
            Table("task_category");
            Id(x => x.TaskCategoryId).Column("task_category_id").GeneratedBy.Identity();
            Map(x => x.TaskCategoryCode).Column("task_category_code");
            Map(x => x.TaskCategoryDescription).Column("task_category_description");
            Map(x => x.TaskCategoryName).Column("task_category_name");
            Map(x => x.CreateTs).Column("create_ts");
            Map(x => x.UpdateTs).Column("update_ts");
            Map(x => x.DeleteNbr).Column("delete_nbr");
            Map(x => x.UpdateUser).Column("update_user");
            Map(x => x.CreateUser).Column("create_user");
        }
    }
}
