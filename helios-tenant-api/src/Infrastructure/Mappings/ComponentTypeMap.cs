using SunnyRewards.Helios.Common.Core.Mappings;
using SunnyRewards.Helios.Tenant.Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Tenant.Infrastructure.Mappings
{
    [ExcludeFromCodeCoverage]
    public class ComponentTypeMap : BaseMapping<ComponentTypeModel>
    {
        public ComponentTypeMap()
        {
            Schema("tenant");
            Table("component_type");

            Id(x => x.Pk).Column("pk").GeneratedBy.Identity();
            Map(x => x.ComponentType).Column("component_type");
            Map(x => x.IsActive).Column("is_active");
            Map(x => x.CreateTs).Column("create_ts");
            Map(x => x.UpdateTs).Column("update_ts");
            Map(x => x.CreateUser).Column("create_user");
            Map(x => x.UpdateUser).Column("update_user");
            Map(x => x.DeleteNbr).Column("delete_nbr");
        }
    }
}
