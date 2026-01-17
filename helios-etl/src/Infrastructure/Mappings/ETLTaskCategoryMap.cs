using SunnyRewards.Helios.ETL.Common.Mappings;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.ETL.Infrastructure.Mappings
{
    public class ETLTaskCategoryMap : BaseMapping<ETLTaskCategoryModel>
    {
        public ETLTaskCategoryMap()
        {
            Schema("task");
            Table("task_category");
            Id(x => x.TaskCategoryId).Column("task_category_id").GeneratedBy.Identity();
            Map(x => x.TaskCategoryCode).Column("task_category_code").Not.Nullable();
            Map(x => x.TaskCategoryDescription).Column("task_category_description").Nullable();
            Map(x => x.TaskCategoryName).Column("task_category_name").Not.Nullable();
            Map(x => x.CreateTs).Column("create_ts").Not.Nullable();
            Map(x => x.UpdateTs).Column("update_ts").Nullable();
            Map(x => x.CreateUser).Column("create_user").Not.Nullable();
            Map(x => x.UpdateUser).Column("update_user").Nullable();
            Map(x => x.DeleteNbr).Column("delete_nbr").Not.Nullable();
        }
    }
}
