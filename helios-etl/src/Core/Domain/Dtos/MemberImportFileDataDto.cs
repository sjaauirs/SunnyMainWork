using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos
{
    public class MemberImportFileDataDto
    {
        public long MemberImportFileDataId { get; set; }
        public long MemberImportFileId { get; set; }
        public int RecordNumber { get; set; }
    }
}
