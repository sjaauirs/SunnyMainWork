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
    public class FlowStepMap : BaseMapping<FlowStepModel>
    {
        public FlowStepMap()
        {
            Schema("tenant");
            Table("flow_step");

            Id(x => x.Pk).Column("pk").GeneratedBy.Identity();
            Map(x => x.FlowFk).Column("flow_fk");
            Map(x => x.StepIdx).Column("step_idx");
            Map(x => x.CurrentComponentCatalogueFk).Column("current_component_catalogue_fk").Not.Nullable();
            Map(x => x.OnSuccessComponentCatalogueFk).Column("on_success_component_catalogue_fk").Nullable();
            Map(x => x.OnFailureComponentCatalogueFk).Column("on_failure_component_catalogue_fk").Nullable();
            Map(x => x.CreateTs).Column("create_ts");
            Map(x => x.UpdateTs).Column("update_ts");
            Map(x => x.CreateUser).Column("create_user");
            Map(x => x.UpdateUser).Column("update_user");
            Map(x => x.DeleteNbr).Column("delete_nbr");
            Map(x=>x.StepConfig).Column("step_config").CustomSqlType("jsonb").CustomType<StringAsJsonb>().Nullable();
        }
    }
}
