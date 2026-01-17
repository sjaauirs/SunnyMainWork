using SunnyRewards.Helios.ETL.Common.Mappings;
using SunnyRewards.Helios.ETL.Core.Domain.Models;


namespace SunnyRewards.Helios.ETL.Infrastructure.Mappings
{
    public class BatchFileMap : BaseMapping<ETLBatchFileModel>
    {
        public BatchFileMap()
        {
            Schema("fis");
            Table("batch_file");

            Id(x => x.BatchFileId)
                .Column("batch_file_id")
                .GeneratedBy.Identity();

            Map(x => x.BatchFileCode)
                .Column("batch_file_code")
                .Not.Nullable();

            Map(x => x.Direction)
                .Column("direction")
                .Not.Nullable();

            Map(x => x.FileType)
                .Column("file_type")
                .Not.Nullable();

            Map(x => x.FileName)
                .Column("file_name")
                .Not.Nullable();

            Map(x => x.ProcessStartTs)
                .Column("process_start_ts")
                .Not.Nullable();

            Map(x => x.ProcessEndTs)
                .Column("process_end_ts");

            Map(x => x.CreateTs)
                .Column("create_ts")
                .Not.Nullable();

            Map(x => x.UpdateTs)
                .Column("update_ts");

            Map(x => x.CreateUser)
                .Column("create_user")
                .Not.Nullable();

            Map(x => x.UpdateUser)
                .Column("update_user");

            Map(x => x.DeleteNbr)
                .Column("delete_nbr")
                .Default("0")
                .Not.Nullable();
        }
    }

}
