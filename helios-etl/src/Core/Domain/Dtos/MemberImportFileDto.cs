using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos
{
    public class MemberImportFileDto
    {
        public long MemberImportFileId { get; set; }
        public string MemberImportCode { get; set; } = "mic-" + Guid.NewGuid().ToString("N");
        public string FileName { get; set; }=string.Empty;
      
    }
}
