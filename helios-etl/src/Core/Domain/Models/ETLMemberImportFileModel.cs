using SunnyRewards.Helios.ETL.Common.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.ETL.Core.Domain.Models
{
    public class ETLMemberImportFileModel : BaseModel
    {
        public virtual  long MemberImportFileId { get; set; }
        public virtual string MemberImportCode { get; set; }
        public virtual string FileName { get; set; }
        public virtual string FileStatus { get; set; }
    }
}
