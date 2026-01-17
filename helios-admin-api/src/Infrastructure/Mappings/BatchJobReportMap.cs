using SunnyRewards.Helios.Admin.Core.Domain.Models;
using SunnyRewards.Helios.Common.Core.Mappings;

namespace SunnyRewards.Helios.Admin.Infrastructure.Mappings
{
    public class BatchJobReportMap : BaseMapping<BatchJobReportModel>
    {
        public BatchJobReportMap()
        {
            // Map the class to the table
            Schema("admin");

            Table("batch_job_report");

            // Map the primary key
            Id(x => x.BatchJobReportId)
                .Column("batch_job_report_id")
                .GeneratedBy.Identity();

            // Map properties to columns
            Map(x => x.BatchJobReportCode)
                .Column("batch_job_report_code")
                .Not.Nullable()
                .Length(50);

            Map(x => x.JobType)
                .Column("job_type")
                .Not.Nullable()
                .Length(20);

            Map(x => x.JobResultJson)
                .Column("job_result_json").CustomSqlType("jsonb").CustomType<StringAsJsonb>()
                .Nullable(); 
            Map(x => x.ValidationJson)
                .Column("validation_json").CustomSqlType("jsonb").CustomType<StringAsJsonb>()
                .Nullable();

            Map(x => x.CreateTs)
                .Column("create_ts")
                .Not.Nullable();

            Map(x => x.CreateUser)
                .Column("create_user")
                .Not.Nullable()
                .Length(255);

            Map(x => x.UpdateUser)
                .Column("update_user")
                .Length(255);

            Map(x => x.UpdateTs)
                .Column("update_ts")
                .Nullable();

            Map(x => x.DeleteNbr)
                .Column("delete_nbr")
                .Not.Nullable()
                .Default("0");
        }
    }
}
