using SunnyRewards.Helios.ETL.Common.Domain.Models;

namespace SunnyRewards.Helios.ETL.Core.Domain.Models
{
    public class ETLSweepstakesInstanceModel : BaseModel
        {
            public virtual long SweepstakesInstanceId { get; set; }   
            public virtual long SweepstakesId { get; set; }           
            public virtual long TenantSweepstakesId { get; set; }           
            public virtual DateTime InstanceTs { get; set; }
            public virtual string? PrizeDescriptionJson { get; set; }
            public virtual string? SweepstakesInstanceCode { get; set; }
            public virtual string? Status { get; set; }

    }
}
