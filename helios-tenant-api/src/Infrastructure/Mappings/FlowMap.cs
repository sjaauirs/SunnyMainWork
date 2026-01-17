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
    public class FlowMap : BaseMapping<FlowModel>
    {
        public FlowMap()
        {
            Schema("tenant");
            Table("flow");

            Id(x => x.Pk).Column("pk").GeneratedBy.Identity();
            Map(x => x.TenantCode).Column("tenant_code").Not.Nullable();
            Map(x => x.CohortCode).Column("cohort_code");
            Map(x => x.FlowName).Column("flow_name");
            Map(x => x.VersionNbr).Column("version_nbr");
            Map(x => x.EffectiveStartTs).Column("effective_start_ts");
            Map(x => x.EffectiveEndTs).Column("effective_end_ts");
            Map(x => x.CreateTs).Column("create_ts");
            Map(x => x.UpdateTs).Column("update_ts");
            Map(x => x.CreateUser).Column("create_user");
            Map(x => x.UpdateUser).Column("update_user");
            Map(x => x.DeleteNbr).Column("delete_nbr");
        }
    }
}
