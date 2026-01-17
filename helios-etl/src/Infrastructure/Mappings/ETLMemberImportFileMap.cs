using SunnyRewards.Helios.ETL.Common.Mappings;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.ETL.Infrastructure.Mappings
{
    public class ETLMemberImportFileMap : BaseMapping<ETLMemberImportFileModel>
    {
        public ETLMemberImportFileMap()
        {
            Schema("etl");
            Table("member_import_file");
            Id(x => x.MemberImportFileId).Column("member_import_file_id").GeneratedBy.Identity();
            Map(x => x.MemberImportCode).Column("member_import_code");
            Map(x => x.FileName).Column("file_name");
            Map(x => x.CreateTs).Column("create_ts");
            Map(x => x.UpdateTs).Column("update_ts");
            Map(x => x.CreateUser).Column("create_user");
            Map(x => x.UpdateUser).Column("update_user");
            Map(x => x.DeleteNbr).Column("delete_nbr");
            Map(x => x.FileStatus).Column("file_status");
        }
    }
}
