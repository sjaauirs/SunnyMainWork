using SunnyRewards.Helios.Common.Core.Mappings;
using SunnyRewards.Helios.Task.Core.Domain.Models;

namespace SunnyRewards.Helios.Task.Infrastructure.Mappings
{
    public class TaskExternalMappingMap : BaseMapping<TaskExternalMappingModel>
    {
        public TaskExternalMappingMap()
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
