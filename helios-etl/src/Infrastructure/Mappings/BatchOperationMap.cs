using SunnyRewards.Helios.ETL.Common.Mappings;
using SunnyRewards.Helios.ETL.Core.Domain.Models;

namespace SunnyRewards.Helios.ETL.Infrastructure.Mappings
{
    public class BatchOperationMap : BaseMapping<EtlBatchOperationModel>
    {
        public BatchOperationMap()
        {
            Schema("etl");
            Table("batch_operation");
            Id(x => x.BatchOperationId).Column("batch_operation_id").GeneratedBy.Identity();
            Map(x => x.BatchOperationCode).Column("batch_operation_code ");
            Map(x => x.BatchOperationGroupCode).Column("batch_operation_group_code");
            Map(x => x.BatchAction).Column("batch_action");
            Map(x => x.action_description_json).Column("action_description_json").CustomSqlType("jsonb").CustomType<StringAsJsonb>();
            Map(x => x.CreateTs).Column("create_ts");
            Map(x => x.UpdateTs).Column("update_ts");
            Map(x => x.CreateUser).Column("create_user");
            Map(x => x.UpdateUser).Column("update_user");
            Map(x => x.DeleteNbr).Column("delete_nbr");
        }
    }
}
