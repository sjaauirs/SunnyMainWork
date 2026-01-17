using SunnyRewards.Helios.Common.Core.Mappings;
using SunnyRewards.Helios.Admin.Core.Domain.Models;

namespace SunnyRewards.Helios.Admin.Infrastructure.Mappings
{
    public class EventHandlerScriptMap : BaseMapping<EventHandlerScriptModel>
    {
        public EventHandlerScriptMap()
        {
            Schema("admin");
            Table("event_handler_script");

            Id(x => x.EventHandlerId)
                .Column("event_handler_id")
                .GeneratedBy.Identity();

            Map(x => x.EventHandlerCode)
                .Column("event_handler_code");


            Map(x => x.TenantCode)
                .Column("tenant_code");

            Map(x => x.ScriptId)
                .Column("script_id");

            Map(x => x.EventType)
                .Column("event_type");

            Map(x => x.EventSubType)
                .Column("event_sub_type");

            Map(x => x.CreateTs)
                .Column("create_ts");

            Map(x => x.UpdateTs)
                .Column("update_ts")
                .Nullable();

            Map(x => x.CreateUser)
                .Column("create_user");

            Map(x => x.UpdateUser)
                .Column("update_user");

            Map(x => x.DeleteNbr)
                .Column("delete_nbr");

        }
    }
}
