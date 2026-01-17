using SunnyRewards.Helios.Common.Core.Mappings;
using SunnyRewards.Helios.User.Core.Domain.Models;
using System.Diagnostics.CodeAnalysis;

namespace SunnyRewards.Helios.User.Infrastructure.Mappings
{
    [ExcludeFromCodeCoverage]
    public class ConsumerFlowProgressMap : BaseMapping<ConsumerFlowProgressModel>
        {
            public ConsumerFlowProgressMap()
            {
                Schema("huser");
                Table("consumer_flow_progress");

                Id(x => x.Pk).Column("pk").GeneratedBy.Identity();
                Map(x => x.ConsumerCode).Column("consumer_code");
                Map(x => x.TenantCode).Column("tenant_code").Not.Nullable();
                Map(x => x.CohortCode).Column("cohort_code").Nullable();
                Map(x => x.FlowFk).Column("flow_fk");
                Map(x => x.VersionNbr).Column("version_nbr");
                Map(x => x.FlowStepPk).Column("flow_step_pk");
                Map(x => x.Status).Column("status");
                Map(x => x.ContextJson).Column("context_json").CustomSqlType("jsonb").CustomType<StringAsJsonb>();
                Map(x => x.CreateTs).Column("create_ts");
                Map(x => x.UpdateTs).Column("update_ts");
                Map(x => x.CreateUser).Column("create_user");
                Map(x => x.UpdateUser).Column("update_user");
                Map(x => x.DeleteNbr).Column("delete_nbr");
            }
        }
    
}