using SunnyRewards.Helios.Common.Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Tenant.Core.Domain.Models
{
    public class ComponentTypeModel : BaseModel
    {
        public virtual long Pk { get; set; }
        public virtual string ComponentType { get; set; } = string.Empty;
        public virtual bool IsActive { get; set; }
    }
}
