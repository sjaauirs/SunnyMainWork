using SunnyRewards.Helios.ETL.Common.Mappings;
using SunnyRewards.Helios.ETL.Core.Domain.Models;

namespace SunnyRewards.Helios.ETL.Infrastructure.Mappings
{
    public class ETLTaskMap : BaseMapping<ETLTaskModel>
    {
        public ETLTaskMap()
        {
            Table("task");
            Schema("task");
            Id(x => x.TaskId).Column("task_id").GeneratedBy.Identity();
            Map(x => x.TaskTypeId).Column("task_type_id");
            Map(x => x.TaskCategoryId).Column("task_category_id");
            Map(x => x.TaskCode).Column("task_code");
            Map(x => x.TaskName).Column("task_name");
            Map(x => x.SelfReport).Column("self_report");
            Map(x => x.ConfirmReport).Column("confirm_report");
            Map(x => x.CreateTs).Column("create_ts");
            Map(x => x.UpdateTs).Column("update_ts");
            Map(x => x.DeleteNbr).Column("delete_nbr");
            Map(x => x.UpdateUser).Column("update_user");
            Map(x => x.CreateUser).Column("create_user");
        }
    }
}

