using SunnyRewards.Helios.ETL.Common.Mappings;
using SunnyRewards.Helios.ETL.Core.Domain.Models;

namespace Infrastructure.Mappings
{
    public class ETLCohortConsumerMap: BaseMapping<ETLCohortConsumerModel>
    {
        public ETLCohortConsumerMap()
        {
            Schema("cohort");
            Table("cohort_consumer");
            Id(x => x.CohortConsumerId).Column("cohort_consumer_id").GeneratedBy.Identity();
            Map(x => x.CohortId).Column("cohort_id");
            Map(x => x.TenantCode).Column("tenant_code");
            Map(x => x.CohortDetectDescription).Column("cohort_detect_description");
            Map(x => x.ConsumerCode).Column("consumer_code");
            Map(x => x.CreateTs).Column("create_ts");
            Map(x => x.UpdateTs).Column("update_ts");
            Map(x => x.CreateUser).Column("create_user");
            Map(x => x.UpdateUser).Column("update_user");
            Map(x => x.DeleteNbr).Column("delete_nbr");
        }
    }
}
