using SunnyRewards.Helios.ETL.Common.Mappings;
using SunnyRewards.Helios.ETL.Core.Domain.Models;

namespace SunnyRewards.Helios.ETL.Infrastructure.Mappings
{
    public class ETLTaskDetailMap : BaseMapping<ETLTaskDetailModel>
    {
        public ETLTaskDetailMap()
        {
            Table("task_detail");
            Schema("task");
            Id(x => x.TaskDetailId).Column("task_detail_id").GeneratedBy.Identity();
            Map(x => x.TaskId).Column("task_id");
            Map(x => x.LanguageCode).Column("language_code");
            Map(x => x.TaskHeader).Column("task_header");
            Map(x => x.TaskDescription).Column("task_description");
            Map(x => x.TermsOfServiceId).Column("terms_of_service_id");
            Map(x => x.CreateTs).Column("create_ts");
            Map(x => x.UpdateTs).Column("update_ts");
            Map(x => x.DeleteNbr).Column("delete_nbr");
            Map(x => x.UpdateUser).Column("update_user");
            Map(x => x.CreateUser).Column("create_user");
            Map(x => x.TenantCode).Column("tenant_code").Not.Nullable();
            Map(x => x.TaskCtaButtonText).Column("task_cta_button_text");
        }
    }
}