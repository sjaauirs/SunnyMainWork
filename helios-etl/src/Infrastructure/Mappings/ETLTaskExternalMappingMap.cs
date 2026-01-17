using SunnyRewards.Helios.ETL.Common.Mappings;
using SunnyRewards.Helios.ETL.Core.Domain.Models;

namespace SunnyRewards.Helios.ETL.Infrastructure.Mappings
{
    public class ETLTaskExternalMappingMap : BaseMapping<ETLTaskExternalMappingModel>
    {
        public ETLTaskExternalMappingMap()
        {
            Schema("task");
            Table("task_external_mapping");
            Id(x => x.TaskExternalMappingId).Column("task_external_mapping_id").GeneratedBy.Identity();
            Map(x => x.TenantCode).Column("tenant_code");
            Map(x => x.TaskThirdPartyCode).Column("task_third_party_code");
            Map(x => x.TaskExternalCode).Column("task_external_code");
            Map(x => x.CreateTs).Column("create_ts");
            Map(x => x.UpdateTs).Column("update_ts");
            Map(x => x.CreateUser).Column("create_user");
            Map(x => x.UpdateUser).Column("update_user");
            Map(x => x.DeleteNbr).Column("delete_nbr");
        }
    }
}
