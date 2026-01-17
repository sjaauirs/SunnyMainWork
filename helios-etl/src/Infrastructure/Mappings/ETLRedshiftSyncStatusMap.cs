using SunnyRewards.Helios.ETL.Common.Mappings;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.ETL.Infrastructure.Mappings
{
    public class ETLRedshiftSyncStatusMap : BaseMapping<ETLRedshiftSyncStatusModel>
    {
        public ETLRedshiftSyncStatusMap()
        {
            Table("redshift_sync_status");
            Schema("etl");
            Id(x => x.RedshiftSyncStatusId).Column("redshift_sync_status_id").GeneratedBy.Identity();
            Map(x => x.RecordsProcessed).Column("records_processed").Default("0");
            Map(x => x.ErrorMessage).Column("error_message").Nullable();
            Map(x => x.DataType).Column("data_type").Not.Nullable().Length(250);
            Map(x => x.CreateTs).Column("create_ts").Not.Nullable();
            Map(x => x.UpdateTs).Column("update_ts").Nullable();
            Map(x => x.CreateUser).Column("create_user").Not.Nullable().Length(50);
            Map(x => x.UpdateUser).Column("update_user").Nullable().Length(50);
            Map(x => x.DeleteNbr).Column("delete_nbr").Not.Nullable();
            Map(x => x.LastLoadedId).Column("last_loaded_id").Not.Nullable();
        }
    }
}
