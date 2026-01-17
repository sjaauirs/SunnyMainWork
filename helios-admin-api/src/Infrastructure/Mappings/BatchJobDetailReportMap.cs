using SunnyRewards.Helios.Admin.Core.Domain.Models;
using SunnyRewards.Helios.Common.Core.Mappings;

namespace SunnyRewards.Helios.Admin.Infrastructure.Mappings
{
    public class BatchJobDetailReportMap : BaseMapping<BatchJobDetailReportModel>
    {
        public BatchJobDetailReportMap()
        {
            Schema("admin");
            Table("batch_job_detail_report");
            Id(x => x.BatchJobDetailReportId).Column("batch_job_detail_report_id").GeneratedBy.Identity();
            Map(x => x.BatchJobReportId).Column("batch_job_report_id");
            Map(x => x.FileNum).Column("file_num");
            Map(x => x.RecordNum).Column("record_num");
            Map(x => x.RecordResultJson).Column("record_result_json").CustomSqlType("jsonb").CustomType<StringAsJsonb>();
            Map(x => x.CreateTs).Column("create_ts");
            Map(x => x.UpdateTs).Column("update_ts");
            Map(x => x.CreateUser).Column("create_user");
            Map(x => x.UpdateUser).Column("update_user");
            Map(x => x.DeleteNbr).Column("delete_nbr");
        }
    }
}
