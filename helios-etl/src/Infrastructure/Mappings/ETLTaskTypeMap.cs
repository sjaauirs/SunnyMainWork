using SunnyRewards.Helios.ETL.Common.Mappings;
using SunnyRewards.Helios.ETL.Core.Domain.Models;

namespace SunnyRewards.Helios.ETL.Infrastructure.Mappings
{
    public class ETLTaskTypeMap : BaseMapping<ETLTaskTypeModel>
    {
        public ETLTaskTypeMap()
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
