using SunnyRewards.Helios.ETL.Common.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.ETL.Core.Domain.Models
{
   
    public class ETLTermsOfServiceModel : BaseModel
    {
        public virtual long TermsOfServiceId { get; set; }
        public virtual string? TermsOfServiceText { get; set; }
        public virtual string? LanguageCode { get; set; }
        public virtual string TermsOfServiceCode { get; set; } = null!;

    }
}
