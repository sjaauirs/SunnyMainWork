using SunnyRewards.Helios.ETL.Common.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.ETL.Core.Domain.Models
{
    public class CustomerModel : BaseModel
    {
        public virtual long CustomerId { get; set; }
        public virtual string CustomerCode { get; set; } = string.Empty;
        public virtual string CustomerName { get; set; } = string.Empty;
        public virtual string CustomerDescription { get; set; } = string.Empty;
    }
}
