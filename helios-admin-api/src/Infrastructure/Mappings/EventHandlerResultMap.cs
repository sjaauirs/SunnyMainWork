using SunnyRewards.Helios.Common.Core.Mappings;
using SunnyRewards.Helios.Admin.Core.Domain.Models;

namespace SunnyRewards.Helios.Admin.Infrastructure.Mappings
{
    public class EventHandlerResultMap : BaseMapping<EventHandlerResultModel>
    {
        public EventHandlerResultMap()
        {
            Schema("admin");
            Table("event_handler_result");

            Id(x => x.EventHandlerResultId)
                .Column("event_handler_result_id")
                .GeneratedBy.Identity();

            Map(x => x.EventCode)
                .Column("event_code");

            Map(x => x.EventHandlerScriptId)
                .Column("event_handler_script_id");

            Map(x => x.EventHandlerName)
                .Column("event_handler_name");

            Map(x => x.EventData)
                .Column("event_data")
                .CustomSqlType("jsonb").CustomType<StringAsJsonb>();

            Map(x => x.ResultStatus)
                .Column("result_status");


            Map(x => x.ResultDescriptionJson)
                .Column("result_description_json")
                .CustomSqlType("jsonb").CustomType<StringAsJsonb>();

            Map(x => x.CreateTs)
                .Column("create_ts");

            Map(x => x.UpdateTs)
                .Column("update_ts");

            Map(x => x.CreateUser)
                .Column("create_user");

            Map(x => x.UpdateUser)
                .Column("update_user");

            Map(x => x.DeleteNbr)
                .Column("delete_nbr");
        }
    }
}
