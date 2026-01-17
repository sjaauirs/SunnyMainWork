using SunnyRewards.Helios.ETL.Common.Mappings;
using SunnyRewards.Helios.ETL.Core.Domain.Models;

namespace Infrastructure.Mappings
{
    public class ETLCohortMap : BaseMapping<ETLCohortModel>
    {
        public ETLCohortMap()
        {
            Schema("cohort");
            Table("cohort");
            Id(x => x.CohortId).Column("cohort_id").GeneratedBy.Identity();
            Map(x => x.CohortCode).Column("cohort_code");
            Map(x => x.CohortName).Column("cohort_name");
            Map(x => x.CohortDescription).Column("cohort_description");
            Map(x => x.ParentCohortId).Column("parent_cohort_id");
            Map(x => x.CohortRule).Column("cohort_rule").CustomSqlType("jsonb").CustomType<StringAsJsonb>();

            Map(x => x.CohortEnabled).Column("cohort_enabled");
            Map(x => x.IncludeInCohortingJob).Column("include_in_cohorting_job").Default("true");
            Map(x => x.CreateTs).Column("create_ts");
            Map(x => x.UpdateUser).Column("update_user");
            Map(x => x.CreateUser).Column("create_user");
            Map(x => x.DeleteNbr).Column("delete_nbr");
        }

    }
}
