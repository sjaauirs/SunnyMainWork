using SunnyRewards.Helios.Common.Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Tenant.Core.Domain.Models
{
    public class FlowModel : BaseModel
    {
        public virtual long Pk { get; set; }
        public virtual string TenantCode { get; set; } = string.Empty;
        public virtual string? CohortCode { get; set; }
        public virtual string FlowName { get; set; } = string.Empty;
        public virtual int VersionNbr { get; set; }
        public virtual DateTime EffectiveStartTs { get; set; }
        public virtual DateTime? EffectiveEndTs { get; set; }
    }
}
