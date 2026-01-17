using SunnyRewards.Helios.Common.Core.Mappings;
using SunnyRewards.Helios.Tenant.Core.Domain.Models;

namespace SunnyRewards.Helios.Tenant.Infrastructure.Mappings
{
    public class CategoryMap : BaseMapping<CategoryModel>
    {
        public CategoryMap()
        {
            Schema("tenant");
            Table("category");

            Id(x => x.Id).Column("id").GeneratedBy.Identity();
            Map(x => x.Name).Column("name").Not.Nullable();
            Map(x => x.GoogleType).Column("google_type");
            Map(x => x.IsActive).Column("is_active");

            // BaseModel fields
            Map(x => x.CreateTs).Column("create_ts");
            Map(x => x.UpdateTs).Column("update_ts");
            Map(x => x.CreateUser).Column("create_user");
            Map(x => x.UpdateUser).Column("update_user");
            Map(x => x.DeleteNbr).Column("delete_nbr");
        }
    }
}
