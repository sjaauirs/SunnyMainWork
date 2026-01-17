using SunnyRewards.Helios.ETL.Common.Mappings;
using SunnyRewards.Helios.ETL.Core.Domain.Models;

namespace SunnyRewards.Helios.ETL.Infrastructure.Mappings
{
    public class ConsumerTaskMap : BaseMapping<ETLConsumerTaskModel>
    {
        public ConsumerTaskMap()
        {
            Table("consumer_task");
            Schema("task");
            Id(x => x.ConsumerTaskId).Column("consumer_task_id").GeneratedBy.Identity();
            Map(x => x.TaskId).Column("task_id");
            Map(x => x.TenantCode).Column("tenant_code");
            Map(x => x.TaskStatus).Column("task_status");
            Map(x => x.ConsumerCode).Column("consumer_code");
            Map(x => x.Progress).Column("progress");
            Map(x => x.Notes).Column("notes");
            Map(x => x.TaskStartTs).Column("task_start_ts");
            Map(x => x.TaskCompleteTs).Column("task_completed_ts");
            Map(x => x.UpdateTs).Column("update_ts");
            Map(x => x.CreateTs).Column("create_ts");
            Map(x => x.UpdateUser).Column("update_user");
            Map(x => x.CreateUser).Column("create_user");
            Map(x => x.DeleteNbr).Column("delete_nbr");
            Map(x => x.ProgressDetail).Column("progress_detail").CustomSqlType("jsonb").CustomType<StringAsJsonb>();
            Map(x => x.ParentConsumerTaskId).Column("parent_consumer_task_id");
        }
    }
}
