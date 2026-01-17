using SunnyRewards.Helios.Common.Core.Mappings;
using SunnyRewards.Helios.Task.Core.Domain.Models;

namespace SunnyRewards.Helios.Task.Infrastructure.Mappings
{
    public class TenantTaskCategoryMap : BaseMapping<TenantTaskCategoryModel>
    {
        public TenantTaskCategoryMap()
        {
            Schema("task");
            Table("tenant_task_category");
            Id(x => x.TenantTaskCategoryId).Column("tenant_task_category_id").GeneratedBy.Identity();
            Map(x => x.TaskCategoryId).Column("task_category_id");
            Map(x => x.TenantCode).Column("tenant_code");
            Map(x => x.ResourceJson).Column("resource_json").CustomSqlType("jsonb").CustomType<StringAsJsonb>();
            Map(x => x.CreateTs).Column("create_ts");
            Map(x => x.UpdateTs).Column("update_ts");
            Map(x => x.DeleteNbr).Column("delete_nbr");
            Map(x => x.UpdateUser).Column("update_user");
            Map(x => x.CreateUser).Column("create_user");
        }
    }
}
